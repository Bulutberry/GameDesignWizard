using GameDesignWizard.Core.Catalog;
using GameDesignWizard.Core.Ideas;

namespace GameDesignWizard.Infrastructure.Data;

public sealed class GameIdeaEntity
{
    public Guid Id { get; set; }

    public string NameEnglish { get; set; } = string.Empty;

    public GameIdeaStage Stage { get; set; }

    public PlatformPoolGroup PoolGroup { get; set; }

    public string DocumentJson { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
