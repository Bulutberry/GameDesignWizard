# Idea Pool Management

The Idea Pool is the local index of saved game ideas. Select one row to enable editing, PDF export, and deletion. Complete workbook export remains available whenever the pool contains at least one idea.

## Find and filter ideas

Use the search field to match an idea's name, platform, genre, subgenre, topics, mechanics, features, art styles, development duration, team size, or stage. Search is case-insensitive.

Choose a PC, Mobile, or Other summary card to restrict the table to that fixed pool. The selected card receives an accent background and border. Choose the **VIEW** control to return to all pools while keeping the current search and stage filter, or choose **Clear filters** to reset search, stage, and pool together.

The development-stage list combines with search and pool selection. The result message reports how many saved ideas remain visible. If the selected row leaves the view, its row actions are cleared.

## Edit an idea

1. Open **Idea Pool** and select one row.
2. Choose **Edit**.
3. Review or change any selection in the six-step wizard.
4. Choose **Save Changes** in the final step.

Editing preserves the saved idea ID and creation timestamp. Saving refreshes the update timestamp and updates the existing record instead of creating a duplicate. Changing the platform recalculates the fixed PC, Mobile, or Other destination.

After saving or editing, choose **Create Game Idea** to start a clean draft with a new identity. Navigating away from an unsaved new draft and returning to it keeps the current draft fields.

An option can be archived in Settings after an idea used it. The editor restores such historical selections with an **Archived** suffix. An archived multi-select option remains selected and can be removed, but it does not return to the available list. This prevents old ideas from losing context while keeping archived options unavailable for new choices.

## Development stages

The final wizard step exposes these stages:

- Idea
- Concept
- Prototyping
- Completed
- Shelved

New ideas start at **Idea**. The selected stage appears in the Idea Pool, PDF export, and XLSX export.

## Delete an idea

Select a row and choose **Delete**. The application asks for confirmation and identifies the selected idea before proceeding. A confirmed deletion removes the local SQLite record, then removes the managed image and audio files owned by that idea. Original source files outside the application workspace are never changed.

If a managed file cannot be removed after the record is deleted, the Idea Pool reports the cleanup count. Deleting one idea does not change any other saved idea or catalog option.
