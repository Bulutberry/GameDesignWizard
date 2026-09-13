using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.App.ViewModels;

public sealed class CatalogOptionViewModel : ObservableObject
{
    private bool _isActive;

    public CatalogOptionViewModel(
        Guid id,
        CatalogCategory category,
        string name,
        bool isBuiltIn,
        bool isActive = true,
        Guid? parentOptionId = null,
        string? parentName = null)
    {
        Id = id;
        Category = category;
        Name = name;
        IsBuiltIn = isBuiltIn;
        _isActive = isActive;
        ParentOptionId = parentOptionId;
        ParentName = parentName;
    }

    public Guid Id { get; }

    public CatalogCategory Category { get; }

    public string Name { get; }

    public bool IsBuiltIn { get; }

    public Guid? ParentOptionId { get; }

    public string? ParentName { get; }

    public string Badge
    {
        get
        {
            var words = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(words.Take(2).Select(word => char.ToUpperInvariant(word[0])));
        }
    }

    public string OriginLabel
    {
        get
        {
            var origin = IsBuiltIn ? "Built-in option" : "Custom option";
            if (Category == CatalogCategory.Platform && !IsBuiltIn)
            {
                return $"{origin} · routed to Other";
            }

            return ParentName is null ? origin : $"{origin} · {ParentName}";
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (!SetProperty(ref _isActive, value))
            {
                return;
            }

            OnPropertyChanged(nameof(StateLabel));
            OnPropertyChanged(nameof(ToggleLabel));
        }
    }

    public string StateLabel => IsActive ? "ACTIVE" : "ARCHIVED";

    public string ToggleLabel => IsActive ? "Archive" : "Restore";
}
