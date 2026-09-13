using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameDesignWizard.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private readonly string? _databasePath;

    public AppDbContextFactory()
    {
    }

    public AppDbContextFactory(string databasePath)
    {
        _databasePath = databasePath;
    }

    public AppDbContext CreateDbContext(string[] args)
    {
        var databasePath = _databasePath ?? AppDataPaths.ResolveDatabasePath();
        var dataDirectory = Path.GetDirectoryName(databasePath)
            ?? throw new InvalidOperationException("The database path must include a directory.");
        Directory.CreateDirectory(dataDirectory);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        return new AppDbContext(options);
    }

    public AppDbContext CreateDbContext() => CreateDbContext([]);
}
