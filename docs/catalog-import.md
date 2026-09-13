# Catalog import

GameDesignWizard supports bulk import into the category currently selected under **Settings / Catalogs**.

## TXT files

- Save the file as UTF-8 text.
- Put one option name on each line.
- Blank lines and lines beginning with `#` are ignored.

```text
# Additional topics
Ancient Egypt
Deep Ocean
Time Travel
```

## XLSX files

- The first worksheet is read.
- Option names belong in the first column.
- The first nonblank cell may use a recognized header such as `Name`, `NameEn`, or `Topics`.
- Formula cells are rejected and never used as catalog data.

## Import workflow

1. Open **Settings** and select the destination category.
2. For subgenres, select the parent genre that applies to every imported row.
3. Choose **Import TXT / XLSX** and select the file.
4. Review the row-by-row preview.
5. Select **Import** to save every ready row in one transaction.

Existing names, duplicates within the file, names longer than 200 characters, and formula cells are skipped in the preview. If the accepted batch cannot be saved completely, the database remains unchanged.

The import adds custom options to the local installation. To turn a reviewed local catalog into defaults for future downloads, run the repository's catalog maintenance tool as described in [`catalog-source/README.md`](../catalog-source/README.md). It exports all nine active categories to the versioned English JSON source; users never need to import these bundled defaults themselves.
