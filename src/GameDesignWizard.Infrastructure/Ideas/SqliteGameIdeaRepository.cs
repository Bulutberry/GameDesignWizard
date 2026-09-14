using System.Text.Json;
using System.Text.Json.Serialization;
using GameDesignWizard.Core.Ideas;
using GameDesignWizard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameDesignWizard.Infrastructure.Ideas;

public sealed class SqliteGameIdeaRepository(AppDbContextFactory contextFactory) : IGameIdeaRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task SaveAsync(GameIdeaDocument idea, CancellationToken cancellationToken = default)
    {
        Validate(idea);
        await using var context = contextFactory.CreateDbContext();
        await context.Database.MigrateAsync(cancellationToken);
        var entity = await context.GameIdeas.SingleOrDefaultAsync(
            candidate => candidate.Id == idea.Id,
            cancellationToken);
        var documentJson = JsonSerializer.Serialize(idea, SerializerOptions);

        if (entity is null)
        {
            entity = new GameIdeaEntity
            {
                Id = idea.Id,
                CreatedAtUtc = idea.CreatedAtUtc
            };
            context.GameIdeas.Add(entity);
        }

        entity.NameEnglish = idea.NameEnglish;
        entity.Stage = idea.Stage;
        entity.PoolGroup = idea.PoolGroup;
        entity.DocumentJson = documentJson;
        entity.UpdatedAtUtc = idea.UpdatedAtUtc;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GameIdeaDocument>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = contextFactory.CreateDbContext();
        await context.Database.MigrateAsync(cancellationToken);
        var documents = await context.GameIdeas
            .AsNoTracking()
            .OrderByDescending(idea => idea.UpdatedAtUtc)
            .Select(idea => idea.DocumentJson)
            .ToListAsync(cancellationToken);

        return documents.Select(document =>
                JsonSerializer.Deserialize<GameIdeaDocument>(document, SerializerOptions)
                ?? throw new InvalidOperationException("A saved idea document is invalid."))
            .ToArray();
    }

    private static void Validate(GameIdeaDocument idea)
    {
        if (idea.Id == Guid.Empty)
        {
            throw new InvalidOperationException("The idea must have an ID before it can be saved.");
        }

        if (string.IsNullOrWhiteSpace(idea.NameEnglish) || idea.NameEnglish.Length > 200)
        {
            throw new InvalidOperationException("The game name must contain between 1 and 200 characters.");
        }

        if (idea.CreatedAtUtc.Kind != DateTimeKind.Utc || idea.UpdatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Idea timestamps must use UTC.");
        }
    }
}
