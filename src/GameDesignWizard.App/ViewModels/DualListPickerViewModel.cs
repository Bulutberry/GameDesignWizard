using System.Collections.ObjectModel;
using System.Windows.Input;
using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.App.ViewModels;

public sealed class DualListPickerViewModel : ObservableObject
{
    private readonly List<CatalogOptionViewModel> _allOptions = [];
    private string _searchText = string.Empty;

    public DualListPickerViewModel(string title, CatalogCategory category)
    {
        Title = title;
        Category = category;
        AvailableOptions = [];
        SelectedOptions = [];
        AddCommand = new RelayCommand(Add);
        RemoveCommand = new RelayCommand(Remove);
    }

    public event EventHandler? SelectionChanged;

    public string Title { get; }

    public CatalogCategory Category { get; }

    public string SearchHint => $"Search available {Title.ToLowerInvariant()}";

    public ObservableCollection<CatalogOptionViewModel> AvailableOptions { get; }

    public ObservableCollection<CatalogOptionViewModel> SelectedOptions { get; }

    public ICommand AddCommand { get; }

    public ICommand RemoveCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshAvailableOptions();
            }
        }
    }

    public void LoadOptions(IEnumerable<CatalogOptionViewModel> options)
    {
        var selectedIds = SelectedOptions.Select(option => option.Id).ToHashSet();
        _allOptions.Clear();
        _allOptions.AddRange(options.Where(option => option.IsActive));

        SelectedOptions.Clear();
        foreach (var option in _allOptions.Where(option => selectedIds.Contains(option.Id)))
        {
            option.IsSelected = true;
            SelectedOptions.Add(option);
        }

        RefreshAvailableOptions();
    }

    private void Add(object? parameter)
    {
        if (parameter is not CatalogOptionViewModel option
            || option.Category != Category
            || SelectedOptions.Any(selected => selected.Id == option.Id))
        {
            return;
        }

        option.IsSelected = true;
        SelectedOptions.Add(option);
        RefreshAvailableOptions();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Remove(object? parameter)
    {
        if (parameter is not CatalogOptionViewModel option
            || option.Category != Category)
        {
            return;
        }

        option.IsSelected = false;
        SelectedOptions.Remove(option);
        RefreshAvailableOptions();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshAvailableOptions()
    {
        AvailableOptions.Clear();
        var selectedIds = SelectedOptions.Select(option => option.Id).ToHashSet();
        foreach (var option in _allOptions.Where(option =>
                     !selectedIds.Contains(option.Id)
                     && (SearchText.Length == 0
                         || option.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))))
        {
            AvailableOptions.Add(option);
        }
    }
}
