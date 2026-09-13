# GameDesignWizard

GameDesignWizard is an English-first, offline Windows desktop application for shaping game ideas, organizing a local idea pool, and exporting structured game design documents.

## Current prototype

Prototype 0.5.0 validates the visual direction, persistent dynamic catalogs, the first two wizard steps, bulk catalog import, and distributable defaults:

- Home navigation
- Platform selection with data-bound buttons and a visible selected state
- Genre selection with dependent subgenre choices
- Searchable, virtualized topic transfer lists designed for hundreds of entries
- Double-click or Enter keyboard transfer between available and selected topics
- Fixed PC, Mobile, and Other idea-pool groups
- Settings management for platforms, genres, subgenres, topics, mechanics, features, art styles, development durations, and team sizes
- SQLite-backed add, archive, and restore behavior that survives app restarts
- Parent-genre selection for subgenres and dependent archival when a genre is archived
- TXT and single-column XLSX import with a validation preview and atomic save
- One versioned English JSON source for all nine default catalog categories
- A maintainer command that promotes the active local catalog into the next bundled default catalog
- Custom-platform routing explained as Other

Catalog data is stored locally in `%LOCALAPPDATA%\GameDesignWizard\game-design-wizard.db`.

All default options are versioned in [`catalog-source/en/default-catalog.json`](catalog-source/en/default-catalog.json). The JSON is embedded during build, so a fresh GitHub checkout starts with these defaults without importing any files. Application upgrades add missing shipped defaults while preserving local custom options and archive states. See [`catalog-source/README.md`](catalog-source/README.md) for the maintainer workflow.

To add options to one local installation, select a category in Settings and use **Import TXT / XLSX**. See [`docs/catalog-import.md`](docs/catalog-import.md) for the supported file layout and validation rules.

## Run locally

Requirements:

- Windows
- .NET 10 SDK

Double-click `run-app.cmd`, or run it from a terminal. The launcher resolves the repository path automatically, regardless of the terminal's current directory.

```powershell
.\run-app.cmd
```

The direct .NET command must be run from the repository root:

```powershell
cd path\to\GameDesignWizard
dotnet run --project .\src\GameDesignWizard.App\GameDesignWizard.App.csproj
```

## Project status

This repository is in active prototype development. See [PROJECT_PLAN.md](PROJECT_PLAN.md) for the proposed architecture, delivery phases, and acceptance scenarios.

## License

Original code and documentation are available under the [MIT License](LICENSE).
