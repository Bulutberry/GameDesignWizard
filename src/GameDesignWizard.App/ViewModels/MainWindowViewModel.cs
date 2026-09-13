using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly ICatalogRepository? _catalogRepository;
    private AppPage _currentPage = AppPage.Home;
    private string _newPlatformName = string.Empty;
    private string _selectedPlatformName = "None";
    private string _settingsMessage;

    public MainWindowViewModel()
        : this(null)
    {
    }

    public MainWindowViewModel(ICatalogRepository? catalogRepository)
    {
        _catalogRepository = catalogRepository;
        Platforms = [];
        ActivePlatforms = [];
        _settingsMessage = catalogRepository is null
            ? "Add a custom platform to preview the dynamic wizard behavior."
            : "Platform changes are saved automatically on this device.";

        if (catalogRepository is null)
        {
            AddPreviewPlatforms();
        }

        NavigateHomeCommand = new RelayCommand(_ => CurrentPage = AppPage.Home);
        NavigateWizardCommand = new RelayCommand(_ => CurrentPage = AppPage.Wizard);
        NavigatePoolCommand = new RelayCommand(_ => CurrentPage = AppPage.Pool);
        NavigateSettingsCommand = new RelayCommand(_ => CurrentPage = AppPage.Settings);
        AddPlatformCommand = new AsyncRelayCommand(_ => AddPlatformAsync());
        TogglePlatformCommand = new AsyncRelayCommand(TogglePlatformAsync);
        SelectPlatformCommand = new RelayCommand(SelectPlatform);
        ContinuePrototypeCommand = new RelayCommand(_ =>
            SettingsMessage = "The remaining wizard steps will be added after this prototype is approved.");
    }

    public ObservableCollection<PlatformOptionViewModel> Platforms { get; }

    public ObservableCollection<PlatformOptionViewModel> ActivePlatforms { get; }

    public ICommand NavigateHomeCommand { get; }

    public ICommand NavigateWizardCommand { get; }

    public ICommand NavigatePoolCommand { get; }

    public ICommand NavigateSettingsCommand { get; }

    public ICommand AddPlatformCommand { get; }

    public ICommand TogglePlatformCommand { get; }

    public ICommand SelectPlatformCommand { get; }

    public ICommand ContinuePrototypeCommand { get; }

    public string NewPlatformName
    {
        get => _newPlatformName;
        set => SetProperty(ref _newPlatformName, value);
    }

    public string SelectedPlatformName
    {
        get => _selectedPlatformName;
        private set => SetProperty(ref _selectedPlatformName, value);
    }

    public string SettingsMessage
    {
        get => _settingsMessage;
        private set => SetProperty(ref _settingsMessage, value);
    }

    public Visibility HomeVisibility => CurrentPage == AppPage.Home ? Visibility.Visible : Visibility.Collapsed;

    public Visibility WizardVisibility => CurrentPage == AppPage.Wizard ? Visibility.Visible : Visibility.Collapsed;

    public Visibility PoolVisibility => CurrentPage == AppPage.Pool ? Visibility.Visible : Visibility.Collapsed;

    public Visibility SettingsVisibility => CurrentPage == AppPage.Settings ? Visibility.Visible : Visibility.Collapsed;

    public bool IsHomeSelected => CurrentPage == AppPage.Home;

    public bool IsWizardSelected => CurrentPage == AppPage.Wizard;

    public bool IsPoolSelected => CurrentPage == AppPage.Pool;

    public bool IsSettingsSelected => CurrentPage == AppPage.Settings;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_catalogRepository is null)
        {
            return;
        }

        await _catalogRepository.InitializeAsync(cancellationToken);
        var platforms = await _catalogRepository.GetPlatformsAsync(cancellationToken);
        Platforms.Clear();
        foreach (var platform in platforms)
        {
            Platforms.Add(ToViewModel(platform));
        }

        RebuildActivePlatforms();
        SettingsMessage = $"{Platforms.Count} platforms loaded. Changes are saved automatically.";
    }

    private AppPage CurrentPage
    {
        get => _currentPage;
        set
        {
            if (!SetProperty(ref _currentPage, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HomeVisibility));
            OnPropertyChanged(nameof(WizardVisibility));
            OnPropertyChanged(nameof(PoolVisibility));
            OnPropertyChanged(nameof(SettingsVisibility));
            OnPropertyChanged(nameof(IsHomeSelected));
            OnPropertyChanged(nameof(IsWizardSelected));
            OnPropertyChanged(nameof(IsPoolSelected));
            OnPropertyChanged(nameof(IsSettingsSelected));
        }
    }

    private async Task AddPlatformAsync()
    {
        try
        {
            CatalogOption? savedPlatform = null;
            if (_catalogRepository is not null)
            {
                savedPlatform = await _catalogRepository.AddPlatformAsync(NewPlatformName);
            }

            var name = savedPlatform?.NameEnglish ?? CleanPreviewName(NewPlatformName);
            if (_catalogRepository is null && Platforms.Any(platform =>
                    string.Equals(platform.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"{name} already exists in the platform catalog.");
            }

            var platform = savedPlatform is null
                ? new PlatformOptionViewModel(Guid.NewGuid(), name, false)
                : ToViewModel(savedPlatform);
            Platforms.Add(platform);
            RebuildActivePlatforms();
            NewPlatformName = string.Empty;
            SettingsMessage = $"{name} was added and saved. New ideas using it will route to Other.";
        }
        catch (InvalidOperationException exception)
        {
            SettingsMessage = exception.Message;
        }
        catch (Exception)
        {
            SettingsMessage = "The platform could not be saved. Try again.";
        }
    }

    private async Task TogglePlatformAsync(object? parameter)
    {
        if (parameter is not PlatformOptionViewModel platform)
        {
            return;
        }

        var newState = !platform.IsActive;
        try
        {
            if (_catalogRepository is not null)
            {
                await _catalogRepository.SetPlatformActiveAsync(platform.Id, newState);
            }

            platform.IsActive = newState;
            RebuildActivePlatforms();
            if (!platform.IsActive && SelectedPlatformName == platform.Name)
            {
                SelectedPlatformName = "None";
            }

            SettingsMessage = platform.IsActive
                ? $"{platform.Name} was restored and saved."
                : $"{platform.Name} was archived and saved.";
        }
        catch (InvalidOperationException exception)
        {
            SettingsMessage = exception.Message;
        }
        catch (Exception)
        {
            SettingsMessage = "The platform change could not be saved. Try again.";
        }
    }

    private void SelectPlatform(object? parameter)
    {
        if (parameter is PlatformOptionViewModel platform)
        {
            SelectedPlatformName = platform.Name;
        }
    }

    private void AddPreviewPlatforms()
    {
        Platforms.Add(new PlatformOptionViewModel(Guid.Parse("0d3034e8-e927-4a2f-a96f-ae610a47fb2f"), "PC", true));
        Platforms.Add(new PlatformOptionViewModel(Guid.Parse("db7d530a-5cb9-411c-bb8e-7006282f63d7"), "Mobile", true));
        Platforms.Add(new PlatformOptionViewModel(Guid.Parse("1826ccb8-23f3-44a8-af8c-538d25e7e036"), "Console", true));
        Platforms.Add(new PlatformOptionViewModel(Guid.Parse("ec646f17-5476-4d76-992e-223d4b4d392b"), "VR", true));
        Platforms.Add(new PlatformOptionViewModel(Guid.Parse("e0cb0941-ddc3-4b03-956f-7824da01c224"), "Board Game", true));
        RebuildActivePlatforms();
    }

    private void RebuildActivePlatforms()
    {
        ActivePlatforms.Clear();
        foreach (var platform in Platforms.Where(platform => platform.IsActive))
        {
            ActivePlatforms.Add(platform);
        }
    }

    private static PlatformOptionViewModel ToViewModel(CatalogOption option) =>
        new(option.Id, option.NameEnglish, option.IsBuiltIn, option.IsActive);

    private static string CleanPreviewName(string value)
    {
        var cleaned = string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (cleaned.Length == 0)
        {
            throw new InvalidOperationException("Enter a platform name before adding it.");
        }

        return cleaned;
    }

    private enum AppPage
    {
        Home,
        Wizard,
        Pool,
        Settings
    }
}
