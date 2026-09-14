using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.Core.Ideas;

public sealed class GameIdeaDocument
{
    public Guid Id { get; init; }

    public string NameEnglish { get; init; } = "NewGame";

    public GameIdeaStage Stage { get; init; } = GameIdeaStage.Idea;

    public PlatformPoolGroup PoolGroup { get; init; } = PlatformPoolGroup.Other;

    public CatalogSelectionSnapshot? Platform { get; init; }

    public CatalogSelectionSnapshot? Genre { get; init; }

    public CatalogSelectionSnapshot? Subgenre { get; init; }

    public List<CatalogSelectionSnapshot> Topics { get; init; } = [];

    public List<CatalogSelectionSnapshot> Features { get; init; } = [];

    public List<CatalogSelectionSnapshot> ArtStyles { get; init; } = [];

    public List<CatalogSelectionSnapshot> Mechanics { get; init; } = [];

    public CatalogSelectionSnapshot? DevelopmentDuration { get; init; }

    public CatalogSelectionSnapshot? TeamSize { get; init; }

    public string OverviewEnglish { get; init; } = string.Empty;

    public List<GddSectionContent> Sections { get; init; } = [];

    public List<GameReferenceContent> References { get; init; } = [];

    public List<MediaAttachmentContent> MediaAttachments { get; init; } = [];

    public DateTime CreatedAtUtc { get; init; }

    public DateTime UpdatedAtUtc { get; init; }
}

public sealed record CatalogSelectionSnapshot(Guid OptionId, string NameEnglish);

public sealed record GddSectionContent(Guid Id, string TitleEnglish, string ContentEnglish, int SortOrder);

public sealed record GameReferenceContent(Guid Id, string? Url, string NoteEnglish, int SortOrder);

public sealed record MediaAttachmentContent(
    Guid Id,
    string FileName,
    MediaAttachmentType MediaType,
    string StoredRelativePath,
    string CaptionEnglish,
    int SortOrder);
