This is an updated version of Rulaman's font editor for WFN and SCI fonts in both its AGS plugin and standalone variants.

It includes the following improvements, along with better performance when manipulating multiple glyphs:

- Extending and shrinking the font range at will by entering the desired range number, for allowing some Unicode range support, up to 64kb of file size (current WFN format limitation).
- Live notification text above the preview pane, showing the current file size. The text will turn red if size exceeds the 64kb limit and won't allow saving to prevent corruption; it will re-change back to black and saving will be restored once the file size is within the 64kb threshold, which can be done by reducing the font range to a lower number.
- Glyphs preview is now designed in pages of 256 glyphs each, along with page buttons for jumping to a specific page.
- New character field above the index field where you can type to select that glyph, which also jumps to the page containing it (the index and character fields are tied together; you can either type an index number in the index field or a character in the field above it and the other will sync accordingly).
- Save as button in case of not wanting to overwrite the current font file.
- The buttons that used to affect all characters in the preview, such as *All height/width to current* and *Outline font* will affect only the current page, so glyphs in other pages remain intact.

If you get an error when launching AGS after copying the plugin's .dll file to the AGS installation directory, this is caused by Windows blocking the dll file since it came from another computer; just right-click on the 'AGS.Plugin.FontEditor.dll' file, select 'Properties' and check the 'Unblock' checkbox at the bottom of the window. Then hit Apply and OK and re-launch AGS.
