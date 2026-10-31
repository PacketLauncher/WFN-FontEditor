This version is updated by: Gal Shemesh (PacketLauncher)

This is an updated version of Rulaman's WFN FontEditor v1.2.2.0, maintained by Gal Shemesh (PacketLauncher). The updated version uses its own version numbering, starting from v1.0.0.0, independently of the original project. It includes numerous improvements and performance enhancements. Refer to the changelog below for details.

Troubleshooting:
- If you get an error when launching AGS after copying the plugin's .dll file to the AGS installation directory, this is caused by Windows blocking the dll file since it came from another computer; just right-click on the 'AGS.Plugin.FontEditor.dll' file, select 'Properties' and check the 'Unblock' checkbox at the bottom of the window. Then hit Apply and OK and re-launch AGS.
- If you're getting an error for the EXE variant, check if the file is blocked in its properties and if so, unblock it the same way.
- If you copied the EXE variant to a system folder path, such as within the AGS installation folder under "C:\Program Files (x86)\Adventure Game Studio 3.6.x\", make sure to run it as Administrator, or else you will encounter with permission issues. It was also found that dragging and dropping files on top of the program to opening them is blocked when the program launced from a system folder that, as well as the program cannot access any shared folders in the local network; place it in a non-system folder to address these issues.
- It was found that if the program is launched from a VERY long path (over 238 characters) that Windows File Explorer may crash and restart. This is not the program's issue but Windows'.


