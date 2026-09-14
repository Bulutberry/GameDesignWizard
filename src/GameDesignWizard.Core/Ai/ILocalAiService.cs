namespace GameDesignWizard.Core.Ai;

public interface ILocalAiService
{
    Task<LocalAiGenerationResult> ImproveGddSectionAsync(
        LocalAiConfiguration configuration,
        GddAssistRequest request,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}
