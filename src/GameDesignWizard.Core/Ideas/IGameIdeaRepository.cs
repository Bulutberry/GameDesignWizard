namespace GameDesignWizard.Core.Ideas;

public interface IGameIdeaRepository
{
    Task SaveAsync(GameIdeaDocument idea, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameIdeaDocument>> GetAllAsync(CancellationToken cancellationToken = default);
}
