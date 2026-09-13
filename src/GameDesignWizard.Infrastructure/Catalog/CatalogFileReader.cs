using ClosedXML.Excel;
using GameDesignWizard.Core.Catalog;

namespace GameDesignWizard.Infrastructure.Catalog;

public sealed class CatalogFileReader : ICatalogFileReader
{
    private static readonly HashSet<string> HeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Name",
        "NameEn",
        "NameEnglish",
        "Topic",
        "Topics",
        "Platforms",
        "Genres",
        "Subgenres",
        "Mechanics",
        "Features",
        "Art Styles",
        "Development Durations",
        "Team Sizes"
    };

    public Task<IReadOnlyList<CatalogFileRow>> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(filePath);
        return extension.ToLowerInvariant() switch
        {
            ".txt" => ReadTextAsync(filePath, cancellationToken),
            ".xlsx" => Task.FromResult(ReadWorkbook(filePath, cancellationToken)),
            _ => throw new InvalidOperationException("Choose a UTF-8 TXT or XLSX file.")
        };
    }

    private static async Task<IReadOnlyList<CatalogFileRow>> ReadTextAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var rows = new List<CatalogFileRow>();
        var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
        for (var index = 0; index < lines.Length; index++)
        {
            var name = CleanName(lines[index]);
            if (name.Length == 0 || name.StartsWith('#'))
            {
                continue;
            }

            rows.Add(new CatalogFileRow(index + 1, name));
        }

        return rows;
    }

    private static IReadOnlyList<CatalogFileRow> ReadWorkbook(
        string filePath,
        CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("The workbook does not contain a worksheet.");
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        var rows = new List<CatalogFileRow>();
        var firstValueSeen = false;

        for (var rowNumber = 1; rowNumber <= lastRow; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cell = worksheet.Cell(rowNumber, 1);
            if (cell.HasFormula)
            {
                firstValueSeen = true;
                rows.Add(new CatalogFileRow(rowNumber, "Formula", "Formulas are not allowed in catalog data."));
                continue;
            }

            var name = CleanName(cell.GetFormattedString());
            if (name.Length == 0)
            {
                continue;
            }

            if (!firstValueSeen && HeaderNames.Contains(name))
            {
                firstValueSeen = true;
                continue;
            }

            firstValueSeen = true;
            rows.Add(new CatalogFileRow(rowNumber, name));
        }

        return rows;
    }

    private static string CleanName(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
