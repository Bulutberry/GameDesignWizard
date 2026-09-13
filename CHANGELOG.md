# Changelog

## 0.4.0

- Replace topic plus and minus buttons with double-click transfer in both lists.
- Support Enter as the keyboard equivalent for topic transfer.
- Import UTF-8 TXT and single-column XLSX files into the selected Settings category.
- Preview ready, existing, duplicate, invalid, and formula rows before import.
- Apply every accepted import in one SQLite transaction.
- Require one selected parent genre when importing subgenres.

## 0.3.0

- Add wizard step 2 for genre, dependent subgenre, and multi-topic selection.
- Add search without losing existing topic selections.
- Virtualize topic lists for catalogs containing hundreds of entries.
- Show a visible selected state for platform, genre, and subgenre buttons.
- Load built-in English topics from a one-item-per-line source file.
- Merge newly shipped default topics into existing local catalogs on startup.

## 0.2.0

- Add a path-independent Windows launcher for local development.
- Expand Settings to all nine editable catalog categories.
- Add an initial English fixture for genres, subgenres, topics, mechanics, features, art styles, development durations, and team sizes.
- Require a parent genre for every new subgenre.
- Archive active child subgenres when their parent genre is archived.

## 0.1.0

- Persist the platform catalog locally with SQLite.
- Seed built-in platforms with stable identifiers.
- Preserve custom platforms and archived states across app restarts.
- Route every custom platform to the fixed Other idea-pool group.
- Split domain contracts and data access into Core and Infrastructure projects.

## 0.0.2

- Fix navigation labels inheriting the dark body-text color.
- Use a lighter navy sidebar with brighter labels and larger supporting text.
- Highlight the current page and show a visible keyboard-focus border.
- Keep the selected navigation item synchronized when opening a page from Home.

## 0.0.1

- Add the initial English Windows desktop prototype and project plan.
- Add Home, the platform-selection step, the idea-pool overview, and platform Settings.
- Support adding, archiving, and restoring platforms in memory.
- Fix a read-only binding error during window startup.
