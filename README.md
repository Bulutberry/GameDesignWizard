# GameDesignWizard

GameDesignWizard is an English-first, offline Windows desktop application for shaping game ideas, organizing a local idea pool, and exporting structured game design documents.

## Current prototype

Prototype 0.14.0 validates the visual direction, persistent dynamic catalogs, the complete six-step wizard, local idea saving and editing, searchable pool views, managed media attachments, individual GDD PDF export, complete and filtered idea-pool XLSX export, bulk catalog import, and distributable defaults:

- Home navigation
- Platform selection with data-bound buttons and a visible selected state
- Genre selection with dependent subgenre choices
- Searchable, virtualized topic transfer lists designed for hundreds of entries
- Reusable topic, feature, art-style, and mechanic transfer lists
- Double-click or Enter keyboard transfer between available and selected options
- Repeatable reference URL and note rows with inline validation
- Game name, overview, production-scope choices, and editable GDD sections
- Image and audio attachments with optional captions and source-independent managed copies
- SQLite idea saving with catalog-label snapshots and fixed PC, Mobile, or Other routing
- A read-only saved-idea table with live PC, Mobile, and Other counts
- Selected-idea editing through the complete wizard, including preserved archived catalog selections
- Development stages for Idea, Concept, Prototyping, Completed, and Shelved
- Confirmed idea deletion with cleanup of its managed image and audio files
- A fresh draft after choosing Create Game Idea from a saved or edited idea
- Combined Idea Pool search, development-stage filtering, and clickable PC, Mobile, or Other pool cards
- Filtered XLSX export that contains exactly the ideas currently visible in the table
- Selected-idea PDF export with metadata, written GDD sections, references, images, captions, audio-file notes, and page numbers
- Complete XLSX export across the PC, Mobile, and Other pools, including lossless selection, GDD, reference, and media detail sheets
- Fixed PC, Mobile, and Other idea-pool groups
- Settings management for platforms, genres, subgenres, topics, mechanics, features, art styles, development durations, and team sizes
- SQLite-backed add, archive, and restore behavior that survives app restarts
- Parent-genre selection for subgenres and dependent archival when a genre is archived
- TXT and single-column XLSX import with a validation preview and atomic save
- One versioned English JSON source for all nine default catalog categories
- A maintainer command that promotes the active local catalog into the next bundled default catalog
- Custom-platform routing explained as Other

Catalog data is stored locally in `%LOCALAPPDATA%\GameDesignWizard\game-design-wizard.db`.

Saved image and audio attachments are copied into `%LOCALAPPDATA%\GameDesignWizard\media` and referenced by relative paths. Moving or deleting the original source file does not break the saved idea.

Select a saved row in Idea Pool and choose **Export PDF** to create an A4 game design document. See [`docs/pdf-export.md`](docs/pdf-export.md) for the current document contract.

Choose **Export All XLSX** in Idea Pool to create one workbook containing every saved idea, or **Export Filtered XLSX** to export only the current search, stage, and pool result. Both exports keep the fixed pool views easy to scan and include normalized detail sheets for complete data. See [`docs/idea-pool-export.md`](docs/idea-pool-export.md) for the workbook contract.

Select an Idea Pool row and choose **Edit** to reopen its complete saved snapshot in the wizard. Choose **Delete** to remove the selected idea after confirmation. See [`docs/idea-pool-management.md`](docs/idea-pool-management.md) for editing, stage, routing, and deletion behavior.

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
