namespace GameDesignWizard.Core.Ai;

public sealed record GddAssistRequest(
    string SectionTitle,
    string SectionGuidance,
    string ExistingContent,
    string GameName,
    string Overview,
    string? Platform,
    string? Genre,
    string? Subgenre,
    IReadOnlyList<string> Topics,
    IReadOnlyList<string> Mechanics,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> ArtStyles,
    string? DevelopmentDuration,
    string? TeamSize);
