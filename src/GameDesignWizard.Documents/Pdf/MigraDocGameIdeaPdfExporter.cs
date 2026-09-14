using GameDesignWizard.Core.Ideas;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.Windows.Media.Imaging;

namespace GameDesignWizard.Documents.Pdf;

public sealed class MigraDocGameIdeaPdfExporter(IManagedMediaStorage mediaStorage) : IGameIdeaPdfExporter
{
    private static readonly Color AccentColor = Color.FromRgb(105, 86, 232);
    private static readonly Color DarkColor = Color.FromRgb(31, 42, 68);
    private static readonly Color MutedColor = Color.FromRgb(92, 101, 122);
    private static readonly Color LightColor = Color.FromRgb(244, 245, 251);
    private static readonly Color BorderColor = Color.FromRgb(221, 224, 235);

    public Task ExportAsync(
        GameIdeaDocument idea,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        return Task.Run(() => Export(idea, destinationPath, cancellationToken), cancellationToken);
    }

    private void Export(
        GameIdeaDocument idea,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullDestinationPath = Path.GetFullPath(destinationPath);
        var destinationDirectory = Path.GetDirectoryName(fullDestinationPath)
            ?? throw new InvalidOperationException("The PDF destination directory is invalid.");
        Directory.CreateDirectory(destinationDirectory);
        var temporaryPath = Path.Combine(
            destinationDirectory,
            $".{Path.GetFileName(fullDestinationPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            var document = BuildDocument(idea, cancellationToken);
            var renderer = new PdfDocumentRenderer { Document = document };
            renderer.RenderDocument();
            cancellationToken.ThrowIfCancellationRequested();
            renderer.PdfDocument.Save(temporaryPath);
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

    private Document BuildDocument(GameIdeaDocument idea, CancellationToken cancellationToken)
    {
        var document = new Document();
        document.Info.Title = $"{idea.NameEnglish} - Game Design Document";
        document.Info.Subject = "Game design document exported by GameDesignWizard";
        document.Info.Author = "GameDesignWizard";
        ConfigureStyles(document);

        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.8);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.8);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1.9);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.9);
        AddFooter(section);
        AddTitle(section, idea);
        AddMetadata(section, idea);

        if (!string.IsNullOrWhiteSpace(idea.OverviewEnglish))
        {
            AddContentSection(section, "Overview", idea.OverviewEnglish);
        }

        foreach (var gddSection in idea.Sections.OrderBy(item => item.SortOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.IsNullOrWhiteSpace(gddSection.ContentEnglish))
            {
                AddContentSection(section, gddSection.TitleEnglish, gddSection.ContentEnglish);
            }
        }

        AddMedia(section, idea.MediaAttachments, cancellationToken);
        AddReferences(section, idea.References);
        return document;
    }

    private static void ConfigureStyles(Document document)
    {
        var normal = document.Styles[StyleNames.Normal]!;
        normal.Font.Name = "Segoe UI";
        normal.Font.Size = Unit.FromPoint(10);
        normal.Font.Color = DarkColor;
        normal.ParagraphFormat.SpaceAfter = Unit.FromPoint(6);
        normal.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;

        var heading1 = document.Styles[StyleNames.Heading1]!;
        heading1.Font.Name = "Segoe UI Semibold";
        heading1.Font.Size = Unit.FromPoint(17);
        heading1.Font.Bold = true;
        heading1.Font.Color = DarkColor;
        heading1.ParagraphFormat.SpaceBefore = Unit.FromPoint(17);
        heading1.ParagraphFormat.SpaceAfter = Unit.FromPoint(7);
        heading1.ParagraphFormat.KeepWithNext = true;
        heading1.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
    }

    private static void AddFooter(Section section)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Center;
        footer.Format.Font.Name = "Segoe UI";
        footer.Format.Font.Size = Unit.FromPoint(8);
        footer.Format.Font.Color = MutedColor;
        footer.AddText("GameDesignWizard | ");
        footer.AddPageField();
    }

    private static void AddTitle(Section section, GameIdeaDocument idea)
    {
        var eyebrow = section.AddParagraph();
        eyebrow.Format.SpaceAfter = Unit.FromPoint(8);
        eyebrow.Format.Font.Name = "Segoe UI Semibold";
        eyebrow.Format.Font.Size = Unit.FromPoint(10);
        eyebrow.Format.Font.Bold = true;
        eyebrow.Format.Font.Color = AccentColor;
        eyebrow.AddText("GAME DESIGN DOCUMENT");

        var title = section.AddParagraph();
        title.Format.SpaceAfter = Unit.FromPoint(7);
        title.Format.Font.Name = "Segoe UI Semibold";
        title.Format.Font.Size = Unit.FromPoint(28);
        title.Format.Font.Bold = true;
        title.Format.Font.Color = DarkColor;
        title.AddText(idea.NameEnglish);

        var subtitle = section.AddParagraph();
        subtitle.Format.SpaceAfter = Unit.FromPoint(19);
        subtitle.Format.Font.Size = Unit.FromPoint(9);
        subtitle.Format.Font.Color = MutedColor;
        subtitle.AddText($"{idea.Stage} | {GetPoolName(idea.PoolGroup)} idea pool | Updated {idea.UpdatedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm}");
    }

    private static void AddMetadata(Section section, GameIdeaDocument idea)
    {
        var table = section.AddTable();
        table.Borders.Width = Unit.FromPoint(0.6);
        table.Borders.Color = BorderColor;
        table.Rows.LeftIndent = Unit.Zero;
        table.AddColumn(Unit.FromCentimeter(3.4));
        table.AddColumn(Unit.FromCentimeter(13.1));

        AddMetadataRow(table, "Platform", idea.Platform?.NameEnglish ?? "Unspecified");
        AddMetadataRow(table, "Genre", idea.Genre?.NameEnglish ?? "Unspecified");
        AddMetadataRow(table, "Subgenre", idea.Subgenre?.NameEnglish ?? "Unspecified");
        AddMetadataRow(table, "Topics", JoinSelections(idea.Topics));
        AddMetadataRow(table, "Mechanics", JoinSelections(idea.Mechanics));
        AddMetadataRow(table, "Features", JoinSelections(idea.Features));
        AddMetadataRow(table, "Art styles", JoinSelections(idea.ArtStyles));
        AddMetadataRow(table, "Development duration", idea.DevelopmentDuration?.NameEnglish ?? "Unspecified");
        AddMetadataRow(table, "Team size", idea.TeamSize?.NameEnglish ?? "Unspecified");
    }

    private static void AddMetadataRow(Table table, string label, string value)
    {
        var row = table.AddRow();
        row.TopPadding = Unit.FromPoint(5);
        row.BottomPadding = Unit.FromPoint(5);
        row.Cells[0].Shading.Color = LightColor;
        row.Cells[0].Format.Font.Bold = true;
        row.Cells[0].Format.Font.Color = DarkColor;
        row.Cells[0].AddParagraph(label);
        row.Cells[1].AddParagraph(string.IsNullOrWhiteSpace(value) ? "Unspecified" : value);
    }

    private static void AddContentSection(Section section, string title, string content)
    {
        section.AddParagraph(title, StyleNames.Heading1);
        foreach (var block in SplitParagraphs(content))
        {
            var paragraph = section.AddParagraph(block);
            paragraph.Format.WidowControl = true;
        }
    }

    private void AddMedia(
        Section section,
        IReadOnlyCollection<MediaAttachmentContent> mediaAttachments,
        CancellationToken cancellationToken)
    {
        if (mediaAttachments.Count == 0)
        {
            return;
        }

        section.AddParagraph("Media", StyleNames.Heading1);
        foreach (var attachment in mediaAttachments.OrderBy(item => item.SortOrder))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (attachment.MediaType == MediaAttachmentType.Audio)
            {
                AddAttachmentNote(section, "Audio", attachment.FileName, attachment.CaptionEnglish);
                continue;
            }

            try
            {
                var fullPath = mediaStorage.GetFullPath(attachment.StoredRelativePath);
                if (!File.Exists(fullPath))
                {
                    AddAttachmentNote(section, "Missing image", attachment.FileName, attachment.CaptionEnglish);
                    continue;
                }

                var aspectRatio = GetImageAspectRatio(fullPath);
                var image = section.AddImage(fullPath);
                image.LockAspectRatio = true;
                SetImageSize(image, aspectRatio);
                image.Top = ShapePosition.Top;
                image.Left = ShapePosition.Left;
                if (!string.IsNullOrWhiteSpace(attachment.CaptionEnglish))
                {
                    var caption = section.AddParagraph(attachment.CaptionEnglish.Trim());
                    caption.Format.Font.Size = Unit.FromPoint(8.5);
                    caption.Format.Font.Italic = true;
                    caption.Format.Font.Color = MutedColor;
                    caption.Format.SpaceAfter = Unit.FromPoint(10);
                }
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or ArgumentException
                    or InvalidOperationException
                    or NotSupportedException
                    or FormatException)
            {
                AddAttachmentNote(section, "Image unavailable", attachment.FileName, attachment.CaptionEnglish);
            }
        }
    }

    private static double GetImageAspectRatio(string fullPath)
    {
        using var stream = File.OpenRead(fullPath);
        var decoder = BitmapDecoder.Create(
            stream,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        return frame.PixelWidth / (double)frame.PixelHeight;
    }

    private static void SetImageSize(Image image, double aspectRatio)
    {
        const double maximumWidthCentimeters = 15.8;
        const double maximumHeightCentimeters = 14;
        if (aspectRatio >= maximumWidthCentimeters / maximumHeightCentimeters)
        {
            image.Width = Unit.FromCentimeter(maximumWidthCentimeters);
        }
        else
        {
            image.Height = Unit.FromCentimeter(maximumHeightCentimeters);
        }
    }

    private static void AddAttachmentNote(Section section, string kind, string fileName, string caption)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.LeftIndent = Unit.FromCentimeter(0.25);
        paragraph.Format.SpaceAfter = Unit.FromPoint(7);
        paragraph.AddFormattedText($"{kind}: ", TextFormat.Bold);
        paragraph.AddText(fileName);
        if (!string.IsNullOrWhiteSpace(caption))
        {
            paragraph.AddText($" - {caption.Trim()}");
        }
    }

    private static void AddReferences(Section section, IReadOnlyCollection<GameReferenceContent> references)
    {
        var populated = references
            .Where(reference => !string.IsNullOrWhiteSpace(reference.Url) || !string.IsNullOrWhiteSpace(reference.NoteEnglish))
            .OrderBy(reference => reference.SortOrder)
            .ToArray();
        if (populated.Length == 0)
        {
            return;
        }

        section.AddParagraph("References", StyleNames.Heading1);
        foreach (var reference in populated)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.LeftIndent = Unit.FromCentimeter(0.25);
            paragraph.Format.FirstLineIndent = Unit.FromCentimeter(-0.25);
            paragraph.AddText("- ");
            if (!string.IsNullOrWhiteSpace(reference.Url))
            {
                var url = reference.Url.Trim();
                var hyperlink = paragraph.AddHyperlink(url, HyperlinkType.Web);
                var hyperlinkText = hyperlink.AddFormattedText(url, TextFormat.Underline);
                hyperlinkText.Font.Color = AccentColor;
            }

            if (!string.IsNullOrWhiteSpace(reference.NoteEnglish))
            {
                if (!string.IsNullOrWhiteSpace(reference.Url))
                {
                    paragraph.AddText(" - ");
                }

                paragraph.AddText(reference.NoteEnglish.Trim());
            }
        }
    }

    private static string JoinSelections(IEnumerable<CatalogSelectionSnapshot> selections)
    {
        var names = selections.Select(selection => selection.NameEnglish).Where(name => !string.IsNullOrWhiteSpace(name));
        var result = string.Join(", ", names);
        return string.IsNullOrWhiteSpace(result) ? "Unspecified" : result;
    }

    private static IEnumerable<string> SplitParagraphs(string content) =>
        content.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string GetPoolName(GameDesignWizard.Core.Catalog.PlatformPoolGroup group) => group switch
    {
        GameDesignWizard.Core.Catalog.PlatformPoolGroup.Pc => "PC",
        GameDesignWizard.Core.Catalog.PlatformPoolGroup.Mobile => "Mobile",
        _ => "Other"
    };
}
