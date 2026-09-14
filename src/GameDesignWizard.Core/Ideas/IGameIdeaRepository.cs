namespace GameDesignWizard.Core.Ideas;

public interface IGameIdeaRepository
{
    Task SaveAsync(GameIdeaDocument idea, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameIdeaDocument>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<GameIdeaDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
