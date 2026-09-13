# GameDesignWizard

GameDesignWizard is an English-first, offline Windows desktop application for shaping game ideas, organizing a local idea pool, and exporting structured game design documents.

## Current prototype

Prototype 0.3.0 validates the visual direction, persistent dynamic catalogs, and the first two wizard steps:

- Home navigation
- Platform selection with data-bound buttons and a visible selected state
- Genre selection with dependent subgenre choices
- Searchable, virtualized topic transfer lists designed for hundreds of entries
- Fixed PC, Mobile, and Other idea-pool groups
- Settings management for platforms, genres, subgenres, topics, mechanics, features, art styles, development durations, and team sizes
- SQLite-backed add, archive, and restore behavior that survives app restarts
- Parent-genre selection for subgenres and dependent archival when a genre is archived
- Custom-platform routing explained as Other

Catalog data is stored locally in `%LOCALAPPDATA%\GameDesignWizard\game-design-wizard.db`.

Default English topics are authored one per line in [`catalog-source/en/topics.txt`](catalog-source/en/topics.txt). New names are embedded during build and merged into each local catalog on application startup. See [`catalog-source/README.md`](catalog-source/README.md) for the bulk-authoring rules.

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
