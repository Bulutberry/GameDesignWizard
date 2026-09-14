using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.App.ViewModels;

public sealed class CatalogOptionViewModel : ObservableObject
{
    private bool _isActive;
    private bool _isSelected;

    public CatalogOptionViewModel(
        Guid id,
        CatalogCategory category,
        string name,
        bool isBuiltIn,
        bool isActive = true,
        Guid? parentOptionId = null,
        string? parentName = null,
        PlatformPoolGroup platformPoolGroup = PlatformPoolGroup.Other)
    {
        Id = id;
        Category = category;
        Name = name;
        IsBuiltIn = isBuiltIn;
        _isActive = isActive;
        ParentOptionId = parentOptionId;
        ParentName = parentName;
        PlatformPoolGroup = platformPoolGroup;
    }

    public Guid Id { get; }

    public CatalogCategory Category { get; }

    public string Name { get; }

    public bool IsBuiltIn { get; }

    public Guid? ParentOptionId { get; }

    public string? ParentName { get; }

    public PlatformPoolGroup PlatformPoolGroup { get; }

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
            OnPropertyChanged(nameof(SelectionName));
        }
    }

    public string SelectionName => IsActive ? Name : $"{Name} (Archived)";

    public string StateLabel => IsActive ? "ACTIVE" : "ARCHIVED";

    public string ToggleLabel => IsActive ? "Archive" : "Restore";

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
