using ClosedXML.Excel;
using GameDesignWizard.Core.Catalog;
using GameDesignWizard.Core.Ideas;

namespace GameDesignWizard.Infrastructure.Ideas;

public sealed class ClosedXmlGameIdeaWorkbookExporter : IGameIdeaWorkbookExporter
{
    private const int MaximumChunkLength = 30_000;
    private const string FontName = "Segoe UI";
    private static readonly XLColor HeaderColor = XLColor.FromHtml("#1F2A44");
    private static readonly XLColor AccentColor = XLColor.FromHtml("#6C5CE7");
    private static readonly XLColor AlternateRowColor = XLColor.FromHtml("#F4F5FB");
    private static readonly XLColor BorderColor = XLColor.FromHtml("#DDE0EB");

    public Task ExportAsync(
        IReadOnlyList<GameIdeaDocument> ideas,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ideas);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        return Task.Run(() => Export(ideas, destinationPath, cancellationToken), cancellationToken);
    }

    private static void Export(
        IReadOnlyList<GameIdeaDocument> ideas,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullDestinationPath = Path.GetFullPath(destinationPath);
        var destinationDirectory = Path.GetDirectoryName(fullDestinationPath)
            ?? throw new InvalidOperationException("The workbook destination directory is invalid.");
        Directory.CreateDirectory(destinationDirectory);
        var temporaryPath = Path.Combine(
            destinationDirectory,
            $".{Path.GetFileNameWithoutExtension(fullDestinationPath)}.{Guid.NewGuid():N}.tmp.xlsx");

        try
        {
            using var workbook = BuildWorkbook(ideas, cancellationToken);
            workbook.SaveAs(temporaryPath, true, true);
            cancellationToken.ThrowIfCancellationRequested();
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

    private static XLWorkbook BuildWorkbook(
        IReadOnlyList<GameIdeaDocument> ideas,
        CancellationToken cancellationToken)
    {
        var workbook = new XLWorkbook();
        workbook.Properties.Title = "GameDesignWizard idea pool";
        workbook.Properties.Subject = "Complete local game idea export";
        workbook.Properties.Author = "GameDesignWizard";
        workbook.Properties.Company = "GameDesignWizard";

        WriteIdeaSheet(workbook, "All Ideas", "AllIdeasTable", ideas, cancellationToken);
        WriteIdeaSheet(
            workbook,
            "PC",
            "PcIdeasTable",
            ideas.Where(idea => idea.PoolGroup == PlatformPoolGroup.Pc).ToArray(),
            cancellationToken);
        WriteIdeaSheet(
            workbook,
            "Mobile",
            "MobileIdeasTable",
            ideas.Where(idea => idea.PoolGroup == PlatformPoolGroup.Mobile).ToArray(),
            cancellationToken);
        WriteIdeaSheet(
            workbook,
            "Other",
            "OtherIdeasTable",
            ideas.Where(idea => idea.PoolGroup == PlatformPoolGroup.Other).ToArray(),
            cancellationToken);
        WriteSelectionsSheet(workbook, ideas, cancellationToken);
        WriteDetailsSheet(workbook, ideas, cancellationToken);
        WriteReferencesSheet(workbook, ideas, cancellationToken);
        WriteMediaSheet(workbook, ideas, cancellationToken);
        return workbook;
    }

    private static void WriteIdeaSheet(
        XLWorkbook workbook,
        string sheetName,
        string tableName,
        IReadOnlyCollection<GameIdeaDocument> ideas,
        CancellationToken cancellationToken)
    {
        var worksheet = workbook.Worksheets.Add(sheetName);
        var headers = new[]
        {
            "Idea ID", "Name", "Pool", "Platform", "Genre", "Subgenre", "Topics", "Mechanics",
            "Features", "Art Styles", "Team Size", "Estimated Duration", "Stage", "Created UTC", "Updated UTC"
        };
        WriteHeaders(worksheet, headers);

        var row = 2;
        foreach (var idea in ideas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteText(worksheet.Cell(row, 1), idea.Id.ToString("D"));
            WriteText(worksheet.Cell(row, 2), idea.NameEnglish);
            WriteText(worksheet.Cell(row, 3), GetPoolName(idea.PoolGroup));
            WriteText(worksheet.Cell(row, 4), idea.Platform?.NameEnglish ?? "Unspecified");
            WriteText(worksheet.Cell(row, 5), idea.Genre?.NameEnglish ?? "Unspecified");
            WriteText(worksheet.Cell(row, 6), idea.Subgenre?.NameEnglish ?? "Unspecified");
            WriteText(worksheet.Cell(row, 7), JoinSelectionSummary(idea.Topics));
            WriteText(worksheet.Cell(row, 8), JoinSelectionSummary(idea.Mechanics));
            WriteText(worksheet.Cell(row, 9), JoinSelectionSummary(idea.Features));
            WriteText(worksheet.Cell(row, 10), JoinSelectionSummary(idea.ArtStyles));
            WriteText(worksheet.Cell(row, 11), idea.TeamSize?.NameEnglish ?? "Unspecified");
            WriteText(worksheet.Cell(row, 12), idea.DevelopmentDuration?.NameEnglish ?? "Unspecified");
            WriteText(worksheet.Cell(row, 13), idea.Stage.ToString());
            WriteDate(worksheet.Cell(row, 14), idea.CreatedAtUtc);
            WriteDate(worksheet.Cell(row, 15), idea.UpdatedAtUtc);
            row++;
        }

        FinalizeSheet(
            worksheet,
            tableName,
            row - 1,
            headers.Length,
            [16, 28, 12, 20, 20, 22, 36, 36, 36, 28, 20, 22, 15, 20, 20],
            wrapColumns: [7, 8, 9, 10]);
    }

    private static void WriteSelectionsSheet(
        XLWorkbook workbook,
        IReadOnlyList<GameIdeaDocument> ideas,
        CancellationToken cancellationToken)
    {
        var worksheet = workbook.Worksheets.Add("Selections");
        var headers = new[] { "Idea ID", "Idea Name", "Category", "Sort Order", "Option ID", "Name English" };
        WriteHeaders(worksheet, headers);
        var row = 2;
        foreach (var idea in ideas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var selection in EnumerateSelections(idea))
            {
                WriteText(worksheet.Cell(row, 1), idea.Id.ToString("D"));
                WriteText(worksheet.Cell(row, 2), idea.NameEnglish);
                WriteText(worksheet.Cell(row, 3), selection.Category);
                worksheet.Cell(row, 4).Value = selection.SortOrder;
                WriteText(worksheet.Cell(row, 5), selection.Option.OptionId.ToString("D"));
                WriteText(worksheet.Cell(row, 6), selection.Option.NameEnglish);
                row++;
            }
        }

        FinalizeSheet(
            worksheet,
            "IdeaSelectionsTable",
            row - 1,
            headers.Length,
            [16, 28, 24, 12, 16, 36]);
    }

    private static void WriteDetailsSheet(
        XLWorkbook workbook,
        IReadOnlyList<GameIdeaDocument> ideas,
        CancellationToken cancellationToken)
    {
        var worksheet = workbook.Worksheets.Add("Idea Details");
        var headers = new[]
        {
            "Idea ID", "Idea Name", "Content Type", "Section ID", "Section Title", "Sort Order",
            "Chunk Index", "Chunk Count", "Content English"
        };
        WriteHeaders(worksheet, headers);
        var row = 2;
        foreach (var idea in ideas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            row = WriteContentChunks(
                worksheet,
                row,
                idea,
                "Overview",
                string.Empty,
                "Overview",
                -1,
                idea.OverviewEnglish);
            foreach (var section in idea.Sections.OrderBy(section => section.SortOrder))
            {
                row = WriteContentChunks(
                    worksheet,
                    row,
                    idea,
                    "GDD Section",
                    section.Id.ToString("D"),
                    section.TitleEnglish,
                    section.SortOrder,
                    section.ContentEnglish);
            }
        }

        FinalizeSheet(
            worksheet,
            "IdeaDetailsTable",
            row - 1,
            headers.Length,
            [16, 28, 18, 16, 30, 12, 12, 12, 80],
            wrapColumns: [9],
            tallRows: true);
    }

    private static void WriteReferencesSheet(
        XLWorkbook workbook,
        IReadOnlyList<GameIdeaDocument> ideas,
        CancellationToken cancellationToken)
    {
        var worksheet = workbook.Worksheets.Add("References");
        var headers = new[] { "Idea ID", "Idea Name", "Reference ID", "Sort Order", "URL", "Note English" };
        WriteHeaders(worksheet, headers);
        var row = 2;
        foreach (var idea in ideas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var reference in idea.References.OrderBy(reference => reference.SortOrder))
            {
                WriteText(worksheet.Cell(row, 1), idea.Id.ToString("D"));
                WriteText(worksheet.Cell(row, 2), idea.NameEnglish);
                WriteText(worksheet.Cell(row, 3), reference.Id.ToString("D"));
                worksheet.Cell(row, 4).Value = reference.SortOrder;
                WriteText(worksheet.Cell(row, 5), reference.Url ?? string.Empty);
                WriteText(worksheet.Cell(row, 6), reference.NoteEnglish);
                row++;
            }
        }

        FinalizeSheet(
            worksheet,
            "IdeaReferencesTable",
            row - 1,
            headers.Length,
            [16, 28, 16, 12, 48, 60],
            wrapColumns: [5, 6],
            tallRows: true);
    }

    private static void WriteMediaSheet(
        XLWorkbook workbook,
        IReadOnlyList<GameIdeaDocument> ideas,
        CancellationToken cancellationToken)
    {
        var worksheet = workbook.Worksheets.Add("Media Index");
        var headers = new[]
        {
            "Idea ID", "Idea Name", "Media ID", "Sort Order", "Type", "File Name", "Caption English", "Stored Relative Path"
        };
        WriteHeaders(worksheet, headers);
        var row = 2;
        foreach (var idea in ideas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var media in idea.MediaAttachments.OrderBy(media => media.SortOrder))
            {
                WriteText(worksheet.Cell(row, 1), idea.Id.ToString("D"));
                WriteText(worksheet.Cell(row, 2), idea.NameEnglish);
                WriteText(worksheet.Cell(row, 3), media.Id.ToString("D"));
                worksheet.Cell(row, 4).Value = media.SortOrder;
                WriteText(worksheet.Cell(row, 5), media.MediaType.ToString());
                WriteText(worksheet.Cell(row, 6), media.FileName);
                WriteText(worksheet.Cell(row, 7), media.CaptionEnglish);
                WriteText(worksheet.Cell(row, 8), media.StoredRelativePath);
                row++;
            }
        }

        FinalizeSheet(
            worksheet,
            "IdeaMediaTable",
            row - 1,
            headers.Length,
            [16, 28, 16, 12, 12, 32, 50, 58],
            wrapColumns: [7, 8],
            tallRows: true);
    }

    private static int WriteContentChunks(
        IXLWorksheet worksheet,
        int startingRow,
        GameIdeaDocument idea,
        string contentType,
        string sectionId,
        string sectionTitle,
        int sortOrder,
        string content)
    {
        var chunks = ChunkText(content).ToArray();
        for (var index = 0; index < chunks.Length; index++)
        {
            var row = startingRow + index;
            WriteText(worksheet.Cell(row, 1), idea.Id.ToString("D"));
            WriteText(worksheet.Cell(row, 2), idea.NameEnglish);
            WriteText(worksheet.Cell(row, 3), contentType);
            WriteText(worksheet.Cell(row, 4), sectionId);
            WriteText(worksheet.Cell(row, 5), sectionTitle);
            worksheet.Cell(row, 6).Value = sortOrder;
            worksheet.Cell(row, 7).Value = index + 1;
            worksheet.Cell(row, 8).Value = chunks.Length;
            WriteText(worksheet.Cell(row, 9), chunks[index]);
        }

        return startingRow + chunks.Length;
    }

    private static IEnumerable<(string Category, int SortOrder, CatalogSelectionSnapshot Option)> EnumerateSelections(
        GameIdeaDocument idea)
    {
        if (idea.Platform is not null) yield return ("Platform", 0, idea.Platform);
        if (idea.Genre is not null) yield return ("Genre", 0, idea.Genre);
        if (idea.Subgenre is not null) yield return ("Subgenre", 0, idea.Subgenre);
        foreach (var selection in idea.Topics.Select((option, index) => ("Topic", index, option))) yield return selection;
        foreach (var selection in idea.Mechanics.Select((option, index) => ("Mechanic", index, option))) yield return selection;
        foreach (var selection in idea.Features.Select((option, index) => ("Feature", index, option))) yield return selection;
        foreach (var selection in idea.ArtStyles.Select((option, index) => ("Art Style", index, option))) yield return selection;
        if (idea.DevelopmentDuration is not null) yield return ("Development Duration", 0, idea.DevelopmentDuration);
        if (idea.TeamSize is not null) yield return ("Team Size", 0, idea.TeamSize);
    }

    private static IEnumerable<string> ChunkText(string? value)
    {
        value ??= string.Empty;
        if (value.Length == 0)
        {
            yield return string.Empty;
            yield break;
        }

        var offset = 0;
        while (offset < value.Length)
        {
            var length = Math.Min(MaximumChunkLength, value.Length - offset);
            if (offset + length < value.Length && char.IsHighSurrogate(value[offset + length - 1]))
            {
                length--;
            }

            yield return value.Substring(offset, length);
            offset += length;
        }
    }

    private static string JoinSelectionSummary(IEnumerable<CatalogSelectionSnapshot> selections)
    {
        var fullValue = string.Join(", ", selections.Select(selection => selection.NameEnglish));
        if (fullValue.Length <= MaximumChunkLength)
        {
            return string.IsNullOrWhiteSpace(fullValue) ? "Unspecified" : fullValue;
        }

        return $"{fullValue[..(MaximumChunkLength - 45)]}... [Complete list in Selections sheet]";
    }

    private static void WriteHeaders(IXLWorksheet worksheet, IReadOnlyList<string> headers)
    {
        worksheet.ShowGridLines = false;
        for (var column = 0; column < headers.Count; column++)
        {
            WriteText(worksheet.Cell(1, column + 1), headers[column]);
        }

        var header = worksheet.Range(1, 1, 1, headers.Count);
        header.Style.Fill.BackgroundColor = HeaderColor;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Font.Bold = true;
        header.Style.Font.FontName = FontName;
        header.Style.Font.FontSize = 10;
        header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        header.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        header.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
        header.Style.Border.BottomBorderColor = AccentColor;
        worksheet.Row(1).Height = 24;
        worksheet.SheetView.FreezeRows(1);
    }

    private static void FinalizeSheet(
        IXLWorksheet worksheet,
        string tableName,
        int lastRow,
        int lastColumn,
        IReadOnlyList<double> widths,
        IReadOnlyCollection<int>? wrapColumns = null,
        bool tallRows = false)
    {
        for (var column = 1; column <= lastColumn; column++)
        {
            worksheet.Column(column).Width = widths[column - 1];
        }

        if (lastRow < 2)
        {
            return;
        }

        var dataRange = worksheet.Range(2, 1, lastRow, lastColumn);
        dataRange.Style.Font.FontName = FontName;
        dataRange.Style.Font.FontSize = 10;
        dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
        dataRange.Style.Border.BottomBorder = XLBorderStyleValues.Hair;
        dataRange.Style.Border.BottomBorderColor = BorderColor;
        for (var row = 2; row <= lastRow; row++)
        {
            if (row % 2 == 0)
            {
                worksheet.Range(row, 1, row, lastColumn).Style.Fill.BackgroundColor = AlternateRowColor;
            }

            worksheet.Row(row).Height = tallRows ? 42 : 22;
        }

        if (wrapColumns is not null)
        {
            foreach (var column in wrapColumns)
            {
                worksheet.Range(2, column, lastRow, column).Style.Alignment.WrapText = true;
            }
        }

        var table = worksheet.Range(1, 1, lastRow, lastColumn).CreateTable(tableName);
        table.Theme = XLTableTheme.None;
        table.ShowAutoFilter = true;
        table.ShowRowStripes = false;
    }

    private static void WriteText(IXLCell cell, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        cell.Value = value;
        cell.Style.NumberFormat.Format = "@";
    }

    private static void WriteDate(IXLCell cell, DateTime value)
    {
        cell.Value = value.ToUniversalTime();
        cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
    }

    private static string GetPoolName(PlatformPoolGroup group) => group switch
    {
        PlatformPoolGroup.Pc => "PC",
        PlatformPoolGroup.Mobile => "Mobile",
        _ => "Other"
    };
}
