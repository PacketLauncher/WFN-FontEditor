This is an updated version for Rulaman's font editor for WFN and SCI fonts in both its AGS plugin and standalone variants.
Font extension was increased from up to 256 to 1535 glyphs, which is the Unicode range of the Hebrew alphabet.

Also, performance for glyphs manipulation such as changing all glyphs height/width to current glyph and outline the entire font got improved and now works much faster - the performance fix was done using ChatGPT.

If you get an error when launching AGS after copying the plugin's .exe and .dll files to the AGS installation directory, this is caused by Windows blocking the dll file since it came from another computer; just right-click on the 'AGS.Plugin.FontEditor.dll' file, select 'Properties' and check the 'Unblock' checkbox at the bottom of the window. Then hit Apply and OK and re-launch AGS.
