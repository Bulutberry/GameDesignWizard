using GameDesignWizard.Ai;
using GameDesignWizard.Core.Ai;

namespace GameDesignWizard.App.ViewModels;

public sealed class LocalAiSettingsViewModel(ILocalAiSettingsStore settingsStore) : ObservableObject
{
    private string _runtimePath = string.Empty;
    private string _modelPath = string.Empty;
    private string _contextSize = "4096";
    private string _maxOutputTokens = "384";
    private string _gpuLayers = "0";
    private string _startupTimeoutSeconds = "180";
    private string _requestTimeoutSeconds = "180";
    private string _message = "Choose a llama.cpp server and a GGUF model. Nothing is downloaded automatically.";

    public string RuntimePath
    {
        get => _runtimePath;
        set => SetProperty(ref _runtimePath, value);
    }

    public string ModelPath
    {
        get => _modelPath;
        set => SetProperty(ref _modelPath, value);
    }

    public string ContextSize
    {
        get => _contextSize;
        set => SetProperty(ref _contextSize, value);
    }

    public string MaxOutputTokens
    {
        get => _maxOutputTokens;
        set => SetProperty(ref _maxOutputTokens, value);
    }

    public string GpuLayers
    {
        get => _gpuLayers;
        set => SetProperty(ref _gpuLayers, value);
    }

    public string StartupTimeoutSeconds
    {
        get => _startupTimeoutSeconds;
        set => SetProperty(ref _startupTimeoutSeconds, value);
    }

    public string RequestTimeoutSeconds
    {
        get => _requestTimeoutSeconds;
        set => SetProperty(ref _requestTimeoutSeconds, value);
    }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await settingsStore.LoadAsync(cancellationToken);
        RuntimePath = configuration.RuntimePath;
        ModelPath = configuration.ModelPath;
        ContextSize = configuration.ContextSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
        MaxOutputTokens = configuration.MaxOutputTokens.ToString(System.Globalization.CultureInfo.InvariantCulture);
        GpuLayers = configuration.GpuLayers.ToString(System.Globalization.CultureInfo.InvariantCulture);
        StartupTimeoutSeconds = configuration.StartupTimeoutSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
        RequestTimeoutSeconds = configuration.RequestTimeoutSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var configuration = CreateConfiguration();
        var errors = LocalAiConfigurationValidator.Validate(configuration);
        if (errors.Count > 0)
        {
            Message = string.Join(" ", errors);
            return;
        }

        await settingsStore.SaveAsync(configuration, cancellationToken);
        Message = "Local AI settings were validated and saved on this device.";
    }

    private LocalAiConfiguration CreateConfiguration()
    {
        if (!int.TryParse(ContextSize, out var contextSize)
            || !int.TryParse(MaxOutputTokens, out var maxOutputTokens)
            || !int.TryParse(GpuLayers, out var gpuLayers)
            || !int.TryParse(StartupTimeoutSeconds, out var startupTimeoutSeconds)
            || !int.TryParse(RequestTimeoutSeconds, out var requestTimeoutSeconds))
        {
            throw new InvalidOperationException("Token, GPU layer, and timeout values must be whole numbers.");
        }

        return new LocalAiConfiguration
        {
            RuntimePath = RuntimePath.Trim(),
            ModelPath = ModelPath.Trim(),
            ContextSize = contextSize,
            MaxOutputTokens = maxOutputTokens,
            GpuLayers = gpuLayers,
            StartupTimeoutSeconds = startupTimeoutSeconds,
            RequestTimeoutSeconds = requestTimeoutSeconds
        };
    }
}
