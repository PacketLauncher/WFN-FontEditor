This is an updated version of Rulaman's font editor for WFN and SCI fonts in both its AGS plugin and standalone variants.

It includes the following improvements, along with better performance when manipulating multiple glyphs:

- Full Unicode range support (65536 glyphs), up to 64kb of file size (current WFN format limitation).
- Extending and shrinking the font range at will by entering the desired range number (from 0 to 65535).
- Offsets are now set to the latest glyph that contains data (black or white pixels); if last glyph in the font is 1000 and font range is 65536, the saved font will be in the range of 1000 for reducing the file size.
- Live notification text above the preview pane, showing the current file size. The text will turn red if size exceeds the 64kb limit and won't allow saving to prevent corruption; it will re-change back to black and saving will be restored once the file size is within the 64kb threshold, which can be done by making certain glyphs placeholders instead of bitmap data (see below).
- Glyphs preview is now designed in pages* of 256 glyphs each, along with page buttons and page number text field for jumping to a specific page.
*The buttons that used to affect all characters in the preview, such as *All height/width to current* and *Outline font* will affect only the current page, so glyphs in other pages remain intact.
- New selected glyph indicator in the preview pane (no more guessing where you are).
- New character field above the index field where you can type to select that glyph, which also jumps to the page containing it (the index and character fields are tied together; you can either type an index number in the index field or a character in the field above it and the other will sync accordingly).
- New 'Save as' button in case of not wanting to overwrite the current font file.
- New 'Remove glyph data' context menu item when right-clicking a glyph for reverting it into a placeholder (reduces file size).
- New 'Make all blanks placeholders' button for making ALL empty data glyphs (those which are entirely black) into placeholders for reducing file size (affects only the visible glyphs page). Keep in mind that if you're on the first page that contains glyphs 0-255, this will also affect index number 32 which is the 'space' character; at this stage of version I did not want to exclude this character from the function, so set it back to a clear (black) glyph and adjust its width back to your liking if you don't see spaces in your font.

If you get an error when launching AGS after copying the plugin's .dll file to the AGS installation directory, this is caused by Windows blocking the dll file since it came from another computer; just right-click on the 'AGS.Plugin.FontEditor.dll' file, select 'Properties' and check the 'Unblock' checkbox at the bottom of the window. Then hit Apply and OK and re-launch AGS.