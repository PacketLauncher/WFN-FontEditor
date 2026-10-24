This is an updated version of Rulaman's font editor for WFN and SCI fonts in both its AGS plugin and standalone variants.

It includes the following improvements, along with better performance when manipulating multiple glyphs, compatibility with the newly extended 4-bit WFN format that supports color shading, as well as import / export from and to PNG files:

*Note: The editor's plugin will work in both AGS 3.6.x and 4.x. That said, the extended 4-bit shading features are supported in AGS 4.x and above only!

Plugin & Standalone:
- On first start, you'll be prompt to select a custom text sample to render, as well as default placeholder size. You can change both of these afterwards from the "Set Text" and "Edit Defaults" buttons.
- Full Unicode range support (0-65535 glyphs)
- Extending and shrinking the font range at will by entering the desired range number.
- New layout! No more empty space to the right - when you extend the window, the glyphs preview area will increase and all the right hand size controls will move along with it.
- Backward compatibility with original 1-bit WFN format with up to 64kb of file size (due to its 16-bit offset size limitation)
- An extended 4-bit WFN format that uses 32-bit offset size, which supports 4-bit shading of gray.
- 1-4 bit button for converting fonts, in which will present a 4-bit grayscale shading palette in 4-bit mode for selecting shades.
- Offsets are now set to the latest glyph that contains data (black or white pixels); upon saving, if the last drawn glyph is 1000 and the current range is higher, the saved font will be set to 1000 for reducing the file size. That said, you could still see the higher range placeholders as long as the font is opened* in the editor, so you could keep editing glyphs beyond that. But once the font is re-opened you would see the actual saved range. *The plugin variant saves your changes when you either compile or save your game, but it will show the actual saved range only after re-launching the AGS Editor.
- Live notification text above the preview pane, showing the current file size (in 1-bit mode only). The text will turn red if size exceeds the 64kb limit and won't allow saving to prevent corruption; it will re-change back to black and saving will be restored once the file size is within the 64kb threshold, which can be done by making certain glyphs placeholders instead of bitmap data (see below).
- Glyphs preview is now designed in pages* of 256 glyphs each, along with page buttons and page number text field for jumping to a specific page.
*The buttons that used to affect all characters in the preview, such as *All height/width to current* and *Outline font* will affect only the current page, so glyphs in other pages remain intact.
- Selected glyph indicator in the preview pane (no more guessing where you are).
- Character field above the index field where you can type to select that glyph, which also jumps to the page containing it (the index and character fields are tied together; you can either type an index number in the index field or a character in the field above it and the other will sync accordingly).
- Added A and # labels beside the glyph and index fields.
- 'Remove glyph data' context menu item is now available when right-clicking a glyph for reverting it into a placeholder (reduces file size).
- 'Insert a new glyph before' and 'Insert a new glyph after' context menu item for inserting an empty index slot anywhere you want, making all glyphs after it to move 1 step forward (for safety, this will affect only the current page so indexes position in other pages won't alter, and would work as long as the total index of the current page is less than 256 (note that the representation was fixed to 0-base, so technically a page can contain 0-255 indexes in it (256 in total).
- 'Delete glyph' context menu item for deleting an index glyph entirely (for safety and to preserve the continous index numbering of glyphs in pages down the road, this will NOT delete the actual index but rather clear its glyph data, and then move all glyphs after it on the current page 1 step backward, while restoring the removed index right after them).
- 'Make all blanks placeholders' button for making ALL empty data glyphs (those which are entirely black) into placeholders for reducing file size (affects only the visible glyphs page). Keep in mind that if you're on the first page that contains glyphs 0-255, this will also affect index number 32 which is the 'space' character; there's a new excluding dialog now that will show on the first page for excluding that index from the operation.
- Font tabs can now be closed by the X icon or with the middle mouse click, and an asterisk is showing on them for unsaved work.
- Font tabs can now move amongst each other.
- Added support for copying and pasting glyphs in the same font, between tabs, between separated instances of the program, and to and from external paint programs.
- Multiple glyphs selection support! This is useful for copying and pasting a range of glyphs to another font tab easily, and would preserve the relative index positions for non-contiguous selections.
- Added the right-click context menu keyboard shortcut for the active glyph on the canvas.
- Added keyboard navigating support in the glyphs preview.
- Added an option to hold SHIFT while navigating through the glyphs in the preview to multiple select them.
- Added keyboard shortcuts to the context menu operations, as well as of the main interaface buttons.
- Improved warning dialog when closing an unsaved work.
- Proper Undo and Redo which were fixed further more in this new version.
- One Character operations as well as changing width and height with the arrow controls would work on multiple selection as well, while preserving each glyphs dimensions (unless manually specifying a value in either width and height and hitting Enter, in which case will make all of the selection the same size of the active glyph).
- Ability to drag WFN files over the interface to opening them.
- Set Text will now allow to edit the previous text, and also has a "Reverse" button for reversing the text for RTL representation when hitting the Render Text button.
- New ASCII / ANSI and Unicode text rendering table options for rendering the sample text glyphs from.
- Dark mode!
- New Import from PNG and Export to PNG buttons! For import, each glyhp in your PNG must have a separator border from its right and bottom for the program to distinguish it. Export will export to a single file if there are no placeholders between the glyphs; if there are, it will generate multiple files where each represets its range. When importing from the multiple files, it will automatically detect if they share the same filename, in which case it will import all of them automatically. If you like, you can select to export all placeholders as blank (black) and it will generate a single file of the entire range - you may set the default placeholder size from the "Edit Defaults" button.
- Maximum glyphs size was increased from 32x32 to 64x64.
- Added scrolling support when hovering on the glyphs preview area for navigating between the glyphs.
- Added scrolling suppoort over the 4-bit shading palette to selecting different colors.
- Added scrolling support over the drawing canvas for selecting different shading colors.
- Added an option to pass number on the keyboard when the mouse cursor is over the drawing canvas, for selecting different shading colors.
- 'About' button.

Standalone only:
- Fonts pane was removed altogether, and 'Open Directory' button was replaced with a proper 'Open' button (support opening multiple files at once).
- 'New' button for creating a blank WFN font.
- 'Save as' button in case of not wanting to overwrite the current font.
- 'Convert selected' button was renamed to 'SCI/WFN conversion' and appropriate logic and notifications were set for it.
- 'text height' text field was removed since it never seemed to work.
- Application EXE now has an icon.

Troubleshooting:
- If you get an error when launching AGS after copying the plugin's .dll file to the AGS installation directory, this is caused by Windows blocking the dll file since it came from another computer; just right-click on the 'AGS.Plugin.FontEditor.dll' file, select 'Properties' and check the 'Unblock' checkbox at the bottom of the window. Then hit Apply and OK and re-launch AGS.
- If the you're getting an error for the EXE variant, check if the file is blocked in its properties and if so, unblock it the same way.
- If you copied the EXE variant to a system folder path, such as within the AGS installation folder under "C:\Program Files (x86)\Adventure Game Studio 3.6.x\", make sure to run it as Administrator, or else you will encounter with permission issues.
- It was found that if the program is launched from a VERY long path (over 238 characters) that Windows File Explorer may crash and restart. This is not the program's issue but Windows'.
