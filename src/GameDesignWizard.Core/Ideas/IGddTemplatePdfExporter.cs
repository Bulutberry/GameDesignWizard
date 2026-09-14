namespace GameDesignWizard.Core.Ideas;

public interface IGddTemplatePdfExporter
{
    Task ExportTemplateAsync(
        string destinationPath,
        CancellationToken cancellationToken = default);
}
