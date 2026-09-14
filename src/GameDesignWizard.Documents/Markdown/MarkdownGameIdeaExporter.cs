using System.Text;
using System.Globalization;
using GameDesignWizard.Core.Catalog;
using GameDesignWizard.Core.Ideas;

namespace GameDesignWizard.Documents.Markdown;

public sealed class MarkdownGameIdeaExporter : IGameIdeaMarkdownExporter
{
    public async Task ExportAsync(
        GameIdeaDocument idea,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(idea);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        var fullDestinationPath = Path.GetFullPath(destinationPath);
        var destinationDirectory = Path.GetDirectoryName(fullDestinationPath)
            ?? throw new InvalidOperationException("The Markdown destination directory is invalid.");
        Directory.CreateDirectory(destinationDirectory);
        var temporaryPath = Path.Combine(
            destinationDirectory,
            $".{Path.GetFileName(fullDestinationPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            var content = BuildDocument(idea, cancellationToken);
            await File.WriteAllTextAsync(
                temporaryPath,
                content,
                new UTF8Encoding(false),
                cancellationToken);
            File.Move(temporaryPath, fullDestinationPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static string BuildDocument(GameIdeaDocument idea, CancellationToken cancellationToken)
    {
        var output = new StringBuilder();
        output.AppendLine($"# {EscapeInline(idea.NameEnglish)}");
        output.AppendLine();
        output.AppendLine("> Game design document exported by GameDesignWizard.");
        output.AppendLine();
        output.AppendLine("## Project Information");
        output.AppendLine();
        output.AppendLine("| Field | Value |");
        output.AppendLine("| --- | --- |");
        AddMetadataRow(output, "Stage", idea.Stage.ToString());
        AddMetadataRow(output, "Idea pool", GetPoolName(idea.PoolGroup));
        AddMetadataRow(output, "Platform", idea.Platform?.NameEnglish);
        AddMetadataRow(output, "Genre", idea.Genre?.NameEnglish);
        AddMetadataRow(output, "Subgenre", idea.Subgenre?.NameEnglish);
        AddMetadataRow(output, "Topics", JoinSelections(idea.Topics));
        AddMetadataRow(output, "Mechanics", JoinSelections(idea.Mechanics));
        AddMetadataRow(output, "Features", JoinSelections(idea.Features));
        AddMetadataRow(output, "Art styles", JoinSelections(idea.ArtStyles));
        AddMetadataRow(output, "Development duration", idea.DevelopmentDuration?.NameEnglish);
        AddMetadataRow(output, "Team size", idea.TeamSize?.NameEnglish);
        AddMetadataRow(output, "Created", FormatTimestamp(idea.CreatedAtUtc));
        AddMetadataRow(output, "Updated", FormatTimestamp(idea.UpdatedAtUtc));

        output.AppendLine();
        output.AppendLine("## Overview");
        output.AppendLine();
        AppendContent(output, idea.OverviewEnglish);

        output.AppendLine();
        output.AppendLine("## Game Design Document");
        foreach (var section in idea.Sections.OrderBy(section => section.SortOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();
            output.AppendLine();
            output.AppendLine($"### {EscapeInline(section.TitleEnglish)}");
            output.AppendLine();
            AppendContent(output, section.ContentEnglish);
        }

        AddReferences(output, idea.References);
        AddMedia(output, idea.MediaAttachments);
        output.AppendLine();
        return output.ToString();
    }

    private static void AddMetadataRow(StringBuilder output, string field, string? value) =>
        output.AppendLine($"| {EscapeTableCell(field)} | {EscapeTableCell(NormalizeValue(value))} |");

    private static void AppendContent(StringBuilder output, string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            output.AppendLine("_Not specified._");
            return;
        }

        output.AppendLine(content.Trim().Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    private static void AddReferences(StringBuilder output, IReadOnlyCollection<GameReferenceContent> references)
    {
        var populated = references
            .Where(reference => !string.IsNullOrWhiteSpace(reference.Url)
                || !string.IsNullOrWhiteSpace(reference.NoteEnglish))
            .OrderBy(reference => reference.SortOrder)
            .ToArray();
        if (populated.Length == 0)
        {
            return;
        }

        output.AppendLine();
        output.AppendLine("## References");
        output.AppendLine();
        foreach (var reference in populated)
        {
            var url = reference.Url?.Trim();
            var note = reference.NoteEnglish.Trim();
            var value = string.IsNullOrWhiteSpace(url) ? string.Empty : $"<{url}>";
            if (note.Length > 0)
            {
                value += value.Length == 0 ? EscapeInline(note) : $" - {EscapeInline(note)}";
            }

            output.AppendLine($"- {value}");
        }
    }

    private static void AddMedia(StringBuilder output, IReadOnlyCollection<MediaAttachmentContent> attachments)
    {
        if (attachments.Count == 0)
        {
            return;
        }

        output.AppendLine();
        output.AppendLine("## Media Notes");
        output.AppendLine();
        output.AppendLine("The exported Markdown lists managed attachments but does not copy or embed their files.");
        output.AppendLine();
        foreach (var attachment in attachments.OrderBy(attachment => attachment.SortOrder))
        {
            var caption = string.IsNullOrWhiteSpace(attachment.CaptionEnglish)
                ? string.Empty
                : $" - {EscapeInline(attachment.CaptionEnglish.Trim())}";
            output.AppendLine($"- **{attachment.MediaType}:** `{EscapeCode(attachment.FileName)}`{caption}");
        }
    }

    private static string JoinSelections(IEnumerable<CatalogSelectionSnapshot> selections)
    {
        var value = string.Join(", ", selections.Select(selection => selection.NameEnglish)
            .Where(name => !string.IsNullOrWhiteSpace(name)));
        return NormalizeValue(value);
    }

    private static string NormalizeValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Not specified" : value.Trim();

    private static string EscapeTableCell(string value) => EscapeInline(value)
        .Replace("|", "\\|", StringComparison.Ordinal)
        .Replace("\r\n", "<br>", StringComparison.Ordinal)
        .Replace("\n", "<br>", StringComparison.Ordinal);

    private static string EscapeInline(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("*", "\\*", StringComparison.Ordinal)
        .Replace("_", "\\_", StringComparison.Ordinal)
        .Replace("`", "\\`", StringComparison.Ordinal)
        .Replace("[", "\\[", StringComparison.Ordinal)
        .Replace("]", "\\]", StringComparison.Ordinal);

    private static string EscapeCode(string value) => value.Replace("`", "'", StringComparison.Ordinal);

    private static string FormatTimestamp(DateTime value) =>
        value == default
            ? "Not specified"
            : value.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    private static string GetPoolName(PlatformPoolGroup group) => group switch
    {
        PlatformPoolGroup.Pc => "PC",
        PlatformPoolGroup.Mobile => "Mobile",
        _ => "Other"
    };
}
