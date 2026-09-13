# Default catalog source

`en/default-catalog.json` is the canonical English source for all nine catalogs. It is versioned in Git and embedded into the application during build. Fresh installations therefore receive the maintainer-defined defaults without a separate import.

Each option has a stable ID, category, English name, display order, and optional relationship data. Every subgenre references its parent genre by ID. Platform entries also define their immutable `Pc`, `Mobile`, or `Other` idea-pool route. Keep IDs stable when editing existing entries.

## Bulk authoring workflow

1. Run the app and use **Settings / Catalogs / Import TXT / XLSX** for each category. A subgenre file is imported once for each selected parent genre.
2. Archive any entries that should not ship as defaults.
3. Close the app after reviewing the active lists.
4. From the repository root, export the complete active local catalog:

```powershell
dotnet run --project .\tools\GameDesignWizard.CatalogTool -- export-defaults --version 0.6.0
```

The command reads `%LOCALAPPDATA%\GameDesignWizard\game-design-wizard.db` and atomically replaces `en/default-catalog.json`. Use `--database` or `--output` to select different paths. Review the JSON diff and build the application before committing it.

The exporter preserves IDs, parent relationships, order, descriptions, and platform routing. It omits archived entries and rejects an incomplete catalog. New installations receive the exported lists after the next build. Existing installations keep their custom entries and archive states while receiving shipped options that are missing locally.
