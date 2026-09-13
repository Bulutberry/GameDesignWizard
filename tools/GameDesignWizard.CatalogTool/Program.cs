using GameDesignWizard.Infrastructure.Catalog;
using GameDesignWizard.Infrastructure.Data;

const string usage = """
GameDesignWizard catalog maintenance tool

Usage:
  dotnet run --project tools/GameDesignWizard.CatalogTool -- export-defaults --version <version> [--database <path>] [--output <path>]

Defaults:
  --database  %LOCALAPPDATA%\GameDesignWizard\game-design-wizard.db
  --output    catalog-source\en\default-catalog.json
""";

if (args.Length == 0 || args[0] is "--help" or "-h")
{
    Console.WriteLine(usage);
    return 0;
}

if (!string.Equals(args[0], "export-defaults", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine($"Unknown command '{args[0]}'.");
    Console.Error.WriteLine(usage);
    return 1;
}

try
{
    var arguments = ParseArguments(args.Skip(1).ToArray());
    if (!arguments.TryGetValue("version", out var catalogVersion)
        || string.IsNullOrWhiteSpace(catalogVersion))
    {
        throw new ArgumentException("The --version option is required.");
    }

    var databasePath = arguments.GetValueOrDefault("database", AppDataPaths.DatabasePath);
    var outputPath = arguments.GetValueOrDefault(
        "output",
        Path.Combine(Environment.CurrentDirectory, "catalog-source", "en", "default-catalog.json"));

    var repository = new SqliteCatalogRepository(new AppDbContextFactory(databasePath));
    await repository.InitializeAsync();
    var exportedCount = await new DefaultCatalogExporter().ExportAsync(
        repository,
        outputPath,
        catalogVersion);

    Console.WriteLine($"Exported {exportedCount} active options to {Path.GetFullPath(outputPath)}");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Catalog export failed: {exception.Message}");
    return 1;
}

static Dictionary<string, string> ParseArguments(IReadOnlyList<string> values)
{
    var allowedKeys = new HashSet<string>(["version", "database", "output"], StringComparer.OrdinalIgnoreCase);
    var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 0; index < values.Count; index += 2)
    {
        var key = values[index];
        if (!key.StartsWith("--", StringComparison.Ordinal) || index + 1 >= values.Count)
        {
            throw new ArgumentException($"Expected a value after '{key}'.");
        }

        var normalizedKey = key[2..];
        if (!allowedKeys.Contains(normalizedKey))
        {
            throw new ArgumentException($"Unknown option '{key}'.");
        }

        if (!parsed.TryAdd(normalizedKey, values[index + 1]))
        {
            throw new ArgumentException($"Option '{key}' was provided more than once.");
        }
    }

    return parsed;
}
