using System.Text.Json;
using GameDesignWizard.Core.Ai;

namespace GameDesignWizard.Ai;

public sealed class JsonLocalAiSettingsStore(string settingsPath) : ILocalAiSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<LocalAiConfiguration> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(settingsPath))
        {
            return new LocalAiConfiguration();
        }

        try
        {
            await using var stream = new FileStream(
                settingsPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            return await JsonSerializer.DeserializeAsync<LocalAiConfiguration>(
                       stream,
                       SerializerOptions,
                       cancellationToken)
                   ?? new LocalAiConfiguration();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "The local AI settings file is invalid. Open Local AI Setup and save it again.",
                exception);
        }
    }

    public async Task SaveAsync(
        LocalAiConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(settingsPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException("The local AI settings path is invalid.");
        }

        Directory.CreateDirectory(directory);
        var temporaryPath = settingsPath + ".tmp";
        await using (var stream = new FileStream(
                         temporaryPath,
                         FileMode.Create,
                         FileAccess.Write,
                         FileShare.None,
                         4096,
                         FileOptions.Asynchronous))
        {
            await JsonSerializer.SerializeAsync(stream, configuration, SerializerOptions, cancellationToken);
        }

        File.Move(temporaryPath, settingsPath, true);
    }
}
