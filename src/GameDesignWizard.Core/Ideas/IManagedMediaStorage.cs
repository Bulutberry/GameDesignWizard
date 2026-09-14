namespace GameDesignWizard.Core.Ideas;

public interface IManagedMediaStorage
{
    MediaAttachmentType GetMediaType(string sourcePath);

    Task<ManagedMediaFile> ImportAsync(
        Guid ideaId,
        Guid attachmentId,
        string sourcePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string storedRelativePath, CancellationToken cancellationToken = default);

    string GetFullPath(string storedRelativePath);
}

public sealed record ManagedMediaFile(
    Guid Id,
    string FileName,
    MediaAttachmentType MediaType,
    string StoredRelativePath);
