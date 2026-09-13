namespace GameDesignWizard.Core.Catalog;

public sealed class CatalogOption
{
    public Guid Id { get; set; }

    public CatalogCategory Category { get; set; }

    public string NameEnglish { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string? DescriptionEnglish { get; set; }

    public Guid? ParentOptionId { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsBuiltIn { get; set; }

    public PlatformPoolGroup PlatformPoolGroup { get; set; } = PlatformPoolGroup.Other;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
