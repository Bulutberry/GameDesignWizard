namespace GameDesignWizard.Core.Catalog;

public interface ICatalogRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogOption>> GetOptionsAsync(
        CatalogCategory category,
        CancellationToken cancellationToken = default);

    Task<CatalogOption> AddOptionAsync(
        CatalogCategory category,
        string nameEnglish,
        Guid? parentOptionId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogOption>> AddOptionsAsync(
        CatalogCategory category,
        IReadOnlyCollection<string> namesEnglish,
        Guid? parentOptionId = null,
        CancellationToken cancellationToken = default);

    Task<CatalogOption> SetOptionActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<CatalogOption> RenameOptionAsync(
        Guid id,
        string nameEnglish,
        CancellationToken cancellationToken = default);

    Task<CatalogOption> MoveOptionAsync(
        Guid id,
        int offset,
        CancellationToken cancellationToken = default);
}
