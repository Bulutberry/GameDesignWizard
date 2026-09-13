namespace GameDesignWizard.Core.Catalog;

public interface ICatalogFileReader
{
    Task<IReadOnlyList<CatalogFileRow>> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}
