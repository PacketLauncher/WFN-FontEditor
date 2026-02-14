This is an updated version of Rulaman's font editor for WFN and SCI fonts in both its AGS plugin and standalone variants.

It includes the following improvements, along with better performance when manipulating multiple glyphs:
- Support for the full Unicode range (65535).
- Extend button now extends the font in bulks of 128 glyphs at a time.
- Glyphs preview now supports pages (it shows 256 glyphs per page).
- Pages buttons for jumping between pages, and also a text field where you could type a page number and hit Enter to jump there.
- The buttons that used to affect all characters in the preview, such as *All height/width to current* and *Outline font* will affect only the current page, so glyphs in other pages remain intact.

If you get an error when launching AGS after copying the plugin's .dll file to the AGS installation directory, this is caused by Windows blocking the dll file since it came from another computer; just right-click on the 'AGS.Plugin.FontEditor.dll' file, select 'Properties' and check the 'Unblock' checkbox at the bottom of the window. Then hit Apply and OK and re-launch AGS.
