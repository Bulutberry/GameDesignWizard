namespace GameDesignWizard.Core.Ideas;

public interface IGameIdeaMarkdownExporter
{
    Task ExportAsync(
        GameIdeaDocument idea,
        string destinationPath,
        CancellationToken cancellationToken = default);
}
