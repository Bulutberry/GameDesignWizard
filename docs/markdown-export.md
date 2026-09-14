# Markdown export

GameDesignWizard can export one saved idea as a readable and editable UTF-8 Markdown document.

## Export an idea

1. Open **Idea Pool**.
2. Select one saved idea row.
3. Choose **Export Markdown**.
4. Select the destination and `.md` file name.

The document includes:

- Game name, development stage, and fixed idea-pool group
- Platform, genre, subgenre, topics, mechanics, features, and art styles
- Development duration, team size, and saved timestamps
- Overview and every GDD section stored with the idea
- Clickable HTTP or HTTPS references and their notes
- Image and audio filenames, types, and captions

Empty overview or GDD content is written as `Not specified` so the exported structure remains editable. User-authored GDD text is preserved as Markdown, allowing lists and other formatting to render naturally on GitHub.

The Markdown file does not copy or embed managed image and audio files. It records their public-facing filenames and captions without exposing local managed-storage paths. Use PDF export when images must appear inside the document.

The exporter writes UTF-8 without a byte-order mark. Markdown table delimiters and inline formatting characters in catalog labels are escaped. Output is written to a temporary file first, so an existing destination is replaced only after the complete document has been generated.
