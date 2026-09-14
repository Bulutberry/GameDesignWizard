namespace GameDesignWizard.Core.Ai;

public interface ILocalAiSettingsStore
{
    Task<LocalAiConfiguration> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(LocalAiConfiguration configuration, CancellationToken cancellationToken = default);
}