Changelog
=========
Version v1.2.0.2
Released: 31 October, 2026
- Fixed a sync problem of both the character and index fields when hitting the second undo after a multiple selection operation.
- Fixed and updated the version Information of both EXE and dll files (up until now they had carried the original source version from before PacketLauncher's updates)

Version v1.2.0.1
Released: 30 October 2026
Standalone & plugin:
- Added '<' and '>' hotkeys to cycle back and forth through the glyphs, regardless of whether the glyphs area is in focus.
- HD program icon.

Plugin:
- Added a save button which turns red when a font is modified, along with an asterisk in the font's tab; after saving, the asterisk is removed and the red button gets disabled in its normal theme color.
- Saving and compiling the game also saves modified fonts.
- When closing a modified tab it now prompts with a save dialog.

Version v1.2.0.0
Released: 27 October 2026
- Extended the 4-bit WFN header with the new BaseShadeIndex field, which defaults to shade index 8.
- Added base text color selection using Shift+Left Click on the grayscale palette. Instead of repainting your entire font to achieve lighter or darker text in AGS, you can now choose which shade represents the game's text color, giving you more flexibility with highlights and shadows.
- Added red-number indicators in the grayscale palette and canvas shade indicator to identify the selected base shade.
- Added Undo/Redo support for base shade changes.
- Converting a 1-bit font to 4-bit now automatically maps all filled pixels to shade index 8 (the middle of the palette) and sets the font's base shade to 8, providing 7 darker and 7 lighter shades to work with.
- Improved single and multi-glyph clipboard conversion between 1-bit and 4-bit fonts (filled pixels copied from a 1-bit font are automatically mapped to shade index 8 when pasted into a 4-bit font).
- Fixed clipboard handling when copying images from external applications, preventing previously copied multi-glyph selections from being pasted instead.
- Added backward compatibility for older 4-bit WFN files using the previous 28-byte header. These fonts open with a default base shade of 8 while preserving their original pixel shades, and are saved using the new 32-byte header.

Version v1.1.1.1
Released: 26 October 2026
- Added an eyedropper tool by holding ALT (4-bit mode only).
- Added a paint bucket tool by holding SHIFT (4-bit mode only) for painting all pixels of the same color on either the active glyph and multiple selection glyphs. *You can also paint over ALL glyphs on the current page regardless of which glyph is selected by holding CTRL+SHIFT.
- Added a 'Shade Up' and 'Shade down' buttons (with PageUp and PageDown keyboard hotkeys) to the "One Character / Use on All" section, for increasing/decreasing all shades of the active glyph or selection of glyphs by 1, excluding black (4-bit mode only). *This can affect ALL glyphs on the current page when 'Use on All' checkbox is ticked, regardless of which glyph is selected.

Version v1.1.1.0
Released: 25 October 2026
- Updated the plugin variant to work on both AGS 3.6.x and AGS 4.x.
- Removed the font entries from the plugin's own project-tree section. To edit a font in AGS Editor, right-click the relevant font file under Fonts and select "Edit Font (with WFN-FontEditor)".

Version v1.1.0.0
Released: 24 October 2026
- Added an extended 4-bit WFN format that supports 16 grayscale colors (indexes 0-15) where 0 is transparency and 15 is the font color, and 32-bit offsets and sizes instead of 16-bit, lifting the 64kb file fize limit (compatible in AGS 4.x and above); original 1-bit WFN format support is preserved for opening and saving in the old format for backward compatility, with the original 64kb file size limit.
- Added a '1-4 bit' button for converting a font between the 2 formats; a 4-bit font will show a grayscale palette to choosing shade colors between 0-15 where 0 is transparent and 15 is the font color. In 1-bit fonts the palette will be hidden and the editor will use hardcoded drawing colors which are black (0) and white (15). Hitting the button on a font with shading will convert it to 1-bit, where any pixel that is not black will turn to white.
- Layout update with dynamic glyphs area size that extends and shrinks along with the size of the application window.
- Added a default placecholder size dialog when program starts that is saved to XML, along with an "Edit Defaults" button for changing it afterwards.
- 1-base representation of font range field was changed from to 0-base (0-65535 instead of 1-65536).
- Renamed 'Insert new glyph here' context menu item to 'Insert a new glyph before'.
- Added 'Insert a new glyph after' context menu item.
- Moved asterisk for unsaved files to the beginning of the tab.
- Added the right-click context menu keyboard shortcut for the active glyph on the canvas.
- Added keyboard navigating support in the glyphs preview.
- Added an option to hold SHIFT while navigating through the glyphs in the preview to multiple select them.
- Improved Undo and Redo.
- Added the ability to edit multiple glyphs by the One Character operations; changing their width and height with the arrow controls beside the width/height fields will increase/decreae each of the glyphs size while preserving their dimensions, unless manually specifying a value in either width and height fields and hitting Enter, in which case will make all of the selection the same size of the active glyph.
- Improved performance when manipulating multiple glyphs.
- Added ASCII / ANSI and Unicode text rendering table options for rendering the sample text glyphs from.
- Added an 'Import from PNG': glyphs should be drawn from top left corner towards the right. You can have as many glyphs in a row as you like, and also to have them in different rows, with the thumb rule of having a separator border from the right and bottom of each glyph for the program to distinguish it - the editor will use the right-bottom pixel as the transparent during the import. The separator can be any color you like, including alpha; if the glyphs in the PNG contain shadings, the font will automatically convert to 4-bit and show the shading palette.
- Added an 'Export to PNG' buttons: you can now export the entire font range to a PNG, either as a single file if there are no empty placeholders between the glyphs, or as multiple files if placeholders are present: multiple files will bear the range they represent after their name. When importing from the multiple files, the program will automatically detect if they share the same filename, in which case it will import all of them to the same font automatically. If you like, you can select to export all placeholders as blank (black) and it will generate a single file of the entire range - the placeholders size is set when you run the program the first time, and can be changed from the "Edit Defaults" button.
- Maximum glyphs size was increased from 32x32 to 64x64.
- Added scrolling support when hovering on the glyphs preview area for navigating between the glyphs.
- Added scrolling suppoort over the 4-bit shading palette to selecting different colors.
- Added scrolling support over the drawing canvas for selecting different shading colors.
- Added an option to pass number on the keyboard when the mouse cursor is over the drawing canvas, for selecting different shading colors.

Version v1.0.9.3
Released: 13 October 2026
- Undocumented fixes.

Version v1.0.9.1
Released: 9 September 2026
- Added support for copying and pasting multiple glyphs to external paint programs.
- Added support for pasting glyphs between separated instances of the program (also works in the plugin when opening different fonts in the AGS Editor).
- Set Text now allows to edit the previous text.
- Added a "Reverse" button to the Set Text dialog for reversing the text for RTL representation (text is rendered by hitting the Render Text button).
- Render Text button will now look for existing glyphs to render from the entire font range, rather than only from the current page.
- Added a Dark Mode button for reducing eye strain.

Version 1.1.9.0
Released: 9 September 2026
- Added a 'Delete glyph' context menu item for deleting an index glyph entirely (for safety, this will NOT delete the actual index but only clear the glyph data and move all glyphs after it on the current page 1 step backward).
- Font tabs can now move amongst each other.
- Added keyboard shortcuts to the context menu operations, as well as for the main interaface buttons.
- Improved warning dialog when closing an unsaved work.
- Proper Undo and Redo.
- Added 'A' and '#' labels beside the glyph and index fields.
- 'Insert new glyph here' context menu item for inserting an empty index slot before a glyph, making all glyphs after it to move 1 step forward (for safety, this will affect only the current page so index positions in other pages won't alter, and would work as long as the last index slot of the page is not occupied by a glyph data.
- Added multiple glyphs selection support! This is useful for copying and pasting a range of glyphs to another font easily, and would preserve the relative index positions for non-contiguous selections (One Character operations are not supported on multiple selection and other operations will deselect the multiple selection and affect only the active glyph showing on the canvas).
- Added drag and drop support for opening files by dragging them over the interface.

Version 1.0.8.2
Released: 20 February 2026
- Undocumented fixes.

Version 1.0.8.1
Released: 20 February 2026
- Added an 'About' button for standalone variant.

Version 1.0.8.0
Released: 19 February 2026
- Added a 'New' button for creating a blank WFN font.
- Fonts pane was removed altogether.
- 'Open Directory' button was replaced with a modern 'Open' button that supports opening multiple files at once.
- Font tabs can now be closed by the X icon or with the middle mouse click.
- Added an asterisk to unsaved tabs.
- Intiate program window size is now 60px wider for showing the hidden controls on the right.

Version 1.0.7.0
Released: 18 February 2026
- 'Convert selected' button was renamed to 'SCI/WFN conversion' and adding a dialog message to it.
- 'text height' text field was removed since it never seemed to work.

Version 1.0.6.0
Released: 18 February 2026
- Font range will now shrink upon saving by removing all placeholders beyond the last glyph that contains data (actual black or white pixels), reducing the file size; any placeholders beyond the last drawn glyph will remain visible in the editor until the font is re-opened.
- Added a selection indicator in the glyphs preview pane (no more guessing where you are).
- Added 'Remove glyph data' context menu item when right-clicking a glyph for reverting it into a placeholder (reduces file size).
- Added a 'Make all blanks placeholders' button for making ALL empty data glyphs (those which are entirely black) into placeholders, for reducing file size. This will affect only glyphs on the current page). Bare in mind that index 32 is the 'space' character; after doing this operation, make sure to make it a black glyph with an appropriate space width to your font.
- Added an icon for the standalone variant.

Version 1.0.5.0
Released: 16 February 2026
- Undocumented fixes.

Version 1.0.4.0
Released: 16 February 2026
- Added live notification text above the preview pane that shows the current file size (will turn red and prevent saving if size exceeds the 64kb limit to prevent corruption; will re-change back to black and restore saving once the file size is within the 64kb threshold, which can be done by reducing the font range to a lower number).


Version 1.0.3.0
Released: 15 February 2026
- Extending and shrinking the font range at will by entering the desired range number.
- Added Save-as button to standalone variant.
- Added Character field that works in sync with the index field, and allowing typing in them to jump to a specific glyph (also auto jump between pages).


Version 1.0.2.0
Released: 15 February 2026
- Font range support extended to cover the full Unicode range of 65536 indexes in total, as long as the file does not exceeds the 64kb limit (current WFN format limitation).
- Font range Extend button now extends the font in bulks of 128 glyphs at a time.
- Glyphs preview now supports pages* (showing 256 glyphs per page).
* The All characters manipulation buttons will affect only glyphs on the the current page, so glyphs in other pages remain intact.

Version 1.0.1.0
Released: 14 February 2026
- Undocumented fixes.

Version 1.0.0
Released: 7 February 2026
- Font range support extended from 0-255 to 0-1535 glyphs, covering Unicode range for Hebrew alphabet.
- Improved glyphs manupulation performance.
