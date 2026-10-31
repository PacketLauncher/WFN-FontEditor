using AGS.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGS.Plugin.FontEditor
{
    public partial class FontEditorPane : EditorContentPanel
    {
        private static void ApplyThemeToAllOpenFontEditors()
        {
            foreach (Form form in Application.OpenForms)
            {
                ApplyThemeToFontEditorsRecursive(form);
            }
        }

        private static void ApplyThemeToFontEditorsRecursive(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                FontEditorPane pane = control as FontEditorPane;

                if (pane != null)
                {
                    pane.ApplyTheme();
                }

                if (control.HasChildren)
                {
                    ApplyThemeToFontEditorsRecursive(control);
                }
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                // WS_EX_COMPOSITED:
                // paint this control and its child controls as one buffered surface.
                cp.ExStyle |= 0x02000000;

                return cp;
            }
        }
        private void ApplyTheme()
        {
            Color backColor;
            Color controlColor;
            Color textColor;

            if (_darkMode)
            {
                backColor = Color.FromArgb(37, 37, 38);
                controlColor = Color.FromArgb(63, 63, 70);
                textColor = Color.White;
            }
            else
            {
                backColor = SystemColors.Control;
                controlColor = SystemColors.Window;
                textColor = SystemColors.ControlText;
            }

            // Give the zoom slider the correct theme background explicitly.
            ZoomDrawingArea.BackColor = backColor;

            ApplyThemeToControls(
                this,
                backColor,
                controlColor,
                textColor);

            BtnDarkMode.Text =
                _darkMode ? "Light Mode" : "Dark Mode";

            // Theme the surrounding standalone host area too.
            // Do NOT theme the AGS Editor host in plugin mode.
            Form hostForm = this.FindForm();

            if (hostForm != null &&
                hostForm.GetType().Name == "MainWindow")
            {
                Color outerColor =
                    _darkMode
                        ? Color.FromArgb(45, 45, 48)   // #2D2D30
                        : SystemColors.Control;

                Color hostButtonColor =
                    _darkMode
                        ? Color.FromArgb(63, 63, 70)   // same as lower buttons
                        : SystemColors.Control;

                Color hostTextColor =
                    _darkMode
                        ? Color.White
                        : SystemColors.ControlText;

                hostForm.BackColor = outerColor;

                System.Reflection.MethodInfo applyHostTheme =
                hostForm.GetType().GetMethod(
                    "ApplyStandaloneDarkHost",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);

                if (applyHostTheme != null)
                {
                    applyHostTheme.Invoke(
                        hostForm,
                        new object[] { _darkMode });
                }

                foreach (Control control in hostForm.Controls)
                {
                    // Upper toolbar buttons.
                    if (control is Button)
                    {
                        Button button = (Button)control;

                        button.UseVisualStyleBackColor = false;
                        button.BackColor = hostButtonColor;
                        button.ForeColor = hostTextColor;

                        button.FlatStyle =
                            _darkMode
                                ? FlatStyle.Flat
                                : FlatStyle.Standard;

                        if (_darkMode)
                        {
                            button.FlatAppearance.BorderColor =
                                Color.FromArgb(100, 100, 105);
                        }
                    }

                    // Standalone font tabs.
                    if (control is TabControl)
                    {
                        TabControl tabs = (TabControl)control;

                        tabs.BackColor = outerColor;

                        foreach (TabPage page in tabs.TabPages)
                            page.BackColor = outerColor;

                        // Force our owner-drawn tabs to repaint immediately.
                        tabs.Invalidate();
                    }
                }
            }
            
            UpdatePluginSaveButton();
        }

        public void ReapplyTheme()
        {
            ApplyTheme();
        }
        private void ApplyThemeToControls(
            Control parent,
            Color backColor,
            Color controlColor,
            Color textColor)
        {
            foreach (Control control in parent.Controls)
            {
                // Preserve the blue highlight when "Use on all" is checked.
                if (control == GrpOneCharacter && ChkOneAllCharacters.Checked)
                {
                    control.BackColor = SystemColors.Highlight;
                    control.ForeColor = textColor;

                    ApplyThemeToControls(control, backColor, controlColor, textColor);
                    continue;
                }

                // These are actual font/glyph display surfaces.
                // Their colors must not be changed by the UI theme.
                if (control == FlowCharacterPanel ||
                    control == DrawingArea ||
                    control == PictRenderText ||
                    control == PanelLeftMouse ||
                    control == PanelRightMouse ||
                    control == PictSwap)
                {
                    continue;
                }

                if (control is TextBox ||
                    control is NumericUpDown)
                {
                    control.BackColor = controlColor;
                    control.ForeColor = textColor;
                }
                else if (control is Button)
                {
                    Button button = (Button)control;

                    button.UseVisualStyleBackColor = false;
                    button.BackColor =
                        _darkMode
                            ? Color.FromArgb(63, 63, 70)
                            : SystemColors.Control;

                    button.ForeColor = textColor;

                    button.FlatStyle =
                        _darkMode
                            ? FlatStyle.Flat
                            : FlatStyle.Standard;

                    if (_darkMode)
                    {
                        button.FlatAppearance.BorderColor =
                            Color.FromArgb(100, 100, 105);
                    }
                }
                else if (control is CheckBox)
                {
                    control.BackColor = backColor;
                    control.ForeColor = textColor;
                }
                else
                {
                    control.BackColor = backColor;
                    control.ForeColor = textColor;
                }

                if (control.HasChildren)
                {
                    ApplyThemeToControls(
                        control,
                        backColor,
                        controlColor,
                        textColor);
                }
            }

            // Theme the FontEditorPane itself.
            if (parent == this)
            {
                this.BackColor = backColor;
                this.ForeColor = textColor;
            }
        }

        private Button BtnAboutPlugin;
        private Button BtnSavePlugin;

        private void BtnAboutPlugin_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "WFN-FontEditor (256 ASCII characters) by Rulaman:\nhttps://github.com/Rulaman/WFN-FontEditor\n\nImproved version with full Unicode support and 4-bit shading by PacketLauncher (Gal Shemesh):\nhttps://github.com/PacketLauncher/WFN-FontEditor",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static bool _darkMode = false;
        private void BtnDarkMode_Click(object sender, EventArgs e)
        {
            _darkMode = !_darkMode;

            XmlSettings.DarkMode = _darkMode;
            XmlSettings.Write();

            ApplyThemeToAllOpenFontEditors();

            // Refresh the existing text preview using the new theme.
            if (PictRenderText.Image != null)
            {
                Image oldImage = PictRenderText.Image;
                PictRenderText.Image = RenderTextLine(XmlSettings.CustomText);
                oldImage.Dispose();
            }
        }
        private static Bitmap CreateCombinedGlyphBitmap(List<PictureBox> sources)
        {
            if (sources == null || sources.Count == 0)
                return null;

            int totalWidth = 0;
            int maxHeight = 0;

            foreach (PictureBox picture in sources)
            {
                CCharInfo character = picture.Tag as CCharInfo;

                if (character == null ||
                    character.UnscaledImage == null)
                {
                    continue;
                }

                totalWidth += character.UnscaledImage.Width;

                if (character.UnscaledImage.Height > maxHeight)
                    maxHeight = character.UnscaledImage.Height;
            }

            if (totalWidth <= 0 || maxHeight <= 0)
                return null;

            Bitmap combined =
                new Bitmap(totalWidth, maxHeight);

            using (Graphics g = Graphics.FromImage(combined))
            {
                g.Clear(Color.Black);

                int x = 0;

                foreach (PictureBox picture in sources)
                {
                    CCharInfo character =
                        picture.Tag as CCharInfo;

                    if (character == null ||
                        character.UnscaledImage == null)
                    {
                        continue;
                    }

                    g.DrawImageUnscaled(
                        character.UnscaledImage,
                        x,
                        0);

                    x += character.UnscaledImage.Width;
                }
            }

            return combined;
        }
        Settings XmlSettings = new Settings();
        private List<PictureBox> CharacterPictureList = new List<PictureBox>();
        private PictureBox _selectedPreview = null;
        private bool _canvasSelectionActive = false;
        private List<PictureBox> _selectedPreviews = new List<PictureBox>();
        private int _normalImportExportTop;
        private int _normalRenderingTableTop;
        private class CopiedGlyph
        {
            public int SourceIndex;
            public UInt16 Width;
            public UInt16 Height;
            public int BitDepth;
            public byte[] ByteLines;
        }

        private static List<CopiedGlyph> _copiedGlyphs =
            new List<CopiedGlyph>();
        private const string GlyphClipboardFormat =
            "WFNFontEditor.MultiGlyphData";

        private static byte[] SerializeCopiedGlyphs(List<CopiedGlyph> glyphs)
        {
            using (System.IO.MemoryStream stream =
                new System.IO.MemoryStream())
            using (System.IO.BinaryWriter writer =
                new System.IO.BinaryWriter(stream))
            {
                writer.Write(2); // format version: includes glyph bit depth
                writer.Write(glyphs.Count);

                foreach (CopiedGlyph glyph in glyphs)
                {
                    writer.Write(glyph.SourceIndex);
                    writer.Write(glyph.Width);
                    writer.Write(glyph.Height);
                    writer.Write(glyph.BitDepth);

                    if (glyph.ByteLines == null)
                    {
                        writer.Write(-1);
                    }
                    else
                    {
                        writer.Write(glyph.ByteLines.Length);
                        writer.Write(glyph.ByteLines);
                    }
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        private static bool TryLoadCopiedGlyphsFromClipboard()
        {
            try
            {
                if (!Clipboard.ContainsData(GlyphClipboardFormat))
                    return false;

                object data =
                    Clipboard.GetData(GlyphClipboardFormat);

                byte[] bytes = data as byte[];
                if (bytes == null)
                    return false;

                List<CopiedGlyph> loadedGlyphs =
                    new List<CopiedGlyph>();

                using (System.IO.MemoryStream stream =
                    new System.IO.MemoryStream(bytes))
                using (System.IO.BinaryReader reader =
                    new System.IO.BinaryReader(stream))
                {
                    int version = reader.ReadInt32();

                    if (version != 1 && version != 2)
                        return false;

                    int count = reader.ReadInt32();

                    if (count <= 0 || count > 256)
                        return false;

                    for (int i = 0; i < count; i++)
                    {
                        CopiedGlyph glyph =
                            new CopiedGlyph();

                        glyph.SourceIndex =
                            reader.ReadInt32();

                        glyph.Width =
                            reader.ReadUInt16();

                        glyph.Height =
                            reader.ReadUInt16();

                        glyph.BitDepth =
                            (version >= 2) ? reader.ReadInt32() : 1;

                        if (glyph.BitDepth != 1 && glyph.BitDepth != 4)
                            return false;

                        int byteCount =
                            reader.ReadInt32();

                        if (byteCount < 0)
                        {
                            glyph.ByteLines = null;
                        }
                        else
                        {
                            if (byteCount > 1024 * 1024)
                                return false;

                            glyph.ByteLines =
                                reader.ReadBytes(byteCount);

                            if (glyph.ByteLines.Length != byteCount)
                                return false;
                        }

                        loadedGlyphs.Add(glyph);
                    }
                }

                _copiedGlyphs = loadedGlyphs;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Stack<List<int>> _multiUndoStack = new Stack<List<int>>();
        private Stack<List<int>> _multiRedoStack = new Stack<List<int>>();
        private enum StructuralEditType
        {
            Insert,
            Delete,
            MultiDelete
        }

        private class StructuralEdit
        {
            public StructuralEditType Type;
            public int Index;
            public int PageEnd;
            public int OldLength;
            public bool InsertGrewArray;
            public CCharInfo DeletedCharacter;
            public CCharInfo InsertedCharacter;
            public CCharInfo DisplacedCharacter;
            public CCharInfo DeletePlaceholder;
            public List<int> DeletedIndexes;
            public List<CCharInfo> DeletedCharacters;
            public List<CCharInfo> DeletePlaceholders;
            public int ActiveIndexBeforeEdit = -1;
            public List<int> SelectionBeforeEdit;
        }

        private Stack<StructuralEdit> _structuralUndoStack =
            new Stack<StructuralEdit>();

        private enum UndoOperationType
        {
            GlyphEdit,
            StructuralEdit,
            MultiGlyphEdit,
            FontBitDepthConversion,
            BaseShadeChange
        }

        private class UndoOperation
        {
            public UndoOperationType Type;
            public CCharInfo Character;
            public bool PreviousFontIs4Bit;
            public uint PreviousBaseShadeIndex;
            public uint NewBaseShadeIndex;

            // Original destination index for a multi-glyph paste.
            // -1 means this is not a paste operation.
            public int PasteStartIndex = -1;
            public List<int> SelectionBeforeEdit;
            public List<int> SelectionAfterEdit;
            public List<int> PastedIndexes;
            public int ActiveIndexBeforeEdit = -1;
        }

        private Stack<UndoOperation> _operationUndoStack =
            new Stack<UndoOperation>();

        private Stack<UndoOperation> _operationRedoStack =
            new Stack<UndoOperation>();

        private Stack<bool> _fontBitDepthUndoStack =
            new Stack<bool>();

        private Stack<bool> _fontBitDepthRedoStack =
            new Stack<bool>();

        private bool _recordingGroupedOperation = false;

        private void ClearAllUndoRedoHistory()
        {
            // Clear chronological operation history.
            _operationUndoStack.Clear();
            _operationRedoStack.Clear();

            // Clear structural and grouped operation history.
            _structuralUndoStack.Clear();
            _structuralRedoStack.Clear();
            _multiUndoStack.Clear();
            _multiRedoStack.Clear();
            _fontBitDepthUndoStack.Clear();
            _fontBitDepthRedoStack.Clear();

            _recordingGroupedOperation = false;

            // Reset each remaining glyph's history to its current state.
            if (FontInfo != null && FontInfo.Character != null)
            {
                foreach (CCharInfo character in FontInfo.Character)
                {
                    if (character == null)
                        continue;

                    character.UndoRedoListClear();
                    character.UndoRedoPosition = 0;
                    character.UndoRedoListAdd(character.ByteLines);
                }
            }
        }

        private void RegisterGlyphUndoOperation(CCharInfo character)
        {
            if (_recordingGroupedOperation ||
                character == null ||
                !character.UndoPossible)
                return;

            _operationUndoStack.Push(new UndoOperation
            {
                Type = UndoOperationType.GlyphEdit,
                Character = character
            });

            _operationRedoStack.Clear();
            _multiRedoStack.Clear();
            _structuralRedoStack.Clear();
            _fontBitDepthRedoStack.Clear();
        }

        private void RegisterFontBitDepthUndoOperation(bool previousIs4Bit)
        {
            _fontBitDepthUndoStack.Push(previousIs4Bit);

            _operationUndoStack.Push(new UndoOperation
            {
                Type = UndoOperationType.FontBitDepthConversion,
                PreviousFontIs4Bit = previousIs4Bit
            });

            _operationRedoStack.Clear();
            _multiRedoStack.Clear();
            _structuralRedoStack.Clear();
            _fontBitDepthRedoStack.Clear();
        }

        private void RecordFontBitDepthUndoSnapshots()
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            foreach (CCharInfo character in FontInfo.Character)
            {
                if (character == null)
                    continue;

                character.UndoRedoListTidyUp();
                character.UndoRedoListAdd(character.ByteLines);
            }
        }

        private void RecordConvertedFontGlyphStates()
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            foreach (CCharInfo character in FontInfo.Character)
            {
                if (character == null)
                    continue;

                character.UndoRedoListAdd(character.ByteLines, true);
            }
        }

        private bool UndoFontBitDepthConversion()
        {
            CWFNFontInfo font = FontInfo as CWFNFontInfo;

            if (font == null ||
                font.Character == null ||
                _fontBitDepthUndoStack.Count == 0)
                return false;

            bool previousIs4Bit = _fontBitDepthUndoStack.Peek();

            foreach (CCharInfo character in font.Character)
            {
                if (character == null || !character.UndoPossible)
                    return false;
            }

            foreach (CCharInfo character in font.Character)
            {
                if (character != null)
                    character.Undo();
            }

            _fontBitDepthUndoStack.Pop();
            _fontBitDepthRedoStack.Push(previousIs4Bit);

            font.SetBitDepth(previousIs4Bit);

            grayscalePalette.Visible = font.Is4Bit;
            UpdateShadeButtonsLayout();

            if (!font.Is4Bit)
            {
                selectedGrayscaleShade = 15;

                Panel drawingColorPanel = grayscaleMouseButtonsSwapped
                    ? PanelRightMouse
                    : PanelLeftMouse;

                drawingColorPanel.BackColor = Color.White;
            }

            RebuildPage();
            UpdateGlyphTextbox();
            CheckChange();
            MarkFontModified();

            return true;
        }

        private bool RedoFontBitDepthConversion()
        {
            CWFNFontInfo font = FontInfo as CWFNFontInfo;

            if (font == null ||
                font.Character == null ||
                _fontBitDepthRedoStack.Count == 0)
                return false;

            bool previousIs4Bit = _fontBitDepthRedoStack.Peek();

            foreach (CCharInfo character in font.Character)
            {
                if (character == null || !character.RedoPossible)
                    return false;
            }

            foreach (CCharInfo character in font.Character)
            {
                if (character != null)
                    character.Redo();
            }

            _fontBitDepthRedoStack.Pop();
            _fontBitDepthUndoStack.Push(previousIs4Bit);

            font.SetBitDepth(!previousIs4Bit);

            grayscalePalette.Visible = font.Is4Bit;
            UpdateShadeButtonsLayout();

            if (!font.Is4Bit)
            {
                selectedGrayscaleShade = 15;

                Panel drawingColorPanel = grayscaleMouseButtonsSwapped
                    ? PanelRightMouse
                    : PanelLeftMouse;

                drawingColorPanel.BackColor = Color.White;
            }

            RebuildPage();
            UpdateGlyphTextbox();
            CheckChange();
            MarkFontModified();

            return true;
        }

        private void BtnBitDepth_Click(object sender, EventArgs e)
        {
            if (FontInfo is CSCIFontInfo)
            {
                MessageBox.Show(
                    "SCI fonts are 1-bit only!\n\nTo enable 4-bit color shading, convert it to WFN first.",
                    "SCI Font",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            CWFNFontInfo font = FontInfo as CWFNFontInfo;

            if (font == null)
                return;

            bool previousIs4Bit = font.Is4Bit;

            if (previousIs4Bit)
            {
                DialogResult result = MessageBox.Show(
                    "Converting this font from 4-bit to 1-bit will permanently " +
                    "discard its grayscale shading.\n\n" +
                    "Do you want to continue?",
                    "Convert Font to 1-bit",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.OK)
                    return;
            }

            RecordFontBitDepthUndoSnapshots();

            font.ConvertBitDepth(!previousIs4Bit);

            RecordConvertedFontGlyphStates();

            RegisterFontBitDepthUndoOperation(previousIs4Bit);

            grayscalePalette.Visible = font.Is4Bit;
            UpdateShadeButtonsLayout();

            if (!font.Is4Bit)
            {
                selectedGrayscaleShade = 15;

                Panel drawingColorPanel = grayscaleMouseButtonsSwapped
                    ? PanelRightMouse
                    : PanelLeftMouse;

                drawingColorPanel.BackColor = Color.White;
            }

            RebuildPage();
            SetGridFix();
            UpdateGlyphTextbox();
            MarkFontModified();
        }

        private Stack<StructuralEdit> _structuralRedoStack =
            new Stack<StructuralEdit>();
        private PictureBox _selectionAnchor = null;
        private readonly ToolTip _toolTip = new ToolTip();
        private bool _bulkUpdate = false;
        private Int32 Index;
        private bool bInEdit = false;
        private bool _eyedropperClick = false;
        private bool _shadeReplaceClick = false;
        private int _replaceSourceShade;
        private int _replaceTargetShade;
        private System.Windows.Forms.Timer _eyedropperCursorTimer;
        private Cursor _bucketCursor;
        private Cursor _eyeDropperCursor;
        private Point EditPoint;
        private Int32 Scalefactor = 2;
        private CFontInfo FontInfo;

        // 4-bit WFN grayscale palette
        private System.Windows.Forms.Panel grayscalePalette;
        private System.Windows.Forms.Button BtnBitDepth;
        private System.Windows.Forms.Button BtnShadeUp;
        private System.Windows.Forms.Button BtnShadeDown;
        private System.Windows.Forms.Panel drawingScrollPanel;
        private System.Windows.Forms.Panel shadeIndicator;
        private System.Windows.Forms.Timer shadeIndicatorTimer;

        private int selectedGrayscaleShade = 15;
        private int pendingShadeDigit = -1;
        private DateTime pendingShadeDigitTime = DateTime.MinValue;

        // Tracks which mouse button uses the selected grayscale shade.
        // False = left paints, right erases.
        // True = right paints, left erases.
        private bool grayscaleMouseButtonsSwapped = false;

        private bool HandleGrayscaleNumberKey(Keys keyData)
        {
            if (grayscalePalette == null || !grayscalePalette.Visible)
                return false;

            // Do not intercept numbers while Ctrl, Alt or Shift is held.
            if ((keyData & Keys.Modifiers) != Keys.None)
                return false;

            Keys key = keyData & Keys.KeyCode;
            int digit;

            if (key >= Keys.D0 && key <= Keys.D9)
                digit = (int)(key - Keys.D0);
            else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
                digit = (int)(key - Keys.NumPad0);
            else
                return false;

            // Only activate when the pointer is inside the drawing canvas.
            Point canvasPoint = DrawingArea.PointToClient(Cursor.Position);

            if (!DrawingArea.ClientRectangle.Contains(canvasPoint))
            {
                pendingShadeDigit = -1;
                return false;
            }

            DateTime now = DateTime.UtcNow;

            bool combine =
                pendingShadeDigit == 1 &&
                (now - pendingShadeDigitTime).TotalMilliseconds <= 700 &&
                digit <= 5;

            if (combine)
            {
                selectedGrayscaleShade = 10 + digit;
                pendingShadeDigit = -1;
            }
            else
            {
                selectedGrayscaleShade = digit;
                pendingShadeDigit = digit == 1 ? 1 : -1;
                pendingShadeDigitTime = now;
            }

            int intensity = selectedGrayscaleShade * 17;
            Color shadeColor = Color.FromArgb(intensity, intensity, intensity);

            Panel drawingColorPanel = grayscaleMouseButtonsSwapped
                ? PanelRightMouse
                : PanelLeftMouse;

            drawingColorPanel.BackColor = shadeColor;
            grayscalePalette.Invalidate();

            // Reuse the same temporary indicator as mouse-wheel selection.
            Point cursorPosition =
                drawingScrollPanel.PointToClient(Cursor.Position);

            shadeIndicator.BackColor = shadeColor;
            shadeIndicator.Invalidate();

            shadeIndicator.Location = new Point(
                cursorPosition.X + 12,
                Math.Max(0, cursorPosition.Y - shadeIndicator.Height + 6));

            shadeIndicator.BringToFront();
            shadeIndicator.Visible = true;

            shadeIndicatorTimer.Stop();
            shadeIndicatorTimer.Start();

            return true;
        }

        private void InitializeEyedropperCursor()
        {
            // Load the embedded bucket cursor.
            System.Reflection.Assembly assembly = GetType().Assembly;

            string resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(
                    ".Bucket.cur", StringComparison.OrdinalIgnoreCase));

            if (resourceName != null)
            {
                using (System.IO.Stream stream =
                    assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                        _bucketCursor = new Cursor(stream);
                }
            }

            // Load the embedded Windows Paint eyedropper cursor.
            string eyeDropperResourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(
                    ".Eye_Dropper.cur", StringComparison.OrdinalIgnoreCase));

            if (eyeDropperResourceName != null)
            {
                using (System.IO.Stream stream =
                    assembly.GetManifestResourceStream(eyeDropperResourceName))
                {
                    if (stream != null)
                        _eyeDropperCursor = new Cursor(stream);
                }
            }

            _eyedropperCursorTimer = new System.Windows.Forms.Timer();
            _eyedropperCursorTimer.Interval = 50;

            _eyedropperCursorTimer.Tick += (sender, e) =>
            {
                if (DrawingArea.IsDisposed)
                    return;

                bool is4Bit =
                    grayscalePalette != null && grayscalePalette.Visible;

                Keys modifiers = Control.ModifierKeys;

                Cursor desiredCursor = Cursors.Default;

                if (is4Bit)
                {
                    if ((modifiers & Keys.Alt) == Keys.Alt)
                    {
                        desiredCursor = _eyeDropperCursor ?? Cursors.Cross;
                    }
                    else if ((modifiers & Keys.Shift) == Keys.Shift &&
                        (modifiers & Keys.Alt) != Keys.Alt)
                    {
                        desiredCursor = _bucketCursor ?? Cursors.Default;
                    }
                }

                if (DrawingArea.Cursor != desiredCursor)
                    DrawingArea.Cursor = desiredCursor;
            };

            this.Disposed += (sender, e) =>
            {
                _eyedropperCursorTimer.Stop();
                _eyedropperCursorTimer.Dispose();

                if (_bucketCursor != null)
                    _bucketCursor.Dispose();

                if (_eyeDropperCursor != null)
                    _eyeDropperCursor.Dispose();
            };

            _eyedropperCursorTimer.Start();
        }

        private void InitializeGrayscalePalette()
        {
            grayscalePalette = new System.Windows.Forms.Panel();
            grayscalePalette.Location = new System.Drawing.Point(335, 70);
            grayscalePalette.Size = new System.Drawing.Size(64, 64);
            grayscalePalette.Visible = false;
            grayscalePalette.Cursor = System.Windows.Forms.Cursors.Hand;

            // Temporary shade indicator displayed above the canvas cursor.
            shadeIndicator = new System.Windows.Forms.Panel();
            shadeIndicator.Size = new Size(15, 15);
            shadeIndicator.BackColor = Color.White;
            shadeIndicator.BorderStyle = BorderStyle.FixedSingle;

            shadeIndicator.Paint += (sender, e) =>
            {
                string shadeText = selectedGrayscaleShade.ToString();

                CWFNFontInfo wfn = FontInfo as CWFNFontInfo;

                Color textColor =
                    wfn != null &&
                    wfn.Is4Bit &&
                    selectedGrayscaleShade == (int)wfn.BaseShadeIndex
                        ? Color.Red
                        : (selectedGrayscaleShade < 8
                            ? Color.White
                            : Color.Black);

                TextRenderer.DrawText(
                    e.Graphics,
                    shadeText,
                    new System.Drawing.Font("Arial", 7, FontStyle.Regular),
                    shadeIndicator.ClientRectangle,
                    textColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPadding);
            };

            shadeIndicator.Visible = false;
            shadeIndicator.Enabled = false;

            drawingScrollPanel.Controls.Add(shadeIndicator);
            shadeIndicator.BringToFront();

            // Hide the indicator 700 ms after the last wheel movement.
            shadeIndicatorTimer = new System.Windows.Forms.Timer();
            shadeIndicatorTimer.Interval = 700;
            shadeIndicatorTimer.Tick += (sender, e) =>
            {
                shadeIndicatorTimer.Stop();
                shadeIndicator.Visible = false;
            };

            grayscalePalette.Paint += (sender, e) =>
            {
                // Draw all 16 grayscale shades.
                for (int i = 0; i < 16; i++)
                {
                    int x = 1 + (i % 4) * 15;
                    int y = 1 + (i / 4) * 15;
                    int intensity = i * 17;

                    using (System.Drawing.Brush brush =
                        new System.Drawing.SolidBrush(
                            System.Drawing.Color.FromArgb(
                                intensity, intensity, intensity)))
                    {
                        e.Graphics.FillRectangle(brush, x, y, 15, 15);
                    }

                    // Draw the shade index centered inside each palette square.
                    using (System.Drawing.Font shadeFont =
                        new System.Drawing.Font("Arial", 7, FontStyle.Regular))
                    {
                        TextRenderer.DrawText(
                            e.Graphics,
                            i.ToString(),
                            shadeFont,
                            new Rectangle(x, y, 16, 16),
                            (FontInfo is CWFNFontInfo &&
                             ((CWFNFontInfo)FontInfo).Is4Bit &&
                             i == (int)((CWFNFontInfo)FontInfo).BaseShadeIndex)
                                ? Color.Red
                                : (i < 8 ? Color.White : Color.Black),
                            TextFormatFlags.HorizontalCenter |
                            TextFormatFlags.VerticalCenter |
                            TextFormatFlags.NoPadding);
                    }
                }

                // Draw the grid with shared 1-pixel borders.
                using (System.Drawing.Pen pen =
                    new System.Drawing.Pen(System.Drawing.Color.Gray))
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        int position = 1 + i * 15;

                        // Enable these 2 lines if you wish to add a border to the grayscale shades grid
                        //e.Graphics.DrawLine(pen, position, 0, position, 60);
                        //e.Graphics.DrawLine(pen, 0, position, 60, position);
                    }
                }

                // Draw the selected shade outline last, above everything.
                int selectedX = 1 + (selectedGrayscaleShade % 4) * 15;
                int selectedY = 1 + (selectedGrayscaleShade / 4) * 15;

                using (System.Drawing.Pen pen =
                    new System.Drawing.Pen(
                        System.Drawing.Color.FromArgb(0, 120, 215), 2))
                {
                    pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

                    e.Graphics.DrawRectangle(
                        pen, selectedX, selectedY, 16, 16);
                }
            };

            MouseEventHandler shadeWheelHandler = (sender, e) =>
            {
                if (!grayscalePalette.Visible)
                    return;

                // reverse scrolling (uncomment this to use and comment out the other)
                //int change = -Math.Sign(e.Delta);
                int change = Math.Sign(e.Delta);

                selectedGrayscaleShade = Math.Max(0,
                    Math.Min(15, selectedGrayscaleShade + change));

                int intensity = selectedGrayscaleShade * 17;

                Panel drawingColorPanel = grayscaleMouseButtonsSwapped
                    ? PanelRightMouse
                    : PanelLeftMouse;

                drawingColorPanel.BackColor =
                    Color.FromArgb(intensity, intensity, intensity);

                grayscalePalette.Invalidate();

                // Show the temporary shade indicator only over the drawing canvas.
                if (sender == DrawingArea)
                {
                    Point cursorPosition =
                        drawingScrollPanel.PointToClient(Cursor.Position);

                    shadeIndicator.BackColor =
                        Color.FromArgb(intensity, intensity, intensity);

                    shadeIndicator.Invalidate();

                    shadeIndicator.Location = new Point(
                        cursorPosition.X + 12,
                        Math.Max(0, cursorPosition.Y - shadeIndicator.Height + 6));

                    shadeIndicator.BringToFront();
                    shadeIndicator.Visible = true;

                    shadeIndicatorTimer.Stop();
                    shadeIndicatorTimer.Start();
                }
            };

            grayscalePalette.MouseWheel += shadeWheelHandler;
            DrawingArea.MouseWheel += shadeWheelHandler;

            grayscalePalette.MouseClick += (sender, e) =>
            {
                if (e.Button != MouseButtons.Left)
                    return;

                if (e.X >= 1 && e.X < 61 &&
                        e.Y >= 1 && e.Y < 61)
                {
                    int clickedShade =
                        ((e.Y - 1) / 15) * 4 + ((e.X - 1) / 15);

                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                    {
                        CWFNFontInfo font = FontInfo as CWFNFontInfo;

                        if (font == null || !font.Is4Bit)
                            return;

                        // Shade 0 is reserved for transparency.
                        if (clickedShade == 0)
                        {
                            MessageBox.Show(
                                "Index 0 (transparency) cannot be the base text color!",
                                "Invalid Base Text Color",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return;
                        }
                        if (font == null || !font.Is4Bit)
                            return;

                        // Shade 0 is reserved for transparency.
                        if (clickedShade == 0)
                        {
                            MessageBox.Show(
                                "Index 0 (transparency) cannot be the base text color!",
                                "Invalid Base Text Color",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return;
                        }

                        if (font.BaseShadeIndex != (uint)clickedShade)
                        {
                            _operationUndoStack.Push(new UndoOperation
                            {
                                Type = UndoOperationType.BaseShadeChange,
                                PreviousBaseShadeIndex = font.BaseShadeIndex,
                                NewBaseShadeIndex = (uint)clickedShade
                            });

                            _operationRedoStack.Clear();
                            _multiRedoStack.Clear();
                            _structuralRedoStack.Clear();
                            _fontBitDepthRedoStack.Clear();

                            font.BaseShadeIndex = (uint)clickedShade;

                            MarkFontModified();
                            grayscalePalette.Invalidate();
                            shadeIndicator.Invalidate();
                        }

                        return;
                    }

                    selectedGrayscaleShade = clickedShade;

                    int intensity = selectedGrayscaleShade * 17;

                    System.Windows.Forms.Panel drawingColorPanel =
                        grayscaleMouseButtonsSwapped
                            ? PanelRightMouse
                            : PanelLeftMouse;

                    drawingColorPanel.BackColor =
                        System.Drawing.Color.FromArgb(
                            intensity, intensity, intensity);

                    grayscalePalette.Invalidate();
                }
            };

            this.Controls.Add(grayscalePalette);
            grayscalePalette.BringToFront();

            BtnShadeUp = new Button();
            BtnShadeUp.Name = "BtnShadeUp";
            BtnShadeUp.Text = "Shade Up";
            BtnShadeUp.Size = BtnSwapVertically.Size;
            BtnShadeUp.Location = new Point(
                BtnSwapVertically.Left,
                BtnSwapVertically.Bottom + 6);
            BtnShadeUp.TabStop = false;
            BtnShadeUp.Visible = false;
            BtnShadeUp.Click += (sender, e) => IncreaseSelectedShades();

            BtnShadeDown = new Button();
            BtnShadeDown.Name = "BtnShadeDown";
            BtnShadeDown.Text = "Shade Down";
            BtnShadeDown.Size = BtnFill.Size;
            BtnShadeDown.Location = new Point(
                BtnFill.Left,
                BtnFill.Bottom + 6);
            BtnShadeDown.TabStop = false;
            BtnShadeDown.Visible = false;
            BtnShadeDown.Click += (sender, e) => DecreaseSelectedShades();

            GrpOneCharacter.Controls.Add(BtnShadeUp);
            GrpOneCharacter.Controls.Add(BtnShadeDown);

            BtnShadeUp.BringToFront();
            BtnShadeDown.BringToFront();
        }

        private void UpdateShadeButtonsLayout()
        {
            if (grayscalePalette == null ||
                BtnShadeUp == null ||
                BtnShadeDown == null)
                return;

            bool is4Bit = grayscalePalette.Visible;

            const int extraHeight = 29;
            const int normalGroupHeight = 139;
            const int normalNextGroupTop = 146;

            int offset = is4Bit ? extraHeight : 0;

            GrpOneCharacter.Height = normalGroupHeight + offset;
            GrpAllCharacters.Top = normalNextGroupTop + offset;

            // Move the controls below the editing groups.
            GrpImportExport.Top = _normalImportExportTop + offset;
            GrpRenderingTable.Top = _normalRenderingTableTop + offset;

            // Keep the standalone Dark Mode button and plugin buttons
            // aligned beneath the Text Rendering Table.
            if (BtnAboutPlugin != null)
            {
                PositionPluginButtons();
            }
            else
            {
                BtnDarkMode.Top = GrpRenderingTable.Bottom + 8;
            }

            BtnShadeUp.Visible = is4Bit;
            BtnShadeDown.Visible = is4Bit;
        }

        private static byte[] ConvertBitmapTo4BitBytes(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int bytesPerRow = (width + 1) / 2;

            byte[] pixels = new byte[bytesPerRow * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte shade = ColorTo4BitShade(bitmap.GetPixel(x, y));

                    int offset = y * bytesPerRow + x / 2;

                    if ((x & 1) == 0)
                        pixels[offset] |= (byte)(shade << 4);
                    else
                        pixels[offset] |= shade;
                }
            }

            return pixels;
        }


        private static byte[] ConvertCopiedGlyphBitDepth(
            CopiedGlyph glyph, int targetBitDepth)
        {
            if (glyph.ByteLines == null)
                return null;

            if (glyph.BitDepth == targetBitDepth)
                return (byte[])glyph.ByteLines.Clone();

            int width = glyph.Width;
            int height = glyph.Height;

            int sourceStride = glyph.BitDepth == 4
                ? (width + 1) / 2
                : (width + 7) / 8;

            int targetStride = targetBitDepth == 4
                ? (width + 1) / 2
                : (width + 7) / 8;

            byte[] converted = new byte[targetStride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sourceOffset = y * sourceStride +
                        (glyph.BitDepth == 4 ? x / 2 : x / 8);

                    byte source = sourceOffset < glyph.ByteLines.Length
                        ? glyph.ByteLines[sourceOffset]
                        : (byte)0;

                    int shade;

                    if (glyph.BitDepth == 4)
                        shade = (x & 1) == 0
                            ? source >> 4
                            : source & 0x0F;
                    else
                        shade = (source & (0x80 >> (x & 7))) != 0
                            ? (targetBitDepth == 4 ? 8 : 15)
                            : 0;

                    int targetOffset = y * targetStride +
                        (targetBitDepth == 4 ? x / 2 : x / 8);

                    if (targetBitDepth == 4)
                    {
                        if ((x & 1) == 0)
                            converted[targetOffset] |= (byte)(shade << 4);
                        else
                            converted[targetOffset] |= (byte)shade;
                    }
                    else if (shade > 0)
                    {
                        converted[targetOffset] |=
                            (byte)(0x80 >> (x & 7));
                    }
                }
            }

            return converted;
        }

        private static bool IsForbiddenSeparatorColor(Color color)
        {
            // Fully transparent separators are allowed.
            if (color.A == 0)
                return false;

            // The 16 WFN grayscale shades are:
            // RGB(0,0,0), RGB(17,17,17), ... RGB(255,255,255).
            return color.R == color.G &&
                   color.G == color.B &&
                   color.R % 17 == 0;
        }

        private static byte ColorTo4BitShade(Color color)
        {
            // Fully transparent pixels remain transparent.
            if (color.A == 0)
                return 0;

            // Pure black represents transparency in WFN File 2.
            if (color.R == 0 && color.G == 0 && color.B == 0)
                return 0;

            // Calculate perceived brightness.
            double brightness =
                0.299 * color.R +
                0.587 * color.G +
                0.114 * color.B;

            // Choose the nearest of the 16 grayscale levels.
            int shade = (int)Math.Round(
                brightness / 17.0,
                MidpointRounding.AwayFromZero);

            // Non-black pixels must never become transparent.
            return (byte)Math.Max(1, Math.Min(15, shade));
        }

        private const Int32 MaxWidth = Settings.MaxGlyphSize;
        private const Int32 MaxHeight = Settings.MaxGlyphSize;
        private bool ClickedOnCharacter = false;
        private bool FontModifiedSaved = false;
        public bool HasUnsavedChanges
        {
            get { return FontModifiedSaved; }
        }
        protected override void OnPanelClosing(bool canCancel, ref bool cancelClose)
        {
            base.OnPanelClosing(canCancel, ref cancelClose);

            // Do not prompt when AGS itself is closing.
            if (!canCancel || !HasUnsavedChanges)
                return;

            DialogResult result = MessageBox.Show(
                "This font has unsaved changes.\n\n" +
                "Do you want to save them before closing?",
                "Save Font Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
            {
                cancelClose = true;
            }
            else if (result == DialogResult.Yes)
            {
                try
                {
                    Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Failed to save the font:\n\n" + ex.Message,
                        "Save Font",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    cancelClose = true;
                }
            }
        }
        private bool ShowGrid = false;
        private bool GridFix = false;
        private Point Grid = new Point(-1, -1);
        private Pen GridPen = new Pen(Color.FromArgb(255, 64, 64, 64)); // darker gray
        private List<CFontInfo> EmbeddedFontList = new List<CFontInfo>();
        public event EventHandler OnFontModified;
        private const int MAX_VISIBLE_GLYPHS = 256; // UI safety cap
        private int PageStart = 0;         // index of first glyph shown
        private const int PageSize = 256;  // number of glyphs shown at once

        private enum SizeMode
        {
            ChangeNothing,
            ChangeWidth,
            ChangeHeight,
            ChangeBoth,
        }
        private enum EDirection
        {
            Next,
            Prev,
        }

        static bool ArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }
            return true;
        }

        private void UpdatePluginSaveButton()
        {
            if (BtnSavePlugin == null)
                return;

            BtnSavePlugin.Enabled = FontModifiedSaved;

            if (FontModifiedSaved)
            {
                BtnSavePlugin.UseVisualStyleBackColor = false;
                BtnSavePlugin.BackColor = Color.Firebrick;
                BtnSavePlugin.ForeColor = Color.White;
            }
            else
            {
                BtnSavePlugin.UseVisualStyleBackColor = false;
                BtnSavePlugin.BackColor = BtnDarkMode.BackColor;
                BtnSavePlugin.ForeColor = BtnDarkMode.ForeColor;
            }
        }

        private void BtnSavePlugin_Click(object sender, EventArgs e)
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to save the font:\n\n" + ex.Message,
                    "Save Font",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void MarkFontModified()
        {
            if (FontModifiedSaved)
                return;

            FontModifiedSaved = true;
            UpdatePluginSaveButton();

            EventHandler doChange = OnFontModified;

            if (doChange != null)
            {
                MyEventArgs me = new MyEventArgs();
                me.Modified = true;

                doChange(this, me);
            }
        }

        private void CheckChange()
        {
            MyEventArgs me = new MyEventArgs();
            EventHandler DoChange = OnFontModified;

            foreach (CCharInfo item in FontInfo.Character)
            {
                if (!ArraysEqual(item.ByteLines, item.ByteLinesOriginal)
                    || (item.Width != item.WidthOriginal)
                    || (item.Height != item.HeightOriginal))
                {
                    me.Modified = true;
                    break;
                }
            }

            if (FontModifiedSaved != me.Modified)
            {
                FontModifiedSaved = me.Modified;
                UpdatePluginSaveButton();

                if (null != DoChange)
                {
                    DoChange(this, me);
                }
            }
        }

        internal static string Chr(int codePoint)
        {
            if (codePoint < 0 || codePoint > 0xFFFF)
                throw new ArgumentOutOfRangeException(nameof(codePoint), codePoint, "Must be between 0 and 65535.");

            return char.ConvertFromUtf32(codePoint);
        }

        private void LoadInternalResources()
        {
            string[] sa = typeof(FontEditorPane).Assembly.GetManifestResourceNames();

            foreach (string s in sa)
            {
                if (s.EndsWith(".WFN"))
                {
                    // eigebettete Schrift gefunden
                    using (System.IO.Stream stream = typeof(FontEditorPane).Assembly.GetManifestResourceStream(s))
                    {
                        using (System.IO.BinaryReader br = new System.IO.BinaryReader(stream))
                        {
                            CWFNFontInfo fi = new CWFNFontInfo();
                            fi.Read(br);

                            string[] namearray = System.IO.Path.GetFileNameWithoutExtension(s).Split('.');
                            string name = namearray[namearray.Length - 1];

                            fi.FontName = "Internal" + name;
                            fi.FontPath = "InternalResource";
                            EmbeddedFontList.Add((CFontInfo)fi);
                        }
                    }
                }
            }
        }

        private void UpdateDrawingViewportHeight()
        {
            if (drawingScrollPanel == null || GroupBox == null)
                return;

            // Align with the visible glyph grid, leaving a small gap.
            int glyphBottom = GroupBox.Top + FlowCharacterPanel.Bottom + 10;

            drawingScrollPanel.Height = Math.Max(
                1,
                glyphBottom - drawingScrollPanel.Top - 5);
        }

        private void InitializeGlyphFlowPanel()
        {
            // Leave the standard FlowLayoutPanel intact in the Designer.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            FlowLayoutPanel original = FlowCharacterPanel;
            Control parent = original.Parent;

            if (parent == null)
                return;

            int position = parent.Controls.GetChildIndex(original);

            GlyphFlowLayoutPanel replacement = new GlyphFlowLayoutPanel();

            replacement.Name = original.Name;
            replacement.Bounds = original.Bounds;
            replacement.Anchor = original.Anchor;
            replacement.Dock = original.Dock;
            replacement.AutoScroll = original.AutoScroll;
            replacement.WrapContents = original.WrapContents;
            replacement.FlowDirection = original.FlowDirection;
            replacement.BackColor = original.BackColor;
            replacement.Margin = original.Margin;
            replacement.Padding = original.Padding;
            replacement.TabIndex = original.TabIndex;
            replacement.Visible = original.Visible;
            replacement.Enabled = original.Enabled;

            parent.Controls.Remove(original);
            parent.Controls.Add(replacement);
            parent.Controls.SetChildIndex(replacement, position);

            FlowCharacterPanel = replacement;
            replacement.GlyphMouseWheel += Glyphs_MouseWheel;
            replacement.MouseDown += FlowCharacterPanel_MouseDown;

            original.Dispose();
        }

        private int _initialWidthAdjustment = 0;

        public void SetInitialWidthAdjustment(int extraWidth)
        {
            _initialWidthAdjustment = Math.Max(0, extraWidth);
        }

        private Button BtnEditDefault;

        private void InitializeEditDefaultButton()
        {
            BtnEditDefault = new Button();
            BtnEditDefault.Name = "BtnEditDefault";
            BtnEditDefault.Text = "Edit Default";
            BtnEditDefault.Size = new Size(81, 22);
            BtnEditDefault.Location = new Point(
                PanelSize.Left + 6,
                PanelSize.Top - 20);
            BtnEditDefault.UseVisualStyleBackColor = true;
            BtnEditDefault.Click += BtnEditDefault_Click;

            this.Controls.Add(BtnEditDefault);
            BtnEditDefault.BringToFront();
        }

        private void BtnEditDefault_Click(object sender, EventArgs e)
        {
            int width = XmlSettings.PlaceholderWidth;
            int height = XmlSettings.PlaceholderHeight;

            if (Settings.PlaceholderSizeDialog(ref width, ref height) != DialogResult.OK)
                return;

            XmlSettings.PlaceholderWidth = width;
            XmlSettings.PlaceholderHeight = height;
            XmlSettings.Write();

            // Update existing placeholders without modifying actual glyph data.
            RefreshPlaceholders();

            if (FontInfo != null &&
                FontInfo.Character != null &&
                Index >= 0 &&
                Index < FontInfo.Character.Length)
            {
                RefreshDrawingAreaDisplay(FontInfo.Character[Index]);
            }
        }

        private void BaseConstructor()
        {
            /* ++ only for debugging */
            //string[] sa = typeof(FontEditorPane).Assembly.GetManifestResourceNames();

            //foreach (string s in sa)
            //{
            //	System.Diagnostics.Trace.WriteLine(s);
            //}
            /* -- only for debugging */

            // Font resource names
            //AGS.Plugin.FontEditor.EmbeddedResources.AGSFNT0.WFN
            //AGS.Plugin.FontEditor.EmbeddedResources.AGSFNT1.WFN
            //AGS.Plugin.FontEditor.EmbeddedResources.AGSFNT2.WFN

            XmlSettings.Read();

            // Restore the saved theme.
            _darkMode = XmlSettings.DarkMode;

            InitializeComponent();
            InitializeEyedropperCursor();

            _normalImportExportTop = GrpImportExport.Top;
            _normalRenderingTableTop = GrpRenderingTable.Top;

            // Use the same font regardless of the hosting application.
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point);

            // Keep the glyph range field aligned with the right edge of the glyph grid.
            TxtGlyphRange.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            BtnImportPNG.Enabled = true;
            BtnImportPNG.Click += BtnImportPNG_Click;

            BtnExportPNG.Enabled = true;
            BtnExportPNG.Click += BtnExportPNG_Click;

            InitializeEditDefaultButton();

            PictRenderText.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            PictRenderText.SizeMode = PictureBoxSizeMode.AutoSize;
            PictRenderText.Top += 12;

            // Restore the saved text rendering mode without triggering
            // the rendering handlers during initialization.
            RdoRenderUnicode.CheckedChanged -= RdoRenderUnicode_CheckedChanged;
            RdoRenderANSI.CheckedChanged -= RdoRenderANSI_CheckedChanged;

            RdoRenderUnicode.Checked = XmlSettings.RenderUnicode;
            RdoRenderANSI.Checked = !XmlSettings.RenderUnicode;

            RdoRenderUnicode.CheckedChanged += RdoRenderUnicode_CheckedChanged;
            RdoRenderANSI.CheckedChanged += RdoRenderANSI_CheckedChanged;

            LblGlyphField.Location = new System.Drawing.Point(
                TxtGlyph.Left - 18,
                TxtGlyph.Top + 3);

            LblIndexField.Location = new System.Drawing.Point(
                TxtCharacter.Left - 18,
                TxtCharacter.Top + 3);

            numWidth.KeyDown += GlyphSize_KeyDown;
            numHeight.KeyDown += GlyphSize_KeyDown;

            numWidth.KeyPress += GlyphSize_KeyPress;
            numHeight.KeyPress += GlyphSize_KeyPress;

            // Automatically highlight the entire value when entering
            // either size field using Tab or Shift+Tab.
            numWidth.Enter += GlyphSize_Enter;
            numHeight.Enter += GlyphSize_Enter;

            numWidth.Leave += GlyphSize_Leave;
            numHeight.Leave += GlyphSize_Leave;

            numWidth.MouseWheel += (sender, e) =>
            {
                HandledMouseEventArgs handled = e as HandledMouseEventArgs;
                if (handled != null)
                    handled.Handled = true;
            };

            numHeight.MouseWheel += (sender, e) =>
            {
                HandledMouseEventArgs handled = e as HandledMouseEventArgs;
                if (handled != null)
                    handled.Handled = true;
            };

            InitializeGlyphFlowPanel();

            FlowCharacterPanel.Paint += (sender, e) =>
            {
                foreach (PictureBox pict in _selectedPreviews)
                {
                    if (pict.Parent != FlowCharacterPanel)
                        continue;

                    Color outlineColor =
                        (FlowCharacterPanel.ContainsFocus || _canvasSelectionActive)
                            ? Color.DeepSkyBlue
                            : Color.CadetBlue;

                    using (Pen pen = new Pen(outlineColor, 2))
                    {
                        Rectangle rect = pict.Bounds;
                        rect.Inflate(1, 1);

                        e.Graphics.DrawRectangle(pen, rect);
                    }
                }
            };

            FlowCharacterPanel.Enter += (sender, e) =>
            {
                FlowCharacterPanel.Invalidate();
            };

            FlowCharacterPanel.Leave += (sender, e) =>
            {
                _canvasSelectionActive = false;
                FlowCharacterPanel.Invalidate();
            };

            TxtPageNumber.Leave += TxtPageNumber_Leave;

            // Scrollable viewport for the zoomed glyph editor.
            drawingScrollPanel = new System.Windows.Forms.Panel();
            drawingScrollPanel.Name = "drawingScrollPanel";
            drawingScrollPanel.Location = DrawingArea.Location;
            drawingScrollPanel.Size = new Size(
                260,
                GroupBox.Bottom - drawingScrollPanel.Top);
            drawingScrollPanel.AutoScroll = true;
            drawingScrollPanel.BackColor = Color.Black;

            drawingScrollPanel.MouseDown += (sender, e) =>
            {
                ApplyPendingGlyphRange();
            };

            // Move the existing drawing surface inside the viewport.
            this.Controls.Remove(DrawingArea);
            DrawingArea.Location = Point.Empty;
            drawingScrollPanel.Controls.Add(DrawingArea);

            this.Controls.Add(drawingScrollPanel);
            drawingScrollPanel.BringToFront();

            // Automatically resize the glyph grid when the pane width changes.
            int originalPaneWidth = this.ClientSize.Width;
            int originalGlyphWidth = GroupBox.Width;

            var rightSideControls = this.Controls
                .Cast<Control>()
                .Where(control => control != GroupBox &&
                                  control != BtnDarkMode &&
                                  control.Left >= 330)
                .ToDictionary(control => control, control => control.Left);

            this.HandleCreated += (sender, e) =>
            {
                this.BeginInvoke((MethodInvoker)(() =>
                {
                    originalPaneWidth = this.ClientSize.Width;
                    this.OnResize(EventArgs.Empty);

                    // Complete the initial layout before displaying the glyph.
                    GroupBox.PerformLayout();
                    UpdateDrawingViewportHeight();

                    drawingScrollPanel.PerformLayout();
                    DrawingArea.Invalidate();
                }));
            };

            this.Resize += (sender, e) =>
            {
                if (originalPaneWidth == 0)
                    return;

                int widthChange;

                if (this.FindForm() != null &&
                    this.FindForm().GetType().Name == "MainWindow")
                {
                    // Standalone: preserve the existing resizing behavior.
                    widthChange = Math.Max(0,
                        this.ClientSize.Width - originalPaneWidth + _initialWidthAdjustment);
                }
                else
                {
                    // AGS plugin: expand the glyph grid while keeping
                    // the right-hand controls near the edge of the pane.
                    const int rightmostControlEdge = 776;
                    const int rightMargin = 12;

                    widthChange = Math.Max(0,
                        this.ClientSize.Width - rightmostControlEdge - rightMargin);
                }

                GroupBox.Width = originalGlyphWidth + widthChange;

                foreach (var item in rightSideControls)
                    item.Key.Left = item.Value + widthChange;

                // Keep the 4-bit grayscale palette aligned with the other controls.
                if (grayscalePalette != null)
                    grayscalePalette.Left = 335 + widthChange;

                // Keep the 1-4-bit button aligned with the other controls.
                if (BtnBitDepth != null)
                    BtnBitDepth.Left = 392 + widthChange;

                // Keep the drawing viewport's bottom aligned with the glyph grid.
                UpdateDrawingViewportHeight();

                Point renderTextPosition =
                    PictRenderText.Parent.PointToClient(
                        BtnRenderText.Parent.PointToScreen(
                            new Point(BtnRenderText.Right + 5, BtnRenderText.Top)));

            PictRenderText.Location = renderTextPosition;

                if (BtnAboutPlugin != null)
                {
                    PositionPluginButtons();
                }
                else
                {
                    // Preserve the existing standalone positioning.
                    BtnDarkMode.Left = GrpRenderingTable.Right - BtnDarkMode.Width;
                }
            };

            // Update the preview height whenever the glyph area's layout changes.
            GroupBox.Layout += (sender, e) =>
            {
                UpdateDrawingViewportHeight();
            };

            BtnBitDepth = new System.Windows.Forms.Button();
            BtnBitDepth.Name = "BtnBitDepth";
            BtnBitDepth.Text = "1-4-bit";
            BtnBitDepth.Location = new System.Drawing.Point(392, 9);
            BtnBitDepth.Size = new System.Drawing.Size(46, 25);
            BtnBitDepth.UseVisualStyleBackColor = true;
            BtnBitDepth.Click += BtnBitDepth_Click;
            _toolTip.SetToolTip(BtnBitDepth, "Toggle between 1-bit and 4-bit font format");
            BtnBitDepth.Enabled = false; // Conversion will be implemented later.
            this.Controls.Add(BtnBitDepth);
            BtnBitDepth.BringToFront();

            typeof(PictureBox)
                            .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(DrawingArea, true, null);

            typeof(Control)
                .GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(ZoomDrawingArea, true, null);

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            this.UpdateStyles();

            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_MouseDown);

            GroupBox.MouseDown += Form_MouseDown;
            GrpOneCharacter.MouseDown += Form_MouseDown;
            GrpAllCharacters.MouseDown += Form_MouseDown;
            PanelSize.MouseDown += Form_MouseDown;
            PanelMouseColor.MouseDown += Form_MouseDown;

            LblZoom.Text = "x" + ZoomDrawingArea.Value;

            numWidth.Maximum = MaxWidth;
            numHeight.Maximum = MaxHeight;

            numWidth.MouseDown += NumSize_MouseDown;
            numHeight.MouseDown += NumSize_MouseDown;
            numWidth.MouseUp += NumSize_MouseUp;
            numHeight.MouseUp += NumSize_MouseUp;

            this.CreateControl();
            LoadInternalResources();

            ApplyTheme();

            BtnDarkMode.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            BtnDarkMode.Location = new Point(
                GrpRenderingTable.Right - BtnDarkMode.Width,
                GrpRenderingTable.Bottom + 8);

            // The standalone application already has an About button in its toolbar.
            // Create this additional button only when hosted by AGS Editor.
            this.ParentChanged += (sender, e) =>
            {
                Form hostForm = this.FindForm();

                if (hostForm == null)
                    return;

                if (hostForm.GetType().Name == "MainWindow")
                    return;

                if (BtnAboutPlugin != null)
                    return;

                BtnAboutPlugin = new Button();
                BtnAboutPlugin.Name = "BtnAboutPlugin";
                BtnAboutPlugin.Text = "About";
                BtnAboutPlugin.Size = BtnDarkMode.Size;
                BtnAboutPlugin.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                BtnAboutPlugin.UseVisualStyleBackColor = true;
                BtnAboutPlugin.Click += BtnAboutPlugin_Click;

                this.Controls.Add(BtnAboutPlugin);

                BtnSavePlugin = new Button();
                BtnSavePlugin.Name = "BtnSavePlugin";
                BtnSavePlugin.Text = "Save";
                BtnSavePlugin.Click += BtnSavePlugin_Click;
                UpdatePluginSaveButton();
                BtnSavePlugin.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                BtnSavePlugin.UseVisualStyleBackColor = true;

                this.Controls.Add(BtnSavePlugin);

                // Apply the current light/dark theme to both plugin buttons.
                ApplyTheme();

                BtnAboutPlugin.BringToFront();
                BtnSavePlugin.BringToFront();
                PositionPluginButtons();
            };
        }

        private void PositionPluginButtons()
        {
            if (BtnAboutPlugin == null || BtnSavePlugin == null)
                return;

            const int buttonGap = 4;
            const int aboutWidth = 45;
            const int darkModeWidth = 70;

            int availableWidth = GrpRenderingTable.Width;

            int saveWidth = availableWidth
                - aboutWidth
                - darkModeWidth
                - (buttonGap * 2);

            if (saveWidth < 1)
                saveWidth = 1;

            int top = GrpRenderingTable.Bottom + 8;
            int right = GrpRenderingTable.Right;

            BtnAboutPlugin.Size = new Size(aboutWidth, BtnDarkMode.Height);
            BtnDarkMode.Width = darkModeWidth;
            BtnSavePlugin.Size = new Size(saveWidth, BtnDarkMode.Height);

            BtnAboutPlugin.Location = new Point(
                right - aboutWidth, top);

            BtnDarkMode.Location = new Point(
                BtnAboutPlugin.Left - buttonGap - darkModeWidth, top);

            BtnSavePlugin.Location = new Point(
                BtnDarkMode.Left - buttonGap - saveWidth, top);
        }

        public CFontInfo CurrentFontInfo
        {
            get { return FontInfo; }
        }
        public void MarkAsSaved()
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            foreach (CCharInfo character in FontInfo.Character)
            {
                if (character == null)
                    continue;

                character.WidthOriginal = character.Width;
                character.HeightOriginal = character.Height;

                if (character.ByteLines != null)
                    character.ByteLinesOriginal =
                        (byte[])character.ByteLines.Clone();
                else
                    character.ByteLinesOriginal = null;
            }

            // The current state is now the saved baseline.
            FontModifiedSaved = false;
            UpdatePluginSaveButton();
        }
        public FontEditorPane()
        {
            BaseConstructor();
        }
        public FontEditorPane(string filepath, string filename, string fontname)
        {
            BaseConstructor();

            if (System.IO.File.Exists(System.IO.Path.Combine(filepath, filename)))
            {
                if (System.IO.Path.GetExtension(filename).ToLower() == ".wfn")
                {
                    FontInfo = new CWFNFontInfo();
                }
                else if (System.IO.Path.GetFileNameWithoutExtension(filename).ToLower() == "font")
                {
                    FontInfo = new CSCIFontInfo();
                }

                FontInfo.FontPath = System.IO.Path.Combine(filepath, filename);
                FontInfo.FontName = fontname;

                System.IO.FileStream file = null;
                System.IO.BinaryReader binaryReader = null;

                try
                {
                    file = System.IO.File.Open(FontInfo.FontPath, System.IO.FileMode.Open);
                    binaryReader = new System.IO.BinaryReader(file);
                    FontInfo.Read(binaryReader);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Failed to load font:\n\n" + ex.Message,
                        "Load Error",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);

                    throw new System.IO.InvalidDataException("Font loading failed.", ex);
                }
                finally
                {
                    if (null != binaryReader)
                    {
                        binaryReader.Close();
                    }
                    if (null != file)
                    {
                        file.Close();
                    }
                }
                PageStart = 0;

                // Establish the initial Undo state for every loaded glyph.
                foreach (CCharInfo character in FontInfo.Character)
                {
                    if (character != null)
                        character.UndoRedoListAdd(character.ByteLines);
                }

                InitializeGrayscalePalette();

                grayscalePalette.Visible =
                    FontInfo is CWFNFontInfo wfn && wfn.Is4Bit;

                UpdateShadeButtonsLayout();

                BtnBitDepth.Enabled = FontInfo is CWFNFontInfo || FontInfo is CSCIFontInfo;

                RebuildPage();
                UpdateGlyphTextbox();
                TxtGlyphRange.Text = (FontInfo.Character.Length - 1).ToString();
                SetGridFix();
            }

            FlowCharacterPanel.BackColor = XmlSettings.Color;
            ChkGrid.Checked = XmlSettings.Grid;
            ChkGridFix.Checked = XmlSettings.GridFix;
            ShowCharacterCount();
        }

        private void ShowCharacterCount()
        {
            if (FontInfo != null && FontInfo.Character != null)
            {
                long sizeBytes = CalculateCurrentWFNSize();
                double sizeKB = sizeBytes / 1024.0;

                GroupBox.Text = $"Font size {sizeKB:0.00} KB" +
                    ((FontInfo is CWFNFontInfo wfnSize && wfnSize.Is4Bit) ? "" : " / 64 KB");

                if (!(FontInfo is CWFNFontInfo wfn4 && wfn4.Is4Bit) &&
                    sizeBytes > 65535)
                {
                    GroupBox.ForeColor = Color.Red;
                    GroupBox.Text =
                        $"Font size {sizeKB:0.00} KB / 64 KB (THIS WON'T SAVE!)";
                }
                else
                {
                    GroupBox.ForeColor =
                        _darkMode
                            ? Color.White
                            : Color.Black;

                    GroupBox.Text =
                        $"Font size {sizeKB:0.00} KB" +
                            ((FontInfo is CWFNFontInfo wfnSize2 && wfnSize2.Is4Bit) ? "" : " / 64 KB");
                }
            }
            else
            {
                GroupBox.Text = "Selected font settings";

                GroupBox.ForeColor =
                    _darkMode
                        ? Color.White
                        : Color.Black;
            }
        }


        private void RebuildPage()
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            FlowCharacterPanel.SuspendLayout();

            try
            {
                FlowCharacterPanel.Controls.Clear();
                CharacterPictureList.Clear();

                int end = Math.Min(PageStart + PageSize, FontInfo.Character.Length);

                for (int i = PageStart; i < end; i++)
                {
                    AddCharacterToList(FontInfo.Character[i]);
                }
            }
            finally
            {
                FlowCharacterPanel.ResumeLayout();
            }

            ShowCharacterCount();
            UpdatePageButtons();
            
            int localIndex = Index - PageStart;

            if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
            {
                PictureBox targetPicture = CharacterPictureList[localIndex];
                SelectOnlyPreview(targetPicture);
                DisplayCurrentCharacter((CCharInfo)targetPicture.Tag);
            }
        }

        private void AddCharacterToList(CCharInfo item)
        {
            Bitmap bitmap = null;
            PictureBox pict = new PictureBox();
            pict.Margin = new Padding(3);
            CharacterPictureList.Add(pict);

            pict.Tag = item;
            pict.Width = item.Width * Scalefactor;
            pict.Height = item.Height * Scalefactor;
            pict.Click += new EventHandler(Character_Click);
            pict.MouseDown += new MouseEventHandler(Character_MouseDown);
            pict.ContextMenu = new ContextMenu();

            _toolTip.SetToolTip(pict, Chr(item.Index));

            pict.ContextMenu.Tag = pict;
            MenuItem menuUndo = pict.ContextMenu.MenuItems.Add("Undo");
            menuUndo.Shortcut = Shortcut.CtrlZ;
            menuUndo.Click += new EventHandler(MenuUndoClicked);

            MenuItem menuRedo = pict.ContextMenu.MenuItems.Add("Redo");
            menuRedo.Shortcut = Shortcut.CtrlY;
            menuRedo.Click += new EventHandler(MenuRedoClicked);

            MenuItem menuCopy = pict.ContextMenu.MenuItems.Add("Copy");
            menuCopy.Shortcut = Shortcut.CtrlC;
            menuCopy.Click += new EventHandler(MenuCopyClicked);

            MenuItem menuPaste = pict.ContextMenu.MenuItems.Add("Paste");
            menuPaste.Shortcut = Shortcut.CtrlV;
            menuPaste.Click += new EventHandler(MenuPasteClicked);

            MenuItem menuSelectAll = pict.ContextMenu.MenuItems.Add("Select All");
            menuSelectAll.Shortcut = Shortcut.CtrlA;
            menuSelectAll.Click += new EventHandler(MenuSelectAllClicked);

            pict.ContextMenu.MenuItems.Add("-");

            MenuItem menuRemove = pict.ContextMenu.MenuItems.Add("Remove glyph data");
            menuRemove.Shortcut = Shortcut.Del;
            menuRemove.Click += new EventHandler(MenuRemoveGlyphDataClicked);

            pict.ContextMenu.MenuItems.Add("-");

            MenuItem menuInsertBefore = pict.ContextMenu.MenuItems.Add("Insert a new glyph before");
            menuInsertBefore.Shortcut = Shortcut.ShiftIns;
            menuInsertBefore.Click += new EventHandler(MenuInsertGlyphClicked);

            MenuItem menuInsertAfter = pict.ContextMenu.MenuItems.Add("Insert a new glyph after");
            menuInsertAfter.Shortcut = Shortcut.Ins;
            menuInsertAfter.Click += new EventHandler(MenuInsertGlyphClicked);

            MenuItem menuDelete = pict.ContextMenu.MenuItems.Add("Delete glyph");
            menuDelete.Shortcut = Shortcut.ShiftDel;
            menuDelete.Click += new EventHandler(MenuDeleteGlyphClicked);

            pict.ContextMenu.MenuItems[0].Enabled = _operationUndoStack.Count > 0;
            pict.ContextMenu.MenuItems[1].Enabled = _operationRedoStack.Count > 0;

            // If glyph has no size yet, show placeholder
            if (item.Width == 0 || item.Height == 0)
            {
                // Small placeholder preview size
                int previewW = PLACEHOLDER_W * Scalefactor;
                int previewH = PLACEHOLDER_H * Scalefactor;

                Bitmap placeholder = new Bitmap(previewW, previewH);
                using (Graphics g = Graphics.FromImage(placeholder))
                {
                    // Fill placeholder background to the same tone of panel
                    g.Clear(XmlSettings.Color);

                    // White stroke border
                    using (Pen pen = new Pen(Color.White))
                    {
                        g.DrawRectangle(pen, 0, 0, previewW - 1, previewH - 1);
                    }
                }

                pict.Image = placeholder;
                pict.Width = previewW;
                pict.Height = previewH;

                item.UnscaledImage = null;
            }
            else
            {
                CFontUtils.CreateBitmap(item, out bitmap);

                item.UnscaledImage = bitmap;

                // Rebuilding a page must not create an Undo history entry.

                Bitmap outbmp;
                CFontUtils.ScaleBitmap(bitmap, out outbmp, Scalefactor);
                pict.Image = outbmp;

                pict.Width = outbmp.Width;
                pict.Height = outbmp.Height;
            }
            
            FlowCharacterPanel.Controls.Add(pict);
        }


        private void RefreshCharacterPreview(PictureBox pict, CCharInfo item)
        {
            if (pict == null || item == null)
                return;

            // This PictureBox may now represent a different glyph after
            // an Insert/Delete shift.
            pict.Tag = item;
            _toolTip.SetToolTip(pict, Chr(item.Index));

            // Dispose the old displayed preview image.
            if (pict.Image != null)
            {
                Image oldImage = pict.Image;
                pict.Image = null;
                oldImage.Dispose();
            }

            // Empty glyph: draw the normal placeholder.
            if (item.Width == 0 || item.Height == 0)
            {
                int previewW = PLACEHOLDER_W * Scalefactor;
                int previewH = PLACEHOLDER_H * Scalefactor;

                Bitmap placeholder = new Bitmap(previewW, previewH);

                using (Graphics g = Graphics.FromImage(placeholder))
                {
                    g.Clear(XmlSettings.Color);

                    using (Pen pen = new Pen(Color.White))
                    {
                        g.DrawRectangle(
                            pen,
                            0,
                            0,
                            previewW - 1,
                            previewH - 1);
                    }
                }

                pict.Image = placeholder;
                pict.Width = previewW;
                pict.Height = previewH;

                item.UnscaledImage = null;
            }
            else
            {
                Bitmap bitmap;
                CFontUtils.CreateBitmap(item, out bitmap);

                item.UnscaledImage = bitmap;

                Bitmap outbmp;
                CFontUtils.ScaleBitmap(bitmap, out outbmp, Scalefactor);

                pict.Image = outbmp;
                pict.Width = outbmp.Width;
                pict.Height = outbmp.Height;
            }

            if (pict.ContextMenu != null && pict.ContextMenu.MenuItems.Count >= 2)
            {
                pict.ContextMenu.MenuItems[0].Enabled = _operationUndoStack.Count > 0;
                pict.ContextMenu.MenuItems[1].Enabled = _operationRedoStack.Count > 0;
            }

            pict.Invalidate();
        }


        private void RefreshPageFromIndex(int startIndex)
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            int desiredCount =
                Math.Min(PageSize, FontInfo.Character.Length - PageStart);

            if (desiredCount < 0)
                desiredCount = 0;

            FlowCharacterPanel.SuspendLayout();

            try
            {
                // If Insert increased a partial page, create only the
                // newly required preview control(s) at the end.
                while (CharacterPictureList.Count < desiredCount)
                {
                    int characterIndex =
                        PageStart + CharacterPictureList.Count;

                    AddCharacterToList(FontInfo.Character[characterIndex]);
                }

                // Refresh only the glyphs affected by the shift.
                int firstLocalIndex = startIndex - PageStart;

                if (firstLocalIndex < 0)
                    firstLocalIndex = 0;

                for (int localIndex = firstLocalIndex;
                     localIndex < desiredCount;
                     localIndex++)
                {
                    int characterIndex = PageStart + localIndex;

                    RefreshCharacterPreview(
                        CharacterPictureList[localIndex],
                        FontInfo.Character[characterIndex]);
                }
            }
            finally
            {
                FlowCharacterPanel.ResumeLayout();
            }

            ShowCharacterCount();
            UpdatePageButtons();

            // Keep the current glyph selected and refresh the large editor view.
            int selectedLocalIndex = Index - PageStart;

            if (selectedLocalIndex >= 0 &&
                selectedLocalIndex < CharacterPictureList.Count)
            {
                DisplayCurrentCharacter(
                    FontInfo.Character[Index]);
            }
        }


        private bool CorrectImage(Bitmap original, out Bitmap corrected)
        {

            BitmapData bmpData = original.LockBits(new Rectangle(0, 0, original.Width, original.Height), ImageLockMode.ReadWrite, original.PixelFormat);

            if (bmpData.Stride < 0)
            {
                corrected = new Bitmap(original.Width, original.Height, original.PixelFormat);
                BitmapData cbmpdata = corrected.LockBits(new Rectangle(0, 0, corrected.Width, corrected.Height), ImageLockMode.ReadWrite, corrected.PixelFormat);

                IntPtr ptroriginal = bmpData.Scan0;
                IntPtr ptrorrected = cbmpdata.Scan0;
                byte[] array = new byte[Math.Abs(bmpData.Stride)];

                for (int cnt = 0; cnt < original.Height; cnt++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(ptroriginal, array, 0, Math.Abs(bmpData.Stride));
                    System.Runtime.InteropServices.Marshal.Copy(array, 0, ptrorrected, cbmpdata.Stride);

                    ptroriginal = (IntPtr)((int)ptroriginal + bmpData.Stride);
                    ptrorrected = (IntPtr)((int)ptrorrected + cbmpdata.Stride);
                }

                original.UnlockBits(bmpData);
                corrected.UnlockBits(cbmpdata);

                return true;
            }
            else
            {
                original.UnlockBits(bmpData);
                corrected = null;
                return false;
            }
        }


        private bool UndoLastStructuralEdit()
        {
            if (_structuralUndoStack.Count == 0)
                return false;

            StructuralEdit edit = _structuralUndoStack.Pop();

            if (edit.Type == StructuralEditType.Insert)
            {
                if (edit.InsertGrewArray)
                {
                    // Undo an insert that increased the font length:
                    // shift everything back left and restore the old length.
                    for (int i = edit.Index; i < edit.OldLength; i++)
                    {
                        FontInfo.Character[i] = FontInfo.Character[i + 1];
                        FontInfo.Character[i].Index = i;
                    }

                    Array.Resize(ref FontInfo.Character, edit.OldLength);
                }
                else
                {
                    // Undo an insert on a full 256-slot page:
                    // shift everything back left and restore an empty final slot.
                    for (int i = edit.Index; i < edit.PageEnd; i++)
                    {
                        FontInfo.Character[i] = FontInfo.Character[i + 1];
                        FontInfo.Character[i].Index = i;
                    }

                    CCharInfo emptyCharacter = edit.DisplacedCharacter;
                    emptyCharacter.Index = edit.PageEnd;

                    FontInfo.Character[edit.PageEnd] = emptyCharacter;
                }
            }
            else if (edit.Type == StructuralEditType.Delete)
            {
                // Undo Delete by shifting everything right again.
                for (int i = edit.PageEnd; i > edit.Index; i--)
                {
                    FontInfo.Character[i] = FontInfo.Character[i - 1];
                    FontInfo.Character[i].Index = i;
                }

                // Restore the glyph that was deleted.
                FontInfo.Character[edit.Index] = edit.DeletedCharacter;
                FontInfo.Character[edit.Index].Index = edit.Index;
            }

            else // MultiDelete
            {
                // Restore deleted indexes from lowest to highest.
                // Each restoration shifts the remaining glyphs right.
                for (int d = 0; d < edit.DeletedIndexes.Count; d++)
                {
                    int restoreIndex =
                        edit.DeletedIndexes[d];

                    for (int i = edit.PageEnd;
                         i > restoreIndex;
                         i--)
                    {
                        FontInfo.Character[i] =
                            FontInfo.Character[i - 1];

                        FontInfo.Character[i].Index = i;
                    }

                    FontInfo.Character[restoreIndex] =
                        edit.DeletedCharacters[d];

                    FontInfo.Character[restoreIndex].Index =
                        restoreIndex;
                }
            }

            // This operation can now later be Redone.
            _structuralRedoStack.Push(edit);

            FontInfo.NumberOfCharacters = FontInfo.Character.Length;
            TxtGlyphRange.Text = (FontInfo.Character.Length - 1).ToString();

            Index = edit.Index;

            int targetPage = (edit.Index / PageSize) * PageSize;

            if (targetPage != PageStart)
            {
                PageStart = targetPage;
                RebuildPage();
            }
            else
            {
                RefreshPageFromIndex(edit.Index);

                int localIndex = edit.Index - PageStart;

                if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                {
                    Character_Click(CharacterPictureList[localIndex],
                        new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                }
            }

            if (edit.Type == StructuralEditType.Insert)
            {
                ClearPreviewSelection();

                if (edit.SelectionBeforeEdit != null)
                {
                    foreach (int selectedIndex in edit.SelectionBeforeEdit)
                    {
                        int localIndex = selectedIndex - PageStart;

                        if (localIndex >= 0 &&
                            localIndex < CharacterPictureList.Count)
                        {
                            AddPreviewToSelection(CharacterPictureList[localIndex]);
                        }
                    }
                }

                int activeIndex = edit.ActiveIndexBeforeEdit;

                if (activeIndex >= 0 &&
                    activeIndex < FontInfo.Character.Length)
                {
                    Index = activeIndex;

                    int localIndex = activeIndex - PageStart;

                    if (localIndex >= 0 &&
                        localIndex < CharacterPictureList.Count)
                    {
                        _selectedPreview = CharacterPictureList[localIndex];
                        _selectionAnchor = _selectedPreview;

                        RefreshDrawingAreaDisplay(FontInfo.Character[Index]);
                    }
                }

                FlowCharacterPanel.Invalidate();
            }

            // Restore the original multiple selection after undoing
            // a multi-glyph deletion.
            if (edit.Type == StructuralEditType.MultiDelete)
            {
                ClearPreviewSelection();

                foreach (int selectedIndex in edit.DeletedIndexes)
                {
                    int selectedLocalIndex = selectedIndex - PageStart;

                    if (selectedLocalIndex >= 0 &&
                        selectedLocalIndex < CharacterPictureList.Count)
                    {
                        AddPreviewToSelection(
                            CharacterPictureList[selectedLocalIndex]);
                    }
                }

                int activeIndex = edit.ActiveIndexBeforeEdit;

                if (activeIndex >= 0 &&
                    activeIndex < FontInfo.Character.Length)
                {
                    Index = activeIndex;

                    int activeLocalIndex = Index - PageStart;

                    if (activeLocalIndex >= 0 &&
                        activeLocalIndex < CharacterPictureList.Count)
                    {
                        _selectedPreview = CharacterPictureList[activeLocalIndex];
                        _selectionAnchor = _selectedPreview;

                        RefreshDrawingAreaDisplay(FontInfo.Character[Index]);
                    }
                }

                FlowCharacterPanel.PerformLayout();
                FlowCharacterPanel.Invalidate();
            }

            UpdateGlyphTextbox();
            CheckChange();

            return true;
        }


        private bool RedoLastStructuralEdit()
        {
            if (_structuralRedoStack.Count == 0)
                return false;

            StructuralEdit edit = _structuralRedoStack.Pop();

            // Clear the restored selection before shifting glyphs.
            if (edit.Type == StructuralEditType.Insert)
            {
                ClearPreviewSelection();
            }

            if (edit.Type == StructuralEditType.Insert)
            {
                if (edit.InsertGrewArray)
                {
                    Array.Resize(
                        ref FontInfo.Character,
                        FontInfo.Character.Length + 1);

                    for (int i = FontInfo.Character.Length - 1;
                         i > edit.Index;
                         i--)
                    {
                        FontInfo.Character[i] =
                            FontInfo.Character[i - 1];

                        FontInfo.Character[i].Index = i;
                    }
                }
                else
                {
                    for (int i = edit.PageEnd; i > edit.Index; i--)
                    {
                        FontInfo.Character[i] =
                            FontInfo.Character[i - 1];

                        FontInfo.Character[i].Index = i;
                    }
                }

                CCharInfo newCharacter = edit.InsertedCharacter;
                newCharacter.Index = edit.Index;

                FontInfo.Character[edit.Index] = newCharacter;
            }
            
            else if (edit.Type == StructuralEditType.Delete)
            {
                for (int i = edit.Index; i < edit.PageEnd; i++)
                {
                    FontInfo.Character[i] =
                        FontInfo.Character[i + 1];

                    FontInfo.Character[i].Index = i;
                }

                CCharInfo emptyCharacter = edit.DeletePlaceholder;
                emptyCharacter.Index = edit.PageEnd;

                FontInfo.Character[edit.PageEnd] = emptyCharacter;
            }

            else // MultiDelete
            {
                // Delete again from highest selected index to lowest.
                for (int d = edit.DeletedIndexes.Count - 1;
                     d >= 0;
                     d--)
                {
                    int deleteIndex =
                        edit.DeletedIndexes[d];

                    for (int i = deleteIndex;
                         i < edit.PageEnd;
                         i++)
                    {
                        FontInfo.Character[i] =
                            FontInfo.Character[i + 1];

                        FontInfo.Character[i].Index = i;
                    }

                    CCharInfo emptyCharacter = edit.DeletePlaceholders[
                        edit.DeletedIndexes.Count - 1 - d];

                    emptyCharacter.Index = edit.PageEnd;

                    FontInfo.Character[edit.PageEnd] =
                        emptyCharacter;
                }
            }

            _structuralUndoStack.Push(edit);

            FontInfo.NumberOfCharacters = FontInfo.Character.Length;
            TxtGlyphRange.Text = (FontInfo.Character.Length - 1).ToString();

            Index = edit.Index;

            int targetPage = (edit.Index / PageSize) * PageSize;

            if (targetPage != PageStart)
            {
                PageStart = targetPage;
                RebuildPage();
            }
            else
            {
                RefreshPageFromIndex(edit.Index);

                int localIndex = edit.Index - PageStart;

                if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                {
                    Character_Click(CharacterPictureList[localIndex],
                        new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                }
            }

            if (edit.Type == StructuralEditType.Insert)
            {
                int localIndex = edit.Index - PageStart;

                if (localIndex >= 0 &&
                    localIndex < CharacterPictureList.Count)
                {
                    Index = edit.Index;
                    SelectOnlyPreview(CharacterPictureList[localIndex]);
                    RefreshDrawingAreaDisplay(FontInfo.Character[Index]);
                }
            }

            UpdateGlyphTextbox();
            CheckChange();

            return true;
        }


        private bool UndoLastMultiOperation()
        {
            if (_multiUndoStack.Count == 0)
                return false;

            // Take only the most recent grouped operation.
            List<int> operationIndexes = _multiUndoStack.Pop();

            bool anythingUndone = false;

            foreach (int characterIndex in operationIndexes)
            {
                if (characterIndex < 0 ||
                    characterIndex >= FontInfo.Character.Length)
                {
                    continue;
                }

                CCharInfo character = FontInfo.Character[characterIndex];

                if (!character.UndoPossible)
                    continue;

                character.Undo();
                anythingUndone = true;

                int localIndex = characterIndex - PageStart;

                if (localIndex >= 0 &&
                    localIndex < CharacterPictureList.Count)
                {
                    PictureBox pict = CharacterPictureList[localIndex];

                    RefreshCharacterPreview(pict, character);

                    pict.ContextMenu.MenuItems[0].Enabled =
                        _operationUndoStack.Count > 0;

                    pict.ContextMenu.MenuItems[1].Enabled =
                        _operationRedoStack.Count > 0;
                }
            }

            if (anythingUndone)
            {
                // This entire grouped operation can now be redone.
                _multiRedoStack.Push(operationIndexes);

                // Refresh the active glyph without changing the selection.
                if (Index >= 0 && Index < FontInfo.Character.Length)
                    RefreshDrawingAreaDisplay(FontInfo.Character[Index]);

                FlowCharacterPanel.Invalidate();
                CheckChange();
            }

            return anythingUndone;
        }


        private bool RedoLastMultiOperation()
        {
            if (_multiRedoStack.Count == 0)
                return false;

            // Take the most recently undone grouped operation.
            List<int> operationIndexes = _multiRedoStack.Pop();

            bool anythingRedone = false;

            foreach (int characterIndex in operationIndexes)
            {
                if (characterIndex < 0 ||
                    characterIndex >= FontInfo.Character.Length)
                {
                    continue;
                }

                CCharInfo character = FontInfo.Character[characterIndex];

                if (!character.RedoPossible)
                    continue;

                character.Redo();
                anythingRedone = true;

                int localIndex = characterIndex - PageStart;

                if (localIndex >= 0 &&
                    localIndex < CharacterPictureList.Count)
                {
                    PictureBox pict = CharacterPictureList[localIndex];

                    RefreshCharacterPreview(pict, character);

                    pict.ContextMenu.MenuItems[0].Enabled =
                        _operationUndoStack.Count > 0;

                    pict.ContextMenu.MenuItems[1].Enabled =
                        _operationRedoStack.Count > 0;
                }
            }

            if (anythingRedone)
            {
                // The grouped operation is once again part of Undo history.
                _multiUndoStack.Push(operationIndexes);

                // Refresh the active glyph without changing the selection.
                if (Index >= 0 && Index < FontInfo.Character.Length)
                    RefreshDrawingAreaDisplay(FontInfo.Character[Index]);

                FlowCharacterPanel.Invalidate();
                CheckChange();
            }

            return anythingRedone;
        }


        void MenuUndoClicked(object sender, EventArgs e)
        {
            if (_operationUndoStack.Count == 0)
                return;

            UndoOperation operation = _operationUndoStack.Peek();

            if (operation.Type == UndoOperationType.BaseShadeChange)
            {
                CWFNFontInfo font = FontInfo as CWFNFontInfo;

                if (font == null)
                    return;

                font.BaseShadeIndex = operation.PreviousBaseShadeIndex;

                _operationUndoStack.Pop();
                _operationRedoStack.Push(operation);

                grayscalePalette.Invalidate();
                shadeIndicator.Invalidate();
                MarkFontModified();
                return;
            }

            if (operation.Type == UndoOperationType.FontBitDepthConversion)
            {
                if (!UndoFontBitDepthConversion())
                    return;

                _operationUndoStack.Pop();
                _operationRedoStack.Push(operation);
                return;
            }

            if (operation.Type == UndoOperationType.GlyphEdit)
            {
                CCharInfo character = operation.Character;

                if (character == null || !character.UndoPossible)
                    return;

                character.Undo();

                _operationUndoStack.Pop();
                _operationRedoStack.Push(operation);

                int targetIndex = character.Index;
                int targetPage = (targetIndex / PageSize) * PageSize;

                if (targetPage != PageStart)
                {
                    PageStart = targetPage;
                    Index = targetIndex;
                    RebuildPage();
                }
                else
                {
                    int localIndex = targetIndex - PageStart;

                    if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                    {
                        PictureBox picture = CharacterPictureList[localIndex];
                        RefreshCharacterPreview(picture, character);

                        picture.ContextMenu.MenuItems[0].Enabled = _operationUndoStack.Count > 0;
                        picture.ContextMenu.MenuItems[1].Enabled = _operationRedoStack.Count > 0;
                    }

                    Index = targetIndex;

                    if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                    {
                        PictureBox targetPicture = CharacterPictureList[localIndex];
                        SelectOnlyPreview(targetPicture);
                        DisplayCurrentCharacter(character);
                    }
                }

                // Restore the single glyph associated with this Undo operation.
                int selectedLocalIndex = targetIndex - PageStart;

                if (selectedLocalIndex >= 0 &&
                    selectedLocalIndex < CharacterPictureList.Count)
                {
                    PictureBox targetPicture =
                        CharacterPictureList[selectedLocalIndex];

                    SelectOnlyPreview(targetPicture);
                    RefreshDrawingAreaDisplay(character);
                    targetPicture.Focus();
                }

                FlowCharacterPanel.Invalidate();

                UpdateGlyphTextbox();
                CheckChange();
                return;
            }

            if (operation.Type == UndoOperationType.StructuralEdit)
            {
                if (UndoLastStructuralEdit())
                {
                    _operationUndoStack.Pop();
                    _operationRedoStack.Push(operation);
                }

                return;
            }

            if (operation.Type == UndoOperationType.MultiGlyphEdit)
            {
                if (UndoLastMultiOperation())
                {
                    _operationUndoStack.Pop();
                    _operationRedoStack.Push(operation);

                    // Restore the original selection after undoing Remove glyph data.
                    if (operation.SelectionBeforeEdit != null &&
                        operation.SelectionBeforeEdit.Count > 0)
                    {
                        int activeIndex = operation.ActiveIndexBeforeEdit;
                        int targetPage = (activeIndex / PageSize) * PageSize;

                        Index = activeIndex;

                        if (targetPage != PageStart)
                        {
                            PageStart = targetPage;
                            RebuildPage();
                        }

                        ClearPreviewSelection();

                        foreach (int selectedIndex in operation.SelectionBeforeEdit)
                        {
                            int localIndex = selectedIndex - PageStart;

                            if (localIndex >= 0 &&
                                localIndex < CharacterPictureList.Count)
                            {
                                AddPreviewToSelection(CharacterPictureList[localIndex]);
                            }
                        }

                        int activeLocalIndex = activeIndex - PageStart;

                        if (activeLocalIndex >= 0 &&
                            activeLocalIndex < CharacterPictureList.Count)
                        {
                            _selectedPreview = CharacterPictureList[activeLocalIndex];
                            _selectionAnchor = _selectedPreview;

                            RefreshDrawingAreaDisplay(FontInfo.Character[activeIndex]);
                        }

                        FlowCharacterPanel.PerformLayout();
                        FlowCharacterPanel.Invalidate();
                    }


                    // Undo Paste: restore the original destination selection.
                    if (operation.PasteStartIndex >= 0 &&
                        operation.SelectionBeforeEdit != null &&
                        operation.SelectionBeforeEdit.Count > 0)
                    {
                        int activeIndex = operation.ActiveIndexBeforeEdit;
                        int targetPage = (activeIndex / PageSize) * PageSize;

                        Index = activeIndex;

                        if (targetPage != PageStart)
                        {
                            PageStart = targetPage;
                            RebuildPage();
                        }

                        ClearPreviewSelection();

                        foreach (int selectedIndex in operation.SelectionBeforeEdit)
                        {
                            int localIndex = selectedIndex - PageStart;

                            if (localIndex >= 0 &&
                                localIndex < CharacterPictureList.Count)
                            {
                                AddPreviewToSelection(CharacterPictureList[localIndex]);
                            }
                        }

                        int activeLocalIndex = activeIndex - PageStart;

                        if (activeLocalIndex >= 0 &&
                            activeLocalIndex < CharacterPictureList.Count)
                        {
                            _selectedPreview = CharacterPictureList[activeLocalIndex];
                            _selectionAnchor = _selectedPreview;

                            RefreshDrawingAreaDisplay(FontInfo.Character[activeIndex]);
                        }

                        FlowCharacterPanel.PerformLayout();
                        FlowCharacterPanel.Invalidate();
                    }
                }

                return;
            }

            MenuItem menu = sender as MenuItem;

            if (menu != null)
            {
                PictureBox picture = (PictureBox)menu.Parent.Tag;
                CCharInfo character = (CCharInfo)(picture.Tag);

                character.Undo();
                menu.Enabled = character.UndoPossible;

                RefreshCharacterPreview(picture, character);

                CharacterPictureList[character.Index - PageStart]
                    .ContextMenu.MenuItems[0].Enabled = character.UndoPossible;

                CharacterPictureList[character.Index - PageStart]
                    .ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

                if (character.Index == Index)
                    DisplayCurrentCharacter(character);

                CheckChange();
            }
        }
        void MenuRedoClicked(object sender, EventArgs e)
        {
            if (_operationRedoStack.Count == 0)
                return;

            UndoOperation operation = _operationRedoStack.Peek();

            if (operation.Type == UndoOperationType.BaseShadeChange)
            {
                CWFNFontInfo font = FontInfo as CWFNFontInfo;

                if (font == null)
                    return;

                font.BaseShadeIndex = operation.NewBaseShadeIndex;

                _operationRedoStack.Pop();
                _operationUndoStack.Push(operation);

                grayscalePalette.Invalidate();
                shadeIndicator.Invalidate();
                MarkFontModified();
                return;
            }

            if (operation.Type == UndoOperationType.FontBitDepthConversion)
            {
                if (!RedoFontBitDepthConversion())
                    return;

                _operationRedoStack.Pop();
                _operationUndoStack.Push(operation);

                return;
            }

            if (operation.Type == UndoOperationType.GlyphEdit)
            {
                CCharInfo character = operation.Character;

                if (character == null || !character.RedoPossible)
                    return;

                character.Redo();

                _operationRedoStack.Pop();
                _operationUndoStack.Push(operation);

                int targetIndex = character.Index;
                int targetPage = (targetIndex / PageSize) * PageSize;

                if (targetPage != PageStart)
                {
                    PageStart = targetPage;
                    Index = targetIndex;
                    RebuildPage();
                }
                else
                {
                    int localIndex = targetIndex - PageStart;

                    if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                    {
                        PictureBox picture = CharacterPictureList[localIndex];
                        RefreshCharacterPreview(picture, character);

                        picture.ContextMenu.MenuItems[0].Enabled = _operationUndoStack.Count > 0;
                        picture.ContextMenu.MenuItems[1].Enabled = _operationRedoStack.Count > 0;
                    }

                    Index = targetIndex;

                    if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                    {
                        PictureBox targetPicture = CharacterPictureList[localIndex];
                        SelectOnlyPreview(targetPicture);
                        DisplayCurrentCharacter(character);
                    }
                }

                UpdateGlyphTextbox();
                CheckChange();
                return;
            }

            if (operation.Type == UndoOperationType.StructuralEdit)
            {
                if (RedoLastStructuralEdit())
                {
                    _operationRedoStack.Pop();
                    _operationUndoStack.Push(operation);
                }

                return;
            }

            if (operation.Type == UndoOperationType.MultiGlyphEdit)
            {
                if (RedoLastMultiOperation())
                {
                    _operationRedoStack.Pop();
                    _operationUndoStack.Push(operation);

                    // Restore the selection belonging to a normal grouped edit.
                    if (operation.SelectionAfterEdit != null &&
                        operation.SelectionAfterEdit.Count > 0)
                    {
                        int activeIndex = operation.ActiveIndexBeforeEdit;

                        if (activeIndex < 0 ||
                            activeIndex >= FontInfo.Character.Length)
                        {
                            activeIndex = operation.SelectionAfterEdit[0];
                        }

                        int targetPage = (activeIndex / PageSize) * PageSize;

                        Index = activeIndex;

                        if (targetPage != PageStart)
                        {
                            PageStart = targetPage;
                            RebuildPage();
                        }

                        ClearPreviewSelection();

                        foreach (int selectedIndex in operation.SelectionAfterEdit)
                        {
                            int localIndex = selectedIndex - PageStart;

                            if (localIndex >= 0 &&
                                localIndex < CharacterPictureList.Count)
                            {
                                AddPreviewToSelection(
                                    CharacterPictureList[localIndex]);
                            }
                        }

                        int activeLocalIndex = activeIndex - PageStart;

                        if (activeLocalIndex >= 0 &&
                            activeLocalIndex < CharacterPictureList.Count)
                        {
                            _selectedPreview = CharacterPictureList[activeLocalIndex];
                            _selectionAnchor = _selectedPreview;

                            RefreshDrawingAreaDisplay(
                                FontInfo.Character[activeIndex]);
                        }

                        FlowCharacterPanel.PerformLayout();
                        FlowCharacterPanel.Invalidate();
                    }

                    // Remove glyph data: collapse the selection to the
                    // first affected glyph after Redo.
                    if (operation.PasteStartIndex < 0 &&
                        operation.SelectionBeforeEdit != null &&
                        operation.SelectionBeforeEdit.Count > 0)
                    {
                        int firstIndex = operation.SelectionBeforeEdit.Min();
                        int targetPage = (firstIndex / PageSize) * PageSize;

                        Index = firstIndex;

                        if (targetPage != PageStart)
                        {
                            PageStart = targetPage;
                            RebuildPage();
                        }

                        int localIndex = firstIndex - PageStart;

                        if (localIndex >= 0 &&
                            localIndex < CharacterPictureList.Count)
                        {
                            PictureBox firstPreview =
                                CharacterPictureList[localIndex];

                            SelectOnlyPreview(firstPreview);
                            RefreshDrawingAreaDisplay(
                                FontInfo.Character[firstIndex]);
                        }

                        FlowCharacterPanel.PerformLayout();
                        FlowCharacterPanel.Invalidate();
                    }

                    // Paste Redo: select all glyphs that were pasted.
                    if (operation.PasteStartIndex >= 0 &&
                        operation.PastedIndexes != null &&
                        operation.PastedIndexes.Count > 0)
                    {
                        int firstIndex = operation.PastedIndexes.Min();
                        int targetPage = (firstIndex / PageSize) * PageSize;

                        Index = firstIndex;

                        if (targetPage != PageStart)
                        {
                            PageStart = targetPage;
                            RebuildPage();
                        }

                        ClearPreviewSelection();

                        foreach (int pastedIndex in operation.PastedIndexes)
                        {
                            int localIndex = pastedIndex - PageStart;

                            if (localIndex >= 0 &&
                                localIndex < CharacterPictureList.Count)
                            {
                                AddPreviewToSelection(CharacterPictureList[localIndex]);
                            }
                        }

                        int activeLocalIndex = firstIndex - PageStart;

                        if (activeLocalIndex >= 0 &&
                            activeLocalIndex < CharacterPictureList.Count)
                        {
                            _selectedPreview = CharacterPictureList[activeLocalIndex];
                            _selectionAnchor = _selectedPreview;

                            RefreshDrawingAreaDisplay(FontInfo.Character[firstIndex]);
                        }

                        FlowCharacterPanel.PerformLayout();
                        FlowCharacterPanel.Invalidate();
                    }
                }

                return;
            }

            MenuItem menu = sender as MenuItem;

            if (menu != null)
            {
                PictureBox picture = (PictureBox)menu.Parent.Tag;
                CCharInfo character = (CCharInfo)(picture.Tag);

                character.Redo();
                menu.Enabled = character.RedoPossible;

                RefreshCharacterPreview(picture, character);

                CharacterPictureList[character.Index - PageStart]
                    .ContextMenu.MenuItems[0].Enabled = character.UndoPossible;

                CharacterPictureList[character.Index - PageStart]
                    .ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

                if (character.Index == Index)
                    DisplayCurrentCharacter(character);

                CheckChange();
            }

        }
        void MenuCopyClicked(object sender, EventArgs e)
        {
            MenuItem menu = (MenuItem)sender;
            if (menu == null)
                return;

            PictureBox clickedPicture =
                (PictureBox)menu.Parent.Tag;

            if (clickedPicture == null)
                return;

            _copiedGlyphs.Clear();

            List<PictureBox> sources;

            // If this glyph belongs to a multiple selection,
            // copy the entire selection.
            if (_selectedPreviews.Count > 1 &&
                IsPreviewSelected(clickedPicture))
            {
                sources = new List<PictureBox>(_selectedPreviews);
            }
            else
            {
                sources = new List<PictureBox>();
                sources.Add(clickedPicture);
            }

            // Always store them in index order, regardless of
            // the order in which Ctrl-clicks were made.
            sources.Sort(delegate (PictureBox a, PictureBox b)
            {
                CCharInfo ca = (CCharInfo)a.Tag;
                CCharInfo cb = (CCharInfo)b.Tag;

                return ca.Index.CompareTo(cb.Index);
            });

            foreach (PictureBox picture in sources)
            {
                CCharInfo character = picture.Tag as CCharInfo;
                if (character == null)
                    continue;

                CopiedGlyph copied = new CopiedGlyph();

                copied.SourceIndex = character.Index;
                copied.Width = character.Width;
                copied.Height = character.Height;
                copied.BitDepth =
                    (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit) ? 4 : 1;

                if (character.ByteLines != null)
                    copied.ByteLines =
                        (byte[])character.ByteLines.Clone();
                else
                    copied.ByteLines = null;

                _copiedGlyphs.Add(copied);
            }

            try
            {
                DataObject clipboardData = new DataObject();

                // Store our complete WFN glyph data in the Windows clipboard.
                byte[] serializedGlyphs =
                    SerializeCopiedGlyphs(_copiedGlyphs);

                clipboardData.SetData(
                    GlyphClipboardFormat,
                    serializedGlyphs);

                Bitmap clipboardBitmap =
                    CreateCombinedGlyphBitmap(sources);

                if (clipboardBitmap != null)
                {
                    clipboardData.SetData(
                        DataFormats.Bitmap,
                        clipboardBitmap);

                    clipboardData.SetData(
                        DataFormats.Dib,
                        clipboardBitmap);
                }

                Clipboard.SetDataObject(clipboardData, true);
            }
            catch
            {
                // If Windows clipboard access fails, the internal
                // copy buffer still remains available.
            }

            CheckChange();
        }
        private void MenuSelectAllClicked(object sender, EventArgs e)
        {
            SelectAllVisibleGlyphs();
        }

        void MenuPasteClicked(object sender, EventArgs e)
        {
            // Use internal glyph data only if it is still present
            // in the current Windows clipboard.
            if (!TryLoadCopiedGlyphsFromClipboard())
            {
                _copiedGlyphs.Clear();
            }

            MenuItem menu = (MenuItem)sender;
            if (menu == null)
                return;

            PictureBox picture = (PictureBox)menu.Parent.Tag;
            if (picture == null)
                return;

            CCharInfo characterinfo = (CCharInfo)(picture.Tag);
            if (characterinfo == null)
                return;

            // If we have a multi-glyph internal copy buffer,
            // paste the whole copied selection instead of a single bitmap.
            if (_copiedGlyphs.Count > 1)
            {
                int destinationBitDepth =
                    (FontInfo is CWFNFontInfo destinationWfn && destinationWfn.Is4Bit)
                        ? 4 : 1;

                int destinationStart = _selectedPreviews.Count > 1
                    ? _selectedPreviews
                        .Select(p => ((CCharInfo)p.Tag).Index)
                        .Min()
                    : characterinfo.Index;

                // The first copied glyph is the reference point.
                int sourceStart = _copiedGlyphs[0].SourceIndex;

                // Work out the furthest destination index.
                int lastDestinationIndex = destinationStart;

                foreach (CopiedGlyph copied in _copiedGlyphs)
                {
                    int relativeOffset =
                        copied.SourceIndex - sourceStart;

                    int destinationIndex =
                        destinationStart + relativeOffset;

                    if (destinationIndex > lastDestinationIndex)
                        lastDestinationIndex = destinationIndex;
                }

                // Do not allow this paste to cross the current page's
                // maximum of 256 index slots.
                int pageEnd = PageStart + PageSize - 1;

                if (lastDestinationIndex > pageEnd)
                {
                    MessageBox.Show(
                        "No room to paste the selection from this glyph forward!\n\nA page can contain up to 256 index slots in total when the last slot is occupied.",
                        "Max page index reached",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    // Nothing has been changed and the copied glyphs remain
                    // available in the internal clipboard.
                    return;
                }

                // If the destination extends beyond the font's current range,
                // grow the font and fill all newly created indexes with placeholders.
                if (lastDestinationIndex >= FontInfo.Character.Length)
                {
                    int oldLength = FontInfo.Character.Length;
                    int newLength = lastDestinationIndex + 1;

                    Array.Resize(ref FontInfo.Character, newLength);

                    for (int i = oldLength; i < newLength; i++)
                    {
                        CCharInfo emptyCharacter = new CCharInfo();
                        emptyCharacter.Index = i;
                        MakeGlyphPlaceholder(emptyCharacter);

                        FontInfo.Character[i] = emptyCharacter;
                    }

                    FontInfo.NumberOfCharacters = FontInfo.Character.Length;
                    TxtGlyphRange.Text = (FontInfo.Character.Length - 1).ToString();

                    // Add only the newly required preview controls.
                    RefreshPageFromIndex(oldLength);
                }

                // Remember the destination selection before pasting.
                List<int> selectionBeforePaste = _selectedPreviews
                    .Select(p => ((CCharInfo)p.Tag).Index)
                    .OrderBy(i => i)
                    .ToList();

                int activeIndexBeforePaste = Index;

                List<int> changedIndexes = new List<int>();

                foreach (CopiedGlyph copied in _copiedGlyphs)
                {
                    int relativeOffset =
                        copied.SourceIndex - sourceStart;

                    int destinationIndex =
                        destinationStart + relativeOffset;

                    CCharInfo target =
                        FontInfo.Character[destinationIndex];

                    // Save the old destination state for Undo.
                    target.UndoRedoListTidyUp();
                    target.UndoRedoListAdd(target.ByteLines);

                    // Apply the copied glyph.
                    target.Width = copied.Width;
                    target.Height = copied.Height;

                    target.ByteLines = ConvertCopiedGlyphBitDepth(
                        copied, destinationBitDepth);

                    target.BitDepth = destinationBitDepth;

                    target.UnscaledImage = null;

                    // Save the new state for Redo, even if the glyph is unchanged.
                    target.UndoRedoListAdd(target.ByteLines, true);

                    changedIndexes.Add(destinationIndex);

                    int targetLocalIndex = destinationIndex - PageStart;

                    if (targetLocalIndex >= 0 &&
                        targetLocalIndex < CharacterPictureList.Count)
                    {
                        PictureBox targetPicture =
                            CharacterPictureList[targetLocalIndex];

                        RefreshCharacterPreview(
                            targetPicture,
                            target);

                        targetPicture.ContextMenu.MenuItems[0].Enabled =
                            target.UndoPossible;

                        targetPicture.ContextMenu.MenuItems[1].Enabled =
                            target.RedoPossible;
                    }
                }

                // Treat the whole paste as one Undo/Redo operation.
                if (changedIndexes.Count > 0)
                {
                    _multiUndoStack.Push(changedIndexes);

                    _operationUndoStack.Push(new UndoOperation
                    {
                        Type = UndoOperationType.MultiGlyphEdit,
                        PasteStartIndex = destinationStart,
                        SelectionBeforeEdit = selectionBeforePaste,
                        ActiveIndexBeforeEdit = activeIndexBeforePaste,
                        PastedIndexes = new List<int>(changedIndexes)
                    });

                    _multiRedoStack.Clear();
                    _structuralRedoStack.Clear();
                    _operationRedoStack.Clear();
                }

                // Select all destination glyphs after pasting.
                Index = destinationStart;

                ClearPreviewSelection();

                foreach (int destinationIndex in changedIndexes)
                {
                    int pastedLocalIndex = destinationIndex - PageStart;

                    if (pastedLocalIndex >= 0 &&
                        pastedLocalIndex < CharacterPictureList.Count)
                    {
                        AddPreviewToSelection(CharacterPictureList[pastedLocalIndex]);
                    }
                }

                int activeLocalIndex = destinationStart - PageStart;

                if (activeLocalIndex >= 0 &&
                    activeLocalIndex < CharacterPictureList.Count)
                {
                    PictureBox activePreview = CharacterPictureList[activeLocalIndex];

                    _selectedPreview = activePreview;
                    _selectionAnchor = activePreview;

                    RefreshDrawingAreaDisplay(FontInfo.Character[destinationStart]);
                    activePreview.Focus();
                }

                FlowCharacterPanel.Invalidate();

                UpdateGlyphTextbox();
                CheckChange();

                return;
            }

            // Make sure the current glyph state exists in Undo history
            // before Paste replaces it.
            characterinfo.UndoRedoListTidyUp();
            characterinfo.UndoRedoListAdd(characterinfo.ByteLines);

            // Grab clipboard image safely
            Image clipImg = Clipboard.GetImage();
            if (clipImg == null)
                return;

            // Reject clipboard images larger than 64x64 pixels.
            if (clipImg.Width > 64 || clipImg.Height > 64)
            {
                MessageBox.Show(
                    "The bitmap exceeds the maximum 64x64 pixels glyph size.\n\nPlease resize the bitmap before pasting it.",
                    "Bitmap Too Large",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Bitmap clipBmp = clipImg as Bitmap;
            if (clipBmp == null)
                clipBmp = new Bitmap(clipImg); // convert Image -> Bitmap

            // Preserve old tag if existed (placeholders may have null UnscaledImage)
            object oldTag = null;
            if (characterinfo.UnscaledImage != null)
                oldTag = characterinfo.UnscaledImage.Tag;

            // Convert external bitmap according to the destination font's bit depth.
            if (FontInfo is CWFNFontInfo grayscaleWfn && grayscaleWfn.Is4Bit)
            {
                characterinfo.Width = (UInt16)clipBmp.Width;
                characterinfo.Height = (UInt16)clipBmp.Height;
                characterinfo.ByteLines = ConvertBitmapTo4BitBytes(clipBmp);

                // When pasting a copied 1-bit glyph into a 4-bit font,
                // use shade 8 for filled pixels instead of shade 15.
                if (_copiedGlyphs.Count == 1 &&
                    _copiedGlyphs[0].BitDepth == 1)
                {
                    characterinfo.ByteLines = ConvertCopiedGlyphBitDepth(
                        _copiedGlyphs[0], 4);
                }

                CFontUtils.CreateBitmap(characterinfo, out Bitmap grayscaleBitmap);
                characterinfo.UnscaledImage = grayscaleBitmap;
            }
            else
            {
                // Preserve the existing 1-bit bitmap conversion.
                Bitmap untested = Indexed.Image.CopyToBpp(clipBmp, 1);

                // Fix upside-down / stride issues if needed
                Bitmap corrected = null;
            if (CorrectImage(untested, out corrected))
                characterinfo.UnscaledImage = corrected;
            else
                characterinfo.UnscaledImage = untested;

            // Restore tag if we had one
            if (oldTag != null)
                characterinfo.UnscaledImage.Tag = oldTag;

            // Update glyph metrics based on pasted bitmap
            characterinfo.Width = (UInt16)characterinfo.UnscaledImage.Width;
            characterinfo.Height = (UInt16)characterinfo.UnscaledImage.Height;

            // Save ByteLines from the pasted image (allocates ByteLines)
            CFontUtils.SaveByteLinesFromPicture(characterinfo, (Bitmap)characterinfo.UnscaledImage);
            } // End of existing 1-bit conversion path.

            // Update selection/UI
            Index = characterinfo.Index;
            UpdateGlyphTextbox();

            // Update drawing area
            Bitmap scaledbitmap;
            CFontUtils.ScaleBitmap((Bitmap)characterinfo.UnscaledImage, out scaledbitmap, ZoomDrawingArea.Value);
            if (scaledbitmap != null)
            {
                DrawingArea.Size = scaledbitmap.Size;
                DrawingArea.Image = scaledbitmap;
            }

            ClickedOnCharacter = true;
            numWidth.Value = characterinfo.Width;
            numHeight.Value = characterinfo.Height;
            ClickedOnCharacter = false;

            // Update preview cell
            int localIndex = characterinfo.Index - PageStart;
            if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
            {
                Bitmap outbmp;
                CFontUtils.ScaleBitmap((Bitmap)characterinfo.UnscaledImage, out outbmp, Scalefactor);
                CharacterPictureList[localIndex].Image = outbmp;
                CharacterPictureList[localIndex].Size = outbmp.Size;
                CharacterPictureList[localIndex].Invalidate();
            }

            // Undo/redo state
            characterinfo.UndoRedoListTidyUp();
            characterinfo.UndoRedoListAdd(characterinfo.ByteLines, true);

            RegisterGlyphUndoOperation(characterinfo);

            if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
            {
                CharacterPictureList[localIndex].ContextMenu.MenuItems[0].Enabled = characterinfo.UndoPossible;
                CharacterPictureList[localIndex].ContextMenu.MenuItems[1].Enabled = characterinfo.RedoPossible;
            }

            CheckChange();
        }


        public bool CopySelectedGlyph()
        {
            if (_selectedPreview == null || _selectedPreview.ContextMenu == null)
                return false;

            _selectedPreview.ContextMenu.MenuItems[2].PerformClick();
            return true;
        }

        public bool PasteSelectedGlyph()
        {
            if (_selectedPreview == null || _selectedPreview.ContextMenu == null)
                return false;

            _selectedPreview.ContextMenu.MenuItems[3].PerformClick();
            return true;
        }

        public bool RemoveSelectedGlyphData()
        {
            if (_selectedPreview == null ||
                _selectedPreview.ContextMenu == null)
            {
                return false;
            }

            _selectedPreview.ContextMenu.MenuItems[5].PerformClick();
            return true;
        }

        public bool InsertGlyphAtSelected()
        {
            if (_selectedPreview == null ||
                _selectedPreview.ContextMenu == null)
            {
                return false;
            }

            _selectedPreview.ContextMenu.MenuItems[7].PerformClick();
            return true;
        }

        public bool DeleteSelectedGlyph()
        {
            if (_selectedPreview == null ||
                _selectedPreview.ContextMenu == null)
            {
                return false;
            }

            _selectedPreview.ContextMenu.MenuItems[9].PerformClick();
            return true;
        }

        public bool UndoSelectedGlyph()
        {
            if (_operationUndoStack.Count == 0)
                return false;

            MenuUndoClicked(null, EventArgs.Empty);
            return true;
        }

        public bool RedoSelectedGlyph()
        {
            if (_operationRedoStack.Count == 0)
                return false;

            MenuRedoClicked(null, EventArgs.Empty);
            return true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Oemcomma)
            {
                NavigateGlyphShortcut(false);
                return true;
            }

            if (keyData == Keys.OemPeriod)
            {
                NavigateGlyphShortcut(true);
                return true;
            }

            if (grayscalePalette != null && grayscalePalette.Visible &&
                (keyData & Keys.Modifiers) == Keys.None)
            {
                Keys key = keyData & Keys.KeyCode;

                if (key == Keys.PageUp)
                {
                    IncreaseSelectedShades();
                    return true;
                }

                if (key == Keys.PageDown)
                {
                    DecreaseSelectedShades();
                    return true;
                }
            }

            if (HandleGrayscaleNumberKey(keyData))
                return true;

            // Navigate glyphs only while the glyph preview has keyboard focus.
            if (FlowCharacterPanel.ContainsFocus)
            {
                // Open the active glyph's context menu with the keyboard Menu key.
                if ((keyData & Keys.KeyCode) == Keys.Apps &&
                    (keyData & Keys.Modifiers) == Keys.None)
                {
                    ShowActiveGlyphContextMenu();
                    return true;
                }

                Keys direction = keyData & Keys.KeyCode;
                Keys modifiers = keyData & Keys.Modifiers;

                if ((modifiers == Keys.None || modifiers == Keys.Shift) &&
                    (direction == Keys.Left || direction == Keys.Right ||
                     direction == Keys.Up || direction == Keys.Down ||
                     direction == Keys.Home || direction == Keys.End))
                {
                    NavigateGlyphSelection(direction, modifiers == Keys.Shift);
                    return true;
                }
            }

            if (keyData == (Keys.Control | Keys.A))
            {
                // Allow Ctrl+A to select text in editable fields.
                if (TxtGlyphRange.ContainsFocus ||
                    TxtPageNumber.ContainsFocus ||
                    TxtCharacter.ContainsFocus ||
                    TxtGlyph.ContainsFocus ||
                    numWidth.ContainsFocus ||
                    numHeight.ContainsFocus)
                {
                    return base.ProcessCmdKey(ref msg, keyData);
                }

                SelectAllVisibleGlyphs();
                return true;
            }

            if (keyData == (Keys.Control | Keys.C) ||
                keyData == (Keys.Control | Keys.V))
            {
                NumericUpDown numeric = null;

                if (numWidth.ContainsFocus)
                    numeric = numWidth;
                else if (numHeight.ContainsFocus)
                    numeric = numHeight;

                if (numeric != null)
                {
                    if (keyData == (Keys.Control | Keys.C))
                    {
                        Clipboard.SetText(numeric.Text);
                    }
                    else if (Clipboard.ContainsText())
                    {
                        string text = Clipboard.GetText();
                        decimal value;

                        if (decimal.TryParse(text, out value))
                        {
                            TextBox editBox = numeric.Controls
                                .OfType<TextBox>()
                                .FirstOrDefault();

                            if (editBox != null)
                                editBox.Paste();
                        }
                    }

                    return true;
                }
            }

            if ((keyData == (Keys.Control | Keys.C) ||
                 keyData == (Keys.Control | Keys.V)) &&
                (TxtGlyphRange.Focused ||
                 TxtPageNumber.Focused ||
                 TxtCharacter.Focused ||
                 TxtGlyph.Focused))
            {
                TextBox textBox = this.ActiveControl as TextBox;

                if (textBox != null)
                {
                    if (keyData == (Keys.Control | Keys.C))
                        textBox.Copy();
                    else
                        textBox.Paste();

                    return true;
                }
            }

            if (keyData == (Keys.Control | Keys.Z))
            {
                if (UndoSelectedGlyph())
                    return true;
            }

            if (keyData == (Keys.Control | Keys.Y))
            {
                if (RedoSelectedGlyph())
                    return true;
            }

            if (keyData == (Keys.Control | Keys.C))
            {
                if (CopySelectedGlyph())
                    return true;
            }

            if (keyData == (Keys.Control | Keys.V))
            {
                if (PasteSelectedGlyph())
                    return true;
            }

            if (keyData == Keys.Delete)
            {
                if (RemoveSelectedGlyphData())
                    return true;
            }

            if (keyData == Keys.Insert)
            {
                if (InsertGlyphAtSelected())
                    return true;
            }

            if (keyData == (Keys.Shift | Keys.Insert))
            {
                if (_selectedPreview != null &&
                    _selectedPreview.ContextMenu != null)
                {
                    _selectedPreview.ContextMenu.MenuItems[8].PerformClick();
                    return true;
                }
            }

            if (keyData == (Keys.Shift | Keys.Delete))
            {
                if (DeleteSelectedGlyph())
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MenuRemoveGlyphDataClicked(object sender, EventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            if (menu == null)
                return;

            PictureBox clickedPicture = menu.Parent.Tag as PictureBox;
            if (clickedPicture == null)
                return;

            // Keep track of the original active glyph shown on the canvas.
            PictureBox activePreview = _selectedPreview;

            // Decide which glyphs this operation should affect.
            List<PictureBox> targets;

            if (_selectedPreviews.Count > 1 &&
                IsPreviewSelected(clickedPicture))
            {
                targets = new List<PictureBox>(_selectedPreviews);
            }
            else
            {
                targets = new List<PictureBox>();
                targets.Add(clickedPicture);
            }

            // Remember which glyphs actually changed, so Undo/Redo
            // can treat a multi-glyph remove as one grouped operation.
            List<int> changedIndexes = new List<int>();

            // Remember the selection and active glyph before removal.
            List<int> selectionBeforeEdit = targets
                .Select(p => ((CCharInfo)p.Tag).Index)
                .OrderBy(i => i)
                .ToList();

            int activeIndexBeforeEdit = Index;

            foreach (PictureBox pict in targets)
            {
                CCharInfo character = pict.Tag as CCharInfo;
                if (character == null)
                    continue;

                bool hadData =
                    character.Width != 0 ||
                    character.Height != 0 ||
                    character.ByteLines != null;

                // Preserve the original glyph state before removing its data.
                if (hadData)
                {
                    character.UndoRedoListTidyUp();
                    character.UndoRedoListAdd(character.ByteLines);
                }

                // Turn the glyph into a true empty placeholder.
                character.Width = 0;
                character.Height = 0;
                character.ByteLines = null;
                character.UnscaledImage = null;

                // Record this new empty state in the glyph's own
                // Undo/Redo history.
                character.UndoRedoListTidyUp();
                character.UndoRedoListAdd(character.ByteLines);

                if (hadData)
                    changedIndexes.Add(character.Index);

                // Refresh only this preview.
                RefreshCharacterPreview(pict, character);

                // Update this glyph's own Undo/Redo menu state.
                if (pict.ContextMenu != null)
                {
                    pict.ContextMenu.MenuItems[0].Enabled =
                        character.UndoPossible;

                    pict.ContextMenu.MenuItems[1].Enabled =
                        character.RedoPossible;
                }
            }

            // If more than one glyph was affected, remember the whole
            // operation as one grouped Undo action.
            if (targets.Count > 1 && changedIndexes.Count > 0)
            {
                _multiUndoStack.Push(changedIndexes);

                _operationUndoStack.Push(new UndoOperation
                {
                    Type = UndoOperationType.MultiGlyphEdit,
                    SelectionBeforeEdit = selectionBeforeEdit,
                    ActiveIndexBeforeEdit = activeIndexBeforeEdit
                });

                _multiRedoStack.Clear();
                _structuralRedoStack.Clear();
                _operationRedoStack.Clear();
            }
            else if (targets.Count == 1 && changedIndexes.Count > 0)
            {
                CCharInfo character = targets[0].Tag as CCharInfo;

                RegisterGlyphUndoOperation(character);

                _multiUndoStack.Clear();
                _multiRedoStack.Clear();
            }

            // After removal, select only the first affected glyph.
            if (changedIndexes.Count > 0)
            {
                int firstIndex = changedIndexes.Min();
                int localIndex = firstIndex - PageStart;

                if (localIndex >= 0 &&
                    localIndex < CharacterPictureList.Count)
                {
                    PictureBox firstPreview = CharacterPictureList[localIndex];

                    SelectOnlyPreview(firstPreview);
                    RefreshDrawingAreaDisplay(FontInfo.Character[firstIndex]);
                }
            }

            // Repaint the selection outlines after the placeholder layout changes.
            FlowCharacterPanel.PerformLayout();
            FlowCharacterPanel.Invalidate();

            CheckChange();
        }


        private void MenuInsertGlyphClicked(object sender, EventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            if (menu == null)
                return;

            PictureBox pict = menu.Parent.Tag as PictureBox;
            if (pict == null)
                return;

            CCharInfo selectedCharacter = pict.Tag as CCharInfo;
            if (selectedCharacter == null)
                return;

            // Remember the original selection before inserting.
            List<int> selectionBeforeInsert = _selectedPreviews
                .Select(p => ((CCharInfo)p.Tag).Index)
                .ToList();

            int activeIndexBeforeInsert = Index;

            bool insertBefore = menu.Text == "Insert a new glyph before";

            int insertIndex = selectedCharacter.Index + (insertBefore ? 0 : 1);

            int oldLength = FontInfo.Character.Length;
            int pageEndExclusive = Math.Min(PageStart + PageSize, oldLength);
            int glyphsOnPage = pageEndExclusive - PageStart;

            // A full page cannot accept an insertion beyond its final slot.
            if (glyphsOnPage >= PageSize && insertIndex >= pageEndExclusive)
            {
                MessageBox.Show(
                    "Cannot add more index slots beyond the last slot!\n\nA page can contain up to 256 index slots in total when the last slot is occupied.\n\nTo access more index slots beyond this point, extend the font range and navigate to the next page.",
                    "Max page index reached",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            StructuralEdit edit = new StructuralEdit();
            edit.Type = StructuralEditType.Insert;
            edit.Index = insertIndex;
            edit.OldLength = oldLength;

            edit.SelectionBeforeEdit = selectionBeforeInsert;
            edit.ActiveIndexBeforeEdit = activeIndexBeforeInsert;

            // If this page already contains all 256 slots,
            // the final slot must be empty.
            if (glyphsOnPage >= PageSize)
            {
                int pageEnd = PageStart + PageSize - 1;
                CCharInfo lastCharacter = FontInfo.Character[pageEnd];

                if (lastCharacter != null &&
                    lastCharacter.Width != 0 &&
                    lastCharacter.Height != 0)
                {
                    MessageBox.Show(
                        "No more room to push glyphs forward!\n\nA page can contain up to 256 index slots in total when the last slot is occupied.\n\nTo insert another index, the last glyph on this page must be empty. Either delete or remove the last glyph data first.",
                        "Max page index reached",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                edit.InsertGrewArray = false;
                edit.PageEnd = pageEnd;
                edit.DisplacedCharacter = lastCharacter;

                // Shift everything one index forward within this page.
                for (int i = pageEnd; i > insertIndex; i--)
                {
                    FontInfo.Character[i] = FontInfo.Character[i - 1];
                    FontInfo.Character[i].Index = i;
                }
            }
            else
            {
                // Partial page: grow the font by one index.
                edit.InsertGrewArray = true;
                edit.PageEnd = oldLength;

                Array.Resize(ref FontInfo.Character, oldLength + 1);

                for (int i = oldLength; i > insertIndex; i--)
                {
                    FontInfo.Character[i] = FontInfo.Character[i - 1];
                    FontInfo.Character[i].Index = i;
                }
            }

            // All insertion checks passed.
            // Now select the clicked glyph before completing the insertion.
            SelectOnlyPreview(pict);

            // Create the new empty glyph.
            CCharInfo newCharacter = new CCharInfo();
            newCharacter.Index = insertIndex;
            newCharacter.BitDepth = (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit) ? 4 : 1;
            MakeGlyphPlaceholder(newCharacter);

            FontInfo.Character[insertIndex] = newCharacter;

            // Preserve the inserted glyph object for future Redo operations.
            edit.InsertedCharacter = newCharacter;

            // Remember this structural operation for Undo.
            _structuralUndoStack.Push(edit);

            // Register the operation in chronological Undo history.
            _operationUndoStack.Push(new UndoOperation
            {
                Type = UndoOperationType.StructuralEdit
            });

            // A new operation invalidates Redo history.
            _structuralRedoStack.Clear();
            _multiRedoStack.Clear();
            _operationRedoStack.Clear();

            FontInfo.NumberOfCharacters = FontInfo.Character.Length;
            TxtGlyphRange.Text = (FontInfo.Character.Length - 1).ToString();

            Index = insertIndex;

            RefreshPageFromIndex(insertIndex);
            UpdateGlyphTextbox();

            CheckChange();
            MarkFontModified();
        }


        private void MenuDeleteGlyphClicked(object sender, EventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            if (menu == null)
                return;

            PictureBox clickedPicture = menu.Parent.Tag as PictureBox;
            if (clickedPicture == null)
                return;

            CCharInfo clickedCharacter = clickedPicture.Tag as CCharInfo;
            if (clickedCharacter == null)
                return;

            int pageEnd =
                Math.Min(PageStart + PageSize, FontInfo.Character.Length) - 1;

            if (pageEnd < PageStart)
                return;

            // MULTIPLE SELECTION
            if (_selectedPreviews.Count > 1 &&
                IsPreviewSelected(clickedPicture))
            {
                List<int> deleteIndexes = new List<int>();

                foreach (PictureBox pict in _selectedPreviews)
                {
                    CCharInfo character = pict.Tag as CCharInfo;

                    if (character != null)
                        deleteIndexes.Add(character.Index);
                }

                deleteIndexes.Sort();

                if (deleteIndexes.Count == 0)
                    return;

                StructuralEdit edit = new StructuralEdit();

                edit.Type = StructuralEditType.MultiDelete;
                edit.Index = deleteIndexes[0];
                edit.PageEnd = pageEnd;
                edit.OldLength = FontInfo.Character.Length;

                edit.DeletedIndexes =
                    new List<int>(deleteIndexes);

                edit.ActiveIndexBeforeEdit = Index;

                edit.DeletedCharacters =
                    new List<CCharInfo>();

                edit.DeletePlaceholders =
                    new List<CCharInfo>();

                // Store the actual glyph objects in the same ascending order.
                foreach (int deleteIndex in deleteIndexes)
                {
                    edit.DeletedCharacters.Add(
                        FontInfo.Character[deleteIndex]);
                }

                // Delete from highest index to lowest index.
                // This prevents earlier deletions from changing the positions
                // of indexes we still need to delete.
                for (int d = deleteIndexes.Count - 1; d >= 0; d--)
                {
                    int deleteIndex = deleteIndexes[d];

                    for (int i = deleteIndex; i < pageEnd; i++)
                    {
                        FontInfo.Character[i] =
                            FontInfo.Character[i + 1];

                        FontInfo.Character[i].Index = i;
                    }

                    CCharInfo emptyCharacter = new CCharInfo();
                    emptyCharacter.Index = pageEnd;
                    emptyCharacter.BitDepth = (FontInfo is CWFNFontInfo && ((CWFNFontInfo)FontInfo).Is4Bit) ? 4 : 1;
                    MakeGlyphPlaceholder(emptyCharacter);

                    FontInfo.Character[pageEnd] = emptyCharacter;
                    edit.DeletePlaceholders.Add(emptyCharacter);
                }

                _structuralUndoStack.Push(edit);

                _operationUndoStack.Push(new UndoOperation
                {
                    Type = UndoOperationType.StructuralEdit
                });

                _structuralRedoStack.Clear();
                _multiRedoStack.Clear();
                _operationRedoStack.Clear();

                Index = deleteIndexes[0];

                // The old multi-selection no longer refers to the same glyphs.
                ClearPreviewSelection();

                RefreshPageFromIndex(Index);

                int localIndex = Index - PageStart;

                if (localIndex >= 0 &&
                    localIndex < CharacterPictureList.Count)
                {
                    PictureBox newActive =
                        CharacterPictureList[localIndex];

                    SelectOnlyPreview(newActive);

                    CCharInfo activeCharacter =
                        newActive.Tag as CCharInfo;

                    if (activeCharacter != null)
                        DisplayCurrentCharacter(activeCharacter);
                }

                UpdateGlyphTextbox();
                CheckChange();
                MarkFontModified();

                return;
            }

            // NORMAL SINGLE-GLYPH DELETE
            int singleDeleteIndex = clickedCharacter.Index;

            if (pageEnd < singleDeleteIndex)
                return;

            StructuralEdit singleEdit = new StructuralEdit();

            singleEdit.Type = StructuralEditType.Delete;
            singleEdit.Index = singleDeleteIndex;
            singleEdit.PageEnd = pageEnd;
            singleEdit.OldLength = FontInfo.Character.Length;

            singleEdit.DeletedCharacter =
                FontInfo.Character[singleDeleteIndex];

            for (int i = singleDeleteIndex; i < pageEnd; i++)
            {
                FontInfo.Character[i] =
                    FontInfo.Character[i + 1];

                FontInfo.Character[i].Index = i;
            }

            CCharInfo finalEmptyCharacter = new CCharInfo();
            finalEmptyCharacter.Index = pageEnd;
            finalEmptyCharacter.BitDepth = (FontInfo is CWFNFontInfo && ((CWFNFontInfo)FontInfo).Is4Bit) ? 4 : 1;
            MakeGlyphPlaceholder(finalEmptyCharacter);

            FontInfo.Character[pageEnd] = finalEmptyCharacter;

            singleEdit.DeletePlaceholder = finalEmptyCharacter;

            _structuralUndoStack.Push(singleEdit);

            _operationUndoStack.Push(new UndoOperation
            {
                Type = UndoOperationType.StructuralEdit
            });

            _structuralRedoStack.Clear();
            _multiRedoStack.Clear();
            _operationRedoStack.Clear();

            Index = singleDeleteIndex;

            RefreshPageFromIndex(singleDeleteIndex);
            UpdateGlyphTextbox();
            CheckChange();
            MarkFontModified();
        }


        void PrevNext(EDirection direction)
        {
            switch (direction)
            {
                case EDirection.Next:
                    {
                        Index++;
                    }
                    break;
                case EDirection.Prev:
                    {
                        Index--;
                    }
                    break;
            }
            ;

            foreach (PictureBox item in CharacterPictureList)
            {
                CCharInfo characterinfo = (CCharInfo)(item.Tag);

                if (characterinfo.Index == Index)
                {
                    DisplayCurrentCharacter(characterinfo);
                }
            }
        }


        private void CollapseSelectionToActiveGlyph()
        {
            if (_selectedPreview == null)
                return;

            if (_selectedPreviews.Count <= 1)
                return;

            PictureBox activePreview = _selectedPreview;

            ClearPreviewSelection();
            AddPreviewToSelection(activePreview);

            _selectedPreview = activePreview;
            _selectionAnchor = activePreview;
        }

        private bool IsPreviewSelected(PictureBox pict)
        {
            return _selectedPreviews.Contains(pict);
        }

        private void SelectAllVisibleGlyphs()
        {
            if (CharacterPictureList.Count == 0)
                return;

            ClearPreviewSelection();

            foreach (PictureBox picture in CharacterPictureList)
            {
                AddPreviewToSelection(picture);
            }

            // Preserve the active glyph displayed in the drawing area.
            if (_selectedPreview == null ||
                !CharacterPictureList.Contains(_selectedPreview))
            {
                _selectedPreview = CharacterPictureList[0];
            }

            _selectionAnchor = _selectedPreview;

            FlowCharacterPanel.Invalidate();
        }

        private void ClearPreviewSelection()
        {
            foreach (PictureBox pict in _selectedPreviews)
            {
                pict.BackColor = FlowCharacterPanel.BackColor;
                pict.Invalidate();
            }

            _selectedPreviews.Clear();
        }

        private void AddPreviewToSelection(PictureBox pict)
        {
            if (pict == null)
                return;

            if (!_selectedPreviews.Contains(pict))
                _selectedPreviews.Add(pict);

            pict.BackColor = Color.FromArgb(80, 120, 200);
            pict.Invalidate();
        }

        private void RemovePreviewFromSelection(PictureBox pict)
        {
            if (pict == null)
                return;

            _selectedPreviews.Remove(pict);

            pict.BackColor = FlowCharacterPanel.BackColor;
            pict.Invalidate();
        }

        private void ShowActiveGlyphContextMenu()
        {
            if (_selectedPreview == null ||
                !CharacterPictureList.Contains(_selectedPreview))
                return;

            PictureBox activeGlyph = _selectedPreview;

            if (activeGlyph.ContextMenu == null)
                return;

            // Ensure the active glyph is visible before showing the menu.
            FlowCharacterPanel.ScrollControlIntoView(activeGlyph);

            // Position the menu near the glyph's bottom-right corner.
            Point menuPosition = new Point(
                activeGlyph.Width - 2,
                activeGlyph.Height - 2);

            activeGlyph.ContextMenu.Show(activeGlyph, menuPosition);
        }

        public void NavigateGlyphShortcut(bool forward)
        {
            if (CharacterPictureList.Count == 0)
                return;

            int current = CharacterPictureList.IndexOf(_selectedPreview);

            if (current < 0)
                current = 0;

            int next = Math.Max(0, Math.Min(
                CharacterPictureList.Count - 1,
                current + (forward ? 1 : -1)));

            if (next == current)
            {
                // Even at the page boundary, collapse multiple selection.
                SelectOnlyPreview(CharacterPictureList[current]);
                return;
            }

            NavigateGlyphSelection(forward ? Keys.Right : Keys.Left, false);
        }

        private void NavigateGlyphSelection(Keys direction, bool extendSelection)
        {
            if (CharacterPictureList.Count == 0)
                return;

            int current = CharacterPictureList.IndexOf(_selectedPreview);

            if (current < 0)
                current = 0;

            int next = current;

            if (direction == Keys.Home)
                next = 0;
            else if (direction == Keys.End)
                next = CharacterPictureList.Count - 1;
            else if (direction == Keys.Left)
                next = Math.Max(0, current - 1);
            else if (direction == Keys.Right)
                next = Math.Min(CharacterPictureList.Count - 1, current + 1);
            else
            {
                PictureBox source = CharacterPictureList[current];
                int sourceX = source.Left + source.Width / 2;
                int sourceY = source.Top + source.Height / 2;

                int bestDistance = int.MaxValue;

                foreach (PictureBox candidate in CharacterPictureList)
                {
                    int candidateIndex = CharacterPictureList.IndexOf(candidate);
                    int candidateY = candidate.Top + candidate.Height / 2;

                    bool correctDirection = direction == Keys.Up
                        ? candidateY < sourceY
                        : candidateY > sourceY;

                    if (!correctDirection)
                        continue;

                    int candidateX = candidate.Left + candidate.Width / 2;

                    // Prefer the nearest row, then the nearest column.
                    int distance =
                        Math.Abs(candidateY - sourceY) * 1000 +
                        Math.Abs(candidateX - sourceX);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        next = candidateIndex;
                    }
                }
            }

            if (next == current)
                return;

            PictureBox destination = CharacterPictureList[next];

            if (extendSelection)
            {
                // Keep the original anchor fixed while navigating.
                if (_selectionAnchor == null ||
                    !CharacterPictureList.Contains(_selectionAnchor))
                {
                    _selectionAnchor = CharacterPictureList[current];
                }

                SelectPreviewRange(_selectionAnchor, destination);
            }
            else
            {
                // Ordinary navigation starts a new single-glyph selection.
                SelectOnlyPreview(destination);
            }

            // Update the active glyph and drawing canvas.
            _selectedPreview = destination;
            DisplayCurrentCharacter((CCharInfo)destination.Tag, true);

            // Scroll only if the destination is outside the viewport.
            Rectangle visibleArea = FlowCharacterPanel.ClientRectangle;
            Rectangle destinationArea = new Rectangle(
                destination.Left + FlowCharacterPanel.AutoScrollPosition.X,
                destination.Top + FlowCharacterPanel.AutoScrollPosition.Y,
                destination.Width,
                destination.Height);

            if (!visibleArea.Contains(destinationArea))
                FlowCharacterPanel.ScrollControlIntoView(destination);

            FlowCharacterPanel.Invalidate();
        }

        private void SelectOnlyPreview(PictureBox pict)
        {
            _canvasSelectionActive = false;

            ClearPreviewSelection();

            AddPreviewToSelection(pict);

            _selectedPreview = pict;
            _selectionAnchor = pict;

            FlowCharacterPanel.Invalidate();

            // Move keyboard focus away from text fields.
            if (pict != null)
                pict.Focus();
        }

        private void SelectPreviewRange(PictureBox from, PictureBox to)
        {
            if (from == null || to == null)
                return;

            int fromIndex = CharacterPictureList.IndexOf(from);
            int toIndex = CharacterPictureList.IndexOf(to);

            if (fromIndex < 0 || toIndex < 0)
                return;

            int first = Math.Min(fromIndex, toIndex);
            int last = Math.Max(fromIndex, toIndex);

            ClearPreviewSelection();

            for (int i = first; i <= last; i++)
                AddPreviewToSelection(CharacterPictureList[i]);
        }


        void Character_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            PictureBox picture = (PictureBox)sender;
            CCharInfo characterinfo = (CCharInfo)picture.Tag;
            ContextMenu menu = picture.ContextMenu;

            if (!IsPreviewSelected(picture))
            {
                // Right-click outside the selection:
                // discard the multi-selection and select this glyph normally.
                SelectOnlyPreview(picture);
                DisplayCurrentCharacter(characterinfo);
            }

            // Update Undo/Redo BEFORE the context menu opens.
            if (menu != null)
            {
                menu.MenuItems[0].Enabled =
                    characterinfo.UndoPossible ||
                    _multiUndoStack.Count > 0 ||
                    _structuralUndoStack.Count > 0;

                menu.MenuItems[1].Enabled =
                    characterinfo.RedoPossible ||
                    _multiRedoStack.Count > 0 ||
                    _structuralRedoStack.Count > 0;
            }

            // If right-clicking anywhere inside the existing selection,
            // keep the whole selection and keep the original active glyph
            // on the canvas.
        }

        private void Glyphs_MouseWheel(object sender, MouseEventArgs e)
        {
            if (CharacterPictureList.Count == 0 || e.Delta == 0)
                return;

            int current = Index - PageStart;
            
            // reverse scrolling (uncomment this to use and comment out the other)
            // int next = current + (e.Delta > 0 ? -1 : 1);
            int next = current + (e.Delta > 0 ? 1 : -1);

            // Stay within the current page.
            if (next < 0 || next >= CharacterPictureList.Count)
                return;

            PictureBox picture = CharacterPictureList[next];

            SelectOnlyPreview(picture);
            DisplayCurrentCharacter((CCharInfo)picture.Tag);

            // Update the external selection outline.
            FlowCharacterPanel.Invalidate();

            HandledMouseEventArgs handled = e as HandledMouseEventArgs;
            if (handled != null)
                handled.Handled = true;
        }

        void Character_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouse = (MouseEventArgs)e;
            PictureBox picture = (PictureBox)sender;
            CCharInfo characterinfo = (CCharInfo)picture.Tag;
            ContextMenu menu = (ContextMenu)picture.ContextMenu;

            switch (mouse.Button)
            {
                case MouseButtons.Left:
                    {
                        bool ctrl =
                            (Control.ModifierKeys & Keys.Control) == Keys.Control;

                        bool shift =
                            (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

                        if (shift && _selectionAnchor != null)
                        {
                            // Extend the selection from the original anchor.
                            SelectPreviewRange(_selectionAnchor, picture);

                            // Make the clicked glyph active without clearing the selection.
                            _selectedPreview = picture;
                            DisplayCurrentCharacter(characterinfo, true);
                        }
                        else if (ctrl)
                        {
                            // Ctrl-click toggles individual glyphs without changing
                            // the active glyph shown on the canvas.
                            if (IsPreviewSelected(picture))
                            {
                                RemovePreviewFromSelection(picture);

                                // If the user deselected the active/anchor glyph,
                                // promote another selected glyph to become the new anchor.
                                if (picture == _selectionAnchor)
                                {
                                    if (_selectedPreviews.Count > 0)
                                    {
                                        _selectionAnchor = _selectedPreviews[0];
                                        _selectedPreview = _selectionAnchor;

                                        CCharInfo newActive =
                                            (CCharInfo)_selectedPreview.Tag;

                                        DisplayCurrentCharacter(newActive);
                                    }
                                    else
                                    {
                                        // Nothing remains selected, so select this glyph again.
                                        SelectOnlyPreview(picture);
                                        DisplayCurrentCharacter(characterinfo);
                                    }
                                }
                            }
                            else
                            {
                                AddPreviewToSelection(picture);

                                // Do NOT change _selectedPreview or _selectionAnchor.
                                // The original glyph stays on the canvas.
                            }
                        }
                        else
                        {
                            // Ordinary click starts a completely new selection
                            // and makes this the active canvas glyph.
                            SelectOnlyPreview(picture);
                            DisplayCurrentCharacter(characterinfo);
                        }

                        break;
                    }

                case MouseButtons.Right:
                    {
                        menu.MenuItems[0].Enabled =
                            characterinfo.UndoPossible ||
                            _multiUndoStack.Count > 0;

                        menu.MenuItems[1].Enabled = characterinfo.RedoPossible;
                        break;
                    }
            }

            // Move keyboard focus to the clicked glyph without changing
            // the current single or multiple selection.
            if (mouse.Button == MouseButtons.Left)
                picture.Focus();

            FlowCharacterPanel.Invalidate();
        }

        private void DisplayCurrentCharacter(CCharInfo characterinfo, bool preserveSelection = false)
        {
            if (characterinfo == null)
                return;

            Index = characterinfo.Index;
            UpdateGlyphTextbox();

            LblCharacter.Text = "Hex: 0x" + Convert.ToString(Index, 16).ToUpper();
            TxtCharacter.Text = Convert.ToString(Index);

            // Keep your numeric controls in sync, but do NOT force placeholder to become a real glyph.
            ClickedOnCharacter = true;

            // If placeholder, show default size in UI, but do not write it into the glyph.
            if (characterinfo.Width == 0 || characterinfo.Height == 0)
            {
                numWidth.Value = PLACEHOLDER_W;
                numHeight.Value = PLACEHOLDER_H;
            }
            else
            {
                if (characterinfo.BitDepth == 4)
                {
                    numWidth.Maximum = Math.Max(MaxWidth, (int)characterinfo.Width);
                    numHeight.Maximum = Math.Max(MaxHeight, (int)characterinfo.Height);
                }
                else
                {
                    numWidth.Maximum = MaxWidth;
                    numHeight.Maximum = MaxHeight;
                }

                numWidth.Value = characterinfo.Width;
                numHeight.Value = characterinfo.Height;
            }

            ClickedOnCharacter = false;

            // ✅ This is the key: just refresh the drawing area view (temp bitmap if placeholder).
            RefreshDrawingAreaDisplay(characterinfo);

            // Optional: grid overlay happens in Paint/Mouse logic; don't call PaintOnDrawingArea here.

            int localIndex = Index - PageStart;

            if (!preserveSelection &&
                localIndex >= 0 &&
                localIndex < CharacterPictureList.Count)
            {
                SelectOnlyPreview(
                    CharacterPictureList[localIndex]);
            }

            PaintOnDrawingArea(DrawingArea, null);
        }

        private void ZoomDrawingArea_ValueChanged(object sender, EventArgs e)
        {
            int localIndex = Index - PageStart;
            if (localIndex < 0 || localIndex >= CharacterPictureList.Count)
                return;

            CCharInfo characterinfo = (CCharInfo)CharacterPictureList[localIndex].Tag;

            RefreshDrawingAreaDisplay(characterinfo);

            LblZoom.Text = "x" + ZoomDrawingArea.Value;
        }
        private void PaintOnDrawingArea4Bit(object sender, EventArgs e)
        {
            MouseEventArgs mouse = e as MouseEventArgs;

            if (mouse == null || mouse.Button == MouseButtons.None)
                return;

            int localIndex = Index - PageStart;

            if (localIndex < 0 || localIndex >= CharacterPictureList.Count)
                return;

            CCharInfo character =
                (CCharInfo)CharacterPictureList[localIndex].Tag;

            if (character == null)
                return;

            // Initialize an empty 4-bit glyph when the user starts drawing.
            if (character.Width == 0 || character.Height == 0)
            {
                character.Width = PLACEHOLDER_W;
                character.Height = PLACEHOLDER_H;

                // Two 4-bit pixels per byte, with each row byte-aligned.
                int initialStride = (character.Width + 1) / 2;

                character.ByteLines = new byte[initialStride * character.Height];

                // Record the initialized glyph for Undo/Redo.
                character.UndoRedoListTidyUp();
                character.UndoRedoListAdd(character.ByteLines);

                RefreshCharacterPreview(
                    CharacterPictureList[localIndex],
                    character);

                RefreshDrawingAreaDisplay(character);
            }

            int zoom = ZoomDrawingArea.Value;

            if (mouse.X < 0 || mouse.Y < 0)
                return;

            int x = mouse.X / zoom;
            int y = mouse.Y / zoom;

            if (x >= character.Width || y >= character.Height)
                return;

            int stride = (character.Width + 1) / 2;
            int byteIndex = y * stride + x / 2;

            if (character.ByteLines == null ||
                byteIndex >= character.ByteLines.Length)
                return;

            bool painting =
                mouse.Button == MouseButtons.Left
                    ? !grayscaleMouseButtonsSwapped
                    : mouse.Button == MouseButtons.Right &&
                      grayscaleMouseButtonsSwapped;

            if (mouse.Button != MouseButtons.Left &&
                mouse.Button != MouseButtons.Right)
                return;

            int shade = painting ? selectedGrayscaleShade : 0;

            byte oldValue = character.ByteLines[byteIndex];

            byte newValue = (x % 2 == 0)
                ? (byte)((oldValue & 0x0F) | (shade << 4))
                : (byte)((oldValue & 0xF0) | shade);

            if (newValue == oldValue)
                return;

            character.ByteLines[byteIndex] = newValue;

            RefreshCharacterPreview(
                CharacterPictureList[localIndex],
                character);

            RefreshDrawingAreaDisplay(character);
        }

        private void PaintOnDrawingArea(object sender, EventArgs e)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                PaintOnDrawingArea4Bit(sender, e);
                return;
            }

            MouseEventArgs mouse = e as MouseEventArgs;

            Graphics graphics = ((PictureBox)sender).CreateGraphics();
            Int32 zoom = ZoomDrawingArea.Value;
            Color col = Color.Gray;

            int localIndex = Index - PageStart;

            // Guard against invalid page/index combination
            if (localIndex < 0 || localIndex >= CharacterPictureList.Count)
            {
                graphics.Dispose();
                return;
            }

            CCharInfo character =
                (CCharInfo)CharacterPictureList[localIndex].Tag;

            // 🔴 INITIALIZE EMPTY GLYPH IF NEEDED
            // Do NOT initialize placeholder glyphs unless the user is actually drawing
            Bitmap Selected = character.UnscaledImage as Bitmap;

            bool isUserDrawing = (mouse != null && mouse.Button != MouseButtons.None);

            if (isUserDrawing)
            {
                // If placeholder, materialize it NOW (user started drawing)
                if (character.Width == 0 || character.Height == 0 || Selected == null)
                {
                    character.Width = PLACEHOLDER_W;
                    character.Height = PLACEHOLDER_H;
                    character.WidthOriginal = PLACEHOLDER_W;
                    character.HeightOriginal = PLACEHOLDER_H;

                    int bytesPerLine = (character.Width - 1) / 8 + 1;
                    character.ByteLines = new byte[character.Height * bytesPerLine];

                    Bitmap newBmp;
                    CFontUtils.CreateBitmap(character, out newBmp);
                    character.UnscaledImage = newBmp;
                    Selected = newBmp;

                    // Update preview image immediately
                    Bitmap scaled;
                    CFontUtils.ScaleBitmap(newBmp, out scaled, Scalefactor);
                    CharacterPictureList[localIndex].Image = scaled;
                    CharacterPictureList[localIndex].Invalidate();
                }
            }

            // If still placeholder and not drawing: just exit (keep placeholder!)
            if (Selected == null)
            {
                graphics.Dispose();
                return;
            }

            if (mouse != null && mouse.Button != MouseButtons.None)
            {
                if (((mouse.X / zoom) < Selected.Width) &&
                    ((mouse.Y / zoom) < Selected.Height))
                {
                    SolidBrush brush = new SolidBrush(Color.Gray);

                    BitmapData bmpData = Selected.LockBits(
                        new Rectangle(0, 0, Selected.Width, Selected.Height),
                        ImageLockMode.ReadWrite,
                        Selected.PixelFormat);

                    IntPtr ptr = bmpData.Scan0;
                    ptr = (IntPtr)((int)ptr + bmpData.Stride * (mouse.Y / zoom));

                    byte[] b = new byte[bmpData.Stride];
                    System.Runtime.InteropServices.Marshal.Copy(ptr, b, 0, bmpData.Stride);
                    Array.Reverse(b);

                    UInt32 line = BitConverter.ToUInt32(b, 0);

                    switch (mouse.Button)
                    {
                        case MouseButtons.Left:
                            col = PanelLeftMouse.BackColor;
                            break;

                        case MouseButtons.Right:
                            col = PanelRightMouse.BackColor;
                            break;

                        case MouseButtons.Middle:
                            col = ((line >> (mouse.X / zoom)) > 0)
                                ? PanelLeftMouse.BackColor
                                : PanelRightMouse.BackColor;
                            break;
                    }

                    if (Color.Black == col)
                    {
                        brush = new SolidBrush(Color.Black);
                        line &= ~(UInt32)(0x80000000 >> (mouse.X / zoom));
                    }
                    else
                    {
                        brush = new SolidBrush(Color.White);
                        line |= (UInt32)(0x80000000 >> (mouse.X / zoom));
                    }

                    b = BitConverter.GetBytes(line);
                    Array.Reverse(b);

                    System.Runtime.InteropServices.Marshal.Copy(b, 0, ptr, bmpData.Stride);
                    Selected.UnlockBits(bmpData);

                    graphics.FillRectangle(
                        brush,
                        new Rectangle(
                            (mouse.X / zoom) * zoom,
                            (mouse.Y / zoom) * zoom,
                            zoom,
                            zoom));

                    brush.Dispose();

                    CFontUtils.SaveByteLinesFromPicture(character, Selected);
                    character.UnscaledImage = Selected;

                    Bitmap outbmp;
                    CFontUtils.ScaleBitmap(Selected, out outbmp, Scalefactor);
                    CharacterPictureList[localIndex].Image = outbmp;
                    CharacterPictureList[localIndex].Invalidate();
                }
            }

            if (ShowGrid)
            {
                for (int xcnt = 0; xcnt < Selected.Width; xcnt++)
                {
                    for (int ycnt = 0; ycnt < Selected.Height; ycnt++)
                    {
                        graphics.DrawRectangle(
                            GridPen,
                            xcnt * zoom,
                            ycnt * zoom,
                            zoom,
                            zoom);
                    }
                }
            }

            graphics.Dispose();
        }

        private void ReplaceCharacterShade(CCharInfo character)
        {
            if (character == null ||
                character.ByteLines == null ||
                character.Width == 0 ||
                character.Height == 0 ||
                _replaceSourceShade == _replaceTargetShade)
                return;

            int width = character.Width;
            int height = character.Height;
            int stride = (width + 1) / 2;

            if (character.ByteLines.Length < stride * height)
                return;

            byte[] adjusted = (byte[])character.ByteLines.Clone();
            bool changed = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x / 2;

                    int shade = (x & 1) == 0
                        ? (adjusted[offset] >> 4) & 0x0F
                        : adjusted[offset] & 0x0F;

                    if (shade != _replaceSourceShade)
                        continue;

                    changed = true;

                    if ((x & 1) == 0)
                    {
                        adjusted[offset] = (byte)(
                            (adjusted[offset] & 0x0F) |
                            (_replaceTargetShade << 4));
                    }
                    else
                    {
                        adjusted[offset] = (byte)(
                            (adjusted[offset] & 0xF0) |
                            _replaceTargetShade);
                    }
                }
            }

            if (!changed)
                return;

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            character.ByteLines = adjusted;

            character.UndoRedoListAdd(character.ByteLines);
            RegisterGlyphUndoOperation(character);

            int localIndex = character.Index - PageStart;

            if (localIndex >= 0 &&
                localIndex < CharacterPictureList.Count)
            {
                RefreshCharacterPreview(
                    CharacterPictureList[localIndex], character);
            }

            if (character.Index == Index)
                RefreshDrawingAreaDisplay(character);

            CheckChange();
        }

        private void ReplaceShadeFromCanvas(MouseEventArgs e)
        {
            if (!(FontInfo is CWFNFontInfo wfn) || !wfn.Is4Bit)
                return;

            if (FontInfo.Character == null ||
                Index < 0 ||
                Index >= FontInfo.Character.Length)
                return;

            CCharInfo character = FontInfo.Character[Index];

            if (character == null ||
                character.ByteLines == null ||
                character.Width == 0 ||
                character.Height == 0)
                return;

            int zoom = ZoomDrawingArea.Value;

            if (e.X < 0 || e.Y < 0)
                return;

            int x = e.X / zoom;
            int y = e.Y / zoom;

            if (x >= character.Width || y >= character.Height)
                return;

            int stride = (character.Width + 1) / 2;
            int offset = y * stride + x / 2;

            if (offset >= character.ByteLines.Length)
                return;

            byte value = character.ByteLines[offset];

            _replaceSourceShade = (x & 1) == 0
                ? (value >> 4) & 0x0F
                : value & 0x0F;

            _replaceTargetShade = selectedGrayscaleShade;

            if (_replaceSourceShade == _replaceTargetShade)
                return;

            // Ctrl+Shift replaces the shade throughout the current page.
            // Shift alone replaces it only in the selected glyphs.
            // "Use on all" is deliberately ignored for both operations.
            bool pageWide =
                (Control.ModifierKeys &
                 (Keys.Control | Keys.Shift | Keys.Alt))
                == (Keys.Control | Keys.Shift);

            if (pageWide)
            {
                CharacterFunctionAllAsGrouped(ReplaceCharacterShade);
            }
            else
            {
                CharacterFunctionSelectionOrOneOrAll(
                    ReplaceCharacterShade, false);
            }
        }

        private void SampleDrawingAreaShade(MouseEventArgs e)
        {
            if (!(FontInfo is CWFNFontInfo wfn) || !wfn.Is4Bit)
                return;

            if (FontInfo.Character == null ||
                Index < 0 ||
                Index >= FontInfo.Character.Length)
                return;

            CCharInfo character = FontInfo.Character[Index];

            if (character == null ||
                character.ByteLines == null ||
                character.Width == 0 ||
                character.Height == 0)
                return;

            int zoom = ZoomDrawingArea.Value;

            if (e.X < 0 || e.Y < 0)
                return;

            int x = e.X / zoom;
            int y = e.Y / zoom;

            if (x >= character.Width || y >= character.Height)
                return;

            int stride = (character.Width + 1) / 2;
            int byteIndex = y * stride + x / 2;

            if (byteIndex >= character.ByteLines.Length)
                return;

            byte value = character.ByteLines[byteIndex];

            selectedGrayscaleShade = (x & 1) == 0
                ? (value >> 4)
                : (value & 0x0F);

            pendingShadeDigit = -1;

            int intensity = selectedGrayscaleShade * 17;
            Color shadeColor = Color.FromArgb(
                intensity, intensity, intensity);

            Panel drawingColorPanel = grayscaleMouseButtonsSwapped
                ? PanelRightMouse
                : PanelLeftMouse;

            drawingColorPanel.BackColor = shadeColor;

            grayscalePalette.Invalidate();
        }

        private void DrawingArea_Click(object sender, EventArgs e)
        {
            if (_eyedropperClick ||
                _shadeReplaceClick ||
                (grayscalePalette != null &&
                 grayscalePalette.Visible &&
                 (Control.ModifierKeys & Keys.Alt) == Keys.Alt))
                return;

            PaintOnDrawingArea(sender, e);
            CheckChange();
        }

        private byte[] _strokeBeforeBytes;
        private ushort _strokeBeforeWidth;
        private ushort _strokeBeforeHeight;

        private void DrawingArea_MouseDown(object sender, MouseEventArgs e)
        {
            _eyedropperClick = false;
            _shadeReplaceClick = false;

            if (grayscalePalette != null &&
                grayscalePalette.Visible &&
                e.Button == MouseButtons.Left &&
                ((Control.ModifierKeys & (Keys.Shift | Keys.Control | Keys.Alt))
                    == Keys.Shift ||
                 (Control.ModifierKeys & (Keys.Shift | Keys.Control | Keys.Alt))
                    == (Keys.Control | Keys.Shift)))
            {
                _shadeReplaceClick = true;
                bInEdit = false;

                ReplaceShadeFromCanvas(e);
                return;
            }

            if (grayscalePalette != null &&
                grayscalePalette.Visible &&
                e.Button == MouseButtons.Left &&
                (Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                _eyedropperClick = true;
                bInEdit = false;

                SampleDrawingAreaShade(e);
                return;
            }

            ApplyPendingGlyphRange();

            // Drawing on the canvas returns to a single-glyph selection.
            if (_selectedPreviews.Count > 1)
            {
                CollapseSelectionToActiveGlyph();
            }

            // Restore the active glyph's DeepSkyBlue selection outline
            // without changing keyboard focus or scroll position.
            _canvasSelectionActive = true;
            FlowCharacterPanel.Invalidate();

            bInEdit = true;

            int localIndex = Index - PageStart;

            if (localIndex < 0 || localIndex >= CharacterPictureList.Count)
                return;

            CCharInfo character =
                CharacterPictureList[localIndex].Tag as CCharInfo;

            if (character == null)
                return;

            // Ensure an empty glyph has an initial Undo state before drawing.
            if (!character.UndoPossible && !character.RedoPossible)
            {
                character.UndoRedoListAdd(character.ByteLines);
            }

            _strokeBeforeWidth = character.Width;
            _strokeBeforeHeight = character.Height;

            _strokeBeforeBytes = character.ByteLines == null
                ? null
                : (byte[])character.ByteLines.Clone();
        }
        private void DrawingArea_MouseMove(object sender, MouseEventArgs e)
        {
            bool altHeld = (Control.ModifierKeys & Keys.Alt) == Keys.Alt;

            // Shade replacement is a single operation, not a painting stroke.
            if (_shadeReplaceClick)
                return;

            // Continuously sample shades while Alt + Left Mouse is held.
            if (_eyedropperClick)
            {
                if (altHeld && (e.Button & MouseButtons.Left) != 0)
                {
                    SampleDrawingAreaShade(e);
                }

                return;
            }

            int zoom = ZoomDrawingArea.Value;

            if (bInEdit)
            {
                if (((e.X / zoom) - EditPoint.X != 0) ||
                    ((e.Y / zoom) - EditPoint.Y != 0))
                {
                    EditPoint.X = e.X / zoom;
                    EditPoint.Y = e.Y / zoom;

                    PaintOnDrawingArea(sender, e);
                }
            }
        }
        private void DrawingArea_MouseUp(object sender, MouseEventArgs e)
        {
            if (_shadeReplaceClick)
            {
                bInEdit = false;
                return;
            }

            if (_eyedropperClick)
            {
                bInEdit = false;
                return;
            }

            bInEdit = false;
            CheckChange();

            // If no glyphs exist, do nothing
            if (FontInfo == null || FontInfo.Character == null)
                return;

            if (FontInfo.Character.Length == 0)
                return;

            int localIndex = Index - PageStart;

            if (localIndex < 0 || localIndex >= CharacterPictureList.Count)
                return;

            CCharInfo characterinfo =
                (CCharInfo)(CharacterPictureList[localIndex].Tag);

            characterinfo.UndoRedoListTidyUp();
            characterinfo.UndoRedoListAdd(characterinfo.ByteLines, true);
            RegisterGlyphUndoOperation(characterinfo);

            _strokeBeforeBytes = null;

            if (CharacterPictureList[localIndex].ContextMenu != null)
            {
                CharacterPictureList[localIndex]
                    .ContextMenu.MenuItems[0].Enabled = characterinfo.UndoPossible;

                CharacterPictureList[localIndex]
                    .ContextMenu.MenuItems[1].Enabled = characterinfo.RedoPossible;
            }
        }
        private void DrawingArea_Paint(object sender, PaintEventArgs e)
        {
            int localIndex = Index - PageStart;
            if (localIndex < 0 || localIndex >= CharacterPictureList.Count)
                return;

            CCharInfo character = (CCharInfo)CharacterPictureList[localIndex].Tag;

            Bitmap src = GetDrawingAreaBitmapForGlyph(character);

            Bitmap scaled;
            CFontUtils.ScaleBitmap(src, out scaled, ZoomDrawingArea.Value);
            if (scaled == null)
                return;

            e.Graphics.DrawImage(scaled, 0, 0);

            if (ShowGrid && character != null)
            {
                // For placeholders, grid should match the configured placeholder size,
                // not 0x0.
                int w = (character.Width > 0) ? character.Width : PLACEHOLDER_W;
                int h = (character.Height > 0) ? character.Height : PLACEHOLDER_H;

                Int32 zoom = ZoomDrawingArea.Value;
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        e.Graphics.DrawRectangle(GridPen, x * zoom, y * zoom, zoom, zoom);
                    }
                }
            }
        }

        private void SetGridFix()
        {
            FlowCharacterPanel.SuspendLayout();

            try
            {
                if (GridFix)
                {
                    int maxwidth = 0;

                    foreach (PictureBox item in CharacterPictureList)
                    {
                        maxwidth = Math.Max(item.Width, maxwidth);
                    }

                    foreach (PictureBox item in CharacterPictureList)
                    {
                        item.Margin = new Padding(3, 3, maxwidth + 3 - item.Width, 3);
                    }
                }
                else
                {
                    foreach (PictureBox item in CharacterPictureList)
                    {
                        item.Margin = new Padding(3, 3, 3, 3);
                    }
                }
            }
            finally
            {
                FlowCharacterPanel.ResumeLayout();
            }
        }

        public void Save()
        {
            if (!FontModifiedSaved)
                return;

            CFontUtils.SaveOneFont(FontInfo);
            MarkAsSaved();

            if (OnFontModified != null)
            {
                MyEventArgs args = new MyEventArgs();
                args.Modified = false;
                OnFontModified(this, args);
            }
        }

        private void Resize4BitGlyph(CCharInfo character, int newWidth, int newHeight)
        {
            if (character == null || newWidth < 1 || newHeight < 1)
                return;

            int oldWidth = character.Width;
            int oldHeight = character.Height;

            int oldStride = (oldWidth + 1) / 2;
            int newStride = (newWidth + 1) / 2;

            byte[] oldPixels = character.ByteLines;
            byte[] newPixels = new byte[newStride * newHeight];

            if (oldPixels != null)
            {
                int copyWidth = Math.Min(oldWidth, newWidth);
                int copyHeight = Math.Min(oldHeight, newHeight);

                for (int y = 0; y < copyHeight; y++)
                {
                    for (int x = 0; x < copyWidth; x++)
                    {
                        int oldIndex = y * oldStride + x / 2;
                        int newIndex = y * newStride + x / 2;

                        if (oldIndex >= oldPixels.Length)
                            continue;

                        int shade = (x % 2 == 0)
                            ? (oldPixels[oldIndex] >> 4) & 0x0F
                            : oldPixels[oldIndex] & 0x0F;

                        if (x % 2 == 0)
                            newPixels[newIndex] |= (byte)(shade << 4);
                        else
                            newPixels[newIndex] |= (byte)shade;
                    }
                }
            }

            character.Width = (UInt16)newWidth;
            character.Height = (UInt16)newHeight;
            character.ByteLines = newPixels;
        }

        private void AdjustSize(SizeMode sizeMode)
        {
            if (!ClickedOnCharacter &&
                FontInfo != null &&
                FontInfo.Character != null)
            {
                if (_selectedPreviews.Count > 1)
                {
                    List<int> changedIndexes = new List<int>();

                    bool relative = _glyphSizeArrowClicked;

                    if (relative)
                    {
                        int previousValue =
                            sizeMode == SizeMode.ChangeHeight
                                ? _glyphSizePreviousHeight
                                : _glyphSizePreviousWidth;

                        int currentValue =
                            sizeMode == SizeMode.ChangeHeight
                                ? (int)numHeight.Value
                                : (int)numWidth.Value;

                        if (currentValue != previousValue)
                        {
                            _glyphSizeArrowDirection =
                                Math.Sign(currentValue - previousValue);
                        }
                    }

                    _recordingGroupedOperation = true;

                    try
                    {
                        foreach (PictureBox preview in _selectedPreviews.ToList())
                        {
                            CCharInfo character = preview.Tag as CCharInfo;

                            if (character == null)
                                continue;

                            // Use the displayed placeholder dimensions as the starting
                            // point when adjusting multiple glyphs with the arrow buttons.
                            int baseWidth = character.Width == 0
                                ? PLACEHOLDER_W
                                : character.Width;

                            int baseHeight = character.Height == 0
                                ? PLACEHOLDER_H
                                : character.Height;

                            int newWidth = character.Width;
                            int newHeight = character.Height;

                            if (sizeMode == SizeMode.ChangeWidth ||
                                sizeMode == SizeMode.ChangeBoth)
                            {
                                newWidth = relative
                                    ? Math.Max(1, Math.Min(MaxWidth,
                                        baseWidth + _glyphSizeArrowDirection))
                                    : (int)numWidth.Value;
                            }

                            if (sizeMode == SizeMode.ChangeHeight ||
                                sizeMode == SizeMode.ChangeBoth)
                            {
                                newHeight = relative
                                    ? Math.Max(1, Math.Min(MaxHeight,
                                        baseHeight + _glyphSizeArrowDirection))
                                    : (int)numHeight.Value;
                            }

                            if (newWidth == character.Width &&
                                newHeight == character.Height)
                                continue;

                            CreateAndShow(
                                character,
                                sizeMode,
                                newWidth,
                                newHeight);

                            changedIndexes.Add(character.Index);
                        }
                    }
                    finally
                    {
                        _recordingGroupedOperation = false;
                    }

                    if (changedIndexes.Count > 0)
                    {
                        _multiUndoStack.Push(changedIndexes);

                        _operationUndoStack.Push(new UndoOperation
                        {
                            Type = UndoOperationType.MultiGlyphEdit
                        });

                        _multiRedoStack.Clear();
                        _structuralRedoStack.Clear();
                        _operationRedoStack.Clear();
                        _fontBitDepthRedoStack.Clear();
                    }
                }
                else
                {
                    CCharInfo character =
                        (CCharInfo)CharacterPictureList[Index - PageStart].Tag;

                    CreateAndShow(character, sizeMode);
                }
            }

            SetGridFix();
            CheckChange();
            FlowCharacterPanel.Invalidate();
        }

        private void CreateAndShow(
            CCharInfo character,
            SizeMode sizeMode,
            int? targetWidth = null,
            int? targetHeight = null)
        {
            int newWidth = character.Width;
            int newHeight = character.Height;

            // Use the displayed default dimensions when editing a placeholder.
            if (character.Width == 0 || character.Height == 0)
            {
                newWidth = PLACEHOLDER_W;
                newHeight = PLACEHOLDER_H;
            }

            switch (sizeMode)
            {
                case SizeMode.ChangeBoth:
                    newWidth = targetWidth ?? (int)numWidth.Value;
                    newHeight = targetHeight ?? (int)numHeight.Value;
                    break;

                case SizeMode.ChangeWidth:
                    newWidth = targetWidth ?? (int)numWidth.Value;
                    break;

                case SizeMode.ChangeHeight:
                    newHeight = targetHeight ?? (int)numHeight.Value;
                    break;
            }

            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                if (character.Width == newWidth &&
                    character.Height == newHeight)
                    return;

                character.UndoRedoListTidyUp();
                character.UndoRedoListAdd(character.ByteLines);

                Resize4BitGlyph(character, newWidth, newHeight);

                character.UndoRedoListAdd(character.ByteLines);
                RegisterGlyphUndoOperation(character);

                int localIndex = character.Index - PageStart;

                if (localIndex >= 0 &&
                    localIndex < CharacterPictureList.Count)
                {
                    RefreshCharacterPreview(
                        CharacterPictureList[localIndex], character);
                }

                if (character.Index == Index)
                    RefreshDrawingAreaDisplay(character);
                return;
            }

            bool sizeChanged =
                sizeMode != SizeMode.ChangeNothing &&
                (character.Width != newWidth ||
                 character.Height != newHeight);

            if (sizeChanged)
            {
                character.UndoRedoListTidyUp();
                character.UndoRedoListAdd(character.ByteLines);
            }

            switch (sizeMode)
            {
                case SizeMode.ChangeNothing:
                    CFontUtils.RecreateCharacter(
                        character,
                        character.Width,
                        character.Height);
                    break;

                case SizeMode.ChangeBoth:
                    CFontUtils.RecreateCharacter(
                        character,
                        (UInt16)newWidth,
                        (UInt16)newHeight);
                    break;

                case SizeMode.ChangeHeight:
                    CFontUtils.RecreateCharacter(
                        character,
                        (UInt16)newWidth,
                        (UInt16)newHeight);
                    break;

                case SizeMode.ChangeWidth:
                    CFontUtils.RecreateCharacter(
                        character,
                        (UInt16)newWidth,
                        (UInt16)newHeight);
                    break;
            }

            if (sizeChanged)
            {
                character.UndoRedoListAdd(character.ByteLines);
                RegisterGlyphUndoOperation(character);
            }

            Bitmap bitmap = null;

            CFontUtils.CreateBitmap(character, out bitmap);
            character.UnscaledImage = bitmap;

            Bitmap outbmp;
            CFontUtils.ScaleBitmap(bitmap, out outbmp, Scalefactor);

            CharacterPictureList[character.Index - PageStart].Size = outbmp.Size;
            CharacterPictureList[character.Index - PageStart].Image = outbmp;

            _toolTip.SetToolTip(
                CharacterPictureList[character.Index - PageStart],
                Chr(character.Index));

            if (character.Index == Index)
            {
                Bitmap drawingbitmap;
                CFontUtils.ScaleBitmap(
                    bitmap,
                    out drawingbitmap,
                    ZoomDrawingArea.Value);

                DrawingArea.Size = drawingbitmap.Size;
                DrawingArea.Image = drawingbitmap;
            }
        }

        // Previous dimensions of the active glyph, used for relative resizing.
        private int _glyphSizePreviousWidth = 0;
        private int _glyphSizePreviousHeight = 0;

        // Tracks whether the Width/Height numeric arrows are being used.
        private bool _glyphSizeArrowClicked = false;

        // +1 = increase, -1 = decrease, 0 = not an arrow click.
        private int _glyphSizeArrowDirection = 0;

        private void NumSize_MouseDown(object sender, MouseEventArgs e)
        {
            NumericUpDown numeric = sender as NumericUpDown;

            if (FontInfo != null &&
                FontInfo.Character != null &&
                Index >= 0 &&
                Index < FontInfo.Character.Length)
            {
                CCharInfo activeGlyph = FontInfo.Character[Index];

                _glyphSizePreviousWidth = activeGlyph.Width;
                _glyphSizePreviousHeight = activeGlyph.Height;
            }

            _glyphSizeArrowClicked = false;

            if (numeric != null && e.Button == MouseButtons.Left)
            {
                // NumericUpDown arrow buttons occupy the rightmost portion.
                _glyphSizeArrowClicked =
                    e.X >= numeric.ClientSize.Width -
                    SystemInformation.VerticalScrollBarWidth;

                _glyphSizeArrowDirection = _glyphSizeArrowClicked
                    ? (e.Y < numeric.ClientSize.Height / 2 ? 1 : -1)
                    : 0;
            }
        }

        private void NumSize_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left ||
    !_glyphSizeArrowClicked ||
    _glyphSizeArrowDirection == 0)
                return;

            NumericUpDown numeric = sender as NumericUpDown;

            if (numeric == null)
                return;

            // If the first arrow click changed the displayed value but
            // left the selected glyph as a placeholder, apply it now.
            if (_selectedPreviews.Count <= 1 &&
                FontInfo != null &&
                FontInfo.Character != null &&
                Index >= 0 &&
                Index < FontInfo.Character.Length)
            {
                CCharInfo character = FontInfo.Character[Index];

                if (character != null &&
                    (character.Width == 0 || character.Height == 0))
                {
                    AdjustSize(numeric == numWidth
                        ? SizeMode.ChangeWidth
                        : SizeMode.ChangeHeight);
                }

                return;
            }

            if (_selectedPreviews.Count <= 1)
                return;

            bool isWidth = numeric == numWidth;

            int previousValue = isWidth
                ? _glyphSizePreviousWidth
                : _glyphSizePreviousHeight;

            int currentValue = (int)numeric.Value;

            // A normal value change has already been processed by ValueChanged.
            if (currentValue != previousValue)
                return;

            // Only handle clicks that attempt to move beyond the active glyph's limit.
            if ((_glyphSizeArrowDirection < 0 && currentValue == numeric.Minimum) ||
                (_glyphSizeArrowDirection > 0 && currentValue == numeric.Maximum))
            {
                AdjustSize(isWidth
                    ? SizeMode.ChangeWidth
                    : SizeMode.ChangeHeight);
            }
        }

        private void numWidth_ValueChanged(object sender, EventArgs e)
        {
            // Updating the displayed glyph must not resize the selection.
            if (ClickedOnCharacter)
                return;

            if (numWidth.Value > MaxWidth)
            {
                numWidth.Value = MaxWidth;
            }
            if (numWidth.Value < 1)
            {
                numWidth.Value = 1;
            }

            // For a placeholder, wait until Enter when the user types a size.
            // Arrow clicks should still resize immediately.
            if (FontInfo != null &&
                FontInfo.Character != null &&
                Index >= 0 &&
                Index < FontInfo.Character.Length &&
                _selectedPreviews.Count <= 1)
            {
                CCharInfo character = FontInfo.Character[Index];

                if (character != null &&
                    (character.Width == 0 || character.Height == 0) &&
                    !_glyphSizeArrowClicked)
                {
                    return;
                }
            }

            if (_selectedPreviews.Count > 1)
            {
                BeginInvoke((MethodInvoker)(() =>
                    AdjustSize(SizeMode.ChangeWidth)));
            }
            else
            {
                AdjustSize(SizeMode.ChangeWidth);
            }
        }
        private void numHeight_ValueChanged(object sender, EventArgs e)
        {
            // Updating the displayed glyph must not resize the selection.
            if (ClickedOnCharacter)
                return;

            if (numHeight.Value > MaxHeight)
            {
                numHeight.Value = MaxHeight;
            }
            if (numHeight.Value < 1)
            {
                numHeight.Value = 1;
            }

            // For a placeholder, wait until Enter when the user types a size.
            // Arrow clicks should still resize immediately.
            if (FontInfo != null &&
                FontInfo.Character != null &&
                Index >= 0 &&
                Index < FontInfo.Character.Length &&
                _selectedPreviews.Count <= 1)
            {
                CCharInfo character = FontInfo.Character[Index];

                if (character != null &&
                    (character.Width == 0 || character.Height == 0) &&
                    !_glyphSizeArrowClicked)
                {
                    return;
                }
            }

            if (_selectedPreviews.Count > 1)
            {
                BeginInvoke((MethodInvoker)(() =>
                    AdjustSize(SizeMode.ChangeHeight)));
            }
            else
            {
                AdjustSize(SizeMode.ChangeHeight);
            }
        }

        private decimal _glyphWidthOnEnter;
        private decimal _glyphHeightOnEnter;

        private void GlyphSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Prevent entering negative values in the width/height fields.
            if (e.KeyChar == '-')
                e.Handled = true;
        }

        private void GlyphSize_Enter(object sender, EventArgs e)
        {
            NumericUpDown numeric = sender as NumericUpDown;

            if (numeric == null)
                return;

            if (numeric == numWidth)
                _glyphWidthOnEnter = numeric.Value;
            else if (numeric == numHeight)
                _glyphHeightOnEnter = numeric.Value;

            numeric.Select(0, numeric.Text.Length);
        }

        private void GlyphSize_Leave(object sender, EventArgs e)
        {
            NumericUpDown numeric = sender as NumericUpDown;

            if (numeric == null ||
                FontInfo == null ||
                FontInfo.Character == null ||
                Index < 0 ||
                Index >= FontInfo.Character.Length ||
                _selectedPreviews.Count > 1)
                return;

            CCharInfo character = FontInfo.Character[Index];

            if (character == null ||
                (character.Width != 0 && character.Height != 0))
                return;

            decimal previousValue = numeric == numWidth
                ? _glyphWidthOnEnter
                : _glyphHeightOnEnter;

            if (numeric.Value != previousValue)
                AdjustSize(SizeMode.ChangeBoth);
        }

        private void GlyphSize_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard input is not a mouse-arrow adjustment.
            _glyphSizeArrowClicked = false;
            _glyphSizeArrowDirection = 0;

            if (e.KeyCode == Keys.Enter)
            {
                if (FontInfo != null &&
                    FontInfo.Character != null &&
                    Index >= 0 &&
                    Index < FontInfo.Character.Length)
                {
                    CCharInfo character = FontInfo.Character[Index];

                    if (character != null &&
                        (character.Width == 0 || character.Height == 0))
                    {
                        AdjustSize(SizeMode.ChangeBoth);
                    }
                }

                if (_selectedPreview != null)
                    _selectedPreview.Focus();

                e.SuppressKeyPress = true;
            }
        }

        private void ChkGrid_CheckedChanged(object sender, EventArgs e)
        {
            ShowGrid = ChkGrid.Checked;
            Character_Click(CharacterPictureList[Index - PageStart], new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            XmlSettings.Grid = ShowGrid;
            XmlSettings.Write();
        }
        private void ChkGridFix_CheckedChanged(object sender, EventArgs e)
        {
            GridFix = ChkGridFix.Checked;

            SetGridFix();

            XmlSettings.GridFix = GridFix;
            XmlSettings.Write();
        }

        private int[] GetRenderGlyphIndexes(string text)
        {
            if (XmlSettings.RenderUnicode)
            {
                // Unicode: use the character's code point directly.
                return text.Select(c => (int)c).ToArray();
            }

            // ANSI: use the Windows system ANSI code page.
            Encoding encoding = (Encoding)Encoding.Default.Clone();

            // Reject characters that cannot be represented in ANSI.
            encoding.EncoderFallback = EncoderFallback.ExceptionFallback;

            try
            {
                byte[] bytes = encoding.GetBytes(text);
                return bytes.Select(b => (int)b).ToArray();
            }
            catch (EncoderFallbackException)
            {
                return null;
            }
        }

        private Image RenderTextLine(string text)
        {
            if (FontInfo == null ||
                FontInfo.Character == null ||
                string.IsNullOrEmpty(text))
            {
                return CreateRenderErrorImage();
            }

            int[] glyphIndexes = GetRenderGlyphIndexes(text);

            if (glyphIndexes == null)
                return CreateRenderErrorImage();

            int width = 0;
            int height = 0;

            // First pass: check the requested glyphs against the ENTIRE font.
            foreach (int glyphIndex in glyphIndexes)
            {

                if (glyphIndex < 0 ||
                    glyphIndex >= FontInfo.Character.Length)
                {
                    return CreateRenderErrorImage();
                }

                CCharInfo glyph = FontInfo.Character[glyphIndex];

                // The glyph must actually contain drawable font data.
                if (glyph == null ||
                    glyph.Width == 0 ||
                    glyph.Height == 0 ||
                    glyph.ByteLines == null)
                {
                    return CreateRenderErrorImage();
                }

                width += glyph.Width;
                height = Math.Max(height, glyph.Height);
            }

            Bitmap bmp = new Bitmap(
                Math.Max(width, 1),
                Math.Max(height, 1));

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(
                    Brushes.Gray,
                    0,
                    0,
                    bmp.Width,
                    bmp.Height);

                int xpos = 0;

                foreach (int glyphIndex in glyphIndexes)
                {

                    CCharInfo glyph =
                        FontInfo.Character[glyphIndex];

                    Bitmap glyphBitmap = null;
                    bool temporaryBitmap = false;

                    // If this glyph is on another page, its UnscaledImage
                    // may never have been created yet.
                    if (glyph.UnscaledImage != null)
                    {
                        glyphBitmap = glyph.UnscaledImage as Bitmap;
                    }

                    if (glyphBitmap == null)
                    {
                        CFontUtils.CreateBitmap(
                            glyph,
                            out glyphBitmap);

                        temporaryBitmap = true;
                    }

                    if (glyphBitmap == null)
                    {
                        bmp.Dispose();
                        return CreateRenderErrorImage();
                    }

                    g.DrawImageUnscaled(
                        glyphBitmap,
                        xpos,
                        0);

                    xpos += glyph.Width;

                    // Don't keep temporary render-only bitmaps around.
                    if (temporaryBitmap)
                        glyphBitmap.Dispose();
                }
            }

            // Scale the rendered text to 200% using nearest neighbor.
            Bitmap scaledBmp = new Bitmap(bmp.Width * 2 + 2, bmp.Height * 2 + 2);

            using (Graphics g = Graphics.FromImage(scaledBmp))
            {
                g.InterpolationMode =
                    System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                g.PixelOffsetMode =
                    System.Drawing.Drawing2D.PixelOffsetMode.Half;

                // this is the rendered text border color
                g.Clear(Color.FromArgb(64, 64, 64));

                g.DrawImage(
                    bmp,
                    new Rectangle(1, 1, bmp.Width * 2, bmp.Height * 2),
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    GraphicsUnit.Pixel);
            }

            bmp.Dispose();

            return scaledBmp;
        }

        private Image CreateRenderErrorImage()
        {
            Bitmap bmp = new Bitmap(600, 20);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawString(
                    "Some glyphs are missing! Can't render text.",
                    new System.Drawing.Font("Arial", 12),
                    _darkMode ? Brushes.White : Brushes.Black,
                    2,
                    2);
            }

            return bmp;
        }

        private void FlowCharacterPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Focus the glyph panel without changing selection
                // or focusing an individual glyph.
                FlowCharacterPanel.Focus();
            }
            else if (e.Button == MouseButtons.Right)
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    cd.Color = FlowCharacterPanel.BackColor;

                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        FlowCharacterPanel.BackColor = cd.Color;
                        RefreshPlaceholders();

                        XmlSettings.Color = cd.Color;
                        XmlSettings.Write();
                    }
                }
            }
        }
        private void PictSwap_Click(object sender, EventArgs e)
        {
            Color temp = PanelRightMouse.BackColor;
            PanelRightMouse.BackColor = PanelLeftMouse.BackColor;
            PanelLeftMouse.BackColor = temp;
            grayscaleMouseButtonsSwapped = !grayscaleMouseButtonsSwapped;
        }

        #region Character functions

        private void Shift4BitCharacter(CCharInfo character, int dx, int dy)
        {
            if (character == null ||
                character.Width == 0 ||
                character.Height == 0 ||
                character.ByteLines == null)
                return;

            int width = character.Width;
            int height = character.Height;
            int stride = (width + 1) / 2;

            byte[] source = character.ByteLines;
            byte[] result = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sourceOffset = y * stride + x / 2;

                    int shade = (x & 1) == 0
                        ? (source[sourceOffset] >> 4) & 0x0F
                        : source[sourceOffset] & 0x0F;

                    int targetX = (x + dx + width) % width;
                    int targetY = (y + dy + height) % height;

                    int targetOffset = targetY * stride + targetX / 2;

                    if ((targetX & 1) == 0)
                        result[targetOffset] |= (byte)(shade << 4);
                    else
                        result[targetOffset] |= (byte)shade;
                }
            }

            character.UndoRedoListTidyUp();
            character.ByteLines = result;
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            RefreshCharacterPreview(
                CharacterPictureList[character.Index - PageStart],
                character);

            if (character.Index == Index)
                RefreshDrawingAreaDisplay(character);

            CheckChange();
        }
        private void ShiftUpCharacter(CCharInfo character)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                Shift4BitCharacter(character, 0, -1);
                return;
            }

            // Ignore placeholder glyphs
            if (character.Width == 0 ||
                character.Height == 0 ||
                character.UnscaledImage == null)
            {
                return;
            }

            //CCharInfo character = ((CCharInfo)(((PictureBox)CharacterPictureList[Index - PageStart]).Tag));
            Bitmap Selected;

            if (null != (Selected = (Bitmap)(character.UnscaledImage)))
            {
                BitmapData bmpData = Selected.LockBits(new Rectangle(0, 0, Selected.Width, Selected.Height), ImageLockMode.ReadWrite, Selected.PixelFormat);
                IntPtr ptrbegin = bmpData.Scan0;
                IntPtr ptrrest = bmpData.Scan0;
                ptrrest = (IntPtr)((int)ptrrest + bmpData.Stride);
                byte[] shiftarray = new byte[bmpData.Stride * Selected.Height];

                System.Runtime.InteropServices.Marshal.Copy(ptrrest, shiftarray, 0, bmpData.Stride * (Selected.Height - 1));
                System.Runtime.InteropServices.Marshal.Copy(ptrbegin, shiftarray, bmpData.Stride * (Selected.Height - 1), bmpData.Stride);

                ptrbegin = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(shiftarray, 0, ptrbegin, bmpData.Stride * Selected.Height);
                Selected.UnlockBits(bmpData);

                CFontUtils.SaveByteLinesFromPicture((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag), Selected);
                ((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag)).UnscaledImage = Selected;
            }

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[0].Enabled = character.UndoPossible;
            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }
        private void ShiftDownCharacter(CCharInfo character)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                Shift4BitCharacter(character, 0, 1);
                return;
            }

            // Ignore placeholder glyphs
            if (character.Width == 0 ||
                character.Height == 0 ||
                character.UnscaledImage == null)
            {
                return;
            }

            //CCharInfo character = ((CCharInfo)(((PictureBox)CharacterPictureList[Index - PageStart]).Tag));
            Bitmap Selected;

            if (null != (Selected = (Bitmap)(character.UnscaledImage)))
            {
                BitmapData bmpData = Selected.LockBits(new Rectangle(0, 0, Selected.Width, Selected.Height), ImageLockMode.ReadWrite, Selected.PixelFormat);
                IntPtr ptrbegin = bmpData.Scan0;
                IntPtr ptrrest = bmpData.Scan0;
                ptrrest = (IntPtr)((int)ptrrest + bmpData.Stride * (Selected.Height - 1));
                byte[] shiftarray = new byte[bmpData.Stride * Selected.Height];

                System.Runtime.InteropServices.Marshal.Copy(ptrbegin, shiftarray, bmpData.Stride, bmpData.Stride * (Selected.Height - 1));
                System.Runtime.InteropServices.Marshal.Copy(ptrrest, shiftarray, 0, bmpData.Stride);

                ptrbegin = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(shiftarray, 0, ptrbegin, bmpData.Stride * Selected.Height);
                Selected.UnlockBits(bmpData);

                CFontUtils.SaveByteLinesFromPicture((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag), Selected);
                ((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag)).UnscaledImage = Selected;
            }

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[0].Enabled = character.UndoPossible;
            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }
        private void ShiftLeftCharacter(CCharInfo character)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                Shift4BitCharacter(character, -1, 0);
                return;
            }

            // Ignore placeholder glyphs
            if (character.Width == 0 ||
                character.Height == 0 ||
                character.UnscaledImage == null)
            {
                return;
            }

            //CCharInfo character = ((CCharInfo)(((PictureBox)CharacterPictureList[Index - PageStart]).Tag));
            Bitmap Selected;

            if (null != (Selected = (Bitmap)(character.UnscaledImage)))
            {
                BitmapData bmpData = Selected.LockBits(new Rectangle(0, 0, Selected.Width, Selected.Height), ImageLockMode.ReadWrite, Selected.PixelFormat);
                IntPtr ptrbegin = bmpData.Scan0;
                IntPtr ptrwrite = bmpData.Scan0;
                byte[] linearray = new byte[bmpData.Stride];
                ptrbegin = bmpData.Scan0;
                UInt32 line;
                UInt32 overflow;

                for (int cnt = 0; cnt < Selected.Height; cnt++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(ptrbegin, linearray, 0, bmpData.Stride);
                    Array.Reverse(linearray);
                    line = BitConverter.ToUInt32(linearray, 0);

                    overflow = (line & 0x80000000) >> (Selected.Width - 1);
                    line <<= 1;
                    line += overflow;

                    linearray = BitConverter.GetBytes(line);
                    Array.Reverse(linearray);
                    System.Runtime.InteropServices.Marshal.Copy(linearray, 0, ptrwrite, bmpData.Stride);

                    ptrbegin = (IntPtr)(((int)ptrbegin + bmpData.Stride));
                    ptrwrite = (IntPtr)(((int)ptrwrite + bmpData.Stride));
                }

                Selected.UnlockBits(bmpData);

                CFontUtils.SaveByteLinesFromPicture((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag), Selected);
                ((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag)).UnscaledImage = Selected;
            }

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[0].Enabled = character.UndoPossible;
            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }
        private void ShiftRightCharacter(CCharInfo character)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                Shift4BitCharacter(character, 1, 0);
                return;
            }

            // Ignore placeholder glyphs
            if (character.Width == 0 ||
                character.Height == 0 ||
                character.UnscaledImage == null)
            {
                return;
            }

            //CCharInfo character = ((CCharInfo)(((PictureBox)CharacterPictureList[Index - PageStart]).Tag));
            Bitmap Selected;

            if (null != (Selected = (Bitmap)(character.UnscaledImage)))
            {
                BitmapData bmpData = Selected.LockBits(new Rectangle(0, 0, Selected.Width, Selected.Height), ImageLockMode.ReadWrite, Selected.PixelFormat);
                IntPtr ptrbegin = bmpData.Scan0;
                IntPtr ptrwrite = bmpData.Scan0;
                byte[] linearray = new byte[bmpData.Stride];
                ptrbegin = bmpData.Scan0;
                UInt32 line;
                UInt32 underflow;

                for (int cnt = 0; cnt < Selected.Height; cnt++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(ptrbegin, linearray, 0, bmpData.Stride);
                    Array.Reverse(linearray);
                    line = BitConverter.ToUInt32(linearray, 0);

                    underflow = line << (Selected.Width - 1);
                    line >>= 1;
                    line += underflow;

                    linearray = BitConverter.GetBytes(line);
                    Array.Reverse(linearray);
                    System.Runtime.InteropServices.Marshal.Copy(linearray, 0, ptrwrite, bmpData.Stride);

                    ptrbegin = (IntPtr)(((int)ptrbegin + bmpData.Stride));
                    ptrwrite = (IntPtr)(((int)ptrwrite + bmpData.Stride));
                }

                Selected.UnlockBits(bmpData);

                CFontUtils.SaveByteLinesFromPicture((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag), Selected);
                ((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag)).UnscaledImage = Selected;
            }

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[0].Enabled = character.UndoPossible;
            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }
        private void InvertCharacter(CCharInfo character)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                if (character.Width == 0 ||
                    character.Height == 0 ||
                    character.ByteLines == null)
                    return;

                int width = character.Width;
                int height = character.Height;
                int stride = (width + 1) / 2;

                byte[] inverted4Bit = new byte[stride * height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * stride + x / 2;

                        int shade = (x & 1) == 0
                            ? (character.ByteLines[offset] >> 4) & 0x0F
                            : character.ByteLines[offset] & 0x0F;

                        int invertedShade = 15 - shade;

                        if ((x & 1) == 0)
                            inverted4Bit[offset] |= (byte)(invertedShade << 4);
                        else
                            inverted4Bit[offset] |= (byte)invertedShade;
                    }
                }

                character.UndoRedoListTidyUp();
                character.ByteLines = inverted4Bit;
                character.UndoRedoListAdd(character.ByteLines);

                RegisterGlyphUndoOperation(character);

                int localIndex = character.Index - PageStart;

                if (localIndex >= 0 &&
                    localIndex < CharacterPictureList.Count)
                {
                    RefreshCharacterPreview(
                        CharacterPictureList[localIndex],
                        character);
                }

                if (character.Index == Index)
                    RefreshDrawingAreaDisplay(character);

                CheckChange();
                return;
            }

            if (character.UnscaledImage == null)
                return;

            //CCharInfo character = ((CCharInfo)(((PictureBox)CharacterPictureList[Index - PageStart]).Tag));
            Bitmap Selected;

            if (null != (Selected = (Bitmap)(character.UnscaledImage)))
            {
                BitmapData bmpData = Selected.LockBits(new Rectangle(0, 0, Selected.Width, Selected.Height), ImageLockMode.ReadWrite, Selected.PixelFormat);
                IntPtr ptrbegin = bmpData.Scan0;
                IntPtr ptrwrite = bmpData.Scan0;
                byte[] linearray = new byte[bmpData.Stride];
                UInt32 line;

                for (int cnt = 0; cnt < Selected.Height; cnt++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(ptrbegin, linearray, 0, bmpData.Stride);
                    Array.Reverse(linearray);
                    line = BitConverter.ToUInt32(linearray, 0);
                    line = ~line;

                    linearray = BitConverter.GetBytes(line);
                    Array.Reverse(linearray);
                    System.Runtime.InteropServices.Marshal.Copy(linearray, 0, ptrwrite, bmpData.Stride);

                    ptrbegin = (IntPtr)(((int)ptrbegin + bmpData.Stride));
                    ptrwrite = (IntPtr)(((int)ptrwrite + bmpData.Stride));
                }

                Selected.UnlockBits(bmpData);

                CFontUtils.SaveByteLinesFromPicture((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag), Selected);
                ((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag)).UnscaledImage = Selected;
            }

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[0].Enabled = character.UndoPossible;
            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }

        private void Flip4BitVerticalCharacter(CCharInfo character)
        {
            int width = character.Width;
            int height = character.Height;

            // Never modify zero-sized placeholder glyphs.
            if (width == 0 || height == 0 || character.ByteLines == null)
                return;

            int stride = (width + 1) / 2;
            byte[] source = character.ByteLines;
            byte[] flipped = new byte[stride * height];

            // Reverse the order of the glyph's rows.
            for (int y = 0; y < height; y++)
            {
                Buffer.BlockCopy(
                    source,
                    (height - 1 - y) * stride,
                    flipped,
                    y * stride,
                    stride);
            }

            character.ByteLines = flipped;

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            int localIndex = character.Index - PageStart;

            if (localIndex >= 0 &&
                localIndex < CharacterPictureList.Count)
            {
                RefreshCharacterPreview(
                    CharacterPictureList[localIndex], character);
            }

            if (character.Index == Index)
                RefreshDrawingAreaDisplay(character);

            CheckChange();
        }

        private void SwapHorizontallyCharacter(CCharInfo character)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                Flip4BitVerticalCharacter(character);
                return;
            }

            if (character.UnscaledImage == null)
                return;

            //CCharInfo character = ((CCharInfo)(((PictureBox)CharacterPictureList[Index - PageStart]).Tag));
            Bitmap Selected;

            if (null != (Selected = (Bitmap)(character.UnscaledImage)))
            {
                BitmapData bmpData = Selected.LockBits(new Rectangle(0, 0, Selected.Width, Selected.Height), ImageLockMode.ReadWrite, Selected.PixelFormat);
                IntPtr ptrbegin = bmpData.Scan0;
                byte[][] linearray = new byte[Selected.Height][];

                for (int cnt = 0; cnt < Selected.Height; cnt++)
                {
                    linearray[cnt] = new byte[bmpData.Stride];
                    System.Runtime.InteropServices.Marshal.Copy(ptrbegin, linearray[cnt], 0, bmpData.Stride);
                    ptrbegin = (IntPtr)(((int)ptrbegin + bmpData.Stride));
                }

                ptrbegin = bmpData.Scan0;

                for (int cnt2 = 0; cnt2 < Selected.Height; cnt2++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(linearray[Selected.Height - 1 - cnt2], 0, ptrbegin, bmpData.Stride);

                    ptrbegin = (IntPtr)(((int)ptrbegin + bmpData.Stride));
                }

                Selected.UnlockBits(bmpData);

                CFontUtils.SaveByteLinesFromPicture((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag), Selected);
                ((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag)).UnscaledImage = Selected;
            }

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[0].Enabled = character.UndoPossible;
            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }

        private void Flip4BitHorizontalCharacter(CCharInfo character)
        {
            int width = character.Width;
            int height = character.Height;

            // Do not modify empty placeholder glyphs.
            if (width == 0 || height == 0 || character.ByteLines == null)
                return;

            int stride = (width + 1) / 2;
            byte[] source = character.ByteLines;
            byte[] flipped = new byte[stride * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int sourceX = width - 1 - x;
                    int sourceOffset = y * stride + sourceX / 2;

                    int shade = (sourceX & 1) == 0
                        ? (source[sourceOffset] >> 4) & 0x0F
                        : source[sourceOffset] & 0x0F;

                    int targetOffset = y * stride + x / 2;

                    if ((x & 1) == 0)
                        flipped[targetOffset] |= (byte)(shade << 4);
                    else
                        flipped[targetOffset] |= (byte)shade;
                }
            }

            character.ByteLines = flipped;

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            int localIndex = character.Index - PageStart;

            if (localIndex >= 0 &&
                localIndex < CharacterPictureList.Count)
            {
                RefreshCharacterPreview(
                    CharacterPictureList[localIndex], character);
            }

            if (character.Index == Index)
                RefreshDrawingAreaDisplay(character);

            CheckChange();
        }

        private void SwapVerticallyCharacter(CCharInfo character)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                Flip4BitHorizontalCharacter(character);
                return;
            }

            if (character.UnscaledImage == null)
                return;

            //CCharInfo character = ((CCharInfo)(((PictureBox)CharacterPictureList[Index - PageStart]).Tag));
            Bitmap Selected;

            if (null != (Selected = (Bitmap)(character.UnscaledImage)))
            {
                BitmapData bmpData = Selected.LockBits(new Rectangle(0, 0, Selected.Width, Selected.Height), ImageLockMode.ReadWrite, Selected.PixelFormat);
                IntPtr ptrbegin = bmpData.Scan0;
                IntPtr ptrwrite = bmpData.Scan0;
                byte[] linearray = new byte[bmpData.Stride];
                UInt32 line;

                for (int cnt = 0; cnt < Selected.Height; cnt++)
                {
                    System.Runtime.InteropServices.Marshal.Copy(ptrbegin, linearray, 0, bmpData.Stride);
                    Array.Reverse(linearray);
                    line = BitConverter.ToUInt32(linearray, 0);

                    UInt32 temp = 0;
                    UInt32 temp2 = 0;

                    for (int cnti = 0; cnti < 32; cnti++)
                    {
                        temp2 = line >> (32 - (cnti + 1));
                        temp |= (temp2 & 1) << cnti;
                    }

                    line = temp << (32 - Selected.Width);

                    linearray = BitConverter.GetBytes(line);
                    Array.Reverse(linearray);
                    System.Runtime.InteropServices.Marshal.Copy(linearray, 0, ptrwrite, bmpData.Stride);

                    ptrbegin = (IntPtr)(((int)ptrbegin + bmpData.Stride));
                    ptrwrite = (IntPtr)(((int)ptrwrite + bmpData.Stride));
                }

                Selected.UnlockBits(bmpData);

                CFontUtils.SaveByteLinesFromPicture((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag), Selected);
                ((CCharInfo)(CharacterPictureList[character.Index - PageStart].Tag)).UnscaledImage = Selected;
            }

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[0].Enabled = character.UndoPossible;
            CharacterPictureList[character.Index - PageStart].ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }
        private void ClearCharacter(CCharInfo character)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                if (character.Width == 0 || character.Height == 0)
                {
                    character.Width = PLACEHOLDER_W;
                    character.Height = PLACEHOLDER_H;
                }

                int stride = (character.Width + 1) / 2;

                character.ByteLines = new byte[stride * character.Height];

                character.UndoRedoListTidyUp();
                character.UndoRedoListAdd(character.ByteLines);

                RegisterGlyphUndoOperation(character);

                int localIndex = character.Index - PageStart;

                if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                {
                    RefreshCharacterPreview(
                        CharacterPictureList[localIndex],
                        character);
                }

                if (character.Index == Index)
                    RefreshDrawingAreaDisplay(character);

                CheckChange();
                return;
            }

            // 🟢 If placeholder, materialize it first
            if (character.Width == 0 || character.Height == 0 || character.UnscaledImage == null)
            {
                character.Width = PLACEHOLDER_W;
                character.Height = PLACEHOLDER_H;
                character.WidthOriginal = PLACEHOLDER_W;
                character.HeightOriginal = PLACEHOLDER_H;

                int bytesPerLine = (character.Width - 1) / 8 + 1;
                character.ByteLines = new byte[character.Height * bytesPerLine];
            }

            // Ensure ByteLines exists
            if (character.ByteLines == null)
            {
                int bytesPerLine = (character.Width - 1) / 8 + 1;
                character.ByteLines = new byte[character.Height * bytesPerLine];
            }

            // Create a new cleared bitmap buffer instead of modifying
            // the existing ByteLines array in place. This preserves
            // the previous array stored in the Undo history.
            character.ByteLines = new byte[character.ByteLines.Length];

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart]
                .ContextMenu.MenuItems[0].Enabled = character.UndoPossible;

            CharacterPictureList[character.Index - PageStart]
                .ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }

        private void FillCharacter(CCharInfo character)
        {
            // 4-bit WFN grayscale fill
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                if (character.Width == 0 || character.Height == 0)
                {
                    character.Width = PLACEHOLDER_W;
                    character.Height = PLACEHOLDER_H;
                }

                int width = character.Width;
                int height = character.Height;
                int stride = (width + 1) / 2;

                byte[] filled4Bit = new byte[stride * height];
                byte shade = (byte)(selectedGrayscaleShade & 0x0F);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * stride + x / 2;

                        if ((x & 1) == 0)
                            filled4Bit[offset] |= (byte)(shade << 4);
                        else
                            filled4Bit[offset] |= shade;
                    }
                }

                character.ByteLines = filled4Bit;

                character.UndoRedoListTidyUp();
                character.UndoRedoListAdd(character.ByteLines);

                RegisterGlyphUndoOperation(character);

                int localIndex = character.Index - PageStart;

                if (localIndex >= 0 &&
                    localIndex < CharacterPictureList.Count)
                {
                    RefreshCharacterPreview(
                        CharacterPictureList[localIndex],
                        character);
                }

                if (character.Index == Index)
                    RefreshDrawingAreaDisplay(character);

                CheckChange();
                return;
            }

            // Original 1-bit Fill behavior
            // If placeholder -> materialize first
            if (character.Width == 0 || character.Height == 0 ||
                character.UnscaledImage == null)
            {
                character.Width = PLACEHOLDER_W;
                character.Height = PLACEHOLDER_H;
                character.WidthOriginal = PLACEHOLDER_W;
                character.HeightOriginal = PLACEHOLDER_H;

                int bytesPerLine = (character.Width - 1) / 8 + 1;
                character.ByteLines = new byte[character.Height * bytesPerLine];

                Bitmap newBmp;
                CFontUtils.CreateBitmap(character, out newBmp);
                character.UnscaledImage = newBmp;
            }

            // Now safe to fill
            if (character.ByteLines == null)
            {
                int bytesPerLine = (character.Width - 1) / 8 + 1;
                character.ByteLines = new byte[character.Height * bytesPerLine];
            }

            byte[] filled = new byte[character.ByteLines.Length];

            for (int i = 0; i < filled.Length; i++)
            {
                filled[i] = 0xFF;
            }

            character.ByteLines = filled;

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            CharacterPictureList[character.Index - PageStart]
                .ContextMenu.MenuItems[0].Enabled = character.UndoPossible;

            CharacterPictureList[character.Index - PageStart]
                .ContextMenu.MenuItems[1].Enabled = character.RedoPossible;

            CreateAndShow(character, SizeMode.ChangeNothing);
            CheckChange();
        }

        private void Outline4BitCharacter(CCharInfo character)
        {
            int width = character.Width;
            int height = character.Height;

            if (width == 0 || height == 0 || character.ByteLines == null)
                return;

            int stride = (width + 1) / 2;
            byte[] source = character.ByteLines;
            byte[] outlined = (byte[])source.Clone();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x / 2;
                    int shade = ((x & 1) == 0)
                        ? (source[offset] >> 4) & 0x0F
                        : source[offset] & 0x0F;

                    // Remove the original glyph pixels.
                    if (shade != 0)
                    {
                        if ((x & 1) == 0)
                            outlined[offset] &= 0x0F;
                        else
                            outlined[offset] &= 0xF0;

                        continue;
                    }

                    bool hasNeighbor = false;

                    // Check all eight neighboring pixels.
                    for (int dy = -1; dy <= 1 && !hasNeighbor; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx < 0 || nx >= width ||
                                ny < 0 || ny >= height)
                                continue;

                            int neighborOffset = ny * stride + nx / 2;
                            int neighborShade = ((nx & 1) == 0)
                                ? (source[neighborOffset] >> 4) & 0x0F
                                : source[neighborOffset] & 0x0F;

                            if (neighborShade != 0)
                            {
                                hasNeighbor = true;
                                break;
                            }
                        }
                    }

                    if (hasNeighbor)
                    {
                        if ((x & 1) == 0)
                            outlined[offset] =
                                (byte)((outlined[offset] & 0x0F) | 0xF0);
                        else
                            outlined[offset] =
                                (byte)((outlined[offset] & 0xF0) | 0x0F);
                    }
                }
            }

            if (ArraysEqual(source, outlined))
                return;

            character.ByteLines = outlined;

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            int localIndex = character.Index - PageStart;

            if (localIndex >= 0 &&
                localIndex < CharacterPictureList.Count)
            {
                RefreshCharacterPreview(
                    CharacterPictureList[localIndex], character);
            }

            if (character.Index == Index)
                RefreshDrawingAreaDisplay(character);

            CheckChange();
        }

        private void OutlineCharacter(CCharInfo character)
        {
            if (character == null)
                return;

            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
            {
                Outline4BitCharacter(character);
                return;
            }

            int width = character.Width;
            int height = character.Height;
            byte[] source = character.ByteLines;

            // Ignore placeholders and invalid glyph data.
            if (width <= 0 || height <= 0 || source == null)
                return;

            int stride = (width + 7) / 8;

            if (source.Length < stride * height)
                return;

            // Determine whether the glyph contains both empty and filled pixels.
            bool hasEmpty = false;
            bool hasFilled = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x / 8;
                    int mask = 0x80 >> (x & 7);

                    if ((source[offset] & mask) != 0)
                        hasFilled = true;
                    else
                        hasEmpty = true;
                }
            }

            // Ignore completely black or completely white glyphs.
            if (!hasEmpty || !hasFilled)
                return;

            // Build the outline using the original glyph pixels.
            byte[] outlined = new byte[source.Length];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x / 8;
                    int mask = 0x80 >> (x & 7);

                    // Original filled pixels are removed from the result.
                    if ((source[offset] & mask) != 0)
                        continue;

                    bool hasNeighbor = false;

                    // Check all eight surrounding pixels.
                    for (int dy = -1; dy <= 1 && !hasNeighbor; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx < 0 || nx >= width ||
                                ny < 0 || ny >= height)
                                continue;

                            int neighborOffset = ny * stride + nx / 8;
                            int neighborMask = 0x80 >> (nx & 7);

                            if ((source[neighborOffset] & neighborMask) != 0)
                            {
                                hasNeighbor = true;
                                break;
                            }
                        }
                    }

                    if (hasNeighbor)
                        outlined[offset] |= (byte)mask;
                }
            }

            if (ArraysEqual(source, outlined))
                return;

            character.ByteLines = outlined;

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            RegisterGlyphUndoOperation(character);

            int localIndex = character.Index - PageStart;

            if (localIndex >= 0 &&
                localIndex < CharacterPictureList.Count)
            {
                RefreshCharacterPreview(
                    CharacterPictureList[localIndex], character);
            }

            if (character.Index == Index)
                RefreshDrawingAreaDisplay(character);

            CheckChange();
        }

        private void CharacterFunctionOneOrAll(Action<CCharInfo> function, bool onechar)
        {
            CCharInfo character;

            if (onechar)
            {
                foreach (PictureBox item in CharacterPictureList)
                {
                    character = ((CCharInfo)(item.Tag));
                    function(character);
                }
            }
            else
            {
                character = ((CCharInfo)(((PictureBox)CharacterPictureList[Index - PageStart]).Tag));
                function(character);
            }
        }
        #endregion Character functions


        private void CharacterFunctionAllAsGrouped(Action<CCharInfo> function)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit &&
                function.Method != ((Action<CCharInfo>)ShiftUpCharacter).Method &&
                function.Method != ((Action<CCharInfo>)ShiftDownCharacter).Method &&
                function.Method != ((Action<CCharInfo>)ShiftLeftCharacter).Method &&
                function.Method != ((Action<CCharInfo>)ShiftRightCharacter).Method &&
                function.Method != ((Action<CCharInfo>)ClearCharacter).Method &&
                function.Method != ((Action<CCharInfo>)FillCharacter).Method &&
                function.Method != ((Action<CCharInfo>)InvertCharacter).Method &&
                function.Method != ((Action<CCharInfo>)SwapHorizontallyCharacter).Method &&
                function.Method != ((Action<CCharInfo>)SwapVerticallyCharacter).Method &&
                function.Method != ((Action<CCharInfo>)OutlineCharacter).Method &&
                function.Method != ((Action<CCharInfo>)IncreaseCharacterShade).Method &&
                function.Method != ((Action<CCharInfo>)DecreaseCharacterShade).Method &&
                function.Method != ((Action<CCharInfo>)ReplaceCharacterShade).Method)
                return;

            List<int> changedIndexes = new List<int>();

            _recordingGroupedOperation = true;

            try
            {
                foreach (PictureBox item in CharacterPictureList)
                {
                    CCharInfo character = (CCharInfo)item.Tag;

                UInt16 oldWidth = character.Width;
                UInt16 oldHeight = character.Height;

                byte[] oldBytes = null;
                if (character.ByteLines != null)
                    oldBytes = (byte[])character.ByteLines.Clone();

                function(character);

                bool changed =
                    oldWidth != character.Width ||
                    oldHeight != character.Height ||
                    !ArraysEqual(oldBytes, character.ByteLines);

                if (changed)
                    changedIndexes.Add(character.Index);
                }
            }
            finally
            {
                _recordingGroupedOperation = false;
            }

            if (changedIndexes.Count > 0)
            {
                _multiUndoStack.Push(changedIndexes);

                _operationUndoStack.Push(new UndoOperation
                {
                    Type = UndoOperationType.MultiGlyphEdit,

                    SelectionAfterEdit = _selectedPreviews
                        .Select(p => ((CCharInfo)p.Tag).Index)
                        .ToList(),

                    ActiveIndexBeforeEdit = Index
                });

                _multiRedoStack.Clear();
                _structuralRedoStack.Clear();
                _operationRedoStack.Clear();
            }

            if (_selectedPreview != null)
            {
                CCharInfo activeCharacter =
                    (CCharInfo)_selectedPreview.Tag;

                RefreshDrawingAreaDisplay(activeCharacter);
            }

            FlowCharacterPanel.Invalidate();
        }

        private void CharacterFunctionSelectionOrOneOrAll(
            Action<CCharInfo> function,
            bool onechar)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit &&
                function.Method != ((Action<CCharInfo>)ShiftUpCharacter).Method &&
                function.Method != ((Action<CCharInfo>)ShiftDownCharacter).Method &&
                function.Method != ((Action<CCharInfo>)ShiftLeftCharacter).Method &&
                function.Method != ((Action<CCharInfo>)ShiftRightCharacter).Method &&
                function.Method != ((Action<CCharInfo>)ClearCharacter).Method &&
                function.Method != ((Action<CCharInfo>)FillCharacter).Method &&
                function.Method != ((Action<CCharInfo>)InvertCharacter).Method &&
                function.Method != ((Action<CCharInfo>)SwapHorizontallyCharacter).Method &&
                function.Method != ((Action<CCharInfo>)SwapVerticallyCharacter).Method &&
                function.Method != ((Action<CCharInfo>)OutlineCharacter).Method &&
                function.Method != ((Action<CCharInfo>)IncreaseCharacterShade).Method &&
                function.Method != ((Action<CCharInfo>)DecreaseCharacterShade).Method &&
                function.Method != ((Action<CCharInfo>)ReplaceCharacterShade).Method)
                return;

            // "Use on all" always takes priority over multiple selection.
            if (onechar)
            {
                CharacterFunctionAllAsGrouped(function);
                return;
            }

            if (_selectedPreviews.Count > 1)
            {
                PictureBox activePreview = _selectedPreview;

                List<PictureBox> selected =
                    new List<PictureBox>(_selectedPreviews);

                List<int> changedIndexes = new List<int>();

                _recordingGroupedOperation = true;

                try
                {
                    foreach (PictureBox item in selected)
                    {
                        CCharInfo character = (CCharInfo)item.Tag;

                        // Remember the glyph state before the operation.
                        ushort oldWidth = character.Width;
                        ushort oldHeight = character.Height;

                        byte[] before = null;

                        if (character.ByteLines != null)
                            before = (byte[])character.ByteLines.Clone();

                        function(character);

                        // Only remember glyphs that were genuinely changed.
                        if (oldWidth != character.Width ||
                            oldHeight != character.Height ||
                            !ArraysEqual(before, character.ByteLines))
                        {
                            changedIndexes.Add(character.Index);
                        }
                    }
                }
                finally
                {
                    _recordingGroupedOperation = false;
                }

                // Remember this group independently of the visual selection.
                if (changedIndexes.Count > 0)
                {
                    _multiUndoStack.Push(changedIndexes);

                    _operationUndoStack.Push(new UndoOperation
                    {
                        Type = UndoOperationType.MultiGlyphEdit,

                        SelectionAfterEdit = selected
                            .Select(p => ((CCharInfo)p.Tag).Index)
                            .ToList(),

                        ActiveIndexBeforeEdit = Index
                    });

                    // A new edit invalidates anything that had previously been undone.
                    _multiRedoStack.Clear();
                    _structuralRedoStack.Clear();
                    _operationRedoStack.Clear();
                }

                // Refresh the active glyph without resetting multiple selection.
                if (activePreview != null)
                {
                    CCharInfo activeCharacter =
                        (CCharInfo)activePreview.Tag;

                    RefreshDrawingAreaDisplay(activeCharacter);
                }

                FlowCharacterPanel.Invalidate();
                return;
            }

            // Process a single glyph.
            CharacterFunctionOneOrAll(function, false);
        }


        private void BtnClear_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(
                ClearCharacter,
                ChkOneAllCharacters.Checked);
        }

        private void BtnFill_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(
                FillCharacter,
                ChkOneAllCharacters.Checked);
        }

        private void BtnSwapHorizontally_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(
                SwapHorizontallyCharacter,
                ChkOneAllCharacters.Checked);
        }

        private void BtnSwapVertically_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(
                SwapVerticallyCharacter,
                ChkOneAllCharacters.Checked);
        }

        private void BtnShiftUp_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(
                ShiftUpCharacter,
                ChkOneAllCharacters.Checked);
        }

        private void BtnShiftDown_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(
                ShiftDownCharacter,
                ChkOneAllCharacters.Checked);
        }

        private void BtnShiftLeft_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(
                ShiftLeftCharacter,
                ChkOneAllCharacters.Checked);
        }

        private void BtnShiftRight_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(
                ShiftRightCharacter,
                ChkOneAllCharacters.Checked);
        }

        private void AdjustCharacterShade(CCharInfo character, int direction)
        {
            if (!(FontInfo is CWFNFontInfo wfn) || !wfn.Is4Bit ||
                character == null || character.ByteLines == null ||
                character.Width == 0 || character.Height == 0)
                return;

            int width = character.Width;
            int height = character.Height;
            int stride = (width + 1) / 2;

            byte[] adjusted = (byte[])character.ByteLines.Clone();
            bool changed = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x / 2;

                    int shade = (x & 1) == 0
                        ? (adjusted[offset] >> 4) & 0x0F
                        : adjusted[offset] & 0x0F;

                    if (shade == 0)
                        continue;

                    int newShade = Math.Max(1, Math.Min(15, shade + direction));

                    if (newShade == shade)
                        continue;

                    changed = true;

                    if ((x & 1) == 0)
                        adjusted[offset] =
                            (byte)((adjusted[offset] & 0x0F) | (newShade << 4));
                    else
                        adjusted[offset] =
                            (byte)((adjusted[offset] & 0xF0) | newShade);
                }
            }

            if (!changed)
                return;

            character.UndoRedoListTidyUp();
            character.UndoRedoListAdd(character.ByteLines);

            character.ByteLines = adjusted;

            character.UndoRedoListAdd(character.ByteLines);
            RegisterGlyphUndoOperation(character);

            int localIndex = character.Index - PageStart;

            if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                RefreshCharacterPreview(CharacterPictureList[localIndex], character);

            if (character.Index == Index)
                RefreshDrawingAreaDisplay(character);

            CheckChange();
        }

        private void IncreaseCharacterShade(CCharInfo character)
        {
            AdjustCharacterShade(character, 1);
        }

        private void DecreaseCharacterShade(CCharInfo character)
        {
            AdjustCharacterShade(character, -1);
        }

        private void IncreaseSelectedShades()
        {
            CharacterFunctionSelectionOrOneOrAll(
                IncreaseCharacterShade,
                ChkOneAllCharacters.Checked);
        }

        private void DecreaseSelectedShades()
        {
            CharacterFunctionSelectionOrOneOrAll(
                DecreaseCharacterShade,
                ChkOneAllCharacters.Checked);
        }

        private void BtnInvert_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(InvertCharacter, ChkOneAllCharacters.Checked);
        }


        private void BtnOutline_Click(object sender, EventArgs e)
        {
            CharacterFunctionSelectionOrOneOrAll(OutlineCharacter, ChkOneAllCharacters.Checked);
        }
        private void BtnOutlineFont_Click(object sender, EventArgs e)
        {
            CharacterFunctionAllAsGrouped(OutlineCharacter);
        }

        private void RdoRenderANSI_CheckedChanged(object sender, EventArgs e)
        {
            if (!RdoRenderANSI.Checked)
                return;

            XmlSettings.RenderUnicode = false;
            XmlSettings.Write();

            if (!string.IsNullOrEmpty(XmlSettings.CustomText))
                PictRenderText.Image = RenderTextLine(XmlSettings.CustomText);
        }

        private void RdoRenderUnicode_CheckedChanged(object sender, EventArgs e)
        {
            if (!RdoRenderUnicode.Checked)
                return;

            XmlSettings.RenderUnicode = true;
            XmlSettings.Write();

            if (!string.IsNullOrEmpty(XmlSettings.CustomText))
                PictRenderText.Image = RenderTextLine(XmlSettings.CustomText);
        }

        private void BtnRenderText_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(XmlSettings.CustomText))
            {
                string newValue = "The quick brown Fox jumped over the lazy Dog.";

                if (Settings.InputBox(
                    "Set Text to Render",
                    "Set a text string to render:",
                    ref newValue,
                    true) != DialogResult.OK)
                {
                    return;
                }

                XmlSettings.CustomText = newValue;
                XmlSettings.Write();

                if (string.IsNullOrEmpty(newValue))
                    return;
            }

            PictRenderText.Image = RenderTextLine(XmlSettings.CustomText);
        }
        private void BtnSetText_Click(object sender, EventArgs e)
        {
            string newValue = string.IsNullOrEmpty(XmlSettings.CustomText)
                ? "The quick brown Fox jumped over the lazy Dog."
                : XmlSettings.CustomText;

            DialogResult dialogResult = Settings.InputBox(
                "Set Text to Render",
                "Set a text string to render:",
                ref newValue,
                true);

            if (dialogResult == DialogResult.OK)
            {
                XmlSettings.CustomText = newValue ?? "";
                XmlSettings.Write();

                // Automatically render the newly entered text.
                Image oldImage = PictRenderText.Image;

                PictRenderText.Image = string.IsNullOrEmpty(XmlSettings.CustomText)
                    ? null
                    : RenderTextLine(XmlSettings.CustomText);

                if (oldImage != null)
                    oldImage.Dispose();
            }
        }
        private void BtnPagePrev_Click(object sender, EventArgs e)
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            if (PageStart > 0)
            {
                PageStart -= PageSize;

                Index = PageStart;
                UpdateGlyphTextbox();

                RebuildPage();
                UpdateGlyphTextbox();

                int localIndex = Index - PageStart;

                if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                {
                    Character_Click(CharacterPictureList[localIndex],
                        new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                }
            }
        }
        private void BtnPageNext_Click(object sender, EventArgs e)
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            if (PageStart + PageSize < FontInfo.Character.Length)
            {
                PageStart += PageSize;

                Index = PageStart;
                RebuildPage();
                UpdateGlyphTextbox();

                int localIndex = Index - PageStart;

                if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                {
                    Character_Click(CharacterPictureList[localIndex],
                        new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                }
            }
        }
        private void BtnPrevious_Click(object sender, EventArgs e)
        {
            if (Index - PageStart > 0)
            {
                PrevNext(EDirection.Prev);
            }
        }
        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (Index - PageStart < (CharacterPictureList.Count - 1))
            {
                PrevNext(EDirection.Next);
            }
        }
        private async void BtnAllHeight_Click(object sender, EventArgs e)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
                return;

            CollapseSelectionToActiveGlyph();

            _bulkUpdate = true;
            _recordingGroupedOperation = true;
            FlowCharacterPanel.SuspendLayout();

            List<int> changedIndexes = new List<int>();

            try
            {
                foreach (PictureBox item in CharacterPictureList)
                {
                    if (!ClickedOnCharacter)
                    {
                        CCharInfo character = (CCharInfo)item.Tag;

                        if (character.UnscaledImage == null)
                            continue;

                        UInt16 oldWidth = character.Width;
                        UInt16 oldHeight = character.Height;

                        byte[] oldBytes = null;
                        if (character.ByteLines != null)
                            oldBytes = (byte[])character.ByteLines.Clone();

                        // Make sure the current state is in this glyph's history
                        // before changing its height.
                        character.UndoRedoListTidyUp();
                        character.UndoRedoListAdd(character.ByteLines);

                        CreateAndShow(character, SizeMode.ChangeHeight);

                        bool changed =
                            oldWidth != character.Width ||
                            oldHeight != character.Height ||
                            !ArraysEqual(oldBytes, character.ByteLines);

                        if (changed)
                        {
                            // Store the new resized state too.
                            character.UndoRedoListAdd(character.ByteLines);

                            changedIndexes.Add(character.Index);

                            item.ContextMenu.MenuItems[0].Enabled =
                                character.UndoPossible;

                            item.ContextMenu.MenuItems[1].Enabled =
                                character.RedoPossible;
                        }
                    }

                    await Task.Yield();
                }
            }
            finally
            {
                FlowCharacterPanel.ResumeLayout();
                _bulkUpdate = false;
                _recordingGroupedOperation = false;
            }

            if (changedIndexes.Count > 0)
            {
                _multiUndoStack.Push(changedIndexes);

                _operationUndoStack.Push(new UndoOperation
                {
                    Type = UndoOperationType.MultiGlyphEdit
                });

                _multiRedoStack.Clear();
                _structuralRedoStack.Clear();
                _operationRedoStack.Clear();
            }

            if (_selectedPreview != null)
            {
                CCharInfo activeCharacter =
                    (CCharInfo)_selectedPreview.Tag;

                DisplayCurrentCharacter(activeCharacter);
            }

            CheckChange();
        }

        private async void BtnAllWidth_Click(object sender, EventArgs e)
        {
            if (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit)
                return;

            CollapseSelectionToActiveGlyph();

            _bulkUpdate = true;
            _recordingGroupedOperation = true;
            FlowCharacterPanel.SuspendLayout();

            List<int> changedIndexes = new List<int>();

            try
            {
                foreach (PictureBox item in CharacterPictureList)
                {
                    if (!ClickedOnCharacter)
                    {
                        CCharInfo character = (CCharInfo)item.Tag;

                        if (character.UnscaledImage == null)
                            continue;

                        UInt16 oldWidth = character.Width;
                        UInt16 oldHeight = character.Height;

                        byte[] oldBytes = null;
                        if (character.ByteLines != null)
                            oldBytes = (byte[])character.ByteLines.Clone();

                        // Make sure the current state is in this glyph's history
                        // before changing its width.
                        character.UndoRedoListTidyUp();
                        character.UndoRedoListAdd(character.ByteLines);

                        CreateAndShow(character, SizeMode.ChangeWidth);

                        bool changed =
                            oldWidth != character.Width ||
                            oldHeight != character.Height ||
                            !ArraysEqual(oldBytes, character.ByteLines);

                        if (changed)
                        {
                            // Store the new resized state too.
                            character.UndoRedoListAdd(character.ByteLines);

                            changedIndexes.Add(character.Index);

                            item.ContextMenu.MenuItems[0].Enabled =
                                character.UndoPossible;

                            item.ContextMenu.MenuItems[1].Enabled =
                                character.RedoPossible;
                        }
                    }

                    await Task.Yield();
                }
            }
            finally
            {
                FlowCharacterPanel.ResumeLayout();
                _bulkUpdate = false;
                _recordingGroupedOperation = false;
            }

            if (changedIndexes.Count > 0)
            {
                _multiUndoStack.Push(changedIndexes);

                _operationUndoStack.Push(new UndoOperation
                {
                    Type = UndoOperationType.MultiGlyphEdit
                });

                _multiRedoStack.Clear();
                _structuralRedoStack.Clear();
                _operationRedoStack.Clear();
            }

            if (_selectedPreview != null)
            {
                CCharInfo activeCharacter =
                    (CCharInfo)_selectedPreview.Tag;

                DisplayCurrentCharacter(activeCharacter);
            }

            CheckChange();
        }
        private async void BtnAllBlankClear_Click(object sender, EventArgs e)
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            bool excludeSpaceGlyph = false;

            // Check index 32 only on the first page (0-255).
            if (PageStart == 0 && FontInfo.Character.Length > 32)
            {
                CCharInfo spaceGlyph = FontInfo.Character[32];

                if (!IsGlyphPlaceholder(spaceGlyph))
                {
                    DialogResult result = MessageBox.Show(
                        this.FindForm(),
                        "Index 32 is used as the 'space' in pixels between characters. Do you want to exclude it from this operation?",
                        "Warning",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Cancel)
                        return;

                    // Yes = preserve index 32.
                    // No = include index 32 in the normal conversion.
                    excludeSpaceGlyph = (result == DialogResult.Yes);
                }
            }

            FlowCharacterPanel.SuspendLayout();

            List<int> changedIndexes = new List<int>();

            try
            {
                foreach (PictureBox pict in CharacterPictureList)
                {
                    CCharInfo character = pict.Tag as CCharInfo;

                    if (character == null)
                        continue;

                    // Preserve the space glyph when the user selected OK.
                    if (excludeSpaceGlyph && character.Index == 32)
                        continue;

                    // Skip real placeholders.
                    if (character.ByteLines == null ||
                        character.ByteLines.Length == 0)
                    {
                        continue;
                    }

                    // Check whether this glyph is completely blank.
                    bool isAllBlack = true;

                    foreach (byte b in character.ByteLines)
                    {
                        if (b != 0x00)
                        {
                            isAllBlack = false;
                            break;
                        }
                    }

                    if (!isAllBlack)
                        continue;

                    // Make sure the current non-placeholder state exists
                    // in this glyph's Undo history before changing it.
                    character.UndoRedoListTidyUp();
                    character.UndoRedoListAdd(character.ByteLines);

                    // Convert to a true placeholder.
                    character.Width = 0;
                    character.Height = 0;
                    character.ByteLines = null;
                    character.UnscaledImage = null;

                    // Store the new placeholder state in Undo history.
                    character.UndoRedoListAdd(character.ByteLines);

                    changedIndexes.Add(character.Index);

                    RefreshCharacterPreview(pict, character);

                    if (pict.ContextMenu != null)
                    {
                        pict.ContextMenu.MenuItems[0].Enabled =
                            character.UndoPossible;

                        pict.ContextMenu.MenuItems[1].Enabled =
                            character.RedoPossible;
                    }

                    await Task.Yield();
                }
            }
            finally
            {
                FlowCharacterPanel.ResumeLayout();
            }

            // Treat all converted blank glyphs as one Undo/Redo transaction.
            if (changedIndexes.Count > 0)
            {
                _multiUndoStack.Push(changedIndexes);

                _operationUndoStack.Push(new UndoOperation
                {
                    Type = UndoOperationType.MultiGlyphEdit
                });

                _multiRedoStack.Clear();
                _structuralRedoStack.Clear();
                _operationRedoStack.Clear();
            }

            // Restore the active glyph on the canvas.
            if (_selectedPreview != null)
            {
                CCharInfo activeCharacter =
                    (CCharInfo)_selectedPreview.Tag;

                RefreshDrawingAreaDisplay(activeCharacter);
                FlowCharacterPanel.Invalidate();
            }

            CheckChange();
        }


        private UInt32? ParseXmlNumber(string s)
        {
            UInt32? number;

            if (s.ToLower().StartsWith("0x"))
            {
                number = UInt32.Parse(s.Substring(2), System.Globalization.NumberStyles.HexNumber);
            }
            else if (s.ToLower().EndsWith("h"))
            {
                number = UInt32.Parse(s.TrimEnd('h'), System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                number = UInt32.Parse(s, System.Globalization.NumberStyles.Integer);
            }

            return number;
        }

        private void TxtCharacter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                if (FontInfo == null || FontInfo.Character == null)
                    return;

                UInt32? number = null;

                try
                {
                    number = ParseXmlNumber(TxtCharacter.Text);
                }
                catch
                {
                }

                if (number != null)
                {
                    int newIndex = (int)number;

                    // Clamp to valid range
                    if (newIndex < 0)
                        newIndex = 0;

                    if (newIndex >= FontInfo.Character.Length)
                        newIndex = FontInfo.Character.Length - 1;

                    int newPageStart = (newIndex / PageSize) * PageSize;

                    // Only rebuild page if page actually changes
                    if (newPageStart != PageStart)
                    {
                        PageStart = newPageStart;
                        RebuildPage();
                        UpdateGlyphTextbox();
                    }

                    Index = newIndex;
                    UpdateGlyphTextbox();

                    int localIndex = Index - PageStart;

                    if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
                    {
                        PictureBox pict = CharacterPictureList[localIndex];

                        SelectOnlyPreview(pict);

                        CCharInfo characterinfo =
                            (CCharInfo)pict.Tag;

                        DisplayCurrentCharacter(characterinfo);
                    }

                    // Always update textbox to real index
                    TxtCharacter.Text = Index.ToString();
                }

                e.Handled = true;
            }

            // Allow control keys such as Backspace and Enter.
            if (char.IsControl(e.KeyChar))
            {
                // Let the existing Enter logic below handle Enter.
            }
            else if (char.IsDigit(e.KeyChar))
            {
                // Maximum allowed typed index: 99999.
                // Account for selected text, because typing replaces it.
                int selectedLength = TxtCharacter.SelectionLength;
                int resultingLength =
                    TxtCharacter.Text.Length - selectedLength + 1;

                if (resultingLength > 5)
                {
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                // Index field accepts digits only.
                e.Handled = true;
                return;
            }
        }


        private void ChkOneAllCharacters_CheckedChanged(object sender, EventArgs e)
        {
            if (ChkOneAllCharacters.Checked)
            {
                GrpOneCharacter.BackColor = SystemColors.Highlight;
            }
            else
            {
                GrpOneCharacter.BackColor = _darkMode
                    ? Color.FromArgb(37, 37, 38)
                    : SystemColors.Control;
            }
        }

        private void UpdatePageButtons()
        {
            if (FontInfo == null || FontInfo.Character == null)
            {
                BtnPagePrev.Enabled = false;
                BtnPageNext.Enabled = false;
                TxtPageNumber.Text = "0";
                LblPageTotal.Text = "/ 0";
                return;
            }

            int total = FontInfo.Character.Length;

            int totalPages = (total + PageSize - 1) / PageSize;
            if (totalPages == 0)
                totalPages = 1;

            int currentPage = (PageStart / PageSize) + 1;

            BtnPagePrev.Enabled = (PageStart > 0);
            BtnPageNext.Enabled = (PageStart + PageSize < total);

            TxtPageNumber.Text = currentPage.ToString();
            LblPageTotal.Text = $"/ {totalPages}";
        }


        private void NavigateToEnteredPage()
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            int totalPages = Math.Max(1,
                (FontInfo.Character.Length + PageSize - 1) / PageSize);

            long requestedPage;

            if (!long.TryParse(TxtPageNumber.Text, out requestedPage))
                requestedPage = 1;

            int page = (int)Math.Max(1L, Math.Min(requestedPage, totalPages));
            int newPageStart = (page - 1) * PageSize;

            if (PageStart != newPageStart)
            {
                PageStart = newPageStart;
                Index = PageStart;

                RebuildPage();
                UpdateGlyphTextbox();

                // Select the first glyph on the new page.
                if (CharacterPictureList.Count > 0)
                {
                    Character_Click(CharacterPictureList[0],
                        new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                }
            }

            UpdatePageButtons();
        }


        private void TxtPageNumber_Leave(object sender, EventArgs e)
        {
            NavigateToEnteredPage();
        }

        private void TxtPageNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigateToEnteredPage();
                e.SuppressKeyPress = true;
            }
        }

        private void TxtGlyphRange_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r')
                return;

            if (_applyingGlyphRange)
                return;

            _applyingGlyphRange = true;

            try
            {
                ApplyGlyphRangeChange(e);
            }
            finally
            {
                _applyingGlyphRange = false;
            }
        }

        private void ApplyGlyphRangeChange(KeyPressEventArgs e)
        {

            if (FontInfo == null || FontInfo.Character == null)
                return;

            const int maxIndex = 65535;

            if (!int.TryParse(TxtGlyphRange.Text, out int requestedIndex))
            {
                TxtGlyphRange.Text = (FontInfo.Character.Length - 1).ToString();
                return;
            }

            if (FontInfo is CSCIFontInfo && requestedIndex > 255)
            {
                MessageBox.Show(
                    "SCI fonts are limited to 256 glyphs (0-255).\n\nTo extend beyond that range, convert it to WFN first.",
                    "SCI Glyph Limit",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                TxtGlyphRange.Text = (FontInfo.Character.Length - 1).ToString();
                e.Handled = true;
                return;
            }

            // -1 (or any negative value) requests clearing the entire font.
            bool clearEntireFont = requestedIndex < 0;

            // Clamp the highest glyph index to the valid range.
            if (requestedIndex < 0)
                requestedIndex = 0;

            if (requestedIndex > maxIndex)
                requestedIndex = maxIndex;

            // Convert highest index to actual glyph count.
            int newLength = requestedIndex + 1;

            TxtGlyphRange.Text = (newLength - 1).ToString();

            int currentLength = FontInfo.Character.Length;

            if (clearEntireFont)
            {
                DialogResult result = MessageBox.Show(
                    "ALL GLYPH DATA IN THE FONT WILL BE DELETED!\n\n" +
                    "Changing the font size will also clear ALL Undo/Redo history.\n\n" +
                    "This action cannot be undone.\n\n" +
                    "Continue?",
                    "WARNING",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.OK)
                {
                    TxtGlyphRange.Text = (currentLength - 1).ToString();
                    e.Handled = true;
                    return;
                }

                Array.Resize(ref FontInfo.Character, 1);
                MakeGlyphPlaceholder(FontInfo.Character[0]);

                // Clearing the entire font also resets all Undo/Redo history.
                ClearAllUndoRedoHistory();

                Index = 0;
                PageStart = 0;

                RebuildPage();
                UpdatePageButtons();
                ShowCharacterCount();
                UpdateGlyphTextbox();

                TxtGlyphRange.Text = "0";
                e.Handled = true;
                return;
            }

            // ✅ If user typed 0 (or negative) and we already are at length 1,
            // No change in the highest glyph index.
            if (newLength == currentLength)
            {
                e.Handled = true;
                return;
            }

            // 🔴 SHRINK
            if (newLength < currentLength)
            {
                DialogResult result = MessageBox.Show(
                    $"ALL GLYPHS ABOVE INDEX {newLength - 1} WILL BE DELETED!\n\n" +
                    "Changing the font size will also clear ALL Undo/Redo history.\n\n" +
                    "This action cannot be undone.\n\n" +
                    "Continue?",
                    "WARNING",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.OK)
                {
                    TxtGlyphRange.Text = (currentLength - 1).ToString();
                    return;
                }

                Array.Resize(ref FontInfo.Character, newLength);

                if (Index >= newLength)
                    Index = newLength - 1;
            }
            // 🟢 EXTEND
            else
            {
                DialogResult result = MessageBox.Show(
                    $"The font will be extended to index {newLength - 1}.\n\n" +
                    "Changing the font size will also clear ALL Undo/Redo history.\n\n" +
                    "This action cannot be undone.\n\n" +
                    "Continue?",
                    "WARNING",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.OK)
                {
                    TxtGlyphRange.Text = (currentLength - 1).ToString();
                    e.Handled = true;
                    return;
                }

                Array.Resize(ref FontInfo.Character, newLength);

                for (int i = currentLength; i < newLength; i++)
                {
                    CCharInfo ch = new CCharInfo();
                    ch.BitDepth = (FontInfo is CWFNFontInfo wfn && wfn.Is4Bit) ? 4 : 1;
                    ch.Height = 0;
                    ch.HeightOriginal = 0;
                    ch.Width = 0;
                    ch.WidthOriginal = 0;
                    ch.Index = i;
                    ch.ByteLines = null;
                    ch.ByteLinesOriginal = null;
                    ch.UnscaledImage = null;
                    FontInfo.Character[i] = ch;
                }
            }

            // Font range changed: discard all previous Undo/Redo history.
            ClearAllUndoRedoHistory();

            PageStart = (Index / PageSize) * PageSize;
            RebuildPage();
            UpdatePageButtons();
            ShowCharacterCount();
            UpdateGlyphTextbox();

            TxtGlyphRange.Text = (FontInfo.Character.Length - 1).ToString();
            e.Handled = true;
        }

        private static bool IsSeparatorPixel(Color pixel, Color separator)
        {
            if (separator.A == 0)
                return pixel.A == 0;

            return pixel.A == separator.A &&
                   pixel.R == separator.R &&
                   pixel.G == separator.G &&
                   pixel.B == separator.B;
        }

        private static List<Rectangle> DetectPngGlyphCells(
            Bitmap bitmap, Color separator)
        {
            List<Rectangle> cells = new List<Rectangle>();

            // Find horizontal separator lines spanning the entire image.
            List<int> rowBoundaries = new List<int>();
            rowBoundaries.Add(-1);

            for (int y = 0; y < bitmap.Height; y++)
            {
                bool separatorLine = true;

                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (!IsSeparatorPixel(bitmap.GetPixel(x, y), separator))
                    {
                        separatorLine = false;
                        break;
                    }
                }

                if (separatorLine)
                    rowBoundaries.Add(y);
            }

            rowBoundaries.Add(bitmap.Height);

            // Detect vertical boundaries independently inside each glyph row.
            for (int row = 0; row < rowBoundaries.Count - 1; row++)
            {
                int top = rowBoundaries[row] + 1;
                int bottom = rowBoundaries[row + 1];
                int height = bottom - top;

                if (height <= 0)
                    continue;

                int cellStart = 0;

                for (int x = 0; x <= bitmap.Width; x++)
                {
                    bool separatorLine = x == bitmap.Width;

                    if (!separatorLine)
                    {
                        separatorLine = true;

                        for (int y = top; y < bottom; y++)
                        {
                            if (!IsSeparatorPixel(bitmap.GetPixel(x, y), separator))
                            {
                                separatorLine = false;
                                break;
                            }
                        }
                    }

                    if (separatorLine)
                    {
                        int width = x - cellStart;

                        if (width > 0)
                        {
                            cells.Add(new Rectangle(
                                cellStart, top, width, height));
                        }

                        cellStart = x + 1;
                    }
                }
            }

            return cells;
        }

        private class PngGlyphSheet
        {
            public string FilePath;
            public int Start;
            public int End;
        }

        private class PreparedPngGlyph
        {
            public int Index;
            public Bitmap Image;
        }

        private static List<PreparedPngGlyph> PreparePngGlyphs(
            Bitmap bitmap,
            Color separatorColor,
            int startIndex,
            int endIndex)
        {
            List<Rectangle> cells =
                DetectPngGlyphCells(bitmap, separatorColor);

            int expectedCount = endIndex - startIndex + 1;

            if (cells.Count != expectedCount)
            {
                throw new System.IO.InvalidDataException(
                    "Detected " + cells.Count + " glyph cells, " +
                    "but the filename specifies " + expectedCount + " glyphs.");
            }

            List<PreparedPngGlyph> prepared =
                new List<PreparedPngGlyph>();

            try
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    Rectangle cell = cells[i];

                    if (cell.Width <= 0 || cell.Height <= 0 ||
                        cell.Width > MaxWidth || cell.Height > MaxHeight)
                    {
                        throw new System.IO.InvalidDataException(
                            "Invalid glyph dimensions at index " +
                            (startIndex + i) + ".");
                    }

                    Bitmap glyph = bitmap.Clone(
                        cell,
                        PixelFormat.Format32bppArgb);

                    // Remove separator-colored pixels inside the glyph rectangle.
                    for (int y = 0; y < glyph.Height; y++)
                    {
                        for (int x = 0; x < glyph.Width; x++)
                        {
                            Color pixel = glyph.GetPixel(x, y);

                            if (IsSeparatorPixel(pixel, separatorColor))
                            {
                                glyph.SetPixel(x, y, Color.Transparent);
                            }
                        }
                    }

                    prepared.Add(new PreparedPngGlyph
                    {
                        Index = startIndex + i,
                        Image = glyph
                    });
                }

                return prepared;
            }
            catch
            {
                foreach (PreparedPngGlyph glyph in prepared)
                    glyph.Image.Dispose();

                throw;
            }
        }

        private static byte[] ConvertPngBitmapTo1BitBytes(Bitmap bitmap)
        {
            int stride = (bitmap.Width + 7) / 8;
            byte[] bytes = new byte[stride * bitmap.Height];

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);

                    if (color.A == 0)
                        continue;

                    double brightness =
                        0.299 * color.R +
                        0.587 * color.G +
                        0.114 * color.B;

                    if (brightness >= 128)
                    {
                        bytes[y * stride + x / 8] |=
                            (byte)(0x80 >> (x & 7));
                    }
                }
            }

            return bytes;
        }

        private void BtnImportPNG_Click(object sender, EventArgs e)
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Import PNG Glyph Sheet";
                dialog.Filter = "PNG Images (*.png)|*.png";
                dialog.Multiselect = true;

                if (dialog.ShowDialog(this.FindForm()) != DialogResult.OK)
                    return;

                string selectedFile = dialog.FileName;
                string selectedName =
                    System.IO.Path.GetFileNameWithoutExtension(selectedFile);

                // Expected filename: FontName_START-END.png
                var match = System.Text.RegularExpressions.Regex.Match(
                    selectedName,
                    @"^(.*)_(\d+)-(\d+)$");

                if (!match.Success)
                {
                    MessageBox.Show(
                        "The selected PNG filename must contain a glyph range, " +
                        "for example: SpeechFont_0-127.png",
                        "Invalid PNG Filename",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string baseName = match.Groups[1].Value;
                int selectedStart;
                int selectedEnd;

                if (!int.TryParse(match.Groups[2].Value, out selectedStart) ||
                    !int.TryParse(match.Groups[3].Value, out selectedEnd) ||
                    selectedStart > selectedEnd ||
                    selectedEnd > 65535)
                {
                    MessageBox.Show(
                        "The selected PNG contains an invalid glyph range.",
                        "Invalid Glyph Range",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string folder = System.IO.Path.GetDirectoryName(selectedFile);

                List<PngGlyphSheet> sheets = new List<PngGlyphSheet>();

                foreach (string file in System.IO.Directory.GetFiles(folder, "*.png"))
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(file);

                    var fileMatch = System.Text.RegularExpressions.Regex.Match(
                        name,
                        @"^(.*)_(\d+)-(\d+)$");

                    if (!fileMatch.Success)
                        continue;

                    if (!string.Equals(
                        fileMatch.Groups[1].Value,
                        baseName,
                        StringComparison.OrdinalIgnoreCase))
                        continue;

                    int start;
                    int end;

                    if (!int.TryParse(fileMatch.Groups[2].Value, out start) ||
                        !int.TryParse(fileMatch.Groups[3].Value, out end) ||
                        start > end ||
                        end > 65535)
                        continue;

                    sheets.Add(new PngGlyphSheet
                    {
                        FilePath = file,
                        Start = start,
                        End = end
                    });
                }

                sheets = sheets
                    .OrderBy(s => s.Start)
                    .ThenBy(s => s.End)
                    .ThenBy(s => s.FilePath, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (sheets.Count > 1)
                {
                    DialogResult result = MessageBox.Show(
                        this.FindForm(),
                        "Multiple PNG files containing glyph ranges were detected " +
                        "for \"" + baseName + "\".\n\n" +
                        "Do you want to import glyphs from all " +
                        sheets.Count + " files?",
                        "Multiple PNG Glyph Sheets Detected",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Cancel)
                        return;

                    if (result == DialogResult.No)
                    {
                        sheets = sheets
                            .Where(s => string.Equals(
                                s.FilePath,
                                selectedFile,
                                StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }
                }

                // Check overlapping ranges in import order.
                List<string> overlaps = new List<string>();

                for (int i = 0; i < sheets.Count; i++)
                {
                    for (int j = i + 1; j < sheets.Count; j++)
                    {
                        if (sheets[j].Start > sheets[i].End)
                            break;

                        int overlapStart = Math.Max(sheets[i].Start, sheets[j].Start);
                        int overlapEnd = Math.Min(sheets[i].End, sheets[j].End);

                        overlaps.Add(
                            System.IO.Path.GetFileName(sheets[i].FilePath) +
                            "\n" +
                            System.IO.Path.GetFileName(sheets[j].FilePath) +
                            "\nOverlapping indexes: " +
                            overlapStart + "-" + overlapEnd);
                    }
                }

                if (overlaps.Count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        this.FindForm(),
                        "Overlapping glyph indexes detected in these files:\n\n" +
                        string.Join("\n\n", overlaps) +
                        "\n\nThe glyphs of the latter file will overwrite " +
                        "those of the previous file. Continue?",
                        "Overlapping Glyph Indexes",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.OK)
                        return;
                }

                // Step 3: Automatically detect each PNG's separator color, then prepare and validate its glyphs.
                List<PreparedPngGlyph> preparedGlyphs =
                    new List<PreparedPngGlyph>();

                try
                {
                    foreach (PngGlyphSheet sheet in sheets)
                    {
                        using (Bitmap bitmap = new Bitmap(sheet.FilePath))
                        {
                            // Detect this PNG's separator from its bottom-right pixel.
                            Color separatorColor = bitmap.GetPixel(
                                bitmap.Width - 1,
                                bitmap.Height - 1);

                            preparedGlyphs.AddRange(
                                PreparePngGlyphs(
                                    bitmap,
                                    separatorColor,
                                    sheet.Start,
                                    sheet.End));
                        }
                    }

                    bool shadingDetected = false;

                    foreach (PreparedPngGlyph glyph in preparedGlyphs)
                    {
                        Bitmap image = glyph.Image;

                        for (int y = 0; y < image.Height && !shadingDetected; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                Color pixel = image.GetPixel(x, y);

                                if (pixel.A != 0 &&
                                    (pixel.A != 255 ||
                                     (pixel.R != 0 || pixel.G != 0 || pixel.B != 0) &&
                                     (pixel.R != 255 || pixel.G != 255 || pixel.B != 255)))
                                {
                                    shadingDetected = true;
                                    break;
                                }
                            }

                            if (shadingDetected)
                                break;
                        }
                    }

                    CWFNFontInfo wfnFont = FontInfo as CWFNFontInfo;

                    bool upgradeTo4Bit =
                        shadingDetected && wfnFont != null && !wfnFont.Is4Bit;

                    if (shadingDetected && FontInfo is CSCIFontInfo)
                    {
                        throw new System.IO.InvalidDataException(
                            "The imported PNG contains grayscale shading, but SCI fonts " +
                            "support 1-bit glyphs only.\n\n" +
                            "Convert this font to WFN before importing the PNG.");
                    }

                    int bitDepth =
                        (wfnFont != null && wfnFont.Is4Bit) || upgradeTo4Bit ? 4 : 1;

                    Dictionary<int, byte[]> converted =
                        new Dictionary<int, byte[]>();

                    Dictionary<int, Size> dimensions =
                        new Dictionary<int, Size>();

                    // Convert all PNG glyphs before modifying the font.
                    foreach (PreparedPngGlyph glyph in preparedGlyphs)
                    {
                        converted[glyph.Index] = bitDepth == 4
                            ? ConvertBitmapTo4BitBytes(glyph.Image)
                            : ConvertPngBitmapTo1BitBytes(glyph.Image);

                        dimensions[glyph.Index] = glyph.Image.Size;
                    }

                    if (converted.Count == 0)
                    {
                        throw new System.IO.InvalidDataException(
                            "No glyphs were found in the selected PNG files.");
                    }

                    int highestImportedIndex = converted.Keys.Max();

                    if (FontInfo is CSCIFontInfo && highestImportedIndex > 255)
                    {
                        throw new System.IO.InvalidDataException(
                            "SCI fonts support only glyph indexes 0-255.");
                    }

                    // Find the highest existing glyph containing data,
                    // excluding glyphs that will be replaced by this import.
                    int highestExistingIndex = -1;

                    for (int i = FontInfo.Character.Length - 1; i >= 0; i--)
                    {
                        if (converted.ContainsKey(i))
                            continue;

                        if (!IsGlyphPlaceholder(FontInfo.Character[i]))
                        {
                            highestExistingIndex = i;
                            break;
                        }
                    }

                    int highestRequiredIndex =
                        Math.Max(highestImportedIndex, highestExistingIndex);

                    int finalLength = Math.Max(1, highestRequiredIndex + 1);
                    int originalLength = FontInfo.Character.Length;

                    DialogResult confirmation = MessageBox.Show(
                        this.FindForm(),
                        converted.Count + " glyphs found in " +
                        sheets.Count + " PNG file(s)\n\n" +
                        "Existing glyphs at imported indexes will be replaced!\n" +
                        "The font range will be adjusted from 0-" +
                        (originalLength - 1) + " to 0-" +
                        (finalLength - 1) + ".\n\n" +
                        "This operation cannot be undone and ALL previous Undo/Redo history will be cleared.",
                        "Confirm PNG Import",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning);

                    if (confirmation != DialogResult.OK)
                        return;

                    // Upgrade the entire font before importing shaded glyphs.
                    if (upgradeTo4Bit)
                    {
                        wfnFont.ConvertBitDepth(true);
                        grayscalePalette.Visible = true;
                        UpdateShadeButtonsLayout();
                    }

                    // Extend the character array if necessary.
                    if (finalLength > originalLength)
                    {
                        Array.Resize(ref FontInfo.Character, finalLength);

                        for (int i = originalLength; i < finalLength; i++)
                        {
                            CCharInfo character = new CCharInfo();

                            character.Index = i;
                            character.BitDepth = bitDepth;
                            character.Width = 0;
                            character.Height = 0;
                            character.WidthOriginal = 0;
                            character.HeightOriginal = 0;
                            character.ByteLines = null;
                            character.ByteLinesOriginal = null;
                            character.UnscaledImage = null;

                            FontInfo.Character[i] = character;
                        }
                    }

                    // Import the converted glyphs into their destination indexes.
                    foreach (int glyphIndex in converted.Keys.OrderBy(i => i))
                    {
                        CCharInfo character = FontInfo.Character[glyphIndex];
                        Size size = dimensions[glyphIndex];

                        character.Width = (ushort)size.Width;
                        character.Height = (ushort)size.Height;
                        character.BitDepth = bitDepth;
                        character.ByteLines = converted[glyphIndex];
                        character.UnscaledImage = null;
                    }

                    // Remove trailing unused glyph slots.
                    if (finalLength < FontInfo.Character.Length)
                    {
                        Array.Resize(ref FontInfo.Character, finalLength);
                    }

                    FontInfo.NumberOfCharacters = FontInfo.Character.Length;

                    // The import is not undoable.
                    // Reset all previous Undo/Redo history.
                    ClearAllUndoRedoHistory();

                    // Keep the active glyph within the new range.
                    if (Index >= finalLength)
                        Index = finalLength - 1;

                    if (Index < 0)
                        Index = 0;

                    PageStart = (Index / PageSize) * PageSize;

                    TxtGlyphRange.Text = (finalLength - 1).ToString();

                    RebuildPage();
                    UpdatePageButtons();
                    UpdateGlyphTextbox();
                    ShowCharacterCount();

                    MarkFontModified();

                    MessageBox.Show(
                        this.FindForm(),
                        "PNG import completed successfully.\n\n" +
                        "Files imported: " + sheets.Count + "\n" +
                        "Glyphs imported: " + converted.Count + "\n" +
                        "Final font range: 0-" + (finalLength - 1),
                        "PNG Import Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    if (upgradeTo4Bit)
                    {
                        MessageBox.Show(
                            this.FindForm(),
                            "Glyph shading detected - the font was extended to the 4-bit format, " +
                            "supported in AGS 4.x and above only.\n\n" +
                            "If you need to use this font in older versions, click on the \"1-4-bit\" button " +
                            "to convert it back to 1-bit.",
                            "Font Extended to 4-bit",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this.FindForm(),
                        "PNG import failed:\n\n" + ex.Message,
                        "PNG Import Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    foreach (PreparedPngGlyph glyph in preparedGlyphs)
                        glyph.Image.Dispose();
                }
            }
        }

        private void BtnExportPNG_Click(object sender, EventArgs e)
        {
            ShowPngExportDialog();
        }

        private void ShowPngExportDialog()
        {
            if (FontInfo == null || FontInfo.Character == null)
                return;

            using (PngExportDialog dialog = new PngExportDialog(XmlSettings))
            {
                if (dialog.ShowDialog(this.FindForm()) != DialogResult.OK)
                    return;

                // Save PNG export preferences.
                XmlSettings.PngGlyphsPerRow = dialog.GlyphsPerRow;
                XmlSettings.PngFixedSize = dialog.FixedSize;
                XmlSettings.PngSeparatorColor = dialog.SelectedSeparatorColor;
                XmlSettings.PngTransparent = dialog.TransparentSeparator;
                XmlSettings.PngIncludePlaceholders = dialog.IncludePlaceholders;
                XmlSettings.Write();

                bool fixedSize = dialog.FixedSize;
                bool includePlaceholders = dialog.IncludePlaceholders;
                int glyphsPerRow = dialog.GlyphsPerRow;
                Color separatorColor = dialog.SeparatorColor;

                List<Tuple<int, int>> ranges =
                    GetGlyphExportRanges(includePlaceholders);

                if (ranges.Count == 0)
                {
                    MessageBox.Show(
                        "There are no glyphs to export.",
                        "PNG Export",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                // Calculate one cell size for the entire font.
                int cellWidth = 1;
                int cellHeight = 1;

                foreach (CCharInfo ch in FontInfo.Character)
                {
                    if (IsGlyphPlaceholder(ch))
                    {
                        if (includePlaceholders)
                        {
                            cellWidth = Math.Max(cellWidth, XmlSettings.PlaceholderWidth);
                            cellHeight = Math.Max(cellHeight, XmlSettings.PlaceholderHeight);
                        }
                    }
                    else
                    {
                        cellWidth = Math.Max(cellWidth, ch.Width);
                        cellHeight = Math.Max(cellHeight, ch.Height);
                    }
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Title = "Export PNG Glyph Sheet";
                    saveDialog.Filter = "PNG Image (*.png)|*.png";
                    saveDialog.DefaultExt = "png";
                    saveDialog.AddExtension = true;
                    saveDialog.FileName = "Glyphs.png";
                    saveDialog.OverwritePrompt = false;

                    if (!string.IsNullOrEmpty(FontInfo.FontPath) &&
                        System.IO.File.Exists(FontInfo.FontPath))
                    {
                        saveDialog.InitialDirectory =
                            System.IO.Path.GetDirectoryName(FontInfo.FontPath);
                    }

                    string exportFolder = null;
                    string exportBaseName = null;

                    while (true)
                    {
                        if (saveDialog.ShowDialog(this.FindForm()) != DialogResult.OK)
                            return;

                        exportFolder = System.IO.Path.GetDirectoryName(saveDialog.FileName);
                        exportBaseName = System.IO.Path.GetFileNameWithoutExtension(saveDialog.FileName);

                        // If the user selects an existing range file, remove its
                        // range suffix to recover the common export base name.
                        foreach (Tuple<int, int> range in ranges)
                        {
                            string suffix = "_" + range.Item1 + "-" + range.Item2;

                            if (exportBaseName.EndsWith(
                                suffix, StringComparison.OrdinalIgnoreCase))
                            {
                                exportBaseName = exportBaseName.Substring(
                                    0, exportBaseName.Length - suffix.Length);
                                break;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(exportBaseName))
                        {
                            MessageBox.Show(
                                "Please enter a valid export filename.",
                                "PNG Export",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            continue;
                        }

                        // Check all the files that this export would generate.
                        List<string> existingFiles = new List<string>();

                        foreach (Tuple<int, int> range in ranges)
                        {
                            string filename = System.IO.Path.Combine(
                                exportFolder,
                                exportBaseName + "_" + range.Item1 + "-" + range.Item2 + ".png");

                            if (System.IO.File.Exists(filename))
                                existingFiles.Add(System.IO.Path.GetFileName(filename));
                        }

                        if (existingFiles.Count == 0)
                            break;

                        string message =
                            "The following PNG files already exist:\n\n" +
                            string.Join("\n", existingFiles.ToArray()) +
                            "\n\nDo you want to overwrite them?\n\n" +
                            "Select No to choose a different filename.";

                        DialogResult overwriteResult = MessageBox.Show(
                            message,
                            "Confirm PNG Overwrite",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (overwriteResult == DialogResult.Yes)
                            break;

                        // Reopen Save As in the same folder, ready for a new name.
                        saveDialog.InitialDirectory = exportFolder;
                        saveDialog.FileName = exportBaseName + ".png";
                    }

                    try
                    {
                        foreach (Tuple<int, int> range in ranges)
                        {
                            int count = range.Item2 - range.Item1 + 1;
                            int columns = Math.Min(glyphsPerRow, count);
                            int rows = (count + glyphsPerRow - 1) / glyphsPerRow;

                            int bitmapWidth;
                            int bitmapHeight;

                            // Original Size needs individual glyph widths and row heights.
                            int[] glyphWidths = null;
                            int[] glyphHeights = null;
                            int[] rowHeights = null;

                            if (fixedSize)
                            {
                                // Existing Fixed Size dimensions.
                                bitmapWidth = checked(columns * cellWidth + (columns - 1));
                                bitmapHeight = checked(rows * cellHeight + (rows - 1));
                            }
                            else
                            {
                                glyphWidths = new int[count];
                                glyphHeights = new int[count];
                                rowHeights = new int[rows];

                                // Determine each glyph's native dimensions.
                                for (int i = 0; i < count; i++)
                                {
                                    CCharInfo glyph = FontInfo.Character[range.Item1 + i];

                                    if (IsGlyphPlaceholder(glyph))
                                    {
                                        glyphWidths[i] = Math.Max(1, XmlSettings.PlaceholderWidth);
                                        glyphHeights[i] = Math.Max(1, XmlSettings.PlaceholderHeight);
                                    }
                                    else
                                    {
                                        glyphWidths[i] = Math.Max(1, (int)glyph.Width);
                                        glyphHeights[i] = Math.Max(1, (int)glyph.Height);
                                    }

                                    int row = i / glyphsPerRow;

                                    rowHeights[row] = Math.Max(
                                        rowHeights[row],
                                        glyphHeights[i]);
                                }

                                // The image width is the width of its widest row.
                                bitmapWidth = 1;

                                for (int row = 0; row < rows; row++)
                                {
                                    int start = row * glyphsPerRow;
                                    int end = Math.Min(start + glyphsPerRow, count);

                                    int rowWidth = 0;

                                    for (int i = start; i < end; i++)
                                    {
                                        rowWidth = checked(rowWidth + glyphWidths[i]);

                                        if (i > start)
                                            rowWidth = checked(rowWidth + 1);
                                    }

                                    bitmapWidth = Math.Max(bitmapWidth, checked(rowWidth + 1));
                                }

                                // Each row uses its tallest glyph's height.
                                bitmapHeight = 0;

                                for (int row = 0; row < rows; row++)
                                {
                                    bitmapHeight = checked(bitmapHeight + rowHeights[row]);

                                    if (row > 0)
                                        bitmapHeight = checked(bitmapHeight + 1);
                                }

                                // Reserve one separator row below the final row.
                                bitmapHeight = checked(bitmapHeight + 1);
                            }

                            using (Bitmap bitmap = new Bitmap(
                                bitmapWidth,
                                bitmapHeight,
                                PixelFormat.Format32bppArgb))
                            {
                                using (Graphics graphics = Graphics.FromImage(bitmap))
                                {
                                    // Separator-colored background also fills unused areas.
                                    graphics.Clear(separatorColor);

                                    if (fixedSize)
                                    {
                                        // Existing Fixed Size rendering.
                                        for (int i = 0; i < count; i++)
                                        {
                                            int column = i % glyphsPerRow;
                                            int row = i / glyphsPerRow;

                                            int destinationX = column * (cellWidth + 1);
                                            int destinationY = row * (cellHeight + 1);

                                            graphics.FillRectangle(
                                                Brushes.Black,
                                                destinationX,
                                                destinationY,
                                                cellWidth,
                                                cellHeight);
                                        }
                                    }
                                    else
                                    {
                                        // Original Size rendering.
                                        int destinationY = 0;

                                        for (int row = 0; row < rows; row++)
                                        {
                                            int destinationX = 0;
                                            int start = row * glyphsPerRow;
                                            int end = Math.Min(start + glyphsPerRow, count);

                                            for (int i = start; i < end; i++)
                                            {
                                                // Only the native glyph rectangle is black.
                                                // Space beneath shorter glyphs stays separator-colored.
                                                graphics.FillRectangle(
                                                    Brushes.Black,
                                                    destinationX,
                                                    destinationY,
                                                    glyphWidths[i],
                                                    glyphHeights[i]);

                                                destinationX += glyphWidths[i] + 1;
                                            }

                                            destinationY += rowHeights[row] + 1;
                                        }
                                    }
                                }

                                if (fixedSize)
                                {
                                    // Draw glyphs into their fixed-size cells.
                                    for (int i = 0; i < count; i++)
                                    {
                                        CCharInfo glyph = FontInfo.Character[range.Item1 + i];

                                        if (IsGlyphPlaceholder(glyph))
                                            continue;

                                        int column = i % glyphsPerRow;
                                        int row = i / glyphsPerRow;

                                        int destinationX = column * (cellWidth + 1);
                                        int destinationY = row * (cellHeight + 1);

                                        DrawPngGlyph(bitmap, glyph, destinationX, destinationY);
                                    }
                                }
                                else
                                {
                                    // Draw glyphs using their original dimensions.
                                    int destinationY = 0;

                                    for (int row = 0; row < rows; row++)
                                    {
                                        int destinationX = 0;
                                        int start = row * glyphsPerRow;
                                        int end = Math.Min(start + glyphsPerRow, count);

                                        for (int i = start; i < end; i++)
                                        {
                                            CCharInfo glyph = FontInfo.Character[range.Item1 + i];

                                            if (!IsGlyphPlaceholder(glyph))
                                            {
                                                DrawPngGlyph(
                                                    bitmap,
                                                    glyph,
                                                    destinationX,
                                                    destinationY);
                                            }

                                            destinationX += glyphWidths[i] + 1;
                                        }

                                        destinationY += rowHeights[row] + 1;
                                    }
                                }

                                string filename = System.IO.Path.Combine(
                                    exportFolder,
                                    exportBaseName + "_" + range.Item1 + "-" + range.Item2 + ".png");

                                bitmap.Save(filename, ImageFormat.Png);
                            }
                        }

                        MessageBox.Show(
                            "PNG glyph sheet export completed successfully.",
                            "PNG Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "PNG export failed:\n" + ex.Message,
                            "PNG Export",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }

            }
        }


        private static void DrawPngGlyph(
            Bitmap bitmap, CCharInfo glyph, int destinationX, int destinationY)
        {
            if (glyph == null || glyph.ByteLines == null)
                return;

            int width = glyph.Width;
            int height = glyph.Height;
            bool is4Bit = glyph.BitDepth == 4;

            int bytesPerLine = is4Bit
                ? (width + 1) / 2
                : (width + 7) / 8;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int byteIndex = y * bytesPerLine +
                        (is4Bit ? x / 2 : x / 8);

                    if (byteIndex >= glyph.ByteLines.Length)
                        continue;

                    byte packed = glyph.ByteLines[byteIndex];

                    int shade;

                    if (is4Bit)
                    {
                        shade = (x % 2 == 0)
                            ? (packed >> 4)
                            : (packed & 0x0F);
                    }
                    else
                    {
                        shade = (packed & (0x80 >> (x % 8))) != 0
                            ? 15
                            : 0;
                    }

                    int intensity = shade * 17;

                    bitmap.SetPixel(
                        destinationX + x,
                        destinationY + y,
                        Color.FromArgb(intensity, intensity, intensity));
                }
            }
        }


        private static bool IsGlyphPlaceholder(CCharInfo ch)
        {
            return ch == null ||
                   (ch.Width == 0 &&
                    ch.Height == 0 &&
                    (ch.ByteLines == null || ch.ByteLines.Length == 0));
        }

        private List<Tuple<int, int>> GetGlyphExportRanges(bool includePlaceholders)
        {
            List<Tuple<int, int>> ranges = new List<Tuple<int, int>>();

            if (FontInfo == null ||
                FontInfo.Character == null ||
                FontInfo.Character.Length == 0)
                return ranges;

            if (includePlaceholders)
            {
                // Find the last glyph containing actual data.
                int lastGlyph = -1;

                for (int i = FontInfo.Character.Length - 1; i >= 0; i--)
                {
                    if (!IsGlyphPlaceholder(FontInfo.Character[i]))
                    {
                        lastGlyph = i;
                        break;
                    }
                }

                // Export from index 0 through the last actual glyph.
                // Preserve all placeholders between them.
                if (lastGlyph >= 0)
                    ranges.Add(Tuple.Create(0, lastGlyph));

                return ranges;
            }

            int rangeStart = -1;

            for (int i = 0; i < FontInfo.Character.Length; i++)
            {
                bool placeholder = IsGlyphPlaceholder(FontInfo.Character[i]);

                if (!placeholder && rangeStart == -1)
                {
                    rangeStart = i;
                }
                else if (placeholder && rangeStart != -1)
                {
                    ranges.Add(Tuple.Create(rangeStart, i - 1));
                    rangeStart = -1;
                }
            }

            if (rangeStart != -1)
                ranges.Add(Tuple.Create(rangeStart, FontInfo.Character.Length - 1));

            return ranges;
        }

        // Put this helper anywhere inside FontEditorPane (same class)
        private void MakeGlyphPlaceholder(CCharInfo ch)
        {
            if (ch == null) return;

            ch.Width = 0;
            ch.Height = 0;
            ch.WidthOriginal = 0;
            ch.HeightOriginal = 0;

            ch.ByteLines = null;
            ch.ByteLinesOriginal = null;

            ch.UnscaledImage = null;

            // optional but recommended so undo/redo won’t reference old data
            ch.UndoRedoListClear();
            ch.UndoRedoPosition = 0;
            ch.UndoRedoListAdd(ch.ByteLines);
        }


        private void UpdateGlyphTextbox()
        {
            if (Index >= 0 && Index <= 0xFFFF)
            {
                TxtGlyph.Text = char.ConvertFromUtf32(Index);
            }
            else
            {
                TxtGlyph.Text = "";
            }
        }

        private void TxtGlyph_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If typing a normal character (not Enter), replace existing character
            if (e.KeyChar != '\r' && !char.IsControl(e.KeyChar))
            {
                TxtGlyph.Text = e.KeyChar.ToString();
                TxtGlyph.SelectionStart = 1; // keep cursor at end
                e.Handled = true;            // stop default behavior
                return;
            }

            // From here down is your original Enter logic
            if (e.KeyChar != '\r')
                return;

            if (string.IsNullOrEmpty(TxtGlyph.Text))
                return;

            string input = TxtGlyph.Text;

            int codePoint = char.ConvertToUtf32(input, 0);

            if (codePoint < 0 || codePoint >= FontInfo.Character.Length)
            {
                // Out of range → ignore
                UpdateGlyphTextbox();
                return;
            }

            int newIndex = codePoint;
            int newPageStart = (newIndex / PageSize) * PageSize;

            // Only rebuild if page changes
            if (newPageStart != PageStart)
            {
                PageStart = newPageStart;
                RebuildPage();
            }

            Index = newIndex;

            int localIndex = Index - PageStart;

            if (localIndex >= 0 && localIndex < CharacterPictureList.Count)
            {
                PictureBox pict = CharacterPictureList[localIndex];

                SelectOnlyPreview(pict);

                CCharInfo characterinfo =
                    (CCharInfo)pict.Tag;

                DisplayCurrentCharacter(characterinfo);
            }

            TxtCharacter.Text = Index.ToString();
            UpdateGlyphTextbox();

            e.Handled = true;
        }
        private bool _applyingGlyphRange = false;

        private void TxtGlyphRange_Leave(object sender, EventArgs e)
        {
            if (_applyingGlyphRange)
                return;

            TxtGlyphRange_KeyPress(TxtGlyphRange, new KeyPressEventArgs('\r'));
        }

        public void ApplyPendingGlyphRange()
        {
            if (TxtGlyphRange.Focused)
                TxtGlyphRange_Leave(TxtGlyphRange, EventArgs.Empty);
        }

        private void TxtCharacter_Leave(object sender, EventArgs e)
        {
            // Simulate pressing Enter
            TxtCharacter_KeyPress(TxtCharacter, new KeyPressEventArgs('\r'));
        }

        private void TxtGlyph_Leave(object sender, EventArgs e)
        {
            // Simulate pressing Enter
            TxtGlyph_KeyPress(TxtGlyph, new KeyPressEventArgs('\r'));
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            // Force focus away from textboxes
            this.ActiveControl = null;
            _canvasSelectionActive = false;
            FlowCharacterPanel.Invalidate();
        }

        private void TxtGlyphRange_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                TxtGlyphRange.SelectAll();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.V)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                if (!Clipboard.ContainsText())
                    return;

                string pastedText = Clipboard.GetText();
                TextBox box = TxtGlyphRange;

                string proposedText = box.Text.Remove(
                    box.SelectionStart,
                    box.SelectionLength).Insert(
                    box.SelectionStart,
                    pastedText);

                int value;

                // Accept only -1 or a nonnegative index up to 65535.
                if (!int.TryParse(proposedText, out value) ||
                    (value != -1 && (value < 0 || value > 65535)))
                {
                    return;
                }

                int caretPosition = box.SelectionStart + pastedText.Length;

                box.Text = proposedText;
                box.SelectionStart = caretPosition;
                box.SelectionLength = 0;
            }
        }

        private void TxtGlyphRange_KeyPress_Filter(object sender, KeyPressEventArgs e)
        {
            // Allow digits
            if (char.IsDigit(e.KeyChar))
                return;

            // Allow Backspace
            if (e.KeyChar == (char)Keys.Back)
                return;

            // Allow Enter
            if (e.KeyChar == '\r')
                return;

            // Allow a minus sign only at the beginning of the value.
            if (e.KeyChar == '-' &&
                TxtGlyphRange.SelectionStart == 0 &&
                !TxtGlyphRange.Text.Contains("-"))
                return;

            // Block everything else
            e.Handled = true;
        }
        public void SaveAs(string filePath)
        {
            if (FontInfo == null)
                return;

            // 1️⃣ Perform size check BEFORE file creation
            if (FontInfo is CWFNFontInfo wfn)
            {
                if (!wfn.Is4Bit && wfn.EstimateSize() > 65535)
                {
                    MessageBox.Show(
                        "This font exceeds the 64 KB size limit of the 1-bit WFN format.\n\n" +
                        "Consider converting it to the 4-bit WFN format, which uses 32-bit offsets and sizes and supports larger font files.\n\n" +
                        "Note: 4-bit WFN fonts are compatible with Adventure Game Studio 4.x only!",
                        "Save Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return; // 🔴 Exit BEFORE file creation
                }
            }

            // 2️⃣ Proceed with saving only if no size issue
            using (System.IO.FileStream file = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(file))
            {
                FontInfo.Write(writer);
            }

            // 3️⃣ Update the font path after successful save
            FontInfo.FontPath = filePath;
            FontInfo.FontName = System.IO.Path.GetFileNameWithoutExtension(filePath);
        }

        private long CalculateCurrentWFNSize()
        {
            if (FontInfo == null || FontInfo.Character == null)
                return 0;

            long size = 17; // Header (15 bytes signature + 2 bytes offset pointer)

            foreach (var ch in FontInfo.Character)
            {
                if (ch == null)
                    continue;

                // 🔴 Skip placeholder glyphs
                if (ch.Height <= 0)
                    continue;

                size += 2; // Width
                size += 2; // Height

                int bytesPerLine = (ch.Width - 1) / 8 + 1;
                size += ch.Height * bytesPerLine;
            }

            // 🔵 IMPORTANT NOTE:
            // Offset table size does NOT count toward 64KB limit.
            // Only check that it STARTS before 65535.
            // So we do NOT add Character.Length * 2 here.

            return size;
        }

        private ushort PLACEHOLDER_W
        {
            get { return checked((ushort)XmlSettings.PlaceholderWidth); }
        }

        private ushort PLACEHOLDER_H
        {
            get { return checked((ushort)XmlSettings.PlaceholderHeight); }
        }

        // Temporary drawing-area image for placeholder glyphs.
        // IMPORTANT: This does NOT modify the glyph or store into UnscaledImage.
        private Bitmap GetDrawingAreaBitmapForGlyph(CCharInfo ch)
        {
            if (ch != null && ch.UnscaledImage is Bitmap realBmp)
                return realBmp;

            // Make a temp bitmap (black 1bpp) so Zoom/Draw don't crash and it's 8x9 (not 1x1, not 4x4).
            return new Bitmap(PLACEHOLDER_W, PLACEHOLDER_H, PixelFormat.Format1bppIndexed);
        }

        // Refresh drawing area display without materializing the glyph.
        private void RefreshDrawingAreaDisplay(CCharInfo characterinfo)
        {
            if (characterinfo == null)
                return;

            Bitmap src = GetDrawingAreaBitmapForGlyph(characterinfo);

            Bitmap scaled;
            CFontUtils.ScaleBitmap(src, out scaled, ZoomDrawingArea.Value);

            if (scaled != null)
            {
                DrawingArea.Size = scaled.Size;
                DrawingArea.Image = scaled;
                DrawingArea.Invalidate();
            }
        }

        private void RefreshPlaceholders()
        {
            foreach (PictureBox pict in CharacterPictureList)
            {
                CCharInfo item = pict.Tag as CCharInfo;

                if (item == null)
                    continue;

                // Placeholder = no bitmap data
                if (item.Width == 0 || item.Height == 0 || item.UnscaledImage == null)
                {
                    int previewW = PLACEHOLDER_W * Scalefactor;
                    int previewH = PLACEHOLDER_H * Scalefactor;

                    Bitmap placeholder = new Bitmap(previewW, previewH);
                    using (Graphics g = Graphics.FromImage(placeholder))
                    {
                        g.Clear(FlowCharacterPanel.BackColor);

                        using (Pen pen = new Pen(Color.White))
                        {
                            g.DrawRectangle(pen, 0, 0, previewW - 1, previewH - 1);
                        }
                    }

                    pict.Image = placeholder;
                    pict.Width = previewW;
                    pict.Height = previewH;
                }
            }
        }

        private void SetSelectedPreview(PictureBox pict)
        {
            if (_selectedPreview == pict)
                return;

            // repaint old + new only (fast)
            if (_selectedPreview != null)
                _selectedPreview.Invalidate();

            _selectedPreview = pict;

            if (_selectedPreview != null)
                _selectedPreview.Invalidate();
        }

        public void LoadFontFromMemory(CFontInfo font)
        {
            this.FontInfo = font;

            if (grayscalePalette == null)
                InitializeGrayscalePalette();

            if (FontInfo != null)
                TxtGlyphRange.Text = (FontInfo.NumberOfCharacters - 1).ToString();

            BtnBitDepth.Enabled = FontInfo is CWFNFontInfo || FontInfo is CSCIFontInfo;

            RebuildPage();
        }
    }
}
