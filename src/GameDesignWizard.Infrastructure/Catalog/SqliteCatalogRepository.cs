using GameDesignWizard.Core.Catalog;
using GameDesignWizard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameDesignWizard.Infrastructure.Catalog;

public sealed class SqliteCatalogRepository(AppDbContextFactory contextFactory) : ICatalogRepository
{
    private static readonly CatalogOption[] BuiltInPlatforms =
    [
        CreateBuiltIn("0d3034e8-e927-4a2f-a96f-ae610a47fb2f", "PC", 0, PlatformPoolGroup.Pc),
        CreateBuiltIn("db7d530a-5cb9-411c-bb8e-7006282f63d7", "Mobile", 1, PlatformPoolGroup.Mobile),
        CreateBuiltIn("1826ccb8-23f3-44a8-af8c-538d25e7e036", "Console", 2, PlatformPoolGroup.Other),
        CreateBuiltIn("ec646f17-5476-4d76-992e-223d4b4d392b", "VR", 3, PlatformPoolGroup.Other),
        CreateBuiltIn("e0cb0941-ddc3-4b03-956f-7824da01c224", "Board Game", 4, PlatformPoolGroup.Other)
    ];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.CreateDbContext();
        await context.Database.MigrateAsync(cancellationToken);

        var existingIds = await context.CatalogOptions
            .Where(option => option.Category == CatalogCategory.Platform)
            .Select(option => option.Id)
            .ToListAsync(cancellationToken);
        var missingBuiltIns = BuiltInPlatforms
            .Where(platform => !existingIds.Contains(platform.Id))
            .Select(Clone)
            .ToArray();
        if (missingBuiltIns.Length > 0)
        {
            context.CatalogOptions.AddRange(missingBuiltIns);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<CatalogOption>> GetPlatformsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.CreateDbContext();
        return await context.CatalogOptions
            .AsNoTracking()
            .Where(option => option.Category == CatalogCategory.Platform)
            .OrderBy(option => option.SortOrder)
            .ThenBy(option => option.NameEnglish)
            .ToListAsync(cancellationToken);
    }

    public async Task<CatalogOption> AddPlatformAsync(
        string nameEnglish,
        CancellationToken cancellationToken = default)
    {
        var displayName = CleanName(nameEnglish);
        var normalizedName = NormalizeName(displayName);
        await using var context = contextFactory.CreateDbContext();

        if (await context.CatalogOptions.AnyAsync(
                option => option.Category == CatalogCategory.Platform && option.NormalizedName == normalizedName,
                cancellationToken))
        {
            throw new InvalidOperationException($"{displayName} already exists in the platform catalog.");
        }

        var highestSortOrder = await context.CatalogOptions
            .Where(option => option.Category == CatalogCategory.Platform)
            .Select(option => (int?)option.SortOrder)
            .MaxAsync(cancellationToken);
        var nextSortOrder = highestSortOrder is null ? 0 : highestSortOrder.Value + 1;
        var now = DateTime.UtcNow;
        var platform = new CatalogOption
        {
            Id = Guid.NewGuid(),
            Category = CatalogCategory.Platform,
            NameEnglish = displayName,
            NormalizedName = normalizedName,
            SortOrder = nextSortOrder,
            IsActive = true,
            IsBuiltIn = false,
            PlatformPoolGroup = PlatformPoolGroup.Other,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        context.CatalogOptions.Add(platform);
        await context.SaveChangesAsync(cancellationToken);
        return Clone(platform);
    }

    public async Task<CatalogOption> SetPlatformActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.CreateDbContext();
        var platform = await context.CatalogOptions.SingleOrDefaultAsync(
            option => option.Id == id && option.Category == CatalogCategory.Platform,
            cancellationToken) ?? throw new InvalidOperationException("The platform no longer exists.");

        platform.IsActive = isActive;
        platform.UpdatedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return Clone(platform);
    }

    private static CatalogOption CreateBuiltIn(
        string id,
        string name,
        int sortOrder,
        PlatformPoolGroup poolGroup)
    {
        var timestamp = new DateTime(2026, 9, 13, 0, 0, 0, DateTimeKind.Utc);
        return new CatalogOption
        {
            Id = Guid.Parse(id),
            Category = CatalogCategory.Platform,
            NameEnglish = name,
            NormalizedName = NormalizeName(name),
            SortOrder = sortOrder,
            IsActive = true,
            IsBuiltIn = true,
            PlatformPoolGroup = poolGroup,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };
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
            throw new InvalidOperationException("Enter a platform name before adding it.");
        }

        if (cleaned.Length > 200)
        {
            throw new InvalidOperationException("Platform names must contain 200 characters or fewer.");
        }

        return cleaned;
    }

    private static string NormalizeName(string value) => value.ToUpperInvariant();
}
