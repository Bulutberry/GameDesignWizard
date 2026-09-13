# GameDesignWizard Project Plan

Version 2.0 | September 13, 2026 | Status: implementation in progress

GameDesignWizard will be an English-first Windows desktop application for developing game ideas through a six-step wizard, organizing them in a searchable idea pool, and producing editable GDDs and polished PDF documents. Users will be able to customize every selection catalog and exchange catalogs through Excel files. An optional small local language model will help draft and improve GDD text.

The selected foundation is C# with .NET 10, WPF, SQLite, and an optional LLamaSharp inference worker. Excel is the bulk editing format; SQLite is the application database; versioned JSON is the repository format for default catalogs. PDF layout remains deterministic application code.

This plan incorporates the supplied `Proje-detaylari.docx` and the latest requirements in the conversation. The latest requirements take precedence: Windows only, English throughout the initial product and source code, all listed catalogs editable, and custom platforms routed to Other. It supersedes the earlier `GameDesignWizard-Proje-Plani.md`. Application implementation is in progress; model benchmarking and release packaging have not yet been performed.

## 1 Product boundaries and decisions

| Area | Decision |
| --- | --- |
| Operating system | Windows desktop only. Windows 11 x64 is the initial release test baseline. Windows 10 compatibility is a separate verification target; publish an exact supported OS matrix before release. |
| Language | English UI, default data, templates, prompts, comments, identifiers, errors, developer documentation, and repository materials. |
| Offline operation | Creating, editing, saving, importing, exporting, and local inference work offline. Model downloads are optional; manually importing a compatible model is supported. |
| Idea creation | The user makes selections and writes notes. The Create Game Idea button opens the wizard; it does not automatically generate a random idea or invoke AI. |
| Catalog customization | Platforms, genres, subgenres, topics, mechanics, features, art styles, development durations, and team sizes are editable in Settings. |
| Data ownership | Ideas, catalogs, GDD content, images, and audio remain on the user's computer. No account or hosted database is required. |
| AI | Optional assistance for individual GDD sections. Saving an idea and exporting a document never require a model. |
| Sharing | Whole-pool and platform-pool XLSX/CSV exports; individual PDF, DOCX, and Markdown exports; an archive format for complete restoration. |
| Open source | MIT for original application code and documentation. Preserve separate notices for libraries, fonts, models, and contributed assets. |

The original document's Android target and simultaneous rollout of sixteen languages move out of the initial scope. Image generation, speech transcription, cloud synchronization, collaboration accounts, and model training are also outside v1.0. Recording audio is included; understanding or transcribing it is a different feature.

## 2 Technology selection

