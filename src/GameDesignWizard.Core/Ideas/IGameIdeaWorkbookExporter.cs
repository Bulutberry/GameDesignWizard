namespace GameDesignWizard.Core.Ideas;

public interface IGameIdeaWorkbookExporter
{
    Task ExportAsync(
        IReadOnlyList<GameIdeaDocument> ideas,
        string destinationPath,
        CancellationToken cancellationToken = default);
}
