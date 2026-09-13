using System.Text.Json;
using System.Text.Json.Serialization;
using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.Infrastructure.Catalog;

public sealed class DefaultCatalogExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<int> ExportAsync(
        ICatalogRepository repository,
        string outputPath,
        string catalogVersion,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(catalogVersion))
        {
            throw new ArgumentException("A catalog version is required.", nameof(catalogVersion));
        }

        var options = new List<CatalogOption>();
        foreach (var category in Enum.GetValues<CatalogCategory>())
        {
            var categoryOptions = await repository.GetOptionsAsync(category, cancellationToken);
            options.AddRange(categoryOptions.Where(option => option.IsActive));
        }

        Validate(options);
        var document = new DefaultCatalogDocument
        {
            SchemaVersion = 1,
            CatalogVersion = catalogVersion.Trim(),
            SourceLanguage = "en",
            PublishedAtUtc = DateTime.UtcNow,
            Options = options
                .OrderBy(option => option.Category)
                .ThenBy(option => option.SortOrder)
                .ThenBy(option => option.NameEnglish)
                .Select(option => new DefaultCatalogOptionDefinition
                {
                    Id = option.Id,
                    Category = option.Category,
                    NameEnglish = option.NameEnglish,
                    DescriptionEnglish = option.DescriptionEnglish,
                    ParentOptionId = option.ParentOptionId,
                    SortOrder = option.SortOrder,
                    PlatformPoolGroup = option.PlatformPoolGroup
                })
                .ToList()
        };

        var fullOutputPath = Path.GetFullPath(outputPath);
        var outputDirectory = Path.GetDirectoryName(fullOutputPath)
            ?? throw new InvalidOperationException("The output path must include a directory.");
        Directory.CreateDirectory(outputDirectory);
        var temporaryPath = Path.Combine(outputDirectory, $".{Path.GetFileName(fullOutputPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = File.Create(temporaryPath))
            {
                await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken);
                await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
            }

            File.Move(temporaryPath, fullOutputPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return options.Count;
    }

    private static void Validate(IReadOnlyCollection<CatalogOption> options)
    {
        foreach (var category in Enum.GetValues<CatalogCategory>())
        {
            if (!options.Any(option => option.Category == category))
            {
                throw new InvalidOperationException($"The active catalog has no {category} options to export.");
            }
        }

        var activeGenreIds = options
            .Where(option => option.Category == CatalogCategory.Genre)
            .Select(option => option.Id)
            .ToHashSet();
        if (options.Any(option => option.Category == CatalogCategory.Subgenre
                && (option.ParentOptionId is null || !activeGenreIds.Contains(option.ParentOptionId.Value))))
        {
            throw new InvalidOperationException("Every active subgenre must reference an active genre before export.");
        }

        if (options.Count(option => option.Category == CatalogCategory.Platform
                && option.PlatformPoolGroup == PlatformPoolGroup.Pc) != 1
            || options.Count(option => option.Category == CatalogCategory.Platform
                && option.PlatformPoolGroup == PlatformPoolGroup.Mobile) != 1)
        {
            throw new InvalidOperationException("Exactly one active PC and one active Mobile pool platform are required before export.");
        }
    }
}
