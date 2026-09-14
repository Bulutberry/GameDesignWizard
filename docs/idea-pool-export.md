# Idea Pool XLSX Export

GameDesignWizard exports the complete saved idea pool as one `.xlsx` workbook. The export is a portable snapshot for review, filtering, analysis, and further editing outside the application. It does not change the local SQLite data.

## Export workflow

1. Open **Idea Pool**.
2. Choose **Export All XLSX**.
3. Select a destination and file name.

The command is available when at least one idea is saved. It always includes every saved idea across all pools, regardless of the currently visible group or selected row.

## Workbook sheets

| Sheet | Contents |
| --- | --- |
| All Ideas | One summary row for every saved idea. |
| PC | Ideas routed to the fixed PC pool. |
| Mobile | Ideas routed to the fixed Mobile pool. |
| Other | Ideas routed to Other, with their actual platform labels preserved. |
| Selections | One row per selected platform, genre, subgenre, topic, mechanic, feature, art style, development duration, or team size. |
| Idea Details | Overview and GDD sections, including ordered chunks for long content. |
| References | One row per saved URL and note. |
| Media Index | One row per managed image or audio attachment, including caption and stored relative path. |

Summary sheets are convenient views rather than the sole record of multi-value data. When a combined selection would exceed Excel's cell limit, the summary cell points to the complete rows in **Selections**.

Excel cells can store at most 32,767 characters. **Idea Details** therefore splits long overview or GDD content into ordered chunks of at most 30,000 characters. Reassemble rows with the same Idea ID, Content Type, and Section ID by sorting on Chunk Index. Chunk Count states how many pieces belong to the content item.

## Data and formatting rules

- Identifiers and labels are exported as text so spreadsheet software does not reinterpret them.
- Created and updated timestamps are UTC values with a visible `yyyy-mm-dd hh:mm` format.
- The first row is frozen, filters are enabled for populated tables, and gridlines are hidden.
- The export writes to a temporary file first. An incomplete workbook does not replace an existing destination if generation fails.
- Media files are not embedded or copied into the workbook. **Media Index** records the managed relative paths used by the local application.
