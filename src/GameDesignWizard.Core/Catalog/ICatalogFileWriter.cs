namespace GameDesignWizard.Core.Catalog;

public interface ICatalogFileWriter
{
    Task WriteAsync(
        string filePath,
        CatalogCategory category,
        IReadOnlyList<CatalogOption> options,
        CancellationToken cancellationToken = default);
}
