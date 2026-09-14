namespace GameDesignWizard.Infrastructure.Data;

public static class AppDataPaths
{
    public static string DataDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GameDesignWizard");

    public static string DatabasePath => Path.Combine(DataDirectory, "game-design-wizard.db");

    public static string MediaDirectory => Path.Combine(DataDirectory, "media");

    public static string LocalAiSettingsPath => Path.Combine(DataDirectory, "local-ai-settings.json");

    public static string ResolveDatabasePath()
    {
        var overridePath = Environment.GetEnvironmentVariable("GDW_DATABASE_PATH");
        return string.IsNullOrWhiteSpace(overridePath)
            ? DatabasePath
            : Path.GetFullPath(overridePath);
    }
}
