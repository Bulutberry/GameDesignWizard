using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
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
    private readonly IGameIdeaWorkbookExporter? _gameIdeaWorkbookExporter;
    private readonly ICatalogFileWriter? _catalogFileWriter;
    private readonly List<CatalogOptionViewModel> _allSubgenres = [];
    private readonly List<string> _removedManagedMediaPaths = [];
    private AppPage _currentPage = AppPage.Home;
    private CatalogCategoryItemViewModel _selectedCatalogCategory;
    private CatalogOptionViewModel? _selectedParentGenre;
    private string _newCatalogOptionName = string.Empty;
    private string _catalogSearchText = string.Empty;
    private string _selectedCatalogStatusFilter = "All options";
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
    private GameIdeaStage _selectedDevelopmentStage = GameIdeaStage.Idea;
    private GameIdeaListItemViewModel? _selectedSavedIdea;
    private string? _ideaPoolActionMessage;
    private string _ideaPoolSearchText = string.Empty;
    private string _selectedIdeaPoolFilter = "All";
    private string _selectedIdeaPoolStageFilter = "All stages";
    private Guid _draftIdeaId = Guid.NewGuid();
    private DateTime _draftCreatedAtUtc = DateTime.UtcNow;
    private bool _hasSavedIdea;
    private bool _isInitialized;
    private bool _isExportingPdf;
    private bool _isExportingWorkbook;
    private bool _isLoadingIdea;
    private bool _isDeletingIdea;

    public MainWindowViewModel()
        : this(null, null, null, null, null, null)
    {
    }

    public MainWindowViewModel(ICatalogRepository? catalogRepository)
        : this(catalogRepository, null, null, null, null, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader)
        : this(catalogRepository, catalogFileReader, null, null, null, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader,
        IGameIdeaRepository? gameIdeaRepository)
        : this(catalogRepository, catalogFileReader, gameIdeaRepository, null, null, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader,
        IGameIdeaRepository? gameIdeaRepository,
        IManagedMediaStorage? managedMediaStorage)
        : this(catalogRepository, catalogFileReader, gameIdeaRepository, managedMediaStorage, null, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader,
        IGameIdeaRepository? gameIdeaRepository,
        IManagedMediaStorage? managedMediaStorage,
        IGameIdeaPdfExporter? gameIdeaPdfExporter)
        : this(catalogRepository, catalogFileReader, gameIdeaRepository, managedMediaStorage, gameIdeaPdfExporter, null)
    {
    }

    public MainWindowViewModel(
        ICatalogRepository? catalogRepository,
        ICatalogFileReader? catalogFileReader,
        IGameIdeaRepository? gameIdeaRepository,
        IManagedMediaStorage? managedMediaStorage,
        IGameIdeaPdfExporter? gameIdeaPdfExporter,
        IGameIdeaWorkbookExporter? gameIdeaWorkbookExporter,
        ICatalogFileWriter? catalogFileWriter = null)
    {
        _catalogRepository = catalogRepository;
        _catalogFileReader = catalogFileReader;
        _gameIdeaRepository = gameIdeaRepository;
        _managedMediaStorage = managedMediaStorage;
        _gameIdeaPdfExporter = gameIdeaPdfExporter;
        _gameIdeaWorkbookExporter = gameIdeaWorkbookExporter;
        _catalogFileWriter = catalogFileWriter;
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
        CatalogOptionsView = CollectionViewSource.GetDefaultView(CatalogOptions);
        CatalogOptionsView.Filter = MatchesCatalogFilters;
        GenreChoices = [];
        Genres = [];
        AvailableSubgenres = [];
        References = [];
        DevelopmentDurationChoices = [];
        TeamSizeChoices = [];
        DevelopmentStageChoices = Enum.GetValues<GameIdeaStage>();
        GddSections = CreateDefaultGddSections();
        SavedIdeas = [];
        SavedIdeasView = CollectionViewSource.GetDefaultView(SavedIdeas);
        SavedIdeasView.Filter = MatchesIdeaPoolFilters;
        MediaAttachments = [];
        foreach (var section in GddSections)
        {
            section.PropertyChanged += GddSectionPropertyChanged;
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
        NavigateWizardCommand = new RelayCommand(_ => OpenCreateIdea());
        NavigatePoolCommand = new RelayCommand(_ => CurrentPage = AppPage.Pool);
        NavigateSettingsCommand = new RelayCommand(_ => CurrentPage = AppPage.Settings);
        AddCatalogOptionCommand = new AsyncRelayCommand(_ => AddCatalogOptionAsync());
        ToggleCatalogOptionCommand = new AsyncRelayCommand(ToggleCatalogOptionAsync);
        RenameCatalogOptionCommand = new AsyncRelayCommand(RenameCatalogOptionAsync);
        MoveCatalogOptionUpCommand = new AsyncRelayCommand(option => MoveCatalogOptionAsync(option, -1));
        MoveCatalogOptionDownCommand = new AsyncRelayCommand(option => MoveCatalogOptionAsync(option, 1));
        ClearCatalogFiltersCommand = new RelayCommand(_ => ClearCatalogFilters());
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
        SelectIdeaPoolFilterCommand = new RelayCommand(SelectIdeaPoolFilter);
        ClearIdeaPoolFiltersCommand = new RelayCommand(_ => ClearIdeaPoolFilters());
        WizardNextCommand = new AsyncRelayCommand(_ => MoveWizardNextAsync());
        WizardPreviousCommand = new RelayCommand(_ => MoveWizardPrevious());
    }

    public ObservableCollection<CatalogCategoryItemViewModel> CatalogCategories { get; }

    public ObservableCollection<CatalogOptionViewModel> Platforms { get; }

    public ObservableCollection<CatalogOptionViewModel> ActivePlatforms { get; }

    public ObservableCollection<CatalogOptionViewModel> CatalogOptions { get; }

    public ICollectionView CatalogOptionsView { get; }

    public IReadOnlyList<string> CatalogStatusFilters { get; } = ["All options", "Active", "Archived"];

    public ObservableCollection<CatalogOptionViewModel> GenreChoices { get; }

    public ObservableCollection<CatalogOptionViewModel> Genres { get; }

    public ObservableCollection<CatalogOptionViewModel> AvailableSubgenres { get; }

    public ObservableCollection<GameReferenceViewModel> References { get; }

    public ObservableCollection<CatalogOptionViewModel> DevelopmentDurationChoices { get; }

    public ObservableCollection<CatalogOptionViewModel> TeamSizeChoices { get; }

    public ObservableCollection<GddSectionDraftViewModel> GddSections { get; }

    public ObservableCollection<GameIdeaListItemViewModel> SavedIdeas { get; }

    public ICollectionView SavedIdeasView { get; }

    public IReadOnlyList<string> IdeaPoolStageFilters { get; } =
        ["All stages", "Idea", "Concept", "Prototyping", "Completed", "Shelved"];

    public GameIdeaListItemViewModel? SelectedSavedIdea
    {
        get => _selectedSavedIdea;
        set
        {
            if (SetProperty(ref _selectedSavedIdea, value))
            {
                _ideaPoolActionMessage = null;
                OnPropertyChanged(nameof(HasSelectedSavedIdea));
                NotifyIdeaPoolSelectionActionsChanged();
                OnPropertyChanged(nameof(IdeaPoolMessage));
            }
        }
    }

    public bool HasSelectedSavedIdea => SelectedSavedIdea is not null;

    public bool CanExportSelectedIdeaPdf => HasSelectedSavedIdea && !IsIdeaPoolActionRunning;

    public bool CanExportAllIdeasWorkbook => SavedIdeas.Count > 0 && !IsIdeaPoolActionRunning;

    public bool CanExportFilteredIdeasWorkbook => VisibleIdeaCount > 0 && !IsIdeaPoolActionRunning;

    public bool CanEditSelectedIdea => HasSelectedSavedIdea && !IsIdeaPoolActionRunning;

    public bool CanDeleteSelectedIdea => HasSelectedSavedIdea && !IsIdeaPoolActionRunning;

    private bool IsIdeaPoolActionRunning =>
        _isExportingPdf || _isExportingWorkbook || _isLoadingIdea || _isDeletingIdea;

    public ObservableCollection<DraftMediaAttachmentViewModel> MediaAttachments { get; }

    public IReadOnlyList<GameIdeaStage> DevelopmentStageChoices { get; }

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

    public ICommand RenameCatalogOptionCommand { get; }

    public ICommand MoveCatalogOptionUpCommand { get; }

    public ICommand MoveCatalogOptionDownCommand { get; }

    public ICommand ClearCatalogFiltersCommand { get; }

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

    public ICommand SelectIdeaPoolFilterCommand { get; }

    public ICommand ClearIdeaPoolFiltersCommand { get; }

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
            ClearCatalogFilters();
            OnPropertyChanged(nameof(CatalogHeading));
            OnPropertyChanged(nameof(AddCatalogButtonLabel));
            OnPropertyChanged(nameof(CatalogInputHint));
            OnPropertyChanged(nameof(CatalogExportDescription));
            OnPropertyChanged(nameof(CatalogExportFileName));
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
        set
        {
            if (SetProperty(ref _selectedParentGenre, value))
            {
                OnPropertyChanged(nameof(CatalogExportDescription));
                OnPropertyChanged(nameof(CatalogExportFileName));
            }
        }
    }

    public string NewCatalogOptionName
    {
        get => _newCatalogOptionName;
        set => SetProperty(ref _newCatalogOptionName, value);
    }

    public string CatalogSearchText
    {
        get => _catalogSearchText;
        set
        {
            if (SetProperty(ref _catalogSearchText, value))
            {
                RefreshCatalogOptionsView();
            }
        }
    }

    public string SelectedCatalogStatusFilter
    {
        get => _selectedCatalogStatusFilter;
        set
        {
            if (SetProperty(ref _selectedCatalogStatusFilter, value))
            {
                RefreshCatalogOptionsView();
            }
        }
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

    public GameIdeaStage SelectedDevelopmentStage
    {
        get => _selectedDevelopmentStage;
        set
        {
            if (SetProperty(ref _selectedDevelopmentStage, value))
            {
                OnPropertyChanged(nameof(WizardSelectionSummary));
            }
        }
    }

    public string IdeaPoolSearchText
    {
        get => _ideaPoolSearchText;
        set
        {
            if (SetProperty(ref _ideaPoolSearchText, value ?? string.Empty))
            {
                RefreshIdeaPoolFilters();
            }
        }
    }

    public string SelectedIdeaPoolStageFilter
    {
        get => _selectedIdeaPoolStageFilter;
        set
        {
            var normalizedValue = IdeaPoolStageFilters.Contains(value, StringComparer.Ordinal)
                ? value
                : "All stages";
            if (SetProperty(ref _selectedIdeaPoolStageFilter, normalizedValue))
            {
                RefreshIdeaPoolFilters();
            }
        }
    }

    public string SelectedIdeaPoolFilterLabel => _selectedIdeaPoolFilter == "All"
        ? "VIEW: All pools"
        : $"VIEW: {_selectedIdeaPoolFilter} pool";

    public bool IsPcIdeaPoolFilterSelected => _selectedIdeaPoolFilter == "PC";

    public bool IsMobileIdeaPoolFilterSelected => _selectedIdeaPoolFilter == "Mobile";

    public bool IsOtherIdeaPoolFilterSelected => _selectedIdeaPoolFilter == "Other";

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
        6 => "Name the idea, set its development stage and production scope, and write any GDD sections that help.",
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
        6 => $"Game: {NormalizeGameName(GameName)} · Stage: {SelectedDevelopmentStage} · GDD sections: {GddSections.Count(section => !string.IsNullOrWhiteSpace(section.Content))} · Media: {MediaAttachments.Count}",
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

    public int VisibleIdeaCount => SavedIdeasView.Cast<GameIdeaListItemViewModel>().Count();

    public string IdeaPoolMessage => _ideaPoolActionMessage ?? (SavedIdeas.Count == 0
        ? "No saved ideas yet. Complete the wizard to create the first one."
        : VisibleIdeaCount == SavedIdeas.Count
            ? $"{SavedIdeas.Count} saved idea(s), ordered by the most recent update."
            : $"Showing {VisibleIdeaCount} of {SavedIdeas.Count} saved idea(s).");

    public string CatalogHeading => $"Manage {SelectedCatalogCategory.DisplayName.ToLowerInvariant()}";

    public string AddCatalogButtonLabel => $"Add {SelectedCatalogCategory.SingularName}";

    public string CatalogInputHint => $"Enter a {SelectedCatalogCategory.SingularName.ToLowerInvariant()} name";

    public string CatalogExportDescription => SelectedCatalogCategory.Category == CatalogCategory.Subgenre
        ? SelectedParentGenre is null
            ? "Select a parent genre to import or export its subgenres."
            : $"Import and export operations apply to subgenres under {SelectedParentGenre.Name}."
        : "Exports contain active options in the order shown and can be imported again.";

    public string CatalogExportFileName
    {
        get
        {
            var categoryName = SelectedCatalogCategory.DisplayName.Replace(' ', '-').ToLowerInvariant();
            if (SelectedCatalogCategory.Category != CatalogCategory.Subgenre || SelectedParentGenre is null)
            {
                return $"game-design-wizard-{categoryName}.xlsx";
            }

            var parentName = new string(SelectedParentGenre.Name
                .Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-')
                .ToArray()).Trim('-');
            return $"game-design-wizard-{parentName}-subgenres.xlsx";
        }
    }

    public int VisibleCatalogOptionCount => CatalogOptionsView.Cast<CatalogOptionViewModel>().Count();

    public bool HasCatalogFilters => !string.IsNullOrWhiteSpace(CatalogSearchText)
        || SelectedCatalogStatusFilter != "All options";

    public string CatalogResultsMessage => HasCatalogFilters
        ? $"Showing {VisibleCatalogOptionCount} of {CatalogOptions.Count} option(s). Clear filters to change the saved order."
        : $"{CatalogOptions.Count} option(s) in saved order.";

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

    public async Task ExportSelectedCatalogAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (_catalogFileWriter is null || _catalogRepository is null)
        {
            throw new InvalidOperationException("Catalog export is not available in preview mode.");
        }

        var category = SelectedCatalogCategory.Category;
        var parentId = category == CatalogCategory.Subgenre ? SelectedParentGenre?.Id : null;
        if (category == CatalogCategory.Subgenre && parentId is null)
        {
            throw new InvalidOperationException("Select a parent genre before exporting subgenres.");
        }

        var options = await _catalogRepository.GetOptionsAsync(category, cancellationToken);
        var exportOptions = options
            .Where(option => option.IsActive && (category != CatalogCategory.Subgenre || option.ParentOptionId == parentId))
            .ToArray();
        await _catalogFileWriter.WriteAsync(filePath, category, exportOptions, cancellationToken);
        SettingsMessage = $"{exportOptions.Length} active option(s) exported to {Path.GetFileName(filePath)}.";
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

    private void OpenCreateIdea()
    {
        if (_hasSavedIdea)
        {
            ResetDraft();
        }

        CurrentPage = AppPage.Wizard;
        WizardMessage = "Select a platform or leave the step empty.";
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
            var selectedParentGenreId = SelectedParentGenre?.Id;
            IReadOnlyList<CatalogOption> genres = [];
            if (category == CatalogCategory.Subgenre)
            {
                genres = await _catalogRepository.GetOptionsAsync(CatalogCategory.Genre, cancellationToken);
                GenreChoices.Clear();
                foreach (var genre in genres.Where(option => option.IsActive))
                {
                    GenreChoices.Add(ToViewModel(genre));
                }

                SelectedParentGenre = GenreChoices.FirstOrDefault(genre => genre.Id == selectedParentGenreId)
                    ?? GenreChoices.FirstOrDefault();
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

            RefreshCatalogOptionsView();

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
            RefreshCatalogOptionsView();
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
            RefreshCatalogOptionsView();
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

    private async Task RenameCatalogOptionAsync(object? parameter)
    {
        if (parameter is not CatalogOptionViewModel option)
        {
            return;
        }

        var oldName = option.Name;
        try
        {
            var name = CleanPreviewName(option.EditName);
            if (_catalogRepository is null)
            {
                if (CatalogOptions.Any(candidate => candidate.Id != option.Id
                    && string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"{name} already exists in this catalog.");
                }

                option.AcceptName(name);
            }
            else
            {
                var savedOption = await _catalogRepository.RenameOptionAsync(option.Id, name);
                option.AcceptName(savedOption.NameEnglish);
            }

            RefreshCatalogOptionsView();

            if (option.Category == CatalogCategory.Platform)
            {
                if (_catalogRepository is null)
                {
                    Platforms.Single(candidate => candidate.Id == option.Id).AcceptName(option.Name);
                    RebuildActivePlatforms();
                }
                else
                {
                    await LoadPlatformsAsync();
                }
            }
            else if (_catalogRepository is not null && IsWizardCatalog(option.Category))
            {
                await LoadWizardCatalogsAsync();
            }

            SettingsMessage = oldName == option.Name
                ? $"{option.Name} is already up to date."
                : $"{oldName} was renamed to {option.Name}. Stable catalog links were preserved.";
        }
        catch (InvalidOperationException exception)
        {
            option.EditName = option.Name;
            SettingsMessage = exception.Message;
        }
        catch (Exception)
        {
            option.EditName = option.Name;
            SettingsMessage = "The catalog name could not be saved. Try again.";
        }
    }

    private async Task MoveCatalogOptionAsync(object? parameter, int offset)
    {
        if (parameter is not CatalogOptionViewModel option)
        {
            return;
        }

        if (HasCatalogFilters)
        {
            SettingsMessage = "Clear catalog filters before changing the saved order.";
            return;
        }

        try
        {
            if (_catalogRepository is null)
            {
                var siblings = option.Category == CatalogCategory.Subgenre
                    ? CatalogOptions.Where(candidate => candidate.ParentOptionId == option.ParentOptionId).ToList()
                    : CatalogOptions.ToList();
                var siblingIndex = siblings.IndexOf(option);
                var targetSiblingIndex = siblingIndex + offset;
                if (siblingIndex < 0 || targetSiblingIndex < 0 || targetSiblingIndex >= siblings.Count)
                {
                    return;
                }

                var targetIndex = CatalogOptions.IndexOf(siblings[targetSiblingIndex]);
                CatalogOptions.Move(CatalogOptions.IndexOf(option), targetIndex);
                RefreshCatalogMoveAvailability();
            }
            else
            {
                await _catalogRepository.MoveOptionAsync(option.Id, offset);
                await LoadSelectedCatalogAsync();
                if (option.Category == CatalogCategory.Platform)
                {
                    await LoadPlatformsAsync();
                }
                else if (IsWizardCatalog(option.Category))
                {
                    await LoadWizardCatalogsAsync();
                }
            }

            SettingsMessage = $"{option.Name} moved {(offset < 0 ? "up" : "down")}. The new order was saved.";
        }
        catch (InvalidOperationException exception)
        {
            SettingsMessage = exception.Message;
        }
        catch (Exception)
        {
            SettingsMessage = "The catalog order could not be saved. Try again.";
        }
    }

    private void RefreshCatalogMoveAvailability()
    {
        foreach (var option in CatalogOptions)
        {
            option.CanMoveUp = false;
            option.CanMoveDown = false;
        }

        if (HasCatalogFilters)
        {
            return;
        }

        IEnumerable<List<CatalogOptionViewModel>> groups = SelectedCatalogCategory.Category == CatalogCategory.Subgenre
            ? CatalogOptions.GroupBy(option => option.ParentOptionId).Select(group => group.ToList())
            : [CatalogOptions.ToList()];
        foreach (var group in groups)
        {
            for (var index = 0; index < group.Count; index++)
            {
                group[index].CanMoveUp = index > 0;
                group[index].CanMoveDown = index < group.Count - 1;
            }
        }
    }

    private bool MatchesCatalogFilters(object item)
    {
        if (item is not CatalogOptionViewModel option)
        {
            return false;
        }

        if (SelectedCatalogStatusFilter == "Active" && !option.IsActive)
        {
            return false;
        }

        if (SelectedCatalogStatusFilter == "Archived" && option.IsActive)
        {
            return false;
        }

        var searchText = CatalogSearchText.Trim();
        return searchText.Length == 0
            || option.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || (option.ParentName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private void ClearCatalogFilters()
    {
        var changed = false;
        if (_catalogSearchText.Length > 0)
        {
            _catalogSearchText = string.Empty;
            OnPropertyChanged(nameof(CatalogSearchText));
            changed = true;
        }

        if (_selectedCatalogStatusFilter != "All options")
        {
            _selectedCatalogStatusFilter = "All options";
            OnPropertyChanged(nameof(SelectedCatalogStatusFilter));
            changed = true;
        }

        if (changed)
        {
            RefreshCatalogOptionsView();
        }
    }

    private void RefreshCatalogOptionsView()
    {
        CatalogOptionsView.Refresh();
        RefreshCatalogMoveAvailability();
        OnPropertyChanged(nameof(VisibleCatalogOptionCount));
        OnPropertyChanged(nameof(HasCatalogFilters));
        OnPropertyChanged(nameof(CatalogResultsMessage));
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
                Stage = SelectedDevelopmentStage,
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

    public async Task LoadSelectedIdeaForEditingAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedSavedIdea is not { } selectedIdea)
        {
            SetIdeaPoolActionMessage("Select an idea before editing it.");
            return;
        }

        if (_gameIdeaRepository is null)
        {
            SetIdeaPoolActionMessage("Idea editing is unavailable in preview mode.");
            return;
        }

        if (IsIdeaPoolActionRunning)
        {
            return;
        }

        _isLoadingIdea = true;
        NotifyIdeaPoolSelectionActionsChanged();
        try
        {
            var idea = await _gameIdeaRepository.GetByIdAsync(selectedIdea.Id, cancellationToken)
                ?? throw new InvalidOperationException("The selected idea could not be found.");
            LoadIdeaIntoDraft(idea);
            _wizardStep = 1;
            CurrentPage = AppPage.Wizard;
            NotifyWizardStepChanged();
            WizardMessage = $"{idea.NameEnglish} is open for editing. Saving will update the existing idea.";
        }
        catch (OperationCanceledException)
        {
            SetIdeaPoolActionMessage("Opening the idea was canceled.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException)
        {
            SetIdeaPoolActionMessage(exception.Message);
        }
        catch (Exception)
        {
            SetIdeaPoolActionMessage("The selected idea could not be opened. Try again.");
        }
        finally
        {
            _isLoadingIdea = false;
            NotifyIdeaPoolSelectionActionsChanged();
        }
    }

    public async Task DeleteSelectedIdeaAsync(CancellationToken cancellationToken = default)
    {
        if (SelectedSavedIdea is not { } selectedIdea)
        {
            SetIdeaPoolActionMessage("Select an idea before deleting it.");
            return;
        }

        if (_gameIdeaRepository is null)
        {
            SetIdeaPoolActionMessage("Idea deletion is unavailable in preview mode.");
            return;
        }

        if (IsIdeaPoolActionRunning)
        {
            return;
        }

        _isDeletingIdea = true;
        NotifyIdeaPoolSelectionActionsChanged();
        var ideaDeleted = false;
        try
        {
            var idea = await _gameIdeaRepository.GetByIdAsync(selectedIdea.Id, cancellationToken)
                ?? throw new InvalidOperationException("The selected idea could not be found.");
            if (!await _gameIdeaRepository.DeleteAsync(idea.Id, cancellationToken))
            {
                throw new InvalidOperationException("The selected idea was already deleted.");
            }
            ideaDeleted = true;

            var mediaCleanupFailures = 0;
            if (_managedMediaStorage is not null)
            {
                foreach (var storedPath in idea.MediaAttachments
                             .Select(attachment => attachment.StoredRelativePath)
                             .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    try
                    {
                        await _managedMediaStorage.DeleteAsync(storedPath, CancellationToken.None);
                    }
                    catch
                    {
                        mediaCleanupFailures++;
                    }
                }
            }

            if (_draftIdeaId == idea.Id)
            {
                ResetDraft();
            }

            await LoadIdeaPoolAsync(CancellationToken.None);
            SetIdeaPoolActionMessage(mediaCleanupFailures == 0
                ? $"{idea.NameEnglish} was deleted."
                : $"{idea.NameEnglish} was deleted, but {mediaCleanupFailures} managed media file(s) could not be removed.");
        }
        catch (OperationCanceledException) when (!ideaDeleted)
        {
            SetIdeaPoolActionMessage("Idea deletion was canceled.");
        }
        catch (Exception) when (ideaDeleted)
        {
            SetIdeaPoolActionMessage($"{selectedIdea.Name} was deleted, but the idea pool could not be refreshed.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            SetIdeaPoolActionMessage(exception.Message);
        }
        catch (Exception)
        {
            SetIdeaPoolActionMessage("The selected idea could not be deleted. Try again.");
        }
        finally
        {
            _isDeletingIdea = false;
            NotifyIdeaPoolSelectionActionsChanged();
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

        if (IsIdeaPoolActionRunning)
        {
            return;
        }

        _isExportingPdf = true;
        NotifyIdeaPoolSelectionActionsChanged();
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
            NotifyIdeaPoolSelectionActionsChanged();
        }
    }

    public Task ExportAllIdeasWorkbookAsync(
        string destinationPath,
        CancellationToken cancellationToken = default) =>
        ExportIdeasWorkbookAsync(destinationPath, null, cancellationToken);

    public Task ExportFilteredIdeasWorkbookAsync(
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var visibleIdeaIds = SavedIdeasView
            .Cast<GameIdeaListItemViewModel>()
            .Select(idea => idea.Id)
            .ToArray();
        return ExportIdeasWorkbookAsync(destinationPath, visibleIdeaIds, cancellationToken);
    }

    private async Task ExportIdeasWorkbookAsync(
        string destinationPath,
        IReadOnlyList<Guid>? includedIdeaIds,
        CancellationToken cancellationToken)
    {
        if (_gameIdeaRepository is null || _gameIdeaWorkbookExporter is null)
        {
            SetIdeaPoolActionMessage("Workbook export is unavailable in preview mode.");
            return;
        }

        if (IsIdeaPoolActionRunning)
        {
            return;
        }

        _isExportingWorkbook = true;
        NotifyIdeaPoolSelectionActionsChanged();
        try
        {
            var allIdeas = await _gameIdeaRepository.GetAllAsync(cancellationToken);
            IReadOnlyList<GameIdeaDocument> ideas = allIdeas;
            if (includedIdeaIds is not null)
            {
                var ideasById = allIdeas.ToDictionary(idea => idea.Id);
                ideas = includedIdeaIds
                    .Select(id => ideasById.GetValueOrDefault(id))
                    .OfType<GameIdeaDocument>()
                    .ToArray();
            }

            if (ideas.Count == 0)
            {
                SetIdeaPoolActionMessage(includedIdeaIds is null
                    ? "Save at least one idea before exporting the workbook."
                    : "No ideas match the current filters.");
                return;
            }

            await _gameIdeaWorkbookExporter.ExportAsync(ideas, destinationPath, cancellationToken);
            SetIdeaPoolActionMessage($"{ideas.Count} idea(s) were exported to {Path.GetFileName(destinationPath)}.");
        }
        catch (OperationCanceledException)
        {
            SetIdeaPoolActionMessage("Workbook export was canceled.");
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            SetIdeaPoolActionMessage(exception.Message);
        }
        catch (Exception)
        {
            SetIdeaPoolActionMessage("The workbook could not be exported. Try another location.");
        }
        finally
        {
            _isExportingWorkbook = false;
            NotifyIdeaPoolSelectionActionsChanged();
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
        SavedIdeasView.Refresh();
        if (SelectedSavedIdea is not null && !SavedIdeasView.Contains(SelectedSavedIdea))
        {
            SelectedSavedIdea = null;
        }

        OnPropertyChanged(nameof(PcIdeaCount));
        OnPropertyChanged(nameof(MobileIdeaCount));
        OnPropertyChanged(nameof(OtherIdeaCount));
        OnPropertyChanged(nameof(VisibleIdeaCount));
        OnPropertyChanged(nameof(CanExportAllIdeasWorkbook));
        OnPropertyChanged(nameof(CanExportFilteredIdeasWorkbook));
        OnPropertyChanged(nameof(IdeaPoolMessage));
    }

    private void SetIdeaPoolActionMessage(string message)
    {
        _ideaPoolActionMessage = message;
        OnPropertyChanged(nameof(IdeaPoolMessage));
    }

    private void NotifyIdeaPoolSelectionActionsChanged()
    {
        OnPropertyChanged(nameof(CanExportSelectedIdeaPdf));
        OnPropertyChanged(nameof(CanExportAllIdeasWorkbook));
        OnPropertyChanged(nameof(CanExportFilteredIdeasWorkbook));
        OnPropertyChanged(nameof(CanEditSelectedIdea));
        OnPropertyChanged(nameof(CanDeleteSelectedIdea));
    }

    private void SelectIdeaPoolFilter(object? parameter)
    {
        var filter = parameter as string;
        if (filter is not ("All" or "PC" or "Mobile" or "Other")
            || string.Equals(_selectedIdeaPoolFilter, filter, StringComparison.Ordinal))
        {
            return;
        }

        _selectedIdeaPoolFilter = filter;
        OnPropertyChanged(nameof(SelectedIdeaPoolFilterLabel));
        OnPropertyChanged(nameof(IsPcIdeaPoolFilterSelected));
        OnPropertyChanged(nameof(IsMobileIdeaPoolFilterSelected));
        OnPropertyChanged(nameof(IsOtherIdeaPoolFilterSelected));
        RefreshIdeaPoolFilters();
    }

    private void ClearIdeaPoolFilters()
    {
        var poolChanged = _selectedIdeaPoolFilter != "All";
        _selectedIdeaPoolFilter = "All";
        _ideaPoolSearchText = string.Empty;
        _selectedIdeaPoolStageFilter = "All stages";
        OnPropertyChanged(nameof(IdeaPoolSearchText));
        OnPropertyChanged(nameof(SelectedIdeaPoolStageFilter));
        OnPropertyChanged(nameof(SelectedIdeaPoolFilterLabel));
        if (poolChanged)
        {
            OnPropertyChanged(nameof(IsPcIdeaPoolFilterSelected));
            OnPropertyChanged(nameof(IsMobileIdeaPoolFilterSelected));
            OnPropertyChanged(nameof(IsOtherIdeaPoolFilterSelected));
        }

        RefreshIdeaPoolFilters();
    }

    private void RefreshIdeaPoolFilters()
    {
        _ideaPoolActionMessage = null;
        SavedIdeasView.Refresh();
        if (SelectedSavedIdea is not null && !SavedIdeasView.Contains(SelectedSavedIdea))
        {
            SelectedSavedIdea = null;
        }

        OnPropertyChanged(nameof(VisibleIdeaCount));
        OnPropertyChanged(nameof(CanExportFilteredIdeasWorkbook));
        OnPropertyChanged(nameof(IdeaPoolMessage));
    }

    private bool MatchesIdeaPoolFilters(object candidate)
    {
        if (candidate is not GameIdeaListItemViewModel idea)
        {
            return false;
        }

        if (_selectedIdeaPoolFilter != "All"
            && !string.Equals(idea.PoolName, _selectedIdeaPoolFilter, StringComparison.Ordinal))
        {
            return false;
        }

        if (_selectedIdeaPoolStageFilter != "All stages"
            && !string.Equals(idea.Stage, _selectedIdeaPoolStageFilter, StringComparison.Ordinal))
        {
            return false;
        }

        var search = _ideaPoolSearchText.Trim();
        return search.Length == 0
            || idea.SearchIndex.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private void LoadIdeaIntoDraft(GameIdeaDocument idea)
    {
        _draftIdeaId = idea.Id;
        _draftCreatedAtUtc = idea.CreatedAtUtc;
        _hasSavedIdea = true;
        _removedManagedMediaPaths.Clear();

        GameName = idea.NameEnglish;
        OverviewEnglish = idea.OverviewEnglish;
        SelectedDevelopmentStage = idea.Stage;

        _selectedPlatform = RestoreSingleChoice(
            ActivePlatforms,
            idea.Platform,
            CatalogCategory.Platform,
            platformPoolGroup: idea.PoolGroup);
        SelectedPlatformName = _selectedPlatform?.Name ?? "None";

        SelectedGenre = RestoreSingleChoice(Genres, idea.Genre, CatalogCategory.Genre);
        SelectedSubgenre = RestoreSingleChoice(
            _allSubgenres,
            idea.Subgenre,
            CatalogCategory.Subgenre,
            parentOptionId: SelectedGenre?.Id);
        RefreshAvailableSubgenres();

        TopicPicker.RestoreSelections(idea.Topics);
        FeaturePicker.RestoreSelections(idea.Features);
        ArtStylePicker.RestoreSelections(idea.ArtStyles);
        MechanicsPicker.RestoreSelections(idea.Mechanics);
        SelectedDevelopmentDuration = RestoreSingleChoice(
            DevelopmentDurationChoices,
            idea.DevelopmentDuration,
            CatalogCategory.DevelopmentDuration);
        SelectedTeamSize = RestoreSingleChoice(
            TeamSizeChoices,
            idea.TeamSize,
            CatalogCategory.TeamSize);

        ReplaceReferences(idea.References);
        MediaAttachments.Clear();
        foreach (var attachment in idea.MediaAttachments.OrderBy(attachment => attachment.SortOrder))
        {
            var sourcePath = _managedMediaStorage?.GetFullPath(attachment.StoredRelativePath)
                ?? attachment.StoredRelativePath;
            MediaAttachments.Add(new DraftMediaAttachmentViewModel(
                sourcePath,
                attachment.FileName,
                attachment.MediaType,
                attachment.Id,
                attachment.StoredRelativePath,
                attachment.CaptionEnglish));
        }

        var defaultGuidance = CreateDefaultGddSections()
            .ToDictionary(section => section.Title, section => section.Guidance, StringComparer.OrdinalIgnoreCase);
        var sections = idea.Sections.Count == 0
            ? CreateDefaultGddSections()
            : new ObservableCollection<GddSectionDraftViewModel>(idea.Sections
                .OrderBy(section => section.SortOrder)
                .Select(section => new GddSectionDraftViewModel(
                    section.TitleEnglish,
                    defaultGuidance.GetValueOrDefault(
                        section.TitleEnglish,
                        "Describe the decisions and constraints for this section."),
                    section.SortOrder,
                    section.Id,
                    section.ContentEnglish)));
        ReplaceGddSections(sections);

        OnPropertyChanged(nameof(WizardNextLabel));
        OnPropertyChanged(nameof(WizardSelectionSummary));
    }

    private void ResetDraft()
    {
        _draftIdeaId = Guid.NewGuid();
        _draftCreatedAtUtc = DateTime.UtcNow;
        _hasSavedIdea = false;
        _removedManagedMediaPaths.Clear();
        GameName = "NewGame";
        OverviewEnglish = string.Empty;
        SelectedDevelopmentStage = GameIdeaStage.Idea;
        _selectedPlatform = RestoreSingleChoice(
            ActivePlatforms,
            null,
            CatalogCategory.Platform);
        SelectedPlatformName = "None";
        SelectedGenre = RestoreSingleChoice(Genres, null, CatalogCategory.Genre);
        SelectedSubgenre = RestoreSingleChoice(_allSubgenres, null, CatalogCategory.Subgenre);
        RefreshAvailableSubgenres();
        TopicPicker.RestoreSelections([]);
        FeaturePicker.RestoreSelections([]);
        ArtStylePicker.RestoreSelections([]);
        MechanicsPicker.RestoreSelections([]);
        SelectedDevelopmentDuration = RestoreSingleChoice(
            DevelopmentDurationChoices,
            null,
            CatalogCategory.DevelopmentDuration);
        SelectedTeamSize = RestoreSingleChoice(
            TeamSizeChoices,
            null,
            CatalogCategory.TeamSize);
        ReplaceReferences([]);
        MediaAttachments.Clear();
        ReplaceGddSections(CreateDefaultGddSections());
        _wizardStep = 1;
        NotifyWizardStepChanged();
    }

    private void ReplaceReferences(IEnumerable<GameReferenceContent> references)
    {
        foreach (var reference in References)
        {
            reference.Changed -= ReferenceChanged;
        }

        References.Clear();
        foreach (var reference in references.OrderBy(reference => reference.SortOrder))
        {
            var viewModel = new GameReferenceViewModel(
                reference.Id,
                reference.Url ?? string.Empty,
                reference.NoteEnglish);
            viewModel.Changed += ReferenceChanged;
            References.Add(viewModel);
        }

        if (References.Count == 0)
        {
            var blankReference = new GameReferenceViewModel();
            blankReference.Changed += ReferenceChanged;
            References.Add(blankReference);
        }
    }

    private void ReplaceGddSections(IEnumerable<GddSectionDraftViewModel> sections)
    {
        foreach (var section in GddSections)
        {
            section.PropertyChanged -= GddSectionPropertyChanged;
        }

        GddSections.Clear();
        foreach (var section in sections)
        {
            section.PropertyChanged += GddSectionPropertyChanged;
            GddSections.Add(section);
        }
    }

    private void GddSectionPropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(GddSectionDraftViewModel.Content))
        {
            OnPropertyChanged(nameof(WizardSelectionSummary));
        }
    }

    private static CatalogOptionViewModel? RestoreSingleChoice(
        IList<CatalogOptionViewModel> choices,
        CatalogSelectionSnapshot? snapshot,
        CatalogCategory category,
        Guid? parentOptionId = null,
        PlatformPoolGroup platformPoolGroup = PlatformPoolGroup.Other)
    {
        for (var index = choices.Count - 1; index >= 0; index--)
        {
            choices[index].IsSelected = false;
            if (!choices[index].IsActive)
            {
                choices.RemoveAt(index);
            }
        }

        if (snapshot is null)
        {
            return null;
        }

        var selected = choices.SingleOrDefault(option => option.Id == snapshot.OptionId);
        if (selected is null)
        {
            selected = new CatalogOptionViewModel(
                snapshot.OptionId,
                category,
                snapshot.NameEnglish,
                isBuiltIn: false,
                isActive: false,
                parentOptionId: parentOptionId,
                platformPoolGroup: platformPoolGroup);
            choices.Add(selected);
        }

        selected.IsSelected = true;
        return selected;
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
            throw new InvalidOperationException("Enter an option name.");
        }

        if (cleaned.Length > 200)
        {
            throw new InvalidOperationException("Option names must contain 200 characters or fewer.");
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
