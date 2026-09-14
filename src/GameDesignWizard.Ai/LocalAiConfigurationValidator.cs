using GameDesignWizard.Core.Ai;

namespace GameDesignWizard.Ai;

public static class LocalAiConfigurationValidator
{
    public static IReadOnlyList<string> Validate(LocalAiConfiguration configuration)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(configuration.RuntimePath))
        {
            errors.Add("Choose the llama.cpp server executable.");
        }
        else if (!File.Exists(configuration.RuntimePath))
        {
            errors.Add("The llama.cpp server executable does not exist.");
        }
        else if (!string.Equals(Path.GetExtension(configuration.RuntimePath), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("The llama.cpp server must be a Windows .exe file.");
        }

        if (string.IsNullOrWhiteSpace(configuration.ModelPath))
        {
            errors.Add("Choose a GGUF model file.");
        }
        else if (!File.Exists(configuration.ModelPath))
        {
            errors.Add("The GGUF model file does not exist.");
        }
        else if (!string.Equals(Path.GetExtension(configuration.ModelPath), ".gguf", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("The local model must be a .gguf file.");
        }

        if (configuration.ContextSize is < 2048 or > 32768)
        {
            errors.Add("Context size must be between 2,048 and 32,768 tokens.");
        }

        if (configuration.MaxOutputTokens is < 64 or > 2048)
        {
            errors.Add("Maximum output must be between 64 and 2,048 tokens.");
        }

        if (configuration.MaxOutputTokens >= configuration.ContextSize)
        {
            errors.Add("Maximum output must be smaller than the context size.");
        }

        if (configuration.GpuLayers is < 0 or > 999)
        {
            errors.Add("GPU layers must be between 0 and 999.");
        }

        if (configuration.StartupTimeoutSeconds is < 30 or > 600)
        {
            errors.Add("Startup timeout must be between 30 and 600 seconds.");
        }

        if (configuration.RequestTimeoutSeconds is < 30 or > 600)
        {
            errors.Add("Request timeout must be between 30 and 600 seconds.");
        }

        return errors;
    }

    public static void ThrowIfInvalid(LocalAiConfiguration configuration)
    {
        var errors = Validate(configuration);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }
    }
}
