using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GameDesignWizard.Core.Catalog;
using GameDesignWizard.Core.Ideas;

namespace GameDesignWizard.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly ICatalogRepository? _catalogRepository;
    private readonly ICatalogFileReader? _catalogFileReader;
    private readonly IGameIdeaRepository? _gameIdeaRepository;
    private readonly IManagedMediaStorage? _managedMediaStorage;
    private readonly IGameIdeaPdfExporter? _gameIdeaPdfExporter;
    private readonly List<CatalogOptionViewModel> _allSubgenres = [];
    private readonly List<string> _removedManagedMediaPaths = [];
    private AppPage _currentPage = AppPage.Home;
    private CatalogCategoryItemViewModel _selectedCatalogCategory;
    private CatalogOptionViewModel? _selectedParentGenre;
    private string _newCatalogOptionName = string.Empty;
    private string _selectedPlatformName = "None";
    private string _settingsMessage;
    private string _wizardMessage = "Select a platform or leave the step empty.";
    private int _wizardStep = 1;
    private CatalogOptionViewModel? _selectedGenre;
    private CatalogOptionViewModel? _selectedSubgenre;
    private CatalogOptionViewModel? _selectedPlatform;
    private CatalogOptionViewModel? _selectedDevelopmentDuration;
    private CatalogOptionViewModel? _selectedTeamSize;
    private string _gameName = "NewGame";
    private string _overviewEnglish = string.Empty;
    private GameIdeaListItemViewModel? _selectedSavedIdea;
    private string? _ideaPoolActionMessage;
    private readonly Guid _draftIdeaId = Guid.NewGuid();
    private readonly DateTime _draftCreatedAtUtc = DateTime.UtcNow;
    private bool _hasSavedIdea;
    private bool _isInitialized;
    private bool _isExportingPdf;

    public MainWindowViewModel()
        : this(null, null, null, null, null)
    {
    }

    public MainWindowViewModel(ICatalogRepository? catalogRepository)
        : this(catalogRepository, null, null, null, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader)
        : this(catalogRepository, catalogFileReader, null, null, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader,
        IGameIdeaRepository? gameIdeaRepository)
        : this(catalogRepository, catalogFileReader, gameIdeaRepository, null, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader,
        IGameIdeaRepository? gameIdeaRepository,
        IManagedMediaStorage? managedMediaStorage)
        : this(catalogRepository, catalogFileReader, gameIdeaRepository, managedMediaStorage, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader,
        IGameIdeaRepository? gameIdeaRepository,
        IManagedMediaStorage? managedMediaStorage,
        IGameIdeaPdfExporter? gameIdeaPdfExporter)
    {
        _catalogRepository = catalogRepository;
        _catalogFileReader = catalogFileReader;
        _gameIdeaRepository = gameIdeaRepository;
        _managedMediaStorage = managedMediaStorage;
        _gameIdeaPdfExporter = gameIdeaPdfExporter;
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
        Genres = [];
        AvailableSubgenres = [];
        References = [];
        DevelopmentDurationChoices = [];
        TeamSizeChoices = [];
        GddSections = CreateDefaultGddSections();
        SavedIdeas = [];
        MediaAttachments = [];
        foreach (var section in GddSections)
        {
            section.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(GddSectionDraftViewModel.Content))
                {
                    OnPropertyChanged(nameof(WizardSelectionSummary));
                }
            };
        }
        TopicPicker = new DualListPickerViewModel("Topics", CatalogCategory.Topic);
        FeaturePicker = new DualListPickerViewModel("Features", CatalogCategory.Feature);
        ArtStylePicker = new DualListPickerViewModel("Art styles", CatalogCategory.ArtStyle);
        MechanicsPicker = new DualListPickerViewModel("Mechanics", CatalogCategory.Mechanic);
        TopicPicker.SelectionChanged += (_, _) => PickerSelectionChanged(TopicPicker);
        FeaturePicker.SelectionChanged += (_, _) => PickerSelectionChanged(FeaturePicker);
        ArtStylePicker.SelectionChanged += (_, _) => PickerSelectionChanged(ArtStylePicker);
        MechanicsPicker.SelectionChanged += (_, _) => PickerSelectionChanged(MechanicsPicker);
        AddReference();
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
        SelectGenreCommand = new RelayCommand(SelectGenre);
        SelectSubgenreCommand = new RelayCommand(SelectSubgenre);
        AddTopicCommand = TopicPicker.AddCommand;
        RemoveTopicCommand = TopicPicker.RemoveCommand;
        AddReferenceCommand = new RelayCommand(_ => AddReference());
        RemoveReferenceCommand = new RelayCommand(RemoveReference);
        ClearDevelopmentDurationCommand = new RelayCommand(_ => SelectedDevelopmentDuration = null);
        ClearTeamSizeCommand = new RelayCommand(_ => SelectedTeamSize = null);
        RemoveMediaCommand = new RelayCommand(RemoveMedia);
        WizardNextCommand = new AsyncRelayCommand(_ => MoveWizardNextAsync());
        WizardPreviousCommand = new RelayCommand(_ => MoveWizardPrevious());
    }

    public ObservableCollection<CatalogCategoryItemViewModel> CatalogCategories { get; }

    public ObservableCollection<CatalogOptionViewModel> Platforms { get; }

    public ObservableCollection<CatalogOptionViewModel> ActivePlatforms { get; }

    public ObservableCollection<CatalogOptionViewModel> CatalogOptions { get; }

    public ObservableCollection<CatalogOptionViewModel> GenreChoices { get; }

    public ObservableCollection<CatalogOptionViewModel> Genres { get; }

    public ObservableCollection<CatalogOptionViewModel> AvailableSubgenres { get; }

    public ObservableCollection<GameReferenceViewModel> References { get; }

    public ObservableCollection<CatalogOptionViewModel> DevelopmentDurationChoices { get; }

    public ObservableCollection<CatalogOptionViewModel> TeamSizeChoices { get; }

    public ObservableCollection<GddSectionDraftViewModel> GddSections { get; }

    public ObservableCollection<GameIdeaListItemViewModel> SavedIdeas { get; }

    public GameIdeaListItemViewModel? SelectedSavedIdea
    {
        get => _selectedSavedIdea;
        set
        {
            if (SetProperty(ref _selectedSavedIdea, value))
            {
                _ideaPoolActionMessage = null;
                OnPropertyChanged(nameof(HasSelectedSavedIdea));
                OnPropertyChanged(nameof(CanExportSelectedIdeaPdf));
                OnPropertyChanged(nameof(IdeaPoolMessage));
            }
        }
    }

    public bool HasSelectedSavedIdea => SelectedSavedIdea is not null;

    public bool CanExportSelectedIdeaPdf => HasSelectedSavedIdea && !_isExportingPdf;

    public ObservableCollection<DraftMediaAttachmentViewModel> MediaAttachments { get; }

    public ObservableCollection<CatalogOptionViewModel> AvailableTopics => TopicPicker.AvailableOptions;

    public ObservableCollection<CatalogOptionViewModel> SelectedTopics => TopicPicker.SelectedOptions;

    public DualListPickerViewModel TopicPicker { get; }

    public DualListPickerViewModel FeaturePicker { get; }

    public DualListPickerViewModel ArtStylePicker { get; }

    public DualListPickerViewModel MechanicsPicker { get; }

    public ICommand NavigateHomeCommand { get; }

    public ICommand NavigateWizardCommand { get; }

    public ICommand NavigatePoolCommand { get; }

    public ICommand NavigateSettingsCommand { get; }

    public ICommand AddCatalogOptionCommand { get; }

    public ICommand ToggleCatalogOptionCommand { get; }

    public ICommand SelectPlatformCommand { get; }

    public ICommand SelectGenreCommand { get; }

    public ICommand SelectSubgenreCommand { get; }

    public ICommand AddTopicCommand { get; }

    public ICommand RemoveTopicCommand { get; }

    public ICommand AddReferenceCommand { get; }

    public ICommand RemoveReferenceCommand { get; }

    public ICommand ClearDevelopmentDurationCommand { get; }

    public ICommand ClearTeamSizeCommand { get; }

    public ICommand RemoveMediaCommand { get; }

    public ICommand WizardNextCommand { get; }

    public ICommand WizardPreviousCommand { get; }

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

    public string GameName
    {
        get => _gameName;
        set
        {
            if (SetProperty(ref _gameName, value))
            {
                OnPropertyChanged(nameof(WizardSelectionSummary));
            }
        }
    }

    public string OverviewEnglish
    {
        get => _overviewEnglish;
        set => SetProperty(ref _overviewEnglish, value);
    }

    public CatalogOptionViewModel? SelectedDevelopmentDuration
    {
        get => _selectedDevelopmentDuration;
        set => SetProperty(ref _selectedDevelopmentDuration, value);
    }

    public CatalogOptionViewModel? SelectedTeamSize
    {
        get => _selectedTeamSize;
        set => SetProperty(ref _selectedTeamSize, value);
    }

    public string SettingsMessage
    {
        get => _settingsMessage;
        private set => SetProperty(ref _settingsMessage, value);
    }

    public string TopicSearch
    {
        get => TopicPicker.SearchText;
        set
        {
            if (!string.Equals(TopicPicker.SearchText, value, StringComparison.Ordinal))
            {
                TopicPicker.SearchText = value;
                OnPropertyChanged();
            }
        }
    }

    public string WizardMessage
    {
        get => _wizardMessage;
        private set => SetProperty(ref _wizardMessage, value);
    }

    public CatalogOptionViewModel? SelectedGenre
    {
        get => _selectedGenre;
        private set
        {
            if (SetProperty(ref _selectedGenre, value))
            {
                OnPropertyChanged(nameof(WizardSelectionSummary));
            }
        }
    }

    public CatalogOptionViewModel? SelectedSubgenre
    {
        get => _selectedSubgenre;
        private set
        {
            if (SetProperty(ref _selectedSubgenre, value))
            {
                OnPropertyChanged(nameof(WizardSelectionSummary));
            }
        }
    }

    public string WizardTitle => _wizardStep switch
    {
        1 => "Choose a target platform",
        2 => "Choose genre, subgenre, and topics",
        3 => "Choose features and art styles",
        4 => "Choose gameplay mechanics",
        5 => "Add design references",
        6 => "Describe and save your game",
        _ => "Create your game idea"
    };

    public string WizardInstructions => _wizardStep switch
    {
        1 => "Select one option or leave this step empty and continue.",
        2 => "Choose one genre, an optional related subgenre, and any number of topics.",
        3 => "Choose any number of features and art styles. Double-click or press Enter to move an item.",
        4 => "Choose any number of mechanics. Double-click or press Enter to move an item.",
        5 => "Add useful links and notes. Empty rows will be ignored when the idea is saved.",
        6 => "Name the idea, define its production scope, and write any GDD sections that help.",
        _ => "Complete this step or leave it empty and continue."
    };

    public string WizardStepLabel => $"STEP {_wizardStep} OF 6";

    public string WizardNextLabel => _wizardStep switch
    {
        1 => "Continue",
        2 => "Continue to Step 3",
        3 => "Continue to Step 4",
        4 => "Continue to Step 5",
        5 => "Continue to Step 6",
        6 => _hasSavedIdea ? "Save Changes" : "Save Idea",
        _ => "Continue"
    };

    public string WizardPreviousLabel => _wizardStep == 1 ? "Back to Home" : "Previous";

    public string WizardSelectionSummary => _wizardStep switch
    {
        1 => $"Selected platform: {SelectedPlatformName}",
        2 => $"Genre: {SelectedGenre?.Name ?? "None"} · Subgenre: {SelectedSubgenre?.Name ?? "None"} · Topics: {SelectedTopics.Count}",
        3 => $"Features: {FeaturePicker.SelectedOptions.Count} · Art styles: {ArtStylePicker.SelectedOptions.Count}",
        4 => $"Mechanics: {MechanicsPicker.SelectedOptions.Count}",
        5 => $"References: {References.Count(reference => !reference.IsBlank)}",
        6 => $"Game: {NormalizeGameName(GameName)} · GDD sections: {GddSections.Count(section => !string.IsNullOrWhiteSpace(section.Content))} · Media: {MediaAttachments.Count}",
        _ => string.Empty
    };

    public Visibility WizardStepOneVisibility => _wizardStep == 1 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility WizardStepTwoVisibility => _wizardStep == 2 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility WizardStepThreeVisibility => _wizardStep == 3 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility WizardStepFourVisibility => _wizardStep == 4 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility WizardStepFiveVisibility => _wizardStep == 5 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility WizardStepSixVisibility => _wizardStep == 6 ? Visibility.Visible : Visibility.Collapsed;

    public string WizardSecondProgressColor => _wizardStep >= 2 ? "#6C5CE7" : "#E1E3EC";

    public string WizardThirdProgressColor => _wizardStep >= 3 ? "#6C5CE7" : "#E1E3EC";

    public string WizardFourthProgressColor => _wizardStep >= 4 ? "#6C5CE7" : "#E1E3EC";

    public string WizardFifthProgressColor => _wizardStep >= 5 ? "#6C5CE7" : "#E1E3EC";

    public string WizardSixthProgressColor => _wizardStep >= 6 ? "#6C5CE7" : "#E1E3EC";

    public int PcIdeaCount => SavedIdeas.Count(idea => idea.PoolGroup == PlatformPoolGroup.Pc);

    public int MobileIdeaCount => SavedIdeas.Count(idea => idea.PoolGroup == PlatformPoolGroup.Mobile);

    public int OtherIdeaCount => SavedIdeas.Count(idea => idea.PoolGroup == PlatformPoolGroup.Other);

    public string IdeaPoolMessage => _ideaPoolActionMessage ?? (SavedIdeas.Count == 0
        ? "No saved ideas yet. Complete the wizard to create the first one."
        : $"{SavedIdeas.Count} saved idea(s), ordered by the most recent update.");

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
        await LoadWizardCatalogsAsync(cancellationToken);
        await LoadIdeaPoolAsync(cancellationToken);
        _isInitialized = true;
        await LoadSelectedCatalogAsync(cancellationToken);
    }

    public async Task<CatalogImportPreviewViewModel> CreateCatalogImportPreviewAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (_catalogFileReader is null || _catalogRepository is null)
        {
            throw new InvalidOperationException("Catalog file import is not available in preview mode.");
        }

        var category = SelectedCatalogCategory.Category;
        var parentId = category == CatalogCategory.Subgenre ? SelectedParentGenre?.Id : null;
        if (category == CatalogCategory.Subgenre && parentId is null)
        {
            throw new InvalidOperationException("Select a parent genre before importing subgenres.");
        }

        var fileRows = await _catalogFileReader.ReadAsync(filePath, cancellationToken);
        if (fileRows.Count == 0)
        {
            throw new InvalidOperationException("The selected file does not contain any catalog names.");
        }

        var existingOptions = await _catalogRepository.GetOptionsAsync(category, cancellationToken);
        var existingNames = existingOptions
            .Select(option => option.NormalizedName)
            .ToHashSet(StringComparer.Ordinal);
        var namesInFile = new HashSet<string>(StringComparer.Ordinal);
        var previewRows = new List<CatalogImportRowViewModel>(fileRows.Count);

        foreach (var row in fileRows)
        {
            var normalizedName = row.NameEnglish.ToUpperInvariant();
            var status = row.Error;
            var isReady = false;
            if (status is null && row.NameEnglish.Length > 200)
            {
                status = "Name exceeds 200 characters";
            }
            else if (status is null && existingNames.Contains(normalizedName))
            {
                status = "Already exists";
            }
            else if (status is null && !namesInFile.Add(normalizedName))
            {
                status = "Duplicate in file";
            }
            else if (status is null)
            {
                status = "Ready";
                isReady = true;
            }

            previewRows.Add(new CatalogImportRowViewModel(
                row.SourceRow,
                row.NameEnglish,
                status,
                isReady));
        }

        return new CatalogImportPreviewViewModel(
            Path.GetFileName(filePath),
            category,
            SelectedCatalogCategory.DisplayName,
            parentId,
            previewRows);
    }

    public async Task ApplyCatalogImportAsync(
        CatalogImportPreviewViewModel preview,
        CancellationToken cancellationToken = default)
    {
        if (_catalogRepository is null)
        {
            throw new InvalidOperationException("Catalog import is not available in preview mode.");
        }

        var names = preview.Rows.Where(row => row.IsReady).Select(row => row.Name).ToArray();
        if (names.Length == 0)
        {
            throw new InvalidOperationException("There are no valid rows to import.");
        }

        await _catalogRepository.AddOptionsAsync(
            preview.Category,
            names,
            preview.ParentOptionId,
            cancellationToken);
        await LoadSelectedCatalogAsync(cancellationToken);
        if (preview.Category == CatalogCategory.Platform)
        {
            await LoadPlatformsAsync(cancellationToken);
        }
        else if (IsWizardCatalog(preview.Category))
        {
            await LoadWizardCatalogsAsync(cancellationToken);
        }

        SettingsMessage = $"{names.Length} option(s) imported from {preview.SourceFileName}.";
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

        var selectedPlatformId = _selectedPlatform?.Id;
        var platforms = await _catalogRepository.GetOptionsAsync(CatalogCategory.Platform, cancellationToken);
        Platforms.Clear();
        foreach (var platform in platforms)
        {
            Platforms.Add(ToViewModel(platform));
        }

        RebuildActivePlatforms();
        _selectedPlatform = ActivePlatforms.SingleOrDefault(platform => platform.Id == selectedPlatformId);
        if (_selectedPlatform is not null)
        {
            _selectedPlatform.IsSelected = true;
            SelectedPlatformName = _selectedPlatform.Name;
        }
        else if (selectedPlatformId is not null)
        {
            SelectedPlatformName = "None";
        }
    }

    private async Task LoadWizardCatalogsAsync(CancellationToken cancellationToken = default)
    {
        if (_catalogRepository is null)
        {
            return;
        }

        var genres = await _catalogRepository.GetOptionsAsync(CatalogCategory.Genre, cancellationToken);
        var subgenres = await _catalogRepository.GetOptionsAsync(CatalogCategory.Subgenre, cancellationToken);
        var topics = await _catalogRepository.GetOptionsAsync(CatalogCategory.Topic, cancellationToken);
        var features = await _catalogRepository.GetOptionsAsync(CatalogCategory.Feature, cancellationToken);
        var artStyles = await _catalogRepository.GetOptionsAsync(CatalogCategory.ArtStyle, cancellationToken);
        var mechanics = await _catalogRepository.GetOptionsAsync(CatalogCategory.Mechanic, cancellationToken);
        var developmentDurations = await _catalogRepository.GetOptionsAsync(
            CatalogCategory.DevelopmentDuration,
            cancellationToken);
        var teamSizes = await _catalogRepository.GetOptionsAsync(CatalogCategory.TeamSize, cancellationToken);

        Genres.Clear();
        foreach (var genre in genres.Where(option => option.IsActive))
        {
            Genres.Add(ToViewModel(genre));
        }

        _allSubgenres.Clear();
        _allSubgenres.AddRange(subgenres.Where(option => option.IsActive).Select(option => ToViewModel(option)));
        TopicPicker.LoadOptions(topics.Select(option => ToViewModel(option)));
        FeaturePicker.LoadOptions(features.Select(option => ToViewModel(option)));
        ArtStylePicker.LoadOptions(artStyles.Select(option => ToViewModel(option)));
        MechanicsPicker.LoadOptions(mechanics.Select(option => ToViewModel(option)));
        SelectedDevelopmentDuration = RefreshSingleChoiceCatalog(
            DevelopmentDurationChoices,
            developmentDurations,
            SelectedDevelopmentDuration?.Id);
        SelectedTeamSize = RefreshSingleChoiceCatalog(
            TeamSizeChoices,
            teamSizes,
            SelectedTeamSize?.Id);

        if (SelectedGenre is not null)
        {
            var currentGenre = Genres.SingleOrDefault(option => option.Id == SelectedGenre.Id);
            SelectedGenre = currentGenre;
            if (currentGenre is not null)
            {
                currentGenre.IsSelected = true;
            }
        }

        if (SelectedSubgenre is not null)
        {
            var currentSubgenre = _allSubgenres.SingleOrDefault(option => option.Id == SelectedSubgenre.Id);
            SelectedSubgenre = currentSubgenre;
            if (currentSubgenre is not null)
            {
                currentSubgenre.IsSelected = true;
            }
        }

        RefreshAvailableSubgenres();
        OnPropertyChanged(nameof(WizardSelectionSummary));
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

            else if (_catalogRepository is not null && IsWizardCatalog(category))
            {
                await LoadWizardCatalogsAsync();
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
                if (!newState && _selectedPlatform?.Id == platform.Id)
                {
                    _selectedPlatform = null;
                    SelectedPlatformName = "None";
                }
            }
            else if (_catalogRepository is not null && IsWizardCatalog(option.Category))
            {
                await LoadWizardCatalogsAsync();
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
            foreach (var option in Platforms)
            {
                option.IsSelected = option.Id == platform.Id;
            }

            _selectedPlatform = platform;
            SelectedPlatformName = platform.Name;
            OnPropertyChanged(nameof(WizardSelectionSummary));
        }
    }

    private void SelectGenre(object? parameter)
    {
        if (parameter is not CatalogOptionViewModel genre)
        {
            return;
        }

        foreach (var option in Genres)
        {
            option.IsSelected = option.Id == genre.Id;
        }

        SelectedGenre = genre;
        if (SelectedSubgenre is not null)
        {
            SelectedSubgenre.IsSelected = false;
            SelectedSubgenre = null;
            WizardMessage = "The previous subgenre was cleared because the genre changed.";
        }
        else
        {
            WizardMessage = $"{genre.Name} selected. Choose a related subgenre or skip it.";
        }

        RefreshAvailableSubgenres();
    }

    private void SelectSubgenre(object? parameter)
    {
        if (parameter is not CatalogOptionViewModel subgenre)
        {
            return;
        }

        foreach (var option in AvailableSubgenres)
        {
            option.IsSelected = option.Id == subgenre.Id;
        }

        SelectedSubgenre = subgenre;
        WizardMessage = $"{subgenre.Name} selected.";
    }

    private void RefreshAvailableSubgenres()
    {
        AvailableSubgenres.Clear();
        if (SelectedGenre is null)
        {
            return;
        }

        foreach (var subgenre in _allSubgenres.Where(option => option.ParentOptionId == SelectedGenre.Id))
        {
            subgenre.IsSelected = subgenre.Id == SelectedSubgenre?.Id;
            AvailableSubgenres.Add(subgenre);
        }
    }

    private async Task MoveWizardNextAsync()
    {
        if (_wizardStep == 5 && References.Any(reference => reference.HasValidationError))
        {
            WizardMessage = "Fix the highlighted reference URLs before continuing.";
            return;
        }

        if (_wizardStep < 6)
        {
            _wizardStep++;
            WizardMessage = _wizardStep switch
            {
                2 => "Choose one genre, an optional subgenre, and any number of topics.",
                3 => "Choose features and art styles, or leave either list empty.",
                4 => "Choose gameplay mechanics, or leave this list empty.",
                5 => "Add links and notes, or leave the reference list empty.",
                6 => "Complete any useful fields, then save the idea to your local workspace.",
                _ => string.Empty
            };
            NotifyWizardStepChanged();
            return;
        }

        await SaveIdeaAsync();
    }

    private void MoveWizardPrevious()
    {
        if (_wizardStep == 1)
        {
            CurrentPage = AppPage.Home;
            return;
        }

        _wizardStep--;
        WizardMessage = _wizardStep switch
        {
            1 => "Select a platform or leave the step empty.",
            2 => "Choose one genre, an optional subgenre, and any number of topics.",
            3 => "Choose features and art styles, or leave either list empty.",
            4 => "Choose gameplay mechanics, or leave this list empty.",
            5 => "Add links and notes, or leave the reference list empty.",
            _ => string.Empty
        };
        NotifyWizardStepChanged();
    }

    private void NotifyWizardStepChanged()
    {
        OnPropertyChanged(nameof(WizardTitle));
        OnPropertyChanged(nameof(WizardInstructions));
        OnPropertyChanged(nameof(WizardStepLabel));
        OnPropertyChanged(nameof(WizardNextLabel));
        OnPropertyChanged(nameof(WizardPreviousLabel));
        OnPropertyChanged(nameof(WizardSelectionSummary));
        OnPropertyChanged(nameof(WizardStepOneVisibility));
        OnPropertyChanged(nameof(WizardStepTwoVisibility));
        OnPropertyChanged(nameof(WizardStepThreeVisibility));
        OnPropertyChanged(nameof(WizardStepFourVisibility));
        OnPropertyChanged(nameof(WizardStepFiveVisibility));
        OnPropertyChanged(nameof(WizardStepSixVisibility));
        OnPropertyChanged(nameof(WizardSecondProgressColor));
        OnPropertyChanged(nameof(WizardThirdProgressColor));
        OnPropertyChanged(nameof(WizardFourthProgressColor));
        OnPropertyChanged(nameof(WizardFifthProgressColor));
        OnPropertyChanged(nameof(WizardSixthProgressColor));
    }

    public async Task SaveIdeaAsync(CancellationToken cancellationToken = default)
    {
        if (_gameIdeaRepository is null)
        {
            WizardMessage = "Idea saving is unavailable in preview mode.";
            return;
        }

        if (References.Any(reference => reference.HasValidationError))
        {
            WizardMessage = "Fix the highlighted reference URLs before saving.";
            return;
        }

        var importedThisSave = new List<DraftMediaAttachmentViewModel>();
        var ideaPersisted = false;
        try
        {
            var name = NormalizeGameName(GameName);
            GameName = name;
            if (MediaAttachments.Count > 0 && _managedMediaStorage is null)
            {
                throw new InvalidOperationException("Media storage is unavailable in preview mode.");
            }

            if (_managedMediaStorage is not null)
            {
                foreach (var attachment in MediaAttachments.Where(attachment => attachment.StoredRelativePath is null))
                {
                    var managedFile = await _managedMediaStorage.ImportAsync(
                        _draftIdeaId,
                        attachment.Id,
                        attachment.SourcePath,
                        cancellationToken);
                    attachment.MarkManaged(managedFile.StoredRelativePath);
                    importedThisSave.Add(attachment);
                }
            }

            var document = new GameIdeaDocument
            {
                Id = _draftIdeaId,
                NameEnglish = name,
                Stage = GameIdeaStage.Idea,
                PoolGroup = _selectedPlatform?.PlatformPoolGroup ?? PlatformPoolGroup.Other,
                Platform = ToSnapshot(_selectedPlatform),
                Genre = ToSnapshot(SelectedGenre),
                Subgenre = ToSnapshot(SelectedSubgenre),
                Topics = SelectedTopics.Select(ToSnapshot).OfType<CatalogSelectionSnapshot>().ToList(),
                Features = FeaturePicker.SelectedOptions.Select(ToSnapshot).OfType<CatalogSelectionSnapshot>().ToList(),
                ArtStyles = ArtStylePicker.SelectedOptions.Select(ToSnapshot).OfType<CatalogSelectionSnapshot>().ToList(),
                Mechanics = MechanicsPicker.SelectedOptions.Select(ToSnapshot).OfType<CatalogSelectionSnapshot>().ToList(),
                DevelopmentDuration = ToSnapshot(SelectedDevelopmentDuration),
                TeamSize = ToSnapshot(SelectedTeamSize),
                OverviewEnglish = OverviewEnglish.Trim(),
                Sections = GddSections.Select(section => new GddSectionContent(
                    section.Id,
                    section.Title,
                    section.Content.Trim(),
                    section.SortOrder)).ToList(),
                References = References
                    .Where(reference => !reference.IsBlank)
                    .Select((reference, index) => new GameReferenceContent(
                        reference.Id,
                        string.IsNullOrWhiteSpace(reference.Url) ? null : reference.Url.Trim(),
                        reference.Note.Trim(),
                        index))
                    .ToList(),
                MediaAttachments = MediaAttachments
                    .Select((attachment, index) => new MediaAttachmentContent(
                        attachment.Id,
                        attachment.FileName,
                        attachment.MediaType,
                        attachment.StoredRelativePath
                            ?? throw new InvalidOperationException($"{attachment.FileName} has not been copied to managed storage."),
                        attachment.Caption.Trim(),
                        index))
                    .ToList(),
                CreatedAtUtc = _draftCreatedAtUtc,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await _gameIdeaRepository.SaveAsync(document, cancellationToken);
            ideaPersisted = true;
            _hasSavedIdea = true;
            OnPropertyChanged(nameof(WizardNextLabel));
            if (_managedMediaStorage is not null)
            {
                foreach (var storedPath in _removedManagedMediaPaths.ToArray())
                {
                    try
                    {
                        await _managedMediaStorage.DeleteAsync(storedPath, cancellationToken);
                        _removedManagedMediaPaths.Remove(storedPath);
                    }
                    catch
                    {
                        // An unreferenced file can be cleaned during a later save or maintenance pass.
                    }
                }
            }

            await LoadIdeaPoolAsync(cancellationToken);
            OnPropertyChanged(nameof(WizardSelectionSummary));
            WizardMessage = $"{name} was saved to the {GetPoolLabel(document.PoolGroup)} idea pool.";
        }
        catch (InvalidOperationException exception)
        {
            if (!ideaPersisted)
            {
                await RollBackMediaImportsAsync(importedThisSave);
                WizardMessage = exception.Message;
                return;
            }

            WizardMessage = $"{NormalizeGameName(GameName)} was saved, but the idea pool could not be refreshed.";
        }
        catch (Exception)
        {
            if (!ideaPersisted)
            {
                await RollBackMediaImportsAsync(importedThisSave);
                WizardMessage = "The idea could not be saved. Try again.";
                return;
            }

            WizardMessage = $"{NormalizeGameName(GameName)} was saved, but the idea pool could not be refreshed.";
        }
    }

    public async Task ExportSelectedIdeaPdfAsync(
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        if (SelectedSavedIdea is null)
        {
            SetIdeaPoolActionMessage("Select an idea before exporting a PDF.");
            return;
        }

        if (_gameIdeaRepository is null || _gameIdeaPdfExporter is null)
        {
            SetIdeaPoolActionMessage("PDF export is unavailable in preview mode.");
            return;
        }

        if (_isExportingPdf)
        {
            return;
        }

        _isExportingPdf = true;
        OnPropertyChanged(nameof(CanExportSelectedIdeaPdf));
        try
        {
            var idea = await _gameIdeaRepository.GetByIdAsync(SelectedSavedIdea.Id, cancellationToken)
                ?? throw new InvalidOperationException("The selected idea could not be found.");
            await _gameIdeaPdfExporter.ExportAsync(idea, destinationPath, cancellationToken);
            SetIdeaPoolActionMessage($"{idea.NameEnglish} was exported to {Path.GetFileName(destinationPath)}.");
        }
        catch (OperationCanceledException)
        {
            SetIdeaPoolActionMessage("PDF export was canceled.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            SetIdeaPoolActionMessage(exception.Message);
        }
        catch (Exception)
        {
            SetIdeaPoolActionMessage("The PDF could not be exported. Try another location.");
        }
        finally
        {
            _isExportingPdf = false;
            OnPropertyChanged(nameof(CanExportSelectedIdeaPdf));
        }
    }

    public void AddMediaFiles(IEnumerable<string> filePaths)
    {
        if (_managedMediaStorage is null)
        {
            WizardMessage = "Media selection is unavailable in preview mode.";
            return;
        }

        try
        {
            var candidates = new List<(string FullPath, string FileName, MediaAttachmentType MediaType)>();
            foreach (var path in filePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (MediaAttachments.Any(attachment =>
                        string.Equals(attachment.SourcePath, fullPath, StringComparison.OrdinalIgnoreCase))
                    || candidates.Any(candidate =>
                        string.Equals(candidate.FullPath, fullPath, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (!File.Exists(fullPath))
                {
                    throw new InvalidOperationException($"{Path.GetFileName(fullPath)} could not be found.");
                }

                var mediaType = _managedMediaStorage.GetMediaType(fullPath);
                candidates.Add((fullPath, Path.GetFileName(fullPath), mediaType));
            }

            foreach (var candidate in candidates)
            {
                MediaAttachments.Add(new DraftMediaAttachmentViewModel(
                    candidate.FullPath,
                    candidate.FileName,
                    candidate.MediaType));
            }

            OnPropertyChanged(nameof(WizardSelectionSummary));
            WizardMessage = candidates.Count == 0
                ? "The selected media files are already attached."
                : $"{candidates.Count} media file(s) ready to copy when the idea is saved.";
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or NotSupportedException)
        {
            WizardMessage = exception.Message;
        }
    }

    private void AddReference()
    {
        var reference = new GameReferenceViewModel();
        reference.Changed += ReferenceChanged;
        References.Add(reference);
        OnPropertyChanged(nameof(WizardSelectionSummary));
        if (_wizardStep == 5)
        {
            WizardMessage = "A new reference row was added.";
        }
    }

    private void RemoveMedia(object? parameter)
    {
        if (parameter is not DraftMediaAttachmentViewModel attachment
            || !MediaAttachments.Remove(attachment))
        {
            return;
        }

        if (attachment.StoredRelativePath is not null)
        {
            _removedManagedMediaPaths.Add(attachment.StoredRelativePath);
        }

        OnPropertyChanged(nameof(WizardSelectionSummary));
        WizardMessage = $"{attachment.FileName} will be removed when the idea is saved.";
    }

    private async Task RollBackMediaImportsAsync(IEnumerable<DraftMediaAttachmentViewModel> attachments)
    {
        if (_managedMediaStorage is null)
        {
            return;
        }

        foreach (var attachment in attachments)
        {
            if (attachment.StoredRelativePath is not { } storedPath)
            {
                continue;
            }

            try
            {
                await _managedMediaStorage.DeleteAsync(storedPath);
            }
            catch
            {
                // Preserve the pending attachment so the next save can replace any orphaned file.
            }

            attachment.MarkPending();
        }
    }

    private async Task LoadIdeaPoolAsync(CancellationToken cancellationToken = default)
    {
        if (_gameIdeaRepository is null)
        {
            return;
        }

        var selectedIdeaId = SelectedSavedIdea?.Id;
        var ideas = await _gameIdeaRepository.GetAllAsync(cancellationToken);
        SavedIdeas.Clear();
        foreach (var idea in ideas)
        {
            SavedIdeas.Add(new GameIdeaListItemViewModel(idea));
        }

        SelectedSavedIdea = selectedIdeaId is null
            ? null
            : SavedIdeas.FirstOrDefault(idea => idea.Id == selectedIdeaId);

        OnPropertyChanged(nameof(PcIdeaCount));
        OnPropertyChanged(nameof(MobileIdeaCount));
        OnPropertyChanged(nameof(OtherIdeaCount));
        OnPropertyChanged(nameof(IdeaPoolMessage));
    }

    private void SetIdeaPoolActionMessage(string message)
    {
        _ideaPoolActionMessage = message;
        OnPropertyChanged(nameof(IdeaPoolMessage));
    }

    private void RemoveReference(object? parameter)
    {
        if (parameter is not GameReferenceViewModel reference || !References.Remove(reference))
        {
            return;
        }

        reference.Changed -= ReferenceChanged;
        OnPropertyChanged(nameof(WizardSelectionSummary));
        WizardMessage = "The reference row was removed.";
    }

    private void ReferenceChanged(object? sender, EventArgs eventArgs) =>
        OnPropertyChanged(nameof(WizardSelectionSummary));

    private void PickerSelectionChanged(DualListPickerViewModel picker)
    {
        OnPropertyChanged(nameof(WizardSelectionSummary));
        var label = picker.SelectedOptions.Count == 1
            ? picker.Category switch
            {
                CatalogCategory.ArtStyle => "art style",
                CatalogCategory.Feature => "feature",
                CatalogCategory.Mechanic => "mechanic",
                CatalogCategory.Topic => "topic",
                _ => picker.Title.ToLowerInvariant()
            }
            : picker.Title.ToLowerInvariant();
        WizardMessage = $"{picker.SelectedOptions.Count} {label} selected.";
    }

    private static bool IsWizardCatalog(CatalogCategory category) => category is
        CatalogCategory.Genre or
        CatalogCategory.Subgenre or
        CatalogCategory.Topic or
        CatalogCategory.Mechanic or
        CatalogCategory.Feature or
        CatalogCategory.ArtStyle or
        CatalogCategory.DevelopmentDuration or
        CatalogCategory.TeamSize;

    private void AddPreviewPlatforms()
    {
        AddPreviewPlatform("0d3034e8-e927-4a2f-a96f-ae610a47fb2f", "PC", PlatformPoolGroup.Pc);
        AddPreviewPlatform("db7d530a-5cb9-411c-bb8e-7006282f63d7", "Mobile", PlatformPoolGroup.Mobile);
        AddPreviewPlatform("1826ccb8-23f3-44a8-af8c-538d25e7e036", "Console");
        AddPreviewPlatform("ec646f17-5476-4d76-992e-223d4b4d392b", "VR");
        AddPreviewPlatform("e0cb0941-ddc3-4b03-956f-7824da01c224", "Board Game");
        foreach (var platform in Platforms)
        {
            CatalogOptions.Add(platform);
        }

        RebuildActivePlatforms();
    }

    private void AddPreviewPlatform(
        string id,
        string name,
        PlatformPoolGroup poolGroup = PlatformPoolGroup.Other) =>
        Platforms.Add(new CatalogOptionViewModel(
            Guid.Parse(id),
            CatalogCategory.Platform,
            name,
            true,
            platformPoolGroup: poolGroup));

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
            option.ParentOptionId, parentName, option.PlatformPoolGroup);

    private static CatalogOptionViewModel? RefreshSingleChoiceCatalog(
        ObservableCollection<CatalogOptionViewModel> target,
        IEnumerable<CatalogOption> options,
        Guid? selectedId)
    {
        target.Clear();
        foreach (var option in options.Where(option => option.IsActive))
        {
            target.Add(ToViewModel(option));
        }

        return target.SingleOrDefault(option => option.Id == selectedId);
    }

    private static CatalogSelectionSnapshot? ToSnapshot(CatalogOptionViewModel? option) => option is null
        ? null
        : new CatalogSelectionSnapshot(option.Id, option.Name);

    private static string NormalizeGameName(string value)
    {
        var normalized = string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (normalized.Length == 0)
        {
            return "NewGame";
        }

        if (normalized.Length > 200)
        {
            throw new InvalidOperationException("The game name must contain 200 characters or fewer.");
        }

        return normalized;
    }

    private static string GetPoolLabel(PlatformPoolGroup poolGroup) => poolGroup switch
    {
        PlatformPoolGroup.Pc => "PC",
        PlatformPoolGroup.Mobile => "Mobile",
        _ => "Other"
    };

    private static ObservableCollection<GddSectionDraftViewModel> CreateDefaultGddSections() =>
    [
        new("Design Pillars", "Define the few principles that every major design decision should support.", 0),
        new("Target Platform and Audience", "Describe the intended players, play context, and platform-specific constraints.", 1),
        new("Genre and Themes", "Explain the genre promise, tone, themes, and emotional goals.", 2),
        new("Core Gameplay Loop", "Describe what the player repeatedly does and why the loop remains engaging.", 3),
        new("Mechanics and Systems", "Explain the rules, interactions, resources, and connected systems.", 4),
        new("Progression", "Describe how challenge, abilities, content, and player mastery develop over time.", 5),
        new("Characters", "Record important characters, roles, motivations, and relationships.", 6),
        new("World and Lore", "Describe the setting, locations, history, factions, and world rules.", 7),
        new("Art Direction", "Define the visual language, mood, readability goals, and key references.", 8),
        new("Technical Features", "Record important technical requirements, tools, integrations, and constraints.", 9),
        new("Production Scope", "Set boundaries, milestones, priorities, and assumptions for production.", 10),
        new("Risks and Open Questions", "Track unresolved decisions, design risks, and validation work.", 11)
    ];

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
