# GameDesignWizard

GameDesignWizard is an English-first, offline Windows desktop application for shaping game ideas, organizing a local idea pool, and exporting structured game design documents.

## Current prototype

Prototype 0.0.2 improves sidebar readability with brighter labels, larger supporting text, and a visible current-page indicator. It validates the visual direction and the central dynamic catalog behavior:

- Home navigation
- First wizard step with data-bound platform buttons
- Fixed PC, Mobile, and Other idea-pool groups
- Platform management in Settings
- Immediate add, archive, and restore behavior
- Custom-platform routing explained as Other

The prototype keeps catalog changes in memory. SQLite persistence begins after the interaction and visual direction are approved.

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
