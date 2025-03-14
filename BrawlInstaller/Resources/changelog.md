- Trophy figure textures (as displayed inthe trophy hoard in-game) can now be customized from the trophy editor or fighter editor.
    - A new "Trophy Figure" cosmetic type has been added to cosmetic settings.
    - Settings presets were updated to support the new feature.
- New cosmetic settings have been added.
    - File Prefix: For cosmetics that save to individual BRRES files in the filesystem, this prefix will be used to name the file, similar to how the regular "Prefix" field is used to name textures. If this field is blank, the "Prefix" field will be used to name BRRES files instead.
    - Group Multiplier: This value is used to determine how cosmetics that are grouped together should be grouped. For instance, a value of ten means that cosmetics are saved to a group that is their ID / 10 (e.g. trophy ID 631 would save to group 63 because 631 / 10 = 63 without remainder).
    - Create Archive Files: When checked, cosmetics that are saved as a BRRES file to a directory will always generate a BRRES file if one does not exist.
- When adding a new costume to a fighter, the costume will default to using the nearest costume ID after your currently selected costume.
- SSE settings should now always be updated when deleting fighters.