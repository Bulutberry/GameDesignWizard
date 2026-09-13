using System.Text.Json;
using System.Text.Json.Serialization;
using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.Infrastructure.Catalog;

public sealed class EmbeddedDefaultCatalogProvider : IDefaultCatalogProvider
{
    private const string ResourceName =
        "GameDesignWizard.Infrastructure.Catalog.Defaults.default-catalog.en.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public IReadOnlyList<CatalogOption> Load()
    {
        using var stream = typeof(EmbeddedDefaultCatalogProvider).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The embedded default catalog '{ResourceName}' is missing.");
        var document = JsonSerializer.Deserialize<DefaultCatalogDocument>(stream, SerializerOptions)
            ?? throw new InvalidOperationException("The embedded default catalog is empty or invalid.");

        Validate(document);
        return document.Options.Select(definition => new CatalogOption
        {
            Id = definition.Id,
            Category = definition.Category,
            NameEnglish = definition.NameEnglish.Trim(),
            NormalizedName = NormalizeName(definition.NameEnglish),
            DescriptionEnglish = definition.DescriptionEnglish,
            ParentOptionId = definition.ParentOptionId,
            SortOrder = definition.SortOrder,
            IsActive = true,
            IsBuiltIn = true,
            PlatformPoolGroup = definition.PlatformPoolGroup,
            CreatedAtUtc = document.PublishedAtUtc,
            UpdatedAtUtc = document.PublishedAtUtc
        }).ToArray();
    }

    private static void Validate(DefaultCatalogDocument document)
    {
        if (document.SchemaVersion != 1)
        {
            throw new InvalidOperationException($"Default catalog schema {document.SchemaVersion} is not supported.");
        }

        if (!string.Equals(document.SourceLanguage, "en", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The default catalog source language must be English.");
        }

        if (string.IsNullOrWhiteSpace(document.CatalogVersion) || document.PublishedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("The default catalog metadata is incomplete.");
        }

        var duplicateId = document.Options.GroupBy(option => option.Id).FirstOrDefault(group => group.Count() > 1);
        if (duplicateId is not null || document.Options.Any(option => option.Id == Guid.Empty))
        {
            throw new InvalidOperationException("Every default catalog option must have a unique non-empty ID.");
        }

        var duplicateName = document.Options
            .GroupBy(option => (option.Category, Name: NormalizeName(option.NameEnglish)))
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateName is not null)
        {
            throw new InvalidOperationException(
                $"The default catalog contains duplicate {duplicateName.Key.Category} name '{duplicateName.Key.Name}'.");
        }

        if (document.Options.Any(option => string.IsNullOrWhiteSpace(option.NameEnglish)
                || option.NameEnglish.Trim().Length > 200))
        {
            throw new InvalidOperationException("Default catalog names must contain between 1 and 200 characters.");
        }

        var categories = document.Options.Select(option => option.Category).ToHashSet();
        if (Enum.GetValues<CatalogCategory>().Any(category => !categories.Contains(category)))
        {
            throw new InvalidOperationException("The default catalog must contain at least one option in every category.");
        }

        var genreIds = document.Options
            .Where(option => option.Category == CatalogCategory.Genre)
            .Select(option => option.Id)
            .ToHashSet();
        if (document.Options.Any(option => option.Category == CatalogCategory.Subgenre
                && (option.ParentOptionId is null || !genreIds.Contains(option.ParentOptionId.Value))))
        {
            throw new InvalidOperationException("Every default subgenre must reference a genre in the same catalog.");
        }

        if (document.Options.Any(option => option.Category != CatalogCategory.Subgenre
                && option.ParentOptionId is not null))
        {
            throw new InvalidOperationException("Only subgenres can have a parent option.");
        }

        if (document.Options.Any(option => option.Category != CatalogCategory.Platform
                && option.PlatformPoolGroup != PlatformPoolGroup.Other))
        {
            throw new InvalidOperationException("Pool routing can only be assigned to platform options.");
        }

        if (document.Options.Count(option => option.Category == CatalogCategory.Platform
                && option.PlatformPoolGroup == PlatformPoolGroup.Pc) != 1
            || document.Options.Count(option => option.Category == CatalogCategory.Platform
                && option.PlatformPoolGroup == PlatformPoolGroup.Mobile) != 1)
        {
            throw new InvalidOperationException("The default catalog must define exactly one PC and one Mobile pool platform.");
        }
    }

    private static string NormalizeName(string value) => value.Trim().ToUpperInvariant();
}
