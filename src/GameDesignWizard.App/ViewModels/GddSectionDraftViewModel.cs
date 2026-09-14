namespace GameDesignWizard.App.ViewModels;

public sealed class GddSectionDraftViewModel(
    string title,
    string guidance,
    int sortOrder,
    Guid? id = null,
    string content = "") : ObservableObject
{
    private string? _acceptedAiSource;
    private string _aiMessage = "Local AI changes are shown here before they can replace your text.";
    private string _aiProposal = string.Empty;
    private string _content = content;
    private bool _isAiGenerating;

    public Guid Id { get; } = id ?? Guid.NewGuid();

    public string Title { get; } = title;

    public string Guidance { get; } = guidance;

    public int SortOrder { get; } = sortOrder;

    public string Content
    {
        get => _content;
        set
        {
            if (SetProperty(ref _content, value))
            {
                OnPropertyChanged(nameof(CanRequestAiImprovement));
            }
        }
    }

    public string AiProposal
    {
        get => _aiProposal;
        private set
        {
            if (SetProperty(ref _aiProposal, value))
            {
                OnPropertyChanged(nameof(HasAiProposal));
            }
        }
    }

    public string AiMessage
    {
        get => _aiMessage;
        set => SetProperty(ref _aiMessage, value);
    }

    public bool IsAiGenerating
    {
        get => _isAiGenerating;
        set
        {
            if (SetProperty(ref _isAiGenerating, value))
            {
                OnPropertyChanged(nameof(CanRequestAiImprovement));
            }
        }
    }

    public bool HasAiProposal => !string.IsNullOrWhiteSpace(AiProposal);

    public bool CanRequestAiImprovement => !IsAiGenerating && !string.IsNullOrWhiteSpace(Content);

    public bool CanUndoAiChange => _acceptedAiSource is not null;

    public void ShowAiProposal(string proposal, string message)
    {
        AiProposal = proposal;
        AiMessage = message;
    }

    public void AcceptAiProposal()
    {
        if (!HasAiProposal)
        {
            return;
        }

        _acceptedAiSource = Content;
        Content = AiProposal;
        AiProposal = string.Empty;
        AiMessage = "The proposal was inserted. Save the idea when you are ready.";
        OnPropertyChanged(nameof(CanUndoAiChange));
    }

    public void DiscardAiProposal()
    {
        AiProposal = string.Empty;
        AiMessage = "The proposal was discarded. Your original text is unchanged.";
    }

    public void UndoAcceptedAiChange()
    {
        if (_acceptedAiSource is null)
        {
            return;
        }

        Content = _acceptedAiSource;
        _acceptedAiSource = null;
        AiMessage = "The text from before the accepted AI change was restored.";
        OnPropertyChanged(nameof(CanUndoAiChange));
    }
}
