# GameDesignWizard

GameDesignWizard is an English-first, offline Windows desktop application for shaping game ideas, organizing a local idea pool, and exporting structured game design documents.

## Current prototype

Prototype 0.20.0 validates the visual direction, persistent dynamic catalogs, the complete six-step wizard, local idea saving and editing, searchable pool views, managed media attachments, PDF and Markdown GDD export, complete and filtered idea-pool XLSX export, bulk catalog exchange, distributable defaults, and the first tested local-AI editing slice:

- Home navigation
- Platform selection with data-bound buttons and a visible selected state
- Genre selection with dependent subgenre choices
- Searchable, virtualized topic transfer lists designed for hundreds of entries
- Reusable topic, feature, art-style, and mechanic transfer lists
- Double-click or Enter keyboard transfer between available and selected options
- Repeatable reference URL and note rows with inline validation
- Game name, overview, production-scope choices, and editable GDD sections
- Optional local llama.cpp setup using user-selected runtime and GGUF files
- One-section-at-a-time local writing improvement with cancel, review, accept, discard, and one-level undo
- An isolated model process bound to loopback with a per-run API key and no web interface
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
- Selected-idea Markdown export with readable metadata, editable GDD text, clickable references, and media notes for GitHub or plain-text workflows
- Home-screen export of a five-page printable blank GDD template with project metadata fields, English guidance, and writing areas for all twelve default sections
- Complete XLSX export across the PC, Mobile, and Other pools, including lossless selection, GDD, reference, and media detail sheets
- Fixed PC, Mobile, and Other idea-pool groups
- Settings management for platforms, genres, subgenres, topics, mechanics, features, art styles, development durations, and team sizes
- Inline catalog renaming and persistent ordering while stable identities, subgenre parents, and platform pool routing remain intact
- Combined catalog name and parent search with active or archived status filtering
- A recycling virtualized Settings list suitable for catalogs containing hundreds of options
- Protected ordering controls that require filters to be clear before changing the saved global order
- SQLite-backed add, archive, and restore behavior that survives app restarts
- Parent-genre selection for subgenres and dependent archival when a genre is archived
- TXT and single-column XLSX import with a validation preview and atomic save
- Round-trip UTF-8 TXT and XLSX export of active options in their current order, scoped to one parent genre for subgenres
- One versioned English JSON source for all nine default catalog categories
- A maintainer command that promotes the active local catalog into the next bundled default catalog
- Custom-platform routing explained as Other

Catalog data is stored locally in `%LOCALAPPDATA%\GameDesignWizard\game-design-wizard.db`.

Local AI is optional. Its runtime and model paths are stored in `%LOCALAPPDATA%\GameDesignWizard\local-ai-settings.json`; model weights are never added to the repository or required for ordinary editing and export. The first quality candidate is Qwen3.5-9B Q3_K_M (about 4.67 GB), downloaded separately from its model publisher. Open **Local AI Setup** in Settings, then see [`docs/local-ai-setup.md`](docs/local-ai-setup.md) for manual setup and pinned test files. The first [device benchmark](docs/local-ai-benchmark-qwen35.md) records three CUDA edits and one CPU baseline; the full quality gate is still pending.

Saved image and audio attachments are copied into `%LOCALAPPDATA%\GameDesignWizard\media` and referenced by relative paths. Moving or deleting the original source file does not break the saved idea.

Choose **Export PDF** beside Blank GDD Template on Home to create a printable planning document without first saving an idea. Select a saved row in Idea Pool and choose **Export PDF** to create its filled A4 game design document. See [`docs/pdf-export.md`](docs/pdf-export.md) for both document contracts.

Select a saved row and choose **Export Markdown** to create a portable, editable GDD that renders directly on GitHub. See [`docs/markdown-export.md`](docs/markdown-export.md) for the file contract and media behavior.

Choose **Export All XLSX** in Idea Pool to create one workbook containing every saved idea, or **Export Filtered XLSX** to export only the current search, stage, and pool result. Both exports keep the fixed pool views easy to scan and include normalized detail sheets for complete data. See [`docs/idea-pool-export.md`](docs/idea-pool-export.md) for the workbook contract.

Select an Idea Pool row and choose **Edit** to reopen its complete saved snapshot in the wizard. Choose **Delete** to remove the selected idea after confirmation. See [`docs/idea-pool-management.md`](docs/idea-pool-management.md) for editing, stage, routing, and deletion behavior.

All default options are versioned in [`catalog-source/en/default-catalog.json`](catalog-source/en/default-catalog.json). The JSON is embedded during build, so a fresh GitHub checkout starts with these defaults without importing any files. Application upgrades add missing shipped defaults while preserving local custom options and archive states. See [`catalog-source/README.md`](catalog-source/README.md) for the maintainer workflow.

To exchange options with one local installation, select a category in Settings and use **Import TXT / XLSX** or **Export TXT / XLSX**. Names can also be edited and reordered directly in the table. See [`docs/catalog-import.md`](docs/catalog-import.md) for the supported file layout and behavior.

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
