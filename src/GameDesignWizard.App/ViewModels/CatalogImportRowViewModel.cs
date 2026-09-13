namespace GameDesignWizard.App.ViewModels;

public sealed record CatalogImportRowViewModel(
    int SourceRow,
    string Name,
    string Status,
    bool IsReady);
