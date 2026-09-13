namespace GameDesignWizard.Core.Catalog;

public sealed record CatalogFileRow(
    int SourceRow,
    string NameEnglish,
    string? Error = null);
