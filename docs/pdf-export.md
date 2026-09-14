# PDF export

GameDesignWizard exports a printable blank template or one saved idea at a time as an A4 PDF game design document.

## Export the blank template

1. Open **Home**.
2. Find **Blank GDD Template**.
3. Choose **Export PDF** and select the destination.

The five-page template contains project information fields, a game overview area, all twelve default English GDD sections, focused guidance for each section, writing lines, and page numbers. It does not require a saved game idea and does not read or change local idea data.

## Export an idea

1. Open **Idea Pool**.
2. Select one saved idea row.
3. Choose **Export PDF**.
4. Select the destination and file name.

The generated document contains the game name, stage, pool, update time, selected catalog values, overview, populated GDD sections, references, media captions, and page numbers. Empty GDD sections are omitted. Reference URLs remain readable and clickable.

Managed images are placed inside the document and scaled to stay within the printable page area. Audio remains an external managed attachment, so the PDF lists its filename and caption. If an image is missing or cannot be decoded on the current Windows installation, the export continues and identifies the unavailable image in the document.

The exporter reads the saved idea snapshot rather than current catalog labels. Historical values therefore remain consistent after catalog changes. It writes to a temporary file first and replaces the selected destination only after the complete PDF renders successfully.

The blank and filled exports use the same A4 page setup, typography, restrained color system, atomic file replacement, and numbered GameDesignWizard footer. The default section definitions are shared by the wizard and blank template so their titles and guidance remain aligned.
