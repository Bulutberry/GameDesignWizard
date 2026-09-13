using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.App.ViewModels;

public sealed record CatalogCategoryItemViewModel(
    CatalogCategory Category,
    string DisplayName,
    string SingularName);
