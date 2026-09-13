using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace GameDesignWizard.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private AppPage _currentPage = AppPage.Home;
    private string _newPlatformName = string.Empty;
    private string _selectedPlatformName = "None";
    private string _settingsMessage = "Add a custom platform to preview the dynamic wizard behavior.";

    public MainWindowViewModel()
    {
        Platforms =
        [
            new PlatformOptionViewModel("PC", true),
            new PlatformOptionViewModel("Mobile", true),
            new PlatformOptionViewModel("Console", true),
            new PlatformOptionViewModel("VR", true),
            new PlatformOptionViewModel("Board Game", true)
        ];

        ActivePlatforms = new ObservableCollection<PlatformOptionViewModel>(Platforms.Where(platform => platform.IsActive));

        NavigateHomeCommand = new RelayCommand(_ => CurrentPage = AppPage.Home);
        NavigateWizardCommand = new RelayCommand(_ => CurrentPage = AppPage.Wizard);
        NavigatePoolCommand = new RelayCommand(_ => CurrentPage = AppPage.Pool);
        NavigateSettingsCommand = new RelayCommand(_ => CurrentPage = AppPage.Settings);
        AddPlatformCommand = new RelayCommand(_ => AddPlatform());
        TogglePlatformCommand = new RelayCommand(TogglePlatform);
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

    private void AddPlatform()
    {
        var name = NewPlatformName.Trim();
        if (name.Length == 0)
        {
            SettingsMessage = "Enter a platform name before adding it.";
            return;
        }

        if (Platforms.Any(platform => string.Equals(platform.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            SettingsMessage = $"{name} already exists in the platform catalog.";
            return;
        }

        var platform = new PlatformOptionViewModel(name, false);
        Platforms.Add(platform);
        ActivePlatforms.Add(platform);
        NewPlatformName = string.Empty;
        SettingsMessage = $"{name} was added. It now appears in the wizard and will route saved ideas to Other.";
    }

    private void TogglePlatform(object? parameter)
    {
        if (parameter is not PlatformOptionViewModel platform)
        {
            return;
        }

        platform.IsActive = !platform.IsActive;
        if (platform.IsActive)
        {
            if (!ActivePlatforms.Contains(platform))
            {
                ActivePlatforms.Add(platform);
            }
        }
        else
        {
            ActivePlatforms.Remove(platform);
            if (SelectedPlatformName == platform.Name)
            {
                SelectedPlatformName = "None";
            }
        }

        SettingsMessage = platform.IsActive
            ? $"{platform.Name} was restored to new idea selections."
            : $"{platform.Name} was archived from new idea selections.";
    }

    private void SelectPlatform(object? parameter)
    {
        if (parameter is PlatformOptionViewModel platform)
        {
            SelectedPlatformName = platform.Name;
        }
    }

    private enum AppPage
    {
        Home,
        Wizard,
        Pool,
        Settings
    }
}
