using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.App.ViewModels;

public sealed class CatalogImportPreviewViewModel(
    string sourceFileName,
    CatalogCategory category,
    string categoryName,
    Guid? parentOptionId,
    IReadOnlyList<CatalogImportRowViewModel> rows)
{
    public string SourceFileName { get; } = sourceFileName;

    public CatalogCategory Category { get; } = category;

    public string CategoryName { get; } = categoryName;

    public Guid? ParentOptionId { get; } = parentOptionId;

    public IReadOnlyList<CatalogImportRowViewModel> Rows { get; } = rows;

    public int ReadyCount => Rows.Count(row => row.IsReady);

    public int IssueCount => Rows.Count - ReadyCount;

    public bool CanImport => ReadyCount > 0;

    public string Summary => $"{ReadyCount} ready to import · {IssueCount} skipped or invalid";

    public string ImportButtonLabel => $"Import {ReadyCount}";
}
