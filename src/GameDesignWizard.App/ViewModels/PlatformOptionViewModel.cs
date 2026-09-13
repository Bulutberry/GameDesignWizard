namespace GameDesignWizard.App.ViewModels;

public sealed class PlatformOptionViewModel : ObservableObject
{
    private bool _isActive = true;

    public PlatformOptionViewModel(Guid id, string name, bool isBuiltIn, bool isActive = true)
    {
        Id = id;
        Name = name;
        IsBuiltIn = isBuiltIn;
        _isActive = isActive;
    }

    public Guid Id { get; }

    public string Name { get; }

    public bool IsBuiltIn { get; }

    public string Badge
    {
        get
        {
            var words = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(words.Take(2).Select(word => char.ToUpperInvariant(word[0])));
        }
    }

    public string OriginLabel => IsBuiltIn ? "Built-in platform" : "Custom platform · routed to Other";

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
