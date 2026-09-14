namespace GameDesignWizard.Core.Ai;

public sealed record LocalAiGenerationResult(
    string Text,
    TimeSpan Elapsed,
    int? PromptTokens,
    int? CompletionTokens);
