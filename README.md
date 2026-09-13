# GameDesignWizard

GameDesignWizard is an English-first, offline Windows desktop application for shaping game ideas, organizing a local idea pool, and exporting structured game design documents.

## Current prototype

Prototype 0.2.0 validates the visual direction and persistent dynamic catalog behavior:

- Home navigation
- First wizard step with data-bound platform buttons
- Fixed PC, Mobile, and Other idea-pool groups
- Settings management for platforms, genres, subgenres, topics, mechanics, features, art styles, development durations, and team sizes
- SQLite-backed add, archive, and restore behavior that survives app restarts
- Parent-genre selection for subgenres and dependent archival when a genre is archived
- Custom-platform routing explained as Other

Catalog data is stored locally in `%LOCALAPPDATA%\GameDesignWizard\game-design-wizard.db`.

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
