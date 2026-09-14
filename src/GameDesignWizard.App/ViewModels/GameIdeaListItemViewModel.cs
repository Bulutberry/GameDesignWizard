using GameDesignWizard.Core.Catalog;
using GameDesignWizard.Core.Ideas;

namespace GameDesignWizard.App.ViewModels;

public sealed class GameIdeaListItemViewModel(GameIdeaDocument idea)
{
    public Guid Id { get; } = idea.Id;

    public string Name { get; } = idea.NameEnglish;

    public PlatformPoolGroup PoolGroup { get; } = idea.PoolGroup;

    public string PoolName { get; } = idea.PoolGroup switch
    {
        PlatformPoolGroup.Pc => "PC",
        PlatformPoolGroup.Mobile => "Mobile",
        _ => "Other"
    };

    public string Platform { get; } = idea.Platform?.NameEnglish ?? "Unspecified";

    public string Genre { get; } = idea.Genre?.NameEnglish ?? "Unspecified";

    public string Subgenre { get; } = idea.Subgenre?.NameEnglish ?? "Unspecified";

    public string Duration { get; } = idea.DevelopmentDuration?.NameEnglish ?? "Unspecified";

    public string TeamSize { get; } = idea.TeamSize?.NameEnglish ?? "Unspecified";

    public string Stage { get; } = idea.Stage.ToString();

    public string Updated { get; } = idea.UpdatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    public string SearchIndex { get; } = string.Join(' ', new[]
    {
        idea.NameEnglish,
        idea.Platform?.NameEnglish,
        idea.Genre?.NameEnglish,
        idea.Subgenre?.NameEnglish,
        string.Join(' ', idea.Topics.Select(option => option.NameEnglish)),
        string.Join(' ', idea.Mechanics.Select(option => option.NameEnglish)),
        string.Join(' ', idea.Features.Select(option => option.NameEnglish)),
        string.Join(' ', idea.ArtStyles.Select(option => option.NameEnglish)),
        idea.DevelopmentDuration?.NameEnglish,
        idea.TeamSize?.NameEnglish,
        idea.Stage.ToString()
    }.Where(value => !string.IsNullOrWhiteSpace(value)));
}
