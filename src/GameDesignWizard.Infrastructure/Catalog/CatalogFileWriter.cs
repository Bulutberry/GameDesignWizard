using System.Text;
using ClosedXML.Excel;
using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.Infrastructure.Catalog;

public sealed class CatalogFileWriter : ICatalogFileWriter
{
    public async Task WriteAsync(
        string filePath,
        CatalogCategory category,
        IReadOnlyList<CatalogOption> options,
        CancellationToken cancellationToken = default)
    {
        if (options.Count == 0)
        {
            throw new InvalidOperationException("The selected catalog scope does not contain active options.");
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath))
            ?? throw new InvalidOperationException("Choose a valid export location.");
        Directory.CreateDirectory(directory);
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileNameWithoutExtension(filePath)}.{Guid.NewGuid():N}.tmp{extension}");

        try
        {
            switch (extension)
            {
                case ".txt":
                    await File.WriteAllLinesAsync(
                        temporaryPath,
                        options.Select(option => option.NameEnglish),
                        new UTF8Encoding(false),
                        cancellationToken);
                    break;
                case ".xlsx":
                    WriteWorkbook(temporaryPath, category, options, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException("Choose a UTF-8 TXT or XLSX export file.");
            }

            File.Move(temporaryPath, filePath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static void WriteWorkbook(
        string filePath,
        CatalogCategory category,
        IReadOnlyList<CatalogOption> options,
        CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(GetWorksheetName(category));
        worksheet.ShowGridLines = false;
        worksheet.Cell(1, 1).Value = "NameEnglish";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.White;
        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#6C5CE7");

        for (var index = 0; index < options.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            worksheet.Cell(index + 2, 1).Value = options[index].NameEnglish;
        }

        worksheet.Column(1).Width = Math.Min(
            70,
            Math.Max(24, options.Max(option => option.NameEnglish.Length) + 3));
        worksheet.SheetView.FreezeRows(1);
        worksheet.Range(1, 1, options.Count + 1, 1).SetAutoFilter();
        workbook.SaveAs(filePath);
    }

    private static string GetWorksheetName(CatalogCategory category) => category switch
    {
        CatalogCategory.ArtStyle => "Art Styles",
        CatalogCategory.DevelopmentDuration => "Development Durations",
        CatalogCategory.TeamSize => "Team Sizes",
        _ => $"{category}s"
    };
}