| Responsibility | Selection | Reason and primary reference |
| --- | --- | --- |
| Application language and runtime | C# and .NET 10 LTS | One primary application language. .NET 10 is supported through November 2028. [Microsoft support policy](https://dotnet.microsoft.com/en-us/platform/support/policy) |
| Desktop interface | WPF with XAML and MVVM | Fits a Windows-only application with dynamic selection controls, forms, local media, and a DataGrid. .NET 10 continues WPF development. [WPF documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/whats-new/net100) |
| View models | CommunityToolkit.Mvvm | Observable state and commands with a small MVVM foundation. [Microsoft toolkit documentation](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) |
| Persistence | SQLite with Microsoft.EntityFrameworkCore.Sqlite | Local storage, relationships, transactions, and schema migrations. [Microsoft provider documentation](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/) |
| PDF documents | PDFsharp and MigraDoc | MIT-licensed PDF generation and document layout. [License](https://docs.pdfsharp.net/General/License/License.html), [document example](https://docs.pdfsharp.net/MigraDoc/Topics/Start/HelloMigraDoc.html) |
| Excel files | ClosedXML | Read and write XLSX without requiring Microsoft Excel. The library is MIT licensed. [Project documentation](https://github.com/ClosedXML/ClosedXML) |
| Word documents | DocumentFormat.OpenXml | Create DOCX documents with real headings, paragraphs, tables, and images. [Open XML SDK](https://github.com/dotnet/Open-XML-SDK) |
| Local language model | LLamaSharp with a matching llama.cpp CPU backend | C# integration with local GGUF models. Pin the wrapper and native backend together. [LLamaSharp](https://github.com/SciSharp/LLamaSharp) |
| Images | Built-in WPF image decoding and resizing | Covers the initial PNG/JPEG media workflow without a separate image processing dependency. |
| Audio | NAudio, initially recording WAV | Local microphone recording and playback. [NAudio](https://github.com/naudio/NAudio) |
| Interchange and localization | System.Text.Json and UTF-8 resource files | Versioned, reviewable data and translation files. |
| Development and validation | .NET SDK CLI, Git, xUnit, GitHub Actions on Windows | Build and test from documented commands; a commercial IDE must not be required. Use built-in test assertions initially. |
| Distribution | Self-contained win-x64 publish folder in a portable ZIP | Include required runtime/native files. Add a per-user installer after the portable release has passed clean-machine checks. |

WPF is selected because the project is explicitly Windows-only and centers on editable tables and forms. Cross-platform UI infrastructure would not satisfy an additional current requirement. The model runtime is isolated behind an interface, so future model changes will not require rewriting the application UI.

PDFsharp/MigraDoc is the initial renderer because English documents and permissive reuse are the immediate priorities. QuestPDF has eligibility-based licensing, so this plan does not make it a mandatory dependency. The previous plan's blanket statement about all high-revenue downstream users needing a paid QuestPDF license should not be reused; the current guide includes several distinct eligibility categories. [QuestPDF licensing guide](https://www.questpdf.com/license/guide.html)

Use bundled fonts with redistribution permission and preserve their license notices. Noto fonts are a candidate under the SIL Open Font License. Add language-specific font packs when those languages ship. UTF-8 storage alone does not prove PDF shaping, CJK layout, or RTL correctness; those require a separate rendering milestone. [Noto font license](https://github.com/notofonts/noto-fonts/blob/main/LICENSE)

Lock the tested SDK in `global.json` and packages in central package configuration and lock files during implementation. Select exact stable package versions after the initial integration experiment, rather than describing untested package combinations as verified.

## 3 Screens and behavior

### Home

Home contains Create Game Idea, Idea Pool, and Settings. Offer Resume Draft when an unfinished draft exists. New installations open directly into the usable application; downloading a model is optional and does not block onboarding.

### The six-step wizard

| Step | Contents | Selection behavior |
| --- | --- | --- |
| 1 Platform | Dynamic buttons for PC, Mobile, Console, VR, Board Game, and enabled custom platforms | Zero or one platform in v1.0 |
| 2 Genre and topics | Dynamic genre buttons, dependent subgenre buttons, and a topic transfer control on the same scrollable page | Zero or one genre and subgenre; multiple topics |
| 3 Features and art style | Two separate transfer controls | Multiple features and art styles |
| 4 Mechanics | Mechanic transfer control | Multiple mechanics |
| 5 References | Repeatable URL and note rows | Multiple references; no automatic network retrieval |
| 6 Describe your game | Name, overview, GDD sections, images/audio, duration and team-size dropdowns, Save | Name defaults to NewGame; optional fields stay optional |

Every step has Previous, Next, and Home navigation. A blank step can be skipped. Blank references are omitted; a nonblank malformed URL receives inline validation without losing the draft. Stage is not requested during initial creation.

The single platform/genre/subgenre choice is an explicit first-release design assumption. Topics, mechanics, features, and art styles support multiple selections. Multi-platform and multi-genre ideas can be added later if actual usage requires them.

Use one reusable DualListPicker for topics, mechanics, features, and art styles. Both available and selected lists have search, scrolling, keyboard selection, and plus/minus actions. Search must not erase selections. An item cannot appear twice in one selection set.

Use data-bound button collections for platform, genre, and subgenre controls. Support wrapping, scrolling, search, visible selection, and long labels. Hundreds of entries must not be represented by individually hardcoded XAML buttons. Use pagination or a virtualizing layout where needed; a plain wrapping panel is not assumed to provide virtualization.

Subgenres belong to a genre in v1.0. Changing the genre clears an incompatible subgenre in the draft with a visible explanation and undo. The user can skip the subgenre. Committed Settings changes refresh catalog views without restarting the app.

Drafts autosave after a short debounce and at step changes. Home preserves the draft. Drafts are stored separately from saved pool entries. Save promotes the draft to a saved idea atomically and prevents double-click duplicate records.

### Idea Pool

The entry screen always contains three fixed groups: PC, Mobile, and Other. These are pool navigation groups, separate from editable platform options.

| Selected platform | Pool destination |
| --- | --- |
| Built-in PC identity | PC |
| Built-in Mobile identity | Mobile |
| Built-in Console, VR, Board Game, or other non-PC/mobile options | Other |
| Any user-created or externally imported custom platform | Other |
| No platform selected | Other, displayed as Unspecified |

Routing uses immutable identity and metadata, never the displayed name. Renaming a custom platform to PC does not move it into the PC group. Removing the built-in PC option hides its wizard button but preserves the PC pool and previously saved ideas. An idea retains its saved platform label and pool group even after the catalog changes. Explicitly changing an idea's platform recalculates its destination.

Keep the specification's nine default data columns: Name, Genre, Subgenre, Topics, Features, Art Style, Team Size, Estimated Duration, and Development Stage. Provide optional Platform and Mechanics columns, with Platform visible by default inside Other so custom platforms are distinguishable. Users can sort, search, filter, resize, and reorder columns.

Each row provides Edit, Export, and Delete. Edit opens the saved idea in the wizard and exposes Development Stage for existing ideas. New ideas default to Idea. Other stages are Concept, Prototyping, Completed, and Shelved. Normalize the source document's overlapping initial labels, Idea Stage and Raw Idea, to Idea. Stage colors supplement a readable text label.

Offer Export This Pool, Export All Ideas, and an explicitly labeled Export Filtered Results. Export All Ideas includes every saved idea across all three groups, regardless of the active filter or pagination. Drafts are included in backup archives, not in normal pool exports.

### Settings

Settings contains Catalogs, GDD Templates, Appearance, Data and Backup, and Local AI. Language resources exist from the start, but English is the only selectable UI language until another language is complete enough to ship.

Catalogs exposes all nine categories: Platforms, Genres, Subgenres, Topics, Mechanics, Features, Art Styles, Development Durations, and Team Sizes. Each supports Add, Rename, Reorder, Remove, Restore, Search, Import, and Export. Subgenre editing also selects its parent genre. A table-style editor inside Settings makes small changes possible without any spreadsheet application.

Use archive semantics for Remove: the option disappears from new selections while existing ideas retain it. Explain this briefly in the removal UI and offer undo. Removing a genre previews and archives its active subgenres in the same operation. GDD template changes affect new sections/ideas; they do not silently replace text or headings already saved in ideas.

## 4 Excel catalogs and data flow

Using Excel makes the content author's work easier and is supported by the plan. There are three distinct representations:

| Representation | Purpose | Authority |
| --- | --- | --- |
| Catalog workbook, CSV, or text | Author or bulk-edit lists outside the app | An import proposal until committed |
| Versioned JSON seed files | Ship defaults and review community changes on GitHub | Defaults for new installations and explicit catalog updates |
| Local SQLite database | Store the user's active catalog, overrides, ideas, and relationships | Runtime source of truth |

Do not read and write a live XLSX workbook for every UI action. This would introduce file locks, ambiguous edits, and weak relationship handling. The supported spreadsheet workflow is Export Catalog, edit and save the file, then Import Catalog and review the changes. Settings edits write directly to SQLite; export creates a fresh workbook whenever requested. No automatic two-way workbook synchronization is included in v1.0.

### Workbook contract

Provide a downloadable/exportable `catalog-template.xlsx` during implementation. Use these sheets:

| Sheet | Main columns |
| --- | --- |
| Metadata | SchemaVersion, CatalogVersion, BaseCatalogRevision, SourceLanguage |
| Platforms | Id, NameEn, DescriptionEn, SortOrder, IsActive |
| Genres | Id, NameEn, DescriptionEn, SortOrder, IsActive |
| Subgenres | Id, ParentGenreId, NameEn, DescriptionEn, SortOrder, IsActive |
| Topics | Id, NameEn, DescriptionEn, SortOrder, IsActive |
| Mechanics | Id, NameEn, DescriptionEn, SortOrder, IsActive |
| Features | Id, NameEn, DescriptionEn, SortOrder, IsActive |
| ArtStyles | Id, NameEn, DescriptionEn, SortOrder, IsActive |
| DevelopmentDurations | Id, NameEn, MinMonths, MaxMonths, SortOrder, IsActive |
| TeamSizes | Id, NameEn, MinPeople, MaxPeople, SortOrder, IsActive |
| Translations | OptionId, Locale, DisplayName, Description, SourceHash, ReviewStatus |

English labels are required for distributed defaults. User-authored labels may contain any Unicode text: an English interface must not prevent a user from adding a topic in their own language. Numeric ranges are optional and separate from labels; do not attempt to infer them from text such as Small Team.

For new rows, the author may leave Id blank. Import assigns a UUID and a subsequent export contains the stable Id. Keep existing IDs when renaming rows or exchanging translations. Subgenres reference the genre ID; template validation can display the corresponding genre name. A first import from a simple Genre/Subgenre name workbook can resolve names in a preview and assign IDs before saving.

Built-in identities are seeded by the application. New rows are always custom records; imports cannot claim a built-in identity or PC/Mobile routing merely through their name or an editable flag. SchemaVersion identifies the interchange format, CatalogVersion identifies the distributed dataset, and BaseCatalogRevision detects a workbook exported before later local edits.

Accept simple UTF-8 text imports with one option per line into a selected category. Accept CSV into a selected category and JSON using the canonical schema. Text import handles flat lists; parent relationships and translations use the workbook or JSON format. Free spreadsheet editors can be used; the application itself does not depend on an Office installation.

### Import and removal rules

1. Parse into a staging model without changing the database. Check schema version, categories, duplicate IDs, blank labels, parent references, numeric ranges, and conflicting normalized names. Reject formulas in catalog data columns and never execute macros or external workbook links.
2. Show a preview of additions, renames, updates, archives, conflicts, and errors with sheet and row locations. Trim accidental whitespace, but preserve the user's intended visible spelling.
3. Match updates by stable ID. Name-only matches are candidates for review, not permission to overwrite an existing identity. Importing a workbook twice must not duplicate its existing records.
4. Default to merge: rows absent from a workbook remain unchanged. `IsActive=false` explicitly removes an option from new selections. Offer Replace Selected Categories separately; omitted options are then archived only after its impact preview is accepted.
5. If BaseCatalogRevision is stale, identify overlapping changes and require conflict resolution rather than silently overwriting newer Settings edits.
6. Apply accepted changes in one transaction. Invalid imports leave the original database untouched. Preserve a recoverable backup for replacement operations and record the resulting catalog revision.

This fulfills the Space example: archive Space, add Dogs, and the next topic picker shows Dogs but not Space. Existing ideas still show Space and export it correctly. Editing an existing idea retains its archived selection unless the user explicitly removes or replaces it.

### Repository contribution workflow

The maintainer can prepare hundreds of English entries in a workbook. A catalog import/conversion command validates that workbook and writes consistently ordered JSON for review. Commit the JSON and the blank workbook template; generated working spreadsheets need not become competing default datasets. Community contributors can edit JSON directly or use the same workbook conversion workflow.

On application upgrades, default catalog updates run as a versioned merge. They must not restore options the user removed, overwrite renamed labels, or erase custom items. Track the original seed values and local overrides so untouched defaults can receive updates. Restore Defaults is an explicit Settings operation with an impact preview, not an upgrade side effect.

## 5 Persistence and application architecture

Use stable IDs for catalog options, saved selections, templates, and ideas. Labels are content, not primary keys.

| Entity | Essential fields and purpose |
| --- | --- |
| CatalogOption | Id, Category, NameEn, DescriptionEn, ParentGenreId, SortOrder, IsActive, Origin, SeedVersion, optional numeric bounds, immutable platform pool mapping |
| CatalogOverride | OptionId and locally overridden fields/archive state, separate from the shipped seed baseline |
| OptionTranslation | OptionId, Locale, DisplayName, Description, SourceHash, ReviewStatus |
| GameIdea | Id, Name, Stage, PoolGroupSnapshot, CreatedAtUtc, UpdatedAtUtc, ContentLanguage, Revision |
| IdeaSelection | Id, IdeaId, Category, OptionId, LabelSnapshot, DescriptionSnapshot, selected order, and applicable numeric snapshots |
| WizardDraft | Id, current step, draft payload, schema version, updated timestamp |
| GddTemplate and TemplateSection | Stable IDs, version, English section titles, guidance, and ordering |
| IdeaGddSection | Id, IdeaId, template reference, saved title, order, user notes, accepted text |
| MediaAsset | Id, IdeaId, optional SectionId, kind, relative path, original filename, caption, checksum, order, optional duration |
| ReferenceLink | Id, IdeaId, Url, Note, SortOrder |
| AppSettings and CatalogRevision | Typed settings, active preferences, and import/update revision tracking |

Enforce at most one platform, genre, subgenre, duration, and team-size selection per idea in v1.0. Enforce unique multi-selections and valid subgenre-parent relationships. Save selections and their display snapshots together. Later catalog renames, translations, or removals do not rewrite historical idea content. Relabeling existing ideas would be a separate explicit action.

Store user data below `%LOCALAPPDATA%\GameDesignWizard\`: `data.db`, `media`, `models`, `backups`, and bounded diagnostic logs. Copy attached images/audio into managed storage so moving the original source file does not break an idea. Logs exclude GDD text, prompts, and model output by default.

Use database migrations with a validated backup and recovery path. Backup creation must use a consistent SQLite snapshot, not an unchecked copy of an active database and its journal files. On restore, validate the archive and schema before replacing active data. Archive extraction must reject paths outside the selected restore directory.

An explicit portable mode stores data beside the application only when that location is writable. A portable ZIP may still use LocalAppData by default; document the distinction. An unwritable location must produce a clear recovery choice without data loss.

Proposed repository layout:

```text
GameDesignWizard/
  src/
    GameDesignWizard.App/              WPF views, view models, resources
    GameDesignWizard.Core/             Domain rules and service contracts
    GameDesignWizard.Infrastructure/   SQLite, catalogs, files, backups, audio
    GameDesignWizard.Export/           PDF, DOCX, XLSX, CSV, Markdown
    GameDesignWizard.Ai.Worker/        On-demand local inference process
  tests/
    GameDesignWizard.Core.Tests/
    GameDesignWizard.Integration.Tests/
  data/catalogs/en/
  resources/localization/en.json
  resources/gdd-templates/
  resources/prompts/en/
  assets/fonts/
  tools/catalogs/
  docs/
  .github/workflows/
  global.json
  Directory.Packages.props
  README.md
  CONTRIBUTING.md
  THIRD_PARTY_NOTICES.md
  LICENSE
```

Core defines contracts for storage, catalog import, export, and text assistance without WPF or model dependencies. Infrastructure and Export implement those contracts. App composes them. Do not add a generic plugin framework, web server, or vector database for this initial workflow.

Run LLamaSharp in an on-demand worker process communicating through a current-user-restricted named pipe. This contains native inference crashes and allows cancellation/restart without closing the editor. Start with a single generation request at a time. The worker receives only the relevant textual context and cannot modify the database. The application validates messages and decides whether an accepted suggestion becomes saved content.

## 6 GDD templates and export contracts

The GDD is a structured document, not a PDF blob. Persist sections, content, references, and media separately so users can edit them and choose different output formats later.

The initial template includes Overview, Design Pillars, Target Platform and Audience, Genre and Themes, Core Gameplay Loop, Mechanics and Systems, Progression, Characters, World and Lore, Art Direction, Technical Features, Production Scope, Risks and Open Questions, and References. These headings are proposed defaults, not a requirement to complete every field.

Provide Export Blank Template and Export Idea. The blank template contains English headings and writing guidance. Export Idea fills metadata from the saved selections and renders the user's accepted text. Empty sections can be omitted or retained with a Not specified placeholder through an export option. Never fabricate missing content as part of exporting.

| Output | Contents and limits |
| --- | --- |
| PDF | A4 or Letter layout, title page, metadata, ordered sections, images/captions, references, page numbers, and bookmarks/table of contents where useful. Selectable text and embedded licensed fonts. |
| DOCX | The same logical content using Word heading styles, editable paragraphs/tables, and embedded images. Pagination may differ from PDF. |
| Markdown | Editable UTF-8 GDD text. Offer a ZIP with relative media paths when attachments must travel with it. |
| XLSX pool export | An All Ideas sheet plus PC, Mobile, and Other sheets. Include stable idea IDs, all metadata selections including mechanics and actual platform names, stages, and timestamps. Use formatting, wrapped cells, filtering, and frozen headers. |
| CSV pool export | One UTF-8 table for the chosen scope with proper quoting. Multiple selections use documented JSON-array cells to preserve labels containing separators. No media or document layout. |
| Full archive | A versioned ZIP container with manifest, portable structured data, all ideas/drafts/catalog overrides/translations/templates, and media. Model weights are excluded by default. Restore uses this archive, not PDF or XLSX. |

Pool exports represent the complete table data for the selected scope. Include an Idea Details sheet for GDD text and a References sheet when exporting the full workbook. Split text that exceeds a spreadsheet cell limit into ordered chunks with IdeaId, SectionId, and ChunkIndex; never silently truncate. Binary images and recordings are listed in a Media Index, with a companion archive option. Action buttons and purely visual UI state are not data columns.

PDF/DOCX are document exchange formats, and XLSX/CSV are data views. They are not promised as lossless application backups. Preserve complete editable state with the archive format, including IDs and custom catalog entries. Standard archive import is included by v1.0; general idea import from arbitrary third-party spreadsheets is deferred.

Recorded audio is represented in PDF/DOCX by a caption and attachment filename, with files supplied in an optional companion ZIP. Do not promise universally playable audio inside PDFs or imply that the language model can hear recordings. User-entered transcription text can be included like any other note.

Render exports from an immutable snapshot of the idea. Write to a temporary output and replace the destination only after success; cancellation must not leave a corrupt final file. Store user-controlled XLSX values as text and protect CSV exports against formula interpretation. Test long labels, very long notes, large images, URLs, and multiple page sizes.

## 7 Small local AI strategy

The model improves text; it does not decide the PDF layout. This separates writing quality from reliable headings, page breaks, tables, and image placement. No fine-tuning or separate model-training pipeline is required for the initial release.

### Initial model candidates

Use Qwen3-4B-Instruct-2507 as the initial quality baseline. Its publisher identifies it as a 4.0B-parameter, non-thinking model under Apache 2.0. This is a candidate for the application's workload, not a claim that it is the best small model available. [Publisher model card](https://huggingface.co/Qwen/Qwen3-4B-Instruct-2507)

Compare Qwen3-1.7B as a smaller alternative with thinking disabled through the correct chat template. The official GGUF repository currently provides a Q8_0 file; a Q4 variant would need a separately verified conversion or source. [Publisher model card](https://huggingface.co/Qwen/Qwen3-1.7B-GGUF), [published files](https://huggingface.co/Qwen/Qwen3-1.7B-GGUF/tree/main)

| Candidate | Proposed experiment | Preliminary resource budget |
| --- | --- | --- |
| Qwen3-4B-Instruct-2507 | GGUF Q4_K_M, short section prompts | Roughly 2.5-3 GB weights; begin tests on 16 GB RAM machines and then evaluate 8 GB systems |
| Qwen3-1.7B | Compare official Q8_0 with a verified Q4_K_M build | Official Q8_0 download is listed at about 1.83 GB; Q4 estimate is roughly 1-1.3 GB. Begin testing on 8 GB systems |
| AI disabled | Full editing and export workflow | Measure application memory independently of inference |

These are planning estimates, not minimum requirements or measured performance. Runtime memory also includes the context/KV cache, working buffers, application, and OS. The final model revision, quantization, file sizes, checksums, RAM requirements, and expected speed are release outputs from benchmarking.

Do not assume that a GGUF file for the 4B instruction variant is publisher-hosted. Verify a specific quantized artifact and its provenance or reproducibly convert the official checkpoint with a pinned llama.cpp toolchain. Conversion is a maintainer task; end users do not need Python. Use an exact tested model/runtime pair. LLamaSharp explicitly documents model and native-backend compatibility concerns. [LLamaSharp integration guidance](https://github.com/SciSharp/LLamaSharp)

### User interaction and generation

Each GDD section offers Draft from Notes and Improve Writing. Drafting uses selected attributes and any notes; if neither provides useful context, ask for content within the editor. Improving an existing section preserves its stated design constraints. Missing details remain questions or clearly identified suggestions.

Only an explicit AI action starts inference. Stream the proposal into a preview with Accept, Insert, Regenerate, and Discard actions. Keep original notes and allow undo after acceptance. Never automatically replace a user's text or accept a generated suggestion during PDF export.

Use section-specific English prompts with a compact shared game summary. Generate one section at a time, reserving context space for the answer. Begin evaluation with a 4,096-token context and a 256-512-token output limit. These are application experiment settings, not the model's maximum capability. Use the correct model chat template and tune generation settings against examples rather than one universal prompt.

Bound generation time and output, support cancellation, and unload the worker after inactivity. On an unavailable model, unsupported CPU/backend, corrupted file, or out-of-memory error, keep all editing/export functions operational and present a recoverable AI-specific message.

Model management shows download size and license before an optional download. Support progress, cancellation, resumable downloads where supported, checksum verification, manual local-file import, deletion, and model switching. Maintain a manifest with source repository, revision, filename, SHA-256, license, chat-template identity, supported backend, and benchmark notes. No model weights go into normal Git history or the base application ZIP.

CPU inference is the release baseline. Evaluate a GPU backend only after CPU integration works; ship it as a separately tested option if it provides a worthwhile improvement. Do not require Ollama, a cloud API key, or a Python installation to use the released app.

### Early evaluation and release gate

Build a fixed set of at least 30 English GDD-section cases covering different genres, constraints, sparse notes, contradictory input, and long selections. Score adherence to user facts, usefulness, clarity, repetition, and unsupported assertions. Record cold/warm startup, time to first token, output speed, peak memory, cancellation, and failure recovery on identified hardware.

Proposed quality gate: at least 90 percent of cases preserve all explicit constraints and at least 80 percent are judged useful with minor editing. These are target acceptance criteria, not achieved results. Choose the smallest candidate that meets the agreed quality bar with acceptable measured latency. Keep only the model profiles that pass; do not promise several profiles solely from their parameter counts.

## 8 English first and later translation

Ship English resources first. All newly authored code comments, tests, logs, prompts, templates, sample content, and repository documentation use English. Preserve Unicode in user content without forcing the user to write English.

Separate three kinds of language data:

| Data | Translation behavior |
| --- | --- |
| Interface strings | Stable resource keys in `en.json`, later `tr.json` and other locale files |
| Catalog labels and descriptions | Translations keyed by immutable option ID and locale |
| User-authored ideas and GDD text | Retained as authored; translating them requires a separate deliberate operation |

Missing translations fall back to the English label, then to the user's original label where there is no English version. Switching UI language never translates or rewrites stored ideas. Record source hashes so translations can be flagged as outdated after a source label changes.

Automatic translation is possible, but run it as a batch content-production workflow before shipping language packs. Export English strings plus IDs, category/context, glossary, and placeholders; translate with an available model or service; validate IDs and placeholders; then review terminology. A local model may help, but its suitability for GDD editing does not establish translation quality.

The content owner only needs to supply the English catalog initially. Translation drafts can later be prepared from those files by an AI assistant or community contributors; the owner does not have to manually translate hundreds of labels before development starts. User-created topics remain in their original language unless translations are supplied explicitly.

Use Turkish as the first localization pilot after the English workflow is stable. Expand afterward to German, French, Spanish, Portuguese, Italian, Dutch, Polish, Russian, Ukrainian, Japanese, Simplified Chinese, Korean, Arabic, and Urdu from the source specification. Validate terminology, longer button labels, fonts, PDF output, and RTL behavior before marking each language supported. Do not promise all sixteen languages in v1.0.

## 9 Delivery phases and acceptance

Effort below is a planning range for one developer familiar with C# and WPF, including relevant tests and documentation. It excludes creating the full English content catalog, translation review, and major scope changes. Re-estimate after Phase 0 and the first complete user workflow.

| Phase | Deliverable | Exit condition | Effort |
| --- | --- | --- | --- |
| 0 Technical validation | Minimal WPF/SQLite save, sample PDF, XLSX import preview experiment, isolated model experiment | A real sample document exports; a pinned model/backend runs on a documented machine; stack and resource risks are recorded | 20-35 hours |
| 1 Foundation and catalogs | Solution, English resources, storage/migrations, seed format, Settings editor, XLSX/JSON/CSV/TXT catalog import/export | Add/remove/rename every category, parent validation, safe repeated import, and seed-update preservation work | 45-70 hours |
| 2 Wizard and pool | Six steps, dynamic buttons, dual-list pickers, drafts, stage editing, three pool groups | Create and edit a complete idea; skip all optional steps; custom platform routes to Other | 60-90 hours |
| 3 Documents and data export | Blank/filled GDD, images, PDF/DOCX/Markdown, all-pool XLSX/CSV | Long documents render correctly; all ideas and selected fields export without silent truncation | 40-65 hours |
| 4 Local AI integration | Worker lifecycle, model management, streaming preview and acceptance | Benchmark gate, cancellation, offline operation, model failure recovery, and manual fallback pass | 35-60 hours |
| 5 Media and recovery | Audio recording/playback, media lifecycle, archive backup/restore, GDD template management | Restore onto a clean profile with matching ideas/media; changing templates preserves old GDDs | 30-50 hours |
| 6 Release preparation | Performance/accessibility fixes, clean-machine checks, portable package, installer if ready, contributor docs | A new user can install or unzip, work offline, export, and restore without developer tooling | 30-50 hours |
| Later localization | Turkish pilot and reviewed language packs | Per-language UI and document verification | Estimate separately |

Base estimate: 260-420 hours. With a 20 percent contingency, budget approximately 310-505 hours. At 30 productive hours per week this is roughly 11-17 weeks; at 15 hours per week, roughly 21-34 weeks. These are planning estimates, not delivery commitments.

Milestones:

- Internal prototype: Phase 0 proves the risky integrations early, including AI.
- v0.1 core preview: Phases 1-3 deliver English catalogs, customization, wizard, pool, and exports. Settings is fundamental and is not postponed to the final release.
- v0.5 AI preview: Phase 4 adds a validated local assistant to the functioning editor.
- v1.0: Phases 5-6 complete audio, restoration, template management, distribution, and release quality.
- v1.1 or later: Turkish pilot, additional languages, optional GPU profiles, broader import formats, and features supported by user feedback.

### Required acceptance scenarios

| Scenario | Expected result |
| --- | --- |
| Add Dogs and remove Space | New selections offer Dogs; saved ideas still retain Space |
| Add a genre and a related subgenre | Correct dynamic buttons appear without rebuilding or restarting |
| Remove a genre with children | Impact is shown; archived children disappear from new selection; saved ideas remain readable |
| Add a custom platform | Wizard shows a new button; saved ideas appear in Other with their actual platform name |
| Rename a custom platform to PC | Routing remains Other |
| Skip every wizard step | Save produces NewGame with Idea stage and unspecified fields, in Other |
| Change duration or team-size catalogs | Dropdowns refresh; historical idea values remain intact |
| Import the same catalog twice | No duplicate options or unintended removals |
| Import an invalid or stale workbook | No partial changes; errors/conflicts identify the affected rows |
| Upgrade built-in catalogs | Customizations and archived defaults survive |
| Export All Ideas while a filter is active | Every saved idea from PC, Mobile, and Other is included |
| Export a long GDD with media | Readable PDF/DOCX, intact images, complete text, and explicit audio references |
| Disable AI or disconnect the network | Idea creation, editing, data management, and export remain functional |
| Cancel inference or terminate its worker | Original notes and unsaved edits remain; the UI recovers |
| Move source media files | Managed copies remain available in the idea |
| Restore an archive in a fresh profile | Catalog identities, overrides, ideas, drafts, sections, and media are restored |
| Use thousands of options and saved ideas | Selection/search/grid operations meet measured responsiveness targets with bounded UI work |
| Change to a future partially translated locale | Missing labels fall back predictably; existing idea text remains unchanged |

Use unit tests for identity, routing, snapshot, and selection rules; integration tests for database/import/restore/export behavior; and manual visual checks for dynamic layouts, keyboard use, scaling, and document pagination. Performance targets are to be measured on the Phase 0 reference machine. Do not substitute snapshot-only tests for checking exported document readability.

## 10 Open source and release preparation

Publish original code and documentation under MIT so recipients can use, modify, redistribute, and commercially reuse them while preserving required notices. MIT does not require derivatives to remain open source. Library and model licenses remain separate and must be included as applicable. [MIT license text](https://opensource.org/license/mit)

Use the same documented license for original catalog/template contributions, while recording the provenance and applicable license of external assets. Do not assume that images attached by an application user are licensed for inclusion in the public repository. Generated user GDDs remain user files and are not automatically added to the project's repository or license.

Before the first public release, include an English README, build instructions, architecture notes, catalog and translation guides, CONTRIBUTING, a changelog, issue/PR templates, and THIRD_PARTY_NOTICES. Record direct and transitive dependency versions/licenses, font licenses, and supported model licenses. Recheck these when dependencies change; package names alone are not license guarantees.

The Windows CI workflow restores locked dependencies, builds, tests, validates catalog/resource schemas, and publishes a self-contained package. Model quality benchmarking is a separate controlled workflow so ordinary contributors do not need to download gigabytes to run the normal test suite.

Release artifacts contain the application and required native/runtime files, checksums, and notices. Model weights are downloaded/imported separately. Avoid promising a single-file EXE until native libraries, fonts, worker startup, and extraction behavior are verified. Clean-machine testing covers a system with no .NET SDK, no Office installation, and no model.

Keep databases, personal GDDs, recordings, images, model weights, and generated build outputs out of Git. Review package contents before release. Document signing status and system requirements accurately; code signing and an installer must not become hidden prerequisites for building or modifying the open-source application.

## 11 Immediate implementation backlog

1. Establish the C# solution, build configuration, English resources, and core domain contracts in the project repository.
2. Build the Phase 0 sample GDD and inference experiment; record exact dependency/model artifacts and measurements.
3. Implement stable catalog IDs, archive behavior, snapshots, genre-parent rules, and pool routing before building the complete wizard.
4. Produce the catalog workbook template and versioned JSON format, with a small English fixture containing PC, Mobile, a custom platform, two genres, related subgenres, Space, Zombies, WW2, and Dogs.
5. Complete one user journey: add Dogs in Settings, select it in the wizard, save the idea under a custom platform, open it in Other, and export its PDF and pool XLSX.
6. Expand the six-step flow and accept the owner's full English catalog through the same importer.

The owner can start preparing the English lists in Excel immediately. One sheet per category and one entry per row are enough for an initial draft; include the parent genre beside each subgenre. IDs, validation columns, and translations can be added through the template/import workflow during implementation.
