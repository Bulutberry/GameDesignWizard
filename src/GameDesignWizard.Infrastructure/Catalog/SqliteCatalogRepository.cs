using GameDesignWizard.Core.Catalog;
using GameDesignWizard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameDesignWizard.Infrastructure.Catalog;

public sealed class SqliteCatalogRepository : ICatalogRepository
{
    private readonly AppDbContextFactory _contextFactory;
    private readonly IDefaultCatalogProvider _defaultCatalogProvider;

    public SqliteCatalogRepository(AppDbContextFactory contextFactory)
        : this(contextFactory, new EmbeddedDefaultCatalogProvider())
    {
    }

    public SqliteCatalogRepository(
        AppDbContextFactory contextFactory,
        IDefaultCatalogProvider defaultCatalogProvider)
    {
        _contextFactory = contextFactory;
        _defaultCatalogProvider = defaultCatalogProvider;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory.CreateDbContext();
        await context.Database.MigrateAsync(cancellationToken);

        var defaults = _defaultCatalogProvider.Load();
        var existing = await context.CatalogOptions.ToListAsync(cancellationToken);
        var existingById = existing.ToDictionary(option => option.Id);
        var existingByName = existing.ToDictionary(
            option => (option.Category, option.NormalizedName),
            option => option);
        var identityMap = new Dictionary<Guid, Guid>();

        foreach (var defaultOption in defaults)
        {
            if (existingById.TryGetValue(defaultOption.Id, out var sameIdentity))
            {
                identityMap[defaultOption.Id] = sameIdentity.Id;
            }
            else if (existingByName.TryGetValue(
                         (defaultOption.Category, defaultOption.NormalizedName),
                         out var sameName))
            {
                identityMap[defaultOption.Id] = sameName.Id;
            }
        }

        var missingBuiltIns = defaults
            .Where(option => !identityMap.ContainsKey(option.Id))
            .Select(option =>
            {
                var clone = Clone(option);
                if (clone.ParentOptionId is Guid parentId
                    && identityMap.TryGetValue(parentId, out var mappedParentId))
                {
                    clone.ParentOptionId = mappedParentId;
                }

                return clone;
            })
            .ToArray();
        if (missingBuiltIns.Length > 0)
        {
            context.CatalogOptions.AddRange(missingBuiltIns);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<CatalogOption>> GetOptionsAsync(
        CatalogCategory category,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.CatalogOptions
            .AsNoTracking()
            .Where(option => option.Category == category)
            .OrderBy(option => option.SortOrder)
            .ThenBy(option => option.NameEnglish)
            .ToListAsync(cancellationToken);
    }

    public async Task<CatalogOption> AddOptionAsync(
        CatalogCategory category,
        string nameEnglish,
        Guid? parentOptionId = null,
        CancellationToken cancellationToken = default) =>
        (await AddOptionsAsync(category, [nameEnglish], parentOptionId, cancellationToken)).Single();

    public async Task<IReadOnlyList<CatalogOption>> AddOptionsAsync(
        CatalogCategory category,
        IReadOnlyCollection<string> namesEnglish,
        Guid? parentOptionId = null,
        CancellationToken cancellationToken = default)
    {
        if (namesEnglish.Count == 0)
        {
            return [];
        }

        var names = namesEnglish.Select(CleanName).ToArray();
        var duplicateName = names
            .GroupBy(NormalizeName)
            .FirstOrDefault(group => group.Count() > 1)?.First();
        if (duplicateName is not null)
        {
            throw new InvalidOperationException($"{duplicateName} appears more than once in the import selection.");
        }

        await using var context = _contextFactory.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var normalizedNames = names.Select(NormalizeName).ToArray();
        var existingNames = await context.CatalogOptions
            .Where(option => option.Category == category && normalizedNames.Contains(option.NormalizedName))
            .Select(option => option.NameEnglish)
            .ToListAsync(cancellationToken);

        if (existingNames.Count > 0)
        {
            throw new InvalidOperationException($"{existingNames[0]} already exists in the {GetCategoryLabel(category)} catalog.");
        }

        if (category == CatalogCategory.Subgenre)
        {
            if (parentOptionId is null)
            {
                throw new InvalidOperationException("Select a parent genre before adding a subgenre.");
            }

            var parentExists = await context.CatalogOptions.AnyAsync(
                option => option.Id == parentOptionId && option.Category == CatalogCategory.Genre,
                cancellationToken);
            if (!parentExists)
            {
                throw new InvalidOperationException("The selected parent genre no longer exists.");
            }
        }
        else
        {
            parentOptionId = null;
        }

        var highestSortOrder = await context.CatalogOptions
            .Where(option => option.Category == category)
            .Select(option => (int?)option.SortOrder)
            .MaxAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var firstSortOrder = highestSortOrder is null ? 0 : highestSortOrder.Value + 1;
        var options = names.Select((name, index) => new CatalogOption
            {
                Id = Guid.NewGuid(),
                Category = category,
                NameEnglish = name,
                NormalizedName = NormalizeName(name),
                ParentOptionId = parentOptionId,
                SortOrder = firstSortOrder + index,
                IsActive = true,
                IsBuiltIn = false,
                PlatformPoolGroup = PlatformPoolGroup.Other,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            })
            .ToArray();

        context.CatalogOptions.AddRange(options);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return options.Select(Clone).ToArray();
    }

    public async Task<CatalogOption> SetOptionActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var context = _contextFactory.CreateDbContext();
        var option = await context.CatalogOptions.SingleOrDefaultAsync(
            candidate => candidate.Id == id,
            cancellationToken) ?? throw new InvalidOperationException("The catalog option no longer exists.");

        var now = DateTime.UtcNow;
        option.IsActive = isActive;
        option.UpdatedAtUtc = now;

        if (option.Category == CatalogCategory.Genre && !isActive)
        {
            var childSubgenres = await context.CatalogOptions
                .Where(candidate => candidate.Category == CatalogCategory.Subgenre
                    && candidate.ParentOptionId == option.Id
                    && candidate.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var subgenre in childSubgenres)
            {
                subgenre.IsActive = false;
                subgenre.UpdatedAtUtc = now;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Clone(option);
    }

    public async Task<CatalogOption> RenameOptionAsync(
        Guid id,
        string nameEnglish,
        CancellationToken cancellationToken = default)
    {
        var name = CleanName(nameEnglish);
        var normalizedName = NormalizeName(name);
        await using var context = _contextFactory.CreateDbContext();
        var option = await context.CatalogOptions.SingleOrDefaultAsync(
            candidate => candidate.Id == id,
            cancellationToken) ?? throw new InvalidOperationException("The catalog option no longer exists.");

        var nameExists = await context.CatalogOptions.AnyAsync(
            candidate => candidate.Id != id
                && candidate.Category == option.Category
                && candidate.NormalizedName == normalizedName,
            cancellationToken);
        if (nameExists)
        {
            throw new InvalidOperationException($"{name} already exists in the {GetCategoryLabel(option.Category)} catalog.");
        }

        option.NameEnglish = name;
        option.NormalizedName = normalizedName;
        option.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return Clone(option);
    }

    public async Task<CatalogOption> MoveOptionAsync(
        Guid id,
        int offset,
        CancellationToken cancellationToken = default)
    {
        if (offset is not (-1 or 1))
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "The move offset must be -1 or 1.");
        }

        await using var context = _contextFactory.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var option = await context.CatalogOptions.SingleOrDefaultAsync(
            candidate => candidate.Id == id,
            cancellationToken) ?? throw new InvalidOperationException("The catalog option no longer exists.");
        var categoryOptions = await context.CatalogOptions
            .Where(candidate => candidate.Category == option.Category)
            .OrderBy(candidate => candidate.SortOrder)
            .ThenBy(candidate => candidate.NameEnglish)
            .ToListAsync(cancellationToken);
        var siblings = option.Category == CatalogCategory.Subgenre
            ? categoryOptions.Where(candidate => candidate.ParentOptionId == option.ParentOptionId).ToList()
            : categoryOptions;
        var currentIndex = siblings.FindIndex(candidate => candidate.Id == id);
        var targetIndex = currentIndex + offset;
        if (currentIndex < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
        {
            return Clone(option);
        }

        if (categoryOptions.Select(candidate => candidate.SortOrder).Distinct().Count() != categoryOptions.Count)
        {
            for (var index = 0; index < categoryOptions.Count; index++)
            {
                categoryOptions[index].SortOrder = index;
            }
        }

        var target = siblings[targetIndex];
        (option.SortOrder, target.SortOrder) = (target.SortOrder, option.SortOrder);
        var now = DateTime.UtcNow;
        option.UpdatedAtUtc = now;
        target.UpdatedAtUtc = now;
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Clone(option);
    }

    private static CatalogOption Clone(CatalogOption option) => new()
    {
        Id = option.Id,
        Category = option.Category,
        NameEnglish = option.NameEnglish,
        NormalizedName = option.NormalizedName,
        DescriptionEnglish = option.DescriptionEnglish,
        ParentOptionId = option.ParentOptionId,
        SortOrder = option.SortOrder,
        IsActive = option.IsActive,
        IsBuiltIn = option.IsBuiltIn,
        PlatformPoolGroup = option.PlatformPoolGroup,
        CreatedAtUtc = option.CreatedAtUtc,
        UpdatedAtUtc = option.UpdatedAtUtc
    };

    private static string CleanName(string value)
    {
        var cleaned = string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (cleaned.Length == 0)
        {
            throw new InvalidOperationException("Enter an option name before adding it.");
        }

        if (cleaned.Length > 200)
        {
            throw new InvalidOperationException("Option names must contain 200 characters or fewer.");
        }

        return cleaned;
    }

    private static string NormalizeName(string value) => value.ToUpperInvariant();

    private static string GetCategoryLabel(CatalogCategory category) => category switch
    {
        CatalogCategory.ArtStyle => "art style",
        CatalogCategory.DevelopmentDuration => "development duration",
        CatalogCategory.TeamSize => "team size",
        _ => category.ToString().ToLowerInvariant()
    };
}
