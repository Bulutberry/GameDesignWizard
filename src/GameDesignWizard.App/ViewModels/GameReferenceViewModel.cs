namespace GameDesignWizard.App.ViewModels;

public sealed class GameReferenceViewModel : ObservableObject
{
    private string _url = string.Empty;
    private string _note = string.Empty;
    private string _validationMessage = string.Empty;

    public GameReferenceViewModel(
        Guid? id = null,
        string url = "",
        string note = "")
    {
        Id = id ?? Guid.NewGuid();
        _url = url;
        _note = note;
        ValidateUrl();
    }

    public event EventHandler? Changed;

    public Guid Id { get; }

    public string Url
    {
        get => _url;
        set
        {
            if (!SetProperty(ref _url, value))
            {
                return;
            }

            ValidateUrl();
            OnPropertyChanged(nameof(IsBlank));
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public string Note
    {
        get => _note;
        set
        {
            if (!SetProperty(ref _note, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsBlank));
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set
        {
            if (SetProperty(ref _validationMessage, value))
            {
                OnPropertyChanged(nameof(HasValidationError));
            }
        }
    }

    public bool HasValidationError => ValidationMessage.Length > 0;

    public bool IsBlank => string.IsNullOrWhiteSpace(Url) && string.IsNullOrWhiteSpace(Note);

    private void ValidateUrl()
    {
        var value = Url.Trim();
        if (value.Length == 0)
        {
            ValidationMessage = string.Empty;
            return;
        }

        var isValid = Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && uri is not null
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            && !string.IsNullOrWhiteSpace(uri.Host);
        ValidationMessage = isValid
            ? string.Empty
            : "Enter a complete http:// or https:// URL.";
    }
}
