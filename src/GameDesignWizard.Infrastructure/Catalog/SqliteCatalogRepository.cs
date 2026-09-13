using System.Security.Cryptography;
using System.Text;
using GameDesignWizard.Core.Catalog;
using GameDesignWizard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameDesignWizard.Infrastructure.Catalog;

public sealed class SqliteCatalogRepository(AppDbContextFactory contextFactory) : ICatalogRepository
{
    private static readonly CatalogOption[] BuiltInOptions = CreateBuiltInOptions();

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.CreateDbContext();
        await context.Database.MigrateAsync(cancellationToken);

        var existingIds = await context.CatalogOptions
            .Select(option => option.Id)
            .ToListAsync(cancellationToken);
        var missingBuiltIns = BuiltInOptions
            .Where(option => !existingIds.Contains(option.Id))
            .Select(Clone)
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
        await using var context = contextFactory.CreateDbContext();
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
        CancellationToken cancellationToken = default)
    {
        var displayName = CleanName(nameEnglish);
        var normalizedName = NormalizeName(displayName);
        await using var context = contextFactory.CreateDbContext();

        if (await context.CatalogOptions.AnyAsync(
                option => option.Category == category && option.NormalizedName == normalizedName,
                cancellationToken))
        {
            throw new InvalidOperationException($"{displayName} already exists in the {GetCategoryLabel(category)} catalog.");
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
        var option = new CatalogOption
        {
            Id = Guid.NewGuid(),
            Category = category,
            NameEnglish = displayName,
            NormalizedName = normalizedName,
            ParentOptionId = parentOptionId,
            SortOrder = highestSortOrder is null ? 0 : highestSortOrder.Value + 1,
            IsActive = true,
            IsBuiltIn = false,
            PlatformPoolGroup = PlatformPoolGroup.Other,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        context.CatalogOptions.Add(option);
        await context.SaveChangesAsync(cancellationToken);
        return Clone(option);
    }

    public async Task<CatalogOption> SetOptionActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.CreateDbContext();
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

    private static CatalogOption[] CreateBuiltInOptions()
    {
        var actionId = BuiltInId(CatalogCategory.Genre, "Action");
        var adventureId = BuiltInId(CatalogCategory.Genre, "Adventure");
        var rolePlayingId = BuiltInId(CatalogCategory.Genre, "Role-Playing");
        var strategyId = BuiltInId(CatalogCategory.Genre, "Strategy");
        var simulationId = BuiltInId(CatalogCategory.Genre, "Simulation");

        return
        [
            CreatePlatform("0d3034e8-e927-4a2f-a96f-ae610a47fb2f", "PC", 0, PlatformPoolGroup.Pc),
            CreatePlatform("db7d530a-5cb9-411c-bb8e-7006282f63d7", "Mobile", 1, PlatformPoolGroup.Mobile),
            CreatePlatform("1826ccb8-23f3-44a8-af8c-538d25e7e036", "Console", 2, PlatformPoolGroup.Other),
            CreatePlatform("ec646f17-5476-4d76-992e-223d4b4d392b", "VR", 3, PlatformPoolGroup.Other),
            CreatePlatform("e0cb0941-ddc3-4b03-956f-7824da01c224", "Board Game", 4, PlatformPoolGroup.Other),

            CreateBuiltIn(CatalogCategory.Genre, "Action", 0),
            CreateBuiltIn(CatalogCategory.Genre, "Adventure", 1),
            CreateBuiltIn(CatalogCategory.Genre, "Role-Playing", 2),
            CreateBuiltIn(CatalogCategory.Genre, "Strategy", 3),
            CreateBuiltIn(CatalogCategory.Genre, "Simulation", 4),

            CreateBuiltIn(CatalogCategory.Subgenre, "Platformer", 0, actionId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Shooter", 1, actionId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Point-and-Click", 2, adventureId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Narrative Adventure", 3, adventureId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Action RPG", 4, rolePlayingId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Turn-Based RPG", 5, rolePlayingId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Real-Time Strategy", 6, strategyId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Turn-Based Strategy", 7, strategyId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Life Simulation", 8, simulationId),
            CreateBuiltIn(CatalogCategory.Subgenre, "Management Simulation", 9, simulationId),

            CreateBuiltIn(CatalogCategory.Topic, "Space", 0),
            CreateBuiltIn(CatalogCategory.Topic, "Zombies", 1),
            CreateBuiltIn(CatalogCategory.Topic, "World War II", 2),
            CreateBuiltIn(CatalogCategory.Topic, "Dogs", 3),
            CreateBuiltIn(CatalogCategory.Topic, "Fantasy", 4),
            CreateBuiltIn(CatalogCategory.Topic, "Cyberpunk", 5),

            CreateBuiltIn(CatalogCategory.Mechanic, "Exploration", 0),
            CreateBuiltIn(CatalogCategory.Mechanic, "Combat", 1),
            CreateBuiltIn(CatalogCategory.Mechanic, "Crafting", 2),
            CreateBuiltIn(CatalogCategory.Mechanic, "Resource Management", 3),
            CreateBuiltIn(CatalogCategory.Mechanic, "Dialogue Choices", 4),
            CreateBuiltIn(CatalogCategory.Mechanic, "Stealth", 5),

            CreateBuiltIn(CatalogCategory.Feature, "Single Player", 0),
            CreateBuiltIn(CatalogCategory.Feature, "Multiplayer", 1),
            CreateBuiltIn(CatalogCategory.Feature, "Co-op", 2),
            CreateBuiltIn(CatalogCategory.Feature, "Procedural Generation", 3),
            CreateBuiltIn(CatalogCategory.Feature, "Mod Support", 4),

            CreateBuiltIn(CatalogCategory.ArtStyle, "Pixel Art", 0),
            CreateBuiltIn(CatalogCategory.ArtStyle, "Stylized", 1),
            CreateBuiltIn(CatalogCategory.ArtStyle, "Realistic", 2),
            CreateBuiltIn(CatalogCategory.ArtStyle, "Low Poly", 3),
            CreateBuiltIn(CatalogCategory.ArtStyle, "Hand-Drawn", 4),

            CreateBuiltIn(CatalogCategory.DevelopmentDuration, "1-3 Months", 0),
            CreateBuiltIn(CatalogCategory.DevelopmentDuration, "3-6 Months", 1),
            CreateBuiltIn(CatalogCategory.DevelopmentDuration, "6-12 Months", 2),
            CreateBuiltIn(CatalogCategory.DevelopmentDuration, "1-2 Years", 3),
            CreateBuiltIn(CatalogCategory.DevelopmentDuration, "2+ Years", 4),

            CreateBuiltIn(CatalogCategory.TeamSize, "Solo", 0),
            CreateBuiltIn(CatalogCategory.TeamSize, "2-5 People", 1),
            CreateBuiltIn(CatalogCategory.TeamSize, "6-10 People", 2),
            CreateBuiltIn(CatalogCategory.TeamSize, "11-25 People", 3),
            CreateBuiltIn(CatalogCategory.TeamSize, "26+ People", 4)
        ];
    }

    private static CatalogOption CreatePlatform(
        string id,
        string name,
        int sortOrder,
        PlatformPoolGroup poolGroup) =>
        CreateBuiltIn(CatalogCategory.Platform, name, sortOrder, null, Guid.Parse(id), poolGroup);

    private static CatalogOption CreateBuiltIn(
        CatalogCategory category,
        string name,
        int sortOrder,
        Guid? parentOptionId = null,
        Guid? id = null,
        PlatformPoolGroup poolGroup = PlatformPoolGroup.Other)
    {
        var timestamp = new DateTime(2026, 9, 13, 0, 0, 0, DateTimeKind.Utc);
        return new CatalogOption
        {
            Id = id ?? BuiltInId(category, name),
            Category = category,
            NameEnglish = name,
            NormalizedName = NormalizeName(name),
            ParentOptionId = parentOptionId,
            SortOrder = sortOrder,
            IsActive = true,
            IsBuiltIn = true,
            PlatformPoolGroup = poolGroup,
            CreatedAtUtc = timestamp,
            UpdatedAtUtc = timestamp
        };
    }

    private static Guid BuiltInId(CatalogCategory category, string name)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"GameDesignWizard:{category}:{name}"));
        return new Guid(hash.AsSpan(0, 16));
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
