using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.Infrastructure.Catalog;

public interface IDefaultCatalogProvider
{
    IReadOnlyList<CatalogOption> Load();
}
