# Changelog

## 0.10.0

- Add image and audio attachments with optional English captions to the final wizard step.
- Accept PNG, JPEG, WebP, GIF, BMP, MP3, WAV, M4A, OGG, and FLAC files.
- Copy attachments atomically into per-idea managed local storage when an idea is saved.
- Persist managed relative paths so saved attachments survive source-file moves and deletions.
- Delete removed managed files only after a successful idea save and roll back new copies when saving fails.
- Reject unsupported extensions and managed paths that leave the application media directory.

## 0.9.0

- Add wizard step 6 with game name, overview, development duration, and team-size fields.
- Add twelve optional English GDD sections with focused writing guidance.
- Save the complete six-step idea snapshot to SQLite with Idea as its initial stage.
- Default a blank game name to NewGame and route an unspecified platform to Other.
- Preserve catalog labels, GDD content, and nonblank references inside the saved document.
- Update the same saved idea when Save Changes is selected instead of creating a duplicate.
- Show saved ideas in a local read-only pool table with live PC, Mobile, and Other counts.

## 0.8.0

- Add wizard step 5 with repeatable reference URL and note rows.
- Allow references to be added and removed without losing earlier wizard selections.
- Validate nonblank URLs inline and accept complete HTTP or HTTPS links.
- Keep incomplete reference rows in the draft while preventing progression with malformed URLs.
- Ignore completely blank reference rows in the wizard summary and future saved idea data.

## 0.7.0

- Add wizard step 4 for multi-selection of gameplay mechanics.
- Reuse the searchable, virtualized dual-list control and its double-click and Enter interactions.
- Refresh mechanic choices immediately after Settings additions, imports, archives, and restores.
- Preserve mechanic selections while filtering or refreshing the catalog.
- Extend forward, previous, summary, and progress behavior through the fourth wizard step.

## 0.6.0

- Add wizard step 3 for independent multi-selection of features and art styles.
- Reuse one searchable, virtualized dual-list control for topics, features, and art styles.
- Support double-click and Enter transfer in every dual-list picker.
- Preserve selected options while filtering a picker or refreshing its catalog from Settings.
- Expand the wizard progress indicator to six distinct steps.

## 0.5.0

- Move every built-in catalog category into one versioned English JSON source.
- Embed default catalog data so fresh installations require no manual import.
- Merge missing shipped defaults on startup without restoring archived items or deleting custom entries.
- Add a catalog maintenance command that exports all active local options as the next distributable default catalog.
- Validate identifiers, category coverage, subgenre relationships, names, metadata, and fixed PC/Mobile pool routing before defaults are loaded or exported.

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
