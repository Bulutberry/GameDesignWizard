using GameDesignWizard.Core.Ideas;
using GameDesignWizard.Infrastructure.Data;

namespace GameDesignWizard.Infrastructure.Ideas;

public sealed class ManagedMediaStorage : IManagedMediaStorage
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".m4a", ".ogg", ".flac"
    };

    private readonly string _dataDirectory;
    private readonly string _mediaDirectory;

    public ManagedMediaStorage()
        : this(AppDataPaths.DataDirectory)
    {
    }

    public ManagedMediaStorage(string dataDirectory)
    {
        _dataDirectory = Path.GetFullPath(dataDirectory);
        _mediaDirectory = Path.Combine(_dataDirectory, "media");
    }

    public MediaAttachmentType GetMediaType(string sourcePath)
    {
        var extension = Path.GetExtension(sourcePath);
        if (ImageExtensions.Contains(extension))
        {
            return MediaAttachmentType.Image;
        }

        if (AudioExtensions.Contains(extension))
        {
            return MediaAttachmentType.Audio;
        }

        throw new InvalidOperationException($"{Path.GetFileName(sourcePath)} is not a supported image or audio file.");
    }

    public async Task<ManagedMediaFile> ImportAsync(
        Guid ideaId,
        Guid attachmentId,
        string sourcePath,
        CancellationToken cancellationToken = default)
    {
        var fullSourcePath = Path.GetFullPath(sourcePath);
        if (!File.Exists(fullSourcePath))
        {
            throw new InvalidOperationException($"{Path.GetFileName(sourcePath)} could not be found.");
        }

        var mediaType = GetMediaType(fullSourcePath);
        var extension = Path.GetExtension(fullSourcePath).ToLowerInvariant();
        var destinationDirectory = Path.Combine(_mediaDirectory, ideaId.ToString("N"));
        Directory.CreateDirectory(destinationDirectory);
        var destinationPath = Path.Combine(destinationDirectory, $"{attachmentId:N}{extension}");
        var temporaryPath = Path.Combine(destinationDirectory, $".{attachmentId:N}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var source = new FileStream(
                             fullSourcePath,
                             FileMode.Open,
                             FileAccess.Read,
                             FileShare.Read,
                             81920,
                             FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var destination = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             81920,
                             FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await source.CopyToAsync(destination, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, destinationPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return new ManagedMediaFile(
            attachmentId,
            Path.GetFileName(fullSourcePath),
            mediaType,
            Path.GetRelativePath(_dataDirectory, destinationPath).Replace('\\', '/'));
    }

    public Task DeleteAsync(string storedRelativePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = GetFullPath(storedRelativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null
            && Directory.Exists(directory)
            && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }

        return Task.CompletedTask;
    }

    public string GetFullPath(string storedRelativePath)
    {
        if (string.IsNullOrWhiteSpace(storedRelativePath) || Path.IsPathRooted(storedRelativePath))
        {
            throw new InvalidOperationException("The managed media path is invalid.");
        }

        var fullPath = Path.GetFullPath(Path.Combine(
            _dataDirectory,
            storedRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var allowedPrefix = _mediaDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The managed media path leaves the application media directory.");
        }

        return fullPath;
    }
}
