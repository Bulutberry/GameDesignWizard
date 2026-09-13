using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly ICatalogRepository? _catalogRepository;
    private AppPage _currentPage = AppPage.Home;
    private CatalogCategoryItemViewModel _selectedCatalogCategory;
    private CatalogOptionViewModel? _selectedParentGenre;
    private string _newCatalogOptionName = string.Empty;
    private string _selectedPlatformName = "None";
    private string _settingsMessage;
    private bool _isInitialized;

    public MainWindowViewModel()
        : this(null)
    {
    }

    public MainWindowViewModel(ICatalogRepository? catalogRepository)
    {
        _catalogRepository = catalogRepository;
        CatalogCategories =
        [
            new(CatalogCategory.Platform, "Platforms", "Platform"),
            new(CatalogCategory.Genre, "Genres", "Genre"),
            new(CatalogCategory.Subgenre, "Subgenres", "Subgenre"),
            new(CatalogCategory.Topic, "Topics", "Topic"),
            new(CatalogCategory.Mechanic, "Mechanics", "Mechanic"),
            new(CatalogCategory.Feature, "Features", "Feature"),
            new(CatalogCategory.ArtStyle, "Art Styles", "Art Style"),
            new(CatalogCategory.DevelopmentDuration, "Development Durations", "Development Duration"),
            new(CatalogCategory.TeamSize, "Team Sizes", "Team Size")
        ];
        _selectedCatalogCategory = CatalogCategories[0];
        Platforms = [];
        ActivePlatforms = [];
        CatalogOptions = [];
        GenreChoices = [];
        _settingsMessage = catalogRepository is null
            ? "Catalog changes appear immediately in the wizard."
            : "Catalog changes are saved automatically on this device.";

        if (catalogRepository is null)
        {
            AddPreviewPlatforms();
        }

        NavigateHomeCommand = new RelayCommand(_ => CurrentPage = AppPage.Home);
        NavigateWizardCommand = new RelayCommand(_ => CurrentPage = AppPage.Wizard);
        NavigatePoolCommand = new RelayCommand(_ => CurrentPage = AppPage.Pool);
        NavigateSettingsCommand = new RelayCommand(_ => CurrentPage = AppPage.Settings);
        AddCatalogOptionCommand = new AsyncRelayCommand(_ => AddCatalogOptionAsync());
        ToggleCatalogOptionCommand = new AsyncRelayCommand(ToggleCatalogOptionAsync);
        SelectPlatformCommand = new RelayCommand(SelectPlatform);
        ContinuePrototypeCommand = new RelayCommand(_ =>
            SettingsMessage = "The remaining wizard steps will be added after this catalog prototype is approved.");
    }

    public ObservableCollection<CatalogCategoryItemViewModel> CatalogCategories { get; }

    public ObservableCollection<CatalogOptionViewModel> Platforms { get; }

    public ObservableCollection<CatalogOptionViewModel> ActivePlatforms { get; }

    public ObservableCollection<CatalogOptionViewModel> CatalogOptions { get; }

    public ObservableCollection<CatalogOptionViewModel> GenreChoices { get; }

    public ICommand NavigateHomeCommand { get; }

    public ICommand NavigateWizardCommand { get; }

    public ICommand NavigatePoolCommand { get; }

    public ICommand NavigateSettingsCommand { get; }

    public ICommand AddCatalogOptionCommand { get; }

    public ICommand ToggleCatalogOptionCommand { get; }

    public ICommand SelectPlatformCommand { get; }

    public ICommand ContinuePrototypeCommand { get; }

    public CatalogCategoryItemViewModel SelectedCatalogCategory
    {
        get => _selectedCatalogCategory;
        set
        {
            if (!SetProperty(ref _selectedCatalogCategory, value))
            {
                return;
            }

            NewCatalogOptionName = string.Empty;
            OnPropertyChanged(nameof(CatalogHeading));
            OnPropertyChanged(nameof(AddCatalogButtonLabel));
            OnPropertyChanged(nameof(CatalogInputHint));
            OnPropertyChanged(nameof(SubgenreParentVisibility));
            if (_isInitialized)
            {
                _ = LoadSelectedCatalogAsync();
            }
        }
    }

    public CatalogOptionViewModel? SelectedParentGenre
    {
        get => _selectedParentGenre;
        set => SetProperty(ref _selectedParentGenre, value);
    }

    public string NewCatalogOptionName
    {
        get => _newCatalogOptionName;
        set => SetProperty(ref _newCatalogOptionName, value);
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

    public string CatalogHeading => $"Manage {SelectedCatalogCategory.DisplayName.ToLowerInvariant()}";

    public string AddCatalogButtonLabel => $"Add {SelectedCatalogCategory.SingularName}";

    public string CatalogInputHint => $"Enter a {SelectedCatalogCategory.SingularName.ToLowerInvariant()} name";

    public Visibility SubgenreParentVisibility =>
        SelectedCatalogCategory.Category == CatalogCategory.Subgenre ? Visibility.Visible : Visibility.Collapsed;

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
            _isInitialized = true;
            return;
        }

        await _catalogRepository.InitializeAsync(cancellationToken);
        await LoadPlatformsAsync(cancellationToken);
        _isInitialized = true;
        await LoadSelectedCatalogAsync(cancellationToken);
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

    private async Task LoadSelectedCatalogAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_catalogRepository is null)
            {
                return;
            }

            var category = SelectedCatalogCategory.Category;
            IReadOnlyList<CatalogOption> genres = [];
            if (category == CatalogCategory.Subgenre)
            {
                genres = await _catalogRepository.GetOptionsAsync(CatalogCategory.Genre, cancellationToken);
                GenreChoices.Clear();
                foreach (var genre in genres.Where(option => option.IsActive))
                {
                    GenreChoices.Add(ToViewModel(genre));
                }

                SelectedParentGenre = GenreChoices.FirstOrDefault();
            }

            var options = await _catalogRepository.GetOptionsAsync(category, cancellationToken);
            var genreNames = genres.ToDictionary(option => option.Id, option => option.NameEnglish);
            CatalogOptions.Clear();
            foreach (var option in options)
            {
                CatalogOptions.Add(ToViewModel(
                    option,
                    option.ParentOptionId is Guid parentId && genreNames.TryGetValue(parentId, out var parentName)
                        ? parentName
                        : null));
            }

            SettingsMessage = $"{CatalogOptions.Count} {SelectedCatalogCategory.DisplayName.ToLowerInvariant()} loaded. Changes are saved automatically.";
        }
        catch (Exception)
        {
            SettingsMessage = "The selected catalog could not be loaded. Try again.";
        }
    }

    private async Task LoadPlatformsAsync(CancellationToken cancellationToken = default)
    {
        if (_catalogRepository is null)
        {
            return;
        }

        var platforms = await _catalogRepository.GetOptionsAsync(CatalogCategory.Platform, cancellationToken);
        Platforms.Clear();
        foreach (var platform in platforms)
        {
            Platforms.Add(ToViewModel(platform));
        }

        RebuildActivePlatforms();
    }

    private async Task AddCatalogOptionAsync()
    {
        try
        {
            var category = SelectedCatalogCategory.Category;
            var parentId = category == CatalogCategory.Subgenre ? SelectedParentGenre?.Id : null;
            CatalogOption? savedOption = null;
            if (_catalogRepository is not null)
            {
                savedOption = await _catalogRepository.AddOptionAsync(category, NewCatalogOptionName, parentId);
            }

            var name = savedOption?.NameEnglish ?? CleanPreviewName(NewCatalogOptionName);
            if (_catalogRepository is null && CatalogOptions.Any(option =>
                    string.Equals(option.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"{name} already exists in this catalog.");
            }

            var option = savedOption is null
                ? new CatalogOptionViewModel(Guid.NewGuid(), category, name, false, parentOptionId: parentId,
                    parentName: SelectedParentGenre?.Name)
                : ToViewModel(savedOption, SelectedParentGenre?.Name);
            CatalogOptions.Add(option);
            if (category == CatalogCategory.Platform)
            {
                Platforms.Add(ToViewModel(savedOption ?? new CatalogOption
                {
                    Id = option.Id,
                    Category = category,
                    NameEnglish = option.Name,
                    IsActive = true,
                    IsBuiltIn = false
                }));
                RebuildActivePlatforms();
            }

            NewCatalogOptionName = string.Empty;
            var routingNote = category == CatalogCategory.Platform ? " It will route new ideas to Other." : string.Empty;
            SettingsMessage = $"{name} was added to {SelectedCatalogCategory.DisplayName} and saved.{routingNote}";
        }
        catch (InvalidOperationException exception)
        {
            SettingsMessage = exception.Message;
        }
        catch (Exception)
        {
            SettingsMessage = "The catalog option could not be saved. Try again.";
        }
    }

    private async Task ToggleCatalogOptionAsync(object? parameter)
    {
        if (parameter is not CatalogOptionViewModel option)
        {
            return;
        }

        var newState = !option.IsActive;
        try
        {
            if (_catalogRepository is not null)
            {
                await _catalogRepository.SetOptionActiveAsync(option.Id, newState);
            }

            option.IsActive = newState;
            if (option.Category == CatalogCategory.Platform)
            {
                var platform = Platforms.Single(candidate => candidate.Id == option.Id);
                platform.IsActive = newState;
                RebuildActivePlatforms();
                if (!newState && SelectedPlatformName == platform.Name)
                {
                    SelectedPlatformName = "None";
                }
            }

            var dependentNote = option.Category == CatalogCategory.Genre && !newState
                ? " Its active subgenres were archived as well."
                : string.Empty;
            SettingsMessage = newState
                ? $"{option.Name} was restored and saved."
                : $"{option.Name} was archived and saved.{dependentNote}";
        }
        catch (InvalidOperationException exception)
        {
            SettingsMessage = exception.Message;
        }
        catch (Exception)
        {
            SettingsMessage = "The catalog change could not be saved. Try again.";
        }
    }

    private void SelectPlatform(object? parameter)
    {
        if (parameter is CatalogOptionViewModel platform)
        {
            SelectedPlatformName = platform.Name;
        }
    }

    private void AddPreviewPlatforms()
    {
        AddPreviewPlatform("0d3034e8-e927-4a2f-a96f-ae610a47fb2f", "PC");
        AddPreviewPlatform("db7d530a-5cb9-411c-bb8e-7006282f63d7", "Mobile");
        AddPreviewPlatform("1826ccb8-23f3-44a8-af8c-538d25e7e036", "Console");
        AddPreviewPlatform("ec646f17-5476-4d76-992e-223d4b4d392b", "VR");
        AddPreviewPlatform("e0cb0941-ddc3-4b03-956f-7824da01c224", "Board Game");
        foreach (var platform in Platforms)
        {
            CatalogOptions.Add(platform);
        }

        RebuildActivePlatforms();
    }

    private void AddPreviewPlatform(string id, string name) =>
        Platforms.Add(new CatalogOptionViewModel(Guid.Parse(id), CatalogCategory.Platform, name, true));

    private void RebuildActivePlatforms()
    {
        ActivePlatforms.Clear();
        foreach (var platform in Platforms.Where(platform => platform.IsActive))
        {
            ActivePlatforms.Add(platform);
        }
    }

    private static CatalogOptionViewModel ToViewModel(CatalogOption option, string? parentName = null) =>
        new(option.Id, option.Category, option.NameEnglish, option.IsBuiltIn, option.IsActive,
            option.ParentOptionId, parentName);

    private static string CleanPreviewName(string value)
    {
        var cleaned = string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (cleaned.Length == 0)
        {
            throw new InvalidOperationException("Enter an option name before adding it.");
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
