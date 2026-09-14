namespace GameDesignWizard.Core.Ai;

public sealed record LocalAiConfiguration
{
    public string RuntimePath { get; init; } = string.Empty;

    public string ModelPath { get; init; } = string.Empty;

    public int ContextSize { get; init; } = 4096;

    public int MaxOutputTokens { get; init; } = 384;

    public int GpuLayers { get; init; }

    public int StartupTimeoutSeconds { get; init; } = 180;

    public int RequestTimeoutSeconds { get; init; } = 180;
}
