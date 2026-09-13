using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.Infrastructure.Catalog;

internal sealed class DefaultCatalogDocument
{
    public int SchemaVersion { get; init; }

    public string CatalogVersion { get; init; } = string.Empty;

    public string SourceLanguage { get; init; } = string.Empty;

    public DateTime PublishedAtUtc { get; init; }

    public List<DefaultCatalogOptionDefinition> Options { get; init; } = [];
}

internal sealed class DefaultCatalogOptionDefinition
{
    public Guid Id { get; init; }

    public CatalogCategory Category { get; init; }

    public string NameEnglish { get; init; } = string.Empty;

    public string? DescriptionEnglish { get; init; }

    public Guid? ParentOptionId { get; init; }

    public int SortOrder { get; init; }

    public PlatformPoolGroup PlatformPoolGroup { get; init; } = PlatformPoolGroup.Other;
}
