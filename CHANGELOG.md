# Changelog

## 0.14.0

- Make the PC, Mobile, and Other summary cards clickable pool filters with a visible selected state.
- Add case-insensitive Idea Pool search across names, platforms, genres, subgenres, topics, mechanics, features, art styles, production scope, and stage.
- Add a development-stage filter that combines with the pool and search filters.
- Show visible and total result counts, clear a selection when it leaves the filtered view, and restore every filter with one action.
- Add Export Filtered XLSX with the exact ideas and current view order shown in the table.
- Keep Export All XLSX independent from active filters.
- Disable filtered export when no ideas match while leaving other applicable actions consistent.

## 0.13.0

- Add Edit and Delete actions for the selected Idea Pool row.
- Restore the complete saved snapshot into the six-step wizard and update the same record on save.
- Preserve archived catalog selections while editing so older ideas do not lose historical values.
- Allow Idea, Concept, Prototyping, Completed, and Shelved development stages to be selected in the final wizard step.
- Recalculate PC, Mobile, or Other routing when an edited idea changes platform.
- Preserve the original idea ID and creation timestamp while refreshing its update timestamp.
- Confirm deletion in the UI and remove the idea's managed image and audio files after the database record is deleted.
- Disable competing Idea Pool actions while an edit, delete, PDF export, or workbook export operation is running.
- Start a clean, independently identified draft when Create Game Idea is chosen after saving or editing an idea.

## 0.12.0

- Add complete XLSX export to the Idea Pool without requiring a row selection.
- Include summary sheets for all ideas and the fixed PC, Mobile, and Other pool groups.
- Preserve the actual platform label for ideas routed to Other.
- Add normalized sheets for selections, GDD content, references, and managed media metadata.
- Split long GDD text safely across ordered cells so content beyond Excel's cell limit is not lost.
- Freeze headers, add filters, hide gridlines, and apply restrained workbook formatting for practical review.
- Write workbook output to a temporary file and replace the chosen destination only after generation succeeds.

## 0.11.0

- Add selected-idea PDF export to the Idea Pool.
- Render A4 game design documents with a title, metadata table, overview, populated GDD sections, and page numbers.
- Include managed images and captions while keeping audio attachments as explicit filename and caption notes.
- Render reference URLs as readable, clickable links.
- Bound image dimensions to the printable page area and identify unavailable image files without failing the whole export.
- Write PDF output to a temporary file and replace the chosen destination only after rendering succeeds.
- Add the Windows-specific document project with PDFsharp and MigraDoc 6.2.4.

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
