using GameDesignWizard.Core.Ideas;

namespace GameDesignWizard.App.ViewModels;

public sealed class DraftMediaAttachmentViewModel(
    string sourcePath,
    string fileName,
    MediaAttachmentType mediaType) : ObservableObject
{
    private string _caption = string.Empty;
    private string? _storedRelativePath;

    public Guid Id { get; } = Guid.NewGuid();

    public string SourcePath { get; } = sourcePath;

    public string FileName { get; } = fileName;

    public MediaAttachmentType MediaType { get; } = mediaType;

    public string TypeLabel => MediaType == MediaAttachmentType.Image ? "IMAGE" : "AUDIO";

    public string Caption
    {
        get => _caption;
        set => SetProperty(ref _caption, value);
    }

    public string? StoredRelativePath
    {
        get => _storedRelativePath;
        private set
        {
            if (SetProperty(ref _storedRelativePath, value))
            {
                OnPropertyChanged(nameof(StorageState));
            }
        }
    }

    public string StorageState => StoredRelativePath is null ? "Ready to copy on save" : "Managed copy saved";

    public void MarkManaged(string storedRelativePath) => StoredRelativePath = storedRelativePath;

    public void MarkPending() => StoredRelativePath = null;
}
