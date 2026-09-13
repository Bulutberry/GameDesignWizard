# Catalog source files

These UTF-8 files are the simple authoring source for large flat default catalogs. They are embedded into the application during build and remain reviewable in Git.

## Topics

Edit `en/topics.txt` and add one English topic per line. Blank lines and lines beginning with `#` are ignored. Duplicate names are ignored without regard to letter casing. File order becomes the default display order.

On the next build and application launch, new names are merged into the local SQLite catalog. Removing a line does not archive an option that has already been added to an existing user database. Archive that option in Settings instead.

For hundreds of rows, paste a single Excel column into this file or export that column as UTF-8 text. The planned catalog import screen will accept TXT and XLSX directly and show a validation preview before saving.
