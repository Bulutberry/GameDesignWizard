namespace GameDesignWizard.Core.Catalog;

public interface ICatalogRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogOption>> GetPlatformsAsync(CancellationToken cancellationToken = default);

    Task<CatalogOption> AddPlatformAsync(string nameEnglish, CancellationToken cancellationToken = default);

    Task<CatalogOption> SetPlatformActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default);
}
