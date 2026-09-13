# GameDesignWizard

GameDesignWizard is an English-first, offline Windows desktop application for shaping game ideas, organizing a local idea pool, and exporting structured game design documents.

## Current prototype

Prototype 0.1.0 validates the visual direction and the first persistent dynamic catalog behavior:

- Home navigation
- First wizard step with data-bound platform buttons
- Fixed PC, Mobile, and Other idea-pool groups
- Platform management in Settings
- SQLite-backed add, archive, and restore behavior that survives app restarts
- Custom-platform routing explained as Other

Catalog data is stored locally in `%LOCALAPPDATA%\GameDesignWizard\game-design-wizard.db`.

## Run locally

Requirements:

- Windows
- .NET 10 SDK

```powershell
dotnet run --project src/GameDesignWizard.App
```

## Project status

This repository is in active prototype development. See [PROJECT_PLAN.md](PROJECT_PLAN.md) for the proposed architecture, delivery phases, and acceptance scenarios.

## License

Original code and documentation are available under the [MIT License](LICENSE).
