namespace GameDesignWizard.Core.Ideas;

public interface IGameIdeaPdfExporter
{
    Task ExportAsync(
        GameIdeaDocument idea,
        string destinationPath,
        CancellationToken cancellationToken = default);
}
