# PDF export

GameDesignWizard exports one saved idea at a time as an A4 PDF game design document.

## Export an idea

1. Open **Idea Pool**.
2. Select one saved idea row.
3. Choose **Export PDF**.
4. Select the destination and file name.

The generated document contains the game name, stage, pool, update time, selected catalog values, overview, populated GDD sections, references, media captions, and page numbers. Empty GDD sections are omitted. Reference URLs remain readable and clickable.

Managed images are placed inside the document and scaled to stay within the printable page area. Audio remains an external managed attachment, so the PDF lists its filename and caption. If an image is missing or cannot be decoded on the current Windows installation, the export continues and identifies the unavailable image in the document.

The exporter reads the saved idea snapshot rather than current catalog labels. Historical values therefore remain consistent after catalog changes. It writes to a temporary file first and replaces the selected destination only after the complete PDF renders successfully.
