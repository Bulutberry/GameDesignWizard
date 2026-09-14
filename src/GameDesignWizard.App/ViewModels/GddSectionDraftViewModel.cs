namespace GameDesignWizard.App.ViewModels;

public sealed class GddSectionDraftViewModel(
    string title,
    string guidance,
    int sortOrder,
    Guid? id = null,
    string content = "") : ObservableObject
{
    private string _content = content;

    public Guid Id { get; } = id ?? Guid.NewGuid();

    public string Title { get; } = title;

    public string Guidance { get; } = guidance;

    public int SortOrder { get; } = sortOrder;

    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}
