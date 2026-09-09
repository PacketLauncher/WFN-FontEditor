using System;
using System.Drawing;
using System.Windows.Forms;
using AGS.Plugin.FontEditor;

namespace WFN_FontEditor
{
    public partial class MainWindow : Form
    {
        private TabPage _draggedTab = null;
        private TabPage _pressedTab = null;
        private int _lastDrawnDropTabIndex = -1;

        private TabPage _pressedCloseTab = null;

        private Point _tabDragStartPoint;
        private bool _tabDragging = false;

        private int _dropTabIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            SetupTabClosingUI();
            
            this.AllowDrop = true;
            this.DragEnter += MainWindow_DragEnter;
            this.DragDrop += MainWindow_DragDrop;

            TabControl.AllowDrop = true;
            TabControl.DragEnter += MainWindow_DragEnter;
            TabControl.DragDrop += MainWindow_DragDrop;

            this.StartPosition = FormStartPosition.CenterScreen;

            // Make window wider without breaking layout
            this.Width += 60;   // increase by 60 pixels

            // Optional: prevent shrinking too small
            this.MinimumSize = new Size(this.Width, this.Height);

            this.Shown += (s, e) => BtnNew.Focus();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.N))
            {
                BtnNew_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.O))
            {
                BtnOpen_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.W))
            {
                BtnClose_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == Keys.F1)
            {
                BtnAbout_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.S))
            {
                BtnSave_Click(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.Shift | Keys.S))
            {
                BtnSaveAs_Click(this, EventArgs.Empty);
                return true;
            }

            if (TabControl.SelectedTab != null &&
                TabControl.SelectedTab.Controls.Count > 0)
            {
                FontEditorPane activePane =
                    TabControl.SelectedTab.Controls[0] as FontEditorPane;

                if (activePane != null)
                {
                    if (keyData == (Keys.Control | Keys.Z))
                    {
                        if (activePane.UndoSelectedGlyph())
                            return true;
                    }

                    if (keyData == (Keys.Control | Keys.Y))
                    {
                        if (activePane.RedoSelectedGlyph())
                            return true;
                    }

                    if (keyData == (Keys.Control | Keys.C))
                    {
                        if (activePane.CopySelectedGlyph())
                            return true;
                    }

                    if (keyData == (Keys.Control | Keys.V))
                    {
                        if (activePane.PasteSelectedGlyph())
                            return true;
                    }

                    if (keyData == Keys.Delete)
                    {
                        if (activePane.RemoveSelectedGlyphData())
                            return true;
                    }

                    if (keyData == Keys.Insert)
                    {
                        if (activePane.InsertGlyphAtSelected())
                            return true;
                    }

                    if (keyData == (Keys.Shift | Keys.Delete))
                    {
                        if (activePane.DeleteSelectedGlyph())
                            return true;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            int unicodeRange = 65536;

            // Generate unique untitled name
            int counter = 0;
            string baseName;
            bool exists;

            do
            {
                baseName = $"AGSFNT{counter}.WFN";
                exists = false;

                foreach (TabPage tab in TabControl.TabPages)
                {
                    if (tab.Text.Replace("*", "") == baseName)
                    {
                        exists = true;
                        break;
                    }
                }

                counter++;

            } while (exists);

            CWFNFontInfo newFont = new CWFNFontInfo();
            newFont.FontPath = "";
            newFont.FontName = baseName;
            newFont.NumberOfCharacters = unicodeRange;
            newFont.Character = new CCharInfo[unicodeRange];

            for (int i = 0; i < unicodeRange; i++)
            {
                CCharInfo ch = new CCharInfo();
                ch.Index = i;
                ch.Width = 0;
                ch.Height = 0;
                ch.ByteLines = new byte[0];
                newFont.Character[i] = ch;
            }

            FontEditorPane fep = new FontEditorPane("", "", baseName);
            fep.LoadFontFromMemory(newFont);

            fep.AllowDrop = true;
            fep.DragEnter += MainWindow_DragEnter;
            fep.DragDrop += MainWindow_DragDrop;

            TabPage tp = new TabPage();
            tp.Text = baseName + "*";
            tp.Tag = null;
            tp.Controls.Add(fep);
            fep.Dock = DockStyle.Fill;
            fep.Tag = tp;

            // Subscribe only after fep.Tag points to its TabPage.
            fep.OnFontModified += new EventHandler(fep_OnFontModified);

            TabControl.TabPages.Add(tp);
            TabControl.SelectedTab = tp;
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Open Font";
                dialog.Filter =
                    "WFN and SCI Fonts (*.wfn; FONT.*)|*.wfn;FONT.*|WFN Font (*.wfn)|*.wfn|SCI Font (FONT.*)|FONT.*|All Files (*.*)|*.*";
                dialog.Multiselect = true;
                dialog.RestoreDirectory = true;
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                foreach (string fullPath in dialog.FileNames)
                    OpenFontFileInTab(fullPath);
            }
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            string[] files =
                e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files == null || files.Length == 0)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // Accept only if every dragged item is a WFN file.
            foreach (string file in files)
            {
                if (!System.IO.File.Exists(file) ||
                    !string.Equals(
                        System.IO.Path.GetExtension(file),
                        ".wfn",
                        StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.None;
                    return;
                }
            }

            e.Effect = DragDropEffects.Copy;
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files =
                e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files == null)
                return;

            foreach (string file in files)
            {
                if (System.IO.File.Exists(file) &&
                    string.Equals(
                        System.IO.Path.GetExtension(file),
                        ".wfn",
                        StringComparison.OrdinalIgnoreCase))
                {
                    OpenFontFileInTab(file);
                }
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab == null)
                return;

            int index = TabControl.SelectedIndex;

            if (index >= 0)
                CloseTabAt(index);
        }

        private void OpenFontFileInTab(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return;

            string dir = System.IO.Path.GetDirectoryName(fullPath);
            string file = System.IO.Path.GetFileName(fullPath);

            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(file))
                return;

            // If already open -> focus existing tab
            foreach (TabPage existing in TabControl.TabPages)
            {
                if (existing.Tag is string tagPath &&
                    string.Equals(tagPath, fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    TabControl.SelectedTab = existing;
                    return;
                }
            }

            FontEditorPane fep = new FontEditorPane(dir, file, file);
            fep.OnFontModified += new EventHandler(fep_OnFontModified);

            fep.AllowDrop = true;
            fep.DragEnter += MainWindow_DragEnter;
            fep.DragDrop += MainWindow_DragDrop;

            TabPage tp = new TabPage();
            tp.Text = file;
            tp.Tag = fullPath;
            tp.Controls.Add(fep);
            fep.Dock = DockStyle.Fill;
            fep.Tag = tp;

            TabControl.TabPages.Add(tp);
            TabControl.SelectedTab = tp;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab == null)
                return;

            FontEditorPane activePane = TabControl.SelectedTab.Controls.Count > 0
                ? TabControl.SelectedTab.Controls[0] as FontEditorPane
                : null;

            if (activePane == null)
                return;

            // If no file path yet → redirect to Save As
            if (!(TabControl.SelectedTab.Tag is string fullPath) || string.IsNullOrEmpty(fullPath))
            {
                BtnSaveAs_Click(sender, e);
                return;
            }

            try
            {
                activePane.CurrentFontInfo.Write(fullPath);
                activePane.MarkAsSaved();

                TabControl.SelectedTab.Text = System.IO.Path.GetFileName(fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to save font:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnSaveAs_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab == null)
                return;

            FontEditorPane activePane = TabControl.SelectedTab.Controls.Count > 0
                ? TabControl.SelectedTab.Controls[0] as FontEditorPane
                : null;

            if (activePane == null)
                return;

            CFontInfo font = activePane.CurrentFontInfo;
            if (font == null)
                return;

            bool isWfn = font is CWFNFontInfo;
            bool isSci = font is CSCIFontInfo;

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                if (isWfn)
                {
                    dialog.Filter = "WFN Font Files (*.wfn)|*.wfn|All Files (*.*)|*.*";
                    dialog.DefaultExt = "wfn";
                }
                else if (isSci)
                {
                    dialog.Filter = "SCI Font Files (FONT.xxx)|FONT.*|All Files (*.*)|*.*";
                    dialog.DefaultExt = "";
                }
                else
                {
                    dialog.Filter = "All Files (*.*)|*.*";
                }

                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;

                // 🔥 THIS FIXES THE EMPTY FILENAME
                dialog.FileName = TabControl.SelectedTab.Text.Replace("*", "");

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    font.Write(dialog.FileName);

                    activePane.MarkAsSaved();

                    TabControl.SelectedTab.Tag = dialog.FileName;

                    // Explicitly replace the tab caption with the actual saved filename.
                    // This guarantees that any old * is removed.
                    string savedFileName =
                        System.IO.Path.GetFileName(dialog.FileName);

                    TabControl.SelectedTab.Text = savedFileName;

                    TabControl.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Failed to save font:\n\n" + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        // Keep your existing modified-marker logic (*)
        void fep_OnFontModified(object sender, System.EventArgs e)
        {
            TabPage tp = (TabPage)((FontEditorPane)sender).Tag;
            MyEventArgs me = (MyEventArgs)e;

            if (tp.Text.Contains("*") && me.Modified == false)
            {
                tp.Text = tp.Text.Replace("*", "");
            }
            else if (!tp.Text.Contains("*") && me.Modified == true)
            {
                tp.Text += "*";
            }
        }

        // ----------------------------
        // Closable tabs (X + middle click)
        // ----------------------------
        private void SetupTabClosingUI()
        {
            TabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            TabControl.Padding = new Point(18, 4);

            TabControl.DrawItem += TabControl_DrawItem;
            TabControl.MouseDown += TabControl_MouseDown;
            // Tab dragging/reordering
            TabControl.MouseMove += TabControl_MouseMove;
            TabControl.MouseUp += TabControl_MouseUp;
            TabControl.Selecting += TabControl_Selecting;
        }

        private void TabControl_Selecting(
            object sender,
            TabControlCancelEventArgs e)
        {
            // Don't let Windows switch the displayed font merely because
            // the left mouse button went down on another tab.
            //
            // MouseUp will perform the selection explicitly.
            if (Control.MouseButtons == MouseButtons.Left)
            {
                e.Cancel = true;
            }
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= TabControl.TabPages.Count)
                return;

            TabPage page = TabControl.TabPages[e.Index];
            Rectangle tabRect = TabControl.GetTabRect(e.Index);

            // Background
            e.Graphics.FillRectangle(SystemBrushes.Control, tabRect);

            // Text
            Rectangle textRect = new Rectangle(tabRect.X + 2, tabRect.Y + 4, tabRect.Width - 18, tabRect.Height - 4);
            TextRenderer.DrawText(e.Graphics, page.Text, Font, textRect, SystemColors.ControlText, TextFormatFlags.Left);

            // X button
            Rectangle closeRect = GetCloseRect(tabRect);
            TextRenderer.DrawText(e.Graphics, "x", Font, closeRect, SystemColors.ControlText,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Border highlight on selected
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                ControlPaint.DrawBorder(e.Graphics, tabRect, SystemColors.Highlight, ButtonBorderStyle.Solid);
            }

            // Draw insertion indicator while dragging a tab.
            if (_tabDragging &&
                _draggedTab != null &&
                _dropTabIndex >= 0 &&
                _dropTabIndex < TabControl.TabPages.Count)
            {
                int draggedIndex =
                    TabControl.TabPages.IndexOf(_draggedTab);

                bool draggingRight =
                    draggedIndex >= 0 &&
                    _dropTabIndex > draggedIndex;

                int drawOnTabIndex;

                if (draggingRight &&
                    _dropTabIndex < TabControl.TabPages.Count - 1)
                {
                    // When dragging right, draw during the NEXT tab's
                    // paint pass so that tab cannot paint over the line.
                    drawOnTabIndex = _dropTabIndex + 1;
                }
                else
                {
                    drawOnTabIndex = _dropTabIndex;
                }

                if (e.Index == drawOnTabIndex)
                {
                    Rectangle dropRect =
                        TabControl.GetTabRect(_dropTabIndex);

                    int indicatorX;

                    if (draggingRight)
                    {
                        // Dragging RIGHT:
                        // show the insertion point AFTER the target tab.
                        indicatorX = dropRect.Right + 1;
                    }
                    else
                    {
                        // Dragging LEFT:
                        // show the insertion point BEFORE the target tab.
                        if (_dropTabIndex > 0)
                        {
                            Rectangle previousRect =
                                TabControl.GetTabRect(_dropTabIndex - 1);

                            indicatorX = previousRect.Right + 1;
                        }
                        else
                        {
                            indicatorX = dropRect.Left;
                        }
                    }

                    using (Pen pen = new Pen(SystemColors.Highlight, 3))
                    {
                        e.Graphics.DrawLine(
                            pen,
                            indicatorX,
                            dropRect.Top + 2,
                            indicatorX,
                            dropRect.Bottom - 2);
                    }
                }
            }
        }

        private void InvalidateDropIndicator(int tabIndex)
        {
            if (tabIndex < 0 ||
                tabIndex >= TabControl.TabPages.Count)
            {
                return;
            }

            Rectangle rect =
                TabControl.GetTabRect(tabIndex);

            int draggedIndex =
                (_draggedTab != null)
                    ? TabControl.TabPages.IndexOf(_draggedTab)
                    : -1;

            int x;

            if (draggedIndex >= 0 &&
                tabIndex > draggedIndex)
            {
                // Dragging RIGHT:
                // indicator is after the target tab.
                x = rect.Right + 1;
            }
            else
            {
                // Dragging LEFT:
                // indicator is before the target tab.
                if (tabIndex > 0)
                {
                    Rectangle previousRect =
                        TabControl.GetTabRect(tabIndex - 1);

                    x = previousRect.Right + 1;
                }
                else
                {
                    x = rect.Left;
                }
            }

            Rectangle indicatorArea =
                new Rectangle(
                    x - 3,
                    rect.Top,
                    7,
                    rect.Height);

            TabControl.Invalidate(indicatorArea);
        }

        private void TabControl_MouseDown(object sender, MouseEventArgs e)
        {
            _draggedTab = null;
            _pressedTab = null;
            _pressedCloseTab = null;
            _tabDragging = false;
            _dropTabIndex = -1;

            for (int i = 0; i < TabControl.TabPages.Count; i++)
            {
                Rectangle tabRect = TabControl.GetTabRect(i);

                if (!tabRect.Contains(e.Location))
                    continue;

                TabPage tab = TabControl.TabPages[i];

                if (e.Button == MouseButtons.Middle)
                {
                    CloseTabAt(i);
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    Rectangle closeRect = GetCloseRect(tabRect);

                    if (closeRect.Contains(e.Location))
                    {
                        // Remember it, but close only on MouseUp.
                        _pressedCloseTab = tab;
                        return;
                    }

                    _pressedTab = tab;
                    _draggedTab = tab;
                    _tabDragStartPoint = e.Location;

                    _dropTabIndex = i;
                }

                return;
            }
        }

        private void TabControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (_pressedCloseTab != null)
                return;

            if (_draggedTab == null)
                return;

            if (!_tabDragging)
            {
                Size dragSize = SystemInformation.DragSize;

                Rectangle dragRect = new Rectangle(
                    _tabDragStartPoint.X - dragSize.Width / 2,
                    _tabDragStartPoint.Y - dragSize.Height / 2,
                    dragSize.Width,
                    dragSize.Height);

                if (dragRect.Contains(e.Location))
                    return;

                _tabDragging = true;
            }

            // Merely remember where the tab should eventually go.
            // Do NOT alter TabPages while dragging.
            for (int i = 0; i < TabControl.TabPages.Count; i++)
            {
                Rectangle tabRect = TabControl.GetTabRect(i);

                if (tabRect.Contains(e.Location))
                {
                    if (_dropTabIndex != i)
                    {
                        int oldDropIndex = _dropTabIndex;

                        _dropTabIndex = i;

                        InvalidateDropIndicator(oldDropIndex);
                        InvalidateDropIndicator(_dropTabIndex);

                        _lastDrawnDropTabIndex = _dropTabIndex;
                    }

                    return;
                }
            }

            // If dragged beyond the left edge, target the first tab.
            if (TabControl.TabPages.Count > 0)
            {
                Rectangle firstRect = TabControl.GetTabRect(0);

                if (e.X < firstRect.Left)
                {
                    if (_dropTabIndex != 0)
                    {
                        int oldDropIndex = _dropTabIndex;

                        _dropTabIndex = 0;

                        InvalidateDropIndicator(oldDropIndex);
                        InvalidateDropIndicator(_dropTabIndex);

                        _lastDrawnDropTabIndex = _dropTabIndex;
                    }

                    return;
                }

                // If dragged beyond the right edge, target the final tab.
                int lastIndex = TabControl.TabPages.Count - 1;
                Rectangle lastRect = TabControl.GetTabRect(lastIndex);

                if (e.X > lastRect.Right)
                {
                    if (_dropTabIndex != lastIndex)
                    {
                        int oldDropIndex = _dropTabIndex;

                        _dropTabIndex = lastIndex;

                        InvalidateDropIndicator(oldDropIndex);
                        InvalidateDropIndicator(_dropTabIndex);

                        _lastDrawnDropTabIndex = _dropTabIndex;
                    }
                }
            }
        }

        private void TabControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            // X button handling.
            if (_pressedCloseTab != null)
            {
                int closeIndex =
                    TabControl.TabPages.IndexOf(_pressedCloseTab);

                if (closeIndex >= 0)
                {
                    Rectangle tabRect =
                        TabControl.GetTabRect(closeIndex);

                    Rectangle closeRect =
                        GetCloseRect(tabRect);

                    // Only close if released over the same X.
                    if (closeRect.Contains(e.Location))
                    {
                        CloseTabAt(closeIndex);
                    }
                }

                ResetTabDragState();
                return;
            }

            if (_draggedTab != null)
            {
                if (_tabDragging)
                {
                    int oldIndex =
                        TabControl.TabPages.IndexOf(_draggedTab);

                    int newIndex = _dropTabIndex;

                    if (oldIndex >= 0 &&
                        newIndex >= 0 &&
                        newIndex < TabControl.TabPages.Count &&
                        oldIndex != newIndex)
                    {
                        TabControl.SuspendLayout();

                        try
                        {
                            TabControl.TabPages.Remove(_draggedTab);

                            // Removal reduced Count by one.
                            if (newIndex > TabControl.TabPages.Count)
                                newIndex = TabControl.TabPages.Count;

                            TabControl.TabPages.Insert(
                                newIndex,
                                _draggedTab);
                        }
                        finally
                        {
                            TabControl.ResumeLayout();
                        }
                    }

                    // Only now display the dragged font.
                    TabControl.SelectedTab = _draggedTab;
                }
                else if (_pressedTab != null)
                {
                    // Ordinary click: select on release.
                    TabControl.SelectedTab = _pressedTab;
                }
            }

            TabControl.Invalidate();

            ResetTabDragState();
        }

        private void ResetTabDragState()
        {
            InvalidateDropIndicator(_dropTabIndex);

            _draggedTab = null;
            _pressedTab = null;
            _pressedCloseTab = null;

            _tabDragging = false;
            _dropTabIndex = -1;
            _lastDrawnDropTabIndex = -1;
        }

        private Rectangle GetCloseRect(Rectangle tabRect)
        {
            // Small "x" area on the right side of tab
            int size = 12;
            return new Rectangle(
                tabRect.Right - size - 4,
                tabRect.Top + (tabRect.Height - size) / 2,
                size,
                size);
        }

        private DialogResult ShowUnsavedChangesDialog(string filename)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Unsaved changes";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.ClientSize = new Size(360, 90);

                // No title-bar X, maximize or minimize buttons.
                dialog.ControlBox = false;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.ShowInTaskbar = false;

                PictureBox warningIcon = new PictureBox();
                warningIcon.Size = new Size(32, 32);
                warningIcon.Location = new Point(18, 12);
                warningIcon.Paint += (s, e) =>
                {
                    e.Graphics.DrawIcon(SystemIcons.Warning, 0, 0);
                };

                Label message = new Label();
                message.Text = "Save changes to the \"" + filename + "\" before closing?";
                message.AutoSize = true;
                message.Location = new Point(
                    warningIcon.Right + 10,
                    warningIcon.Top + 8);

                Button buttonYes = new Button();
                buttonYes.Text = "Yes";
                buttonYes.DialogResult = DialogResult.Yes;
                buttonYes.Size = new Size(75, 23);
                buttonYes.Location = new Point(63, 55);

                Button buttonNo = new Button();
                buttonNo.Text = "No";
                buttonNo.DialogResult = DialogResult.No;
                buttonNo.Size = new Size(75, 23);
                buttonNo.Location = new Point(143, 55);

                Button buttonCancel = new Button();
                buttonCancel.Text = "Cancel";
                buttonCancel.DialogResult = DialogResult.Cancel;
                buttonCancel.Size = new Size(75, 23);
                buttonCancel.Location = new Point(223, 55);

                dialog.Controls.Add(warningIcon);
                dialog.Controls.Add(message);
                dialog.Controls.Add(buttonYes);
                dialog.Controls.Add(buttonNo);
                dialog.Controls.Add(buttonCancel);

                // Enter = Yes.
                dialog.AcceptButton = buttonYes;

                // ESC = Cancel.
                dialog.CancelButton = buttonCancel;

                return dialog.ShowDialog(this);
            }
        }
        private void CloseTabAt(int index)
        {
            if (index < 0 || index >= TabControl.TabPages.Count)
                return;

            TabPage tp = TabControl.TabPages[index];

            bool modified = tp.Text.Contains("*");

            if (modified)
            {
                string filename = tp.Text.TrimEnd('*').TrimEnd();

                DialogResult result = ShowUnsavedChangesDialog(filename);

                // Cancel = return to editing.
                if (result == DialogResult.Cancel)
                    return;

                // Yes = save first, then close.
                if (result == DialogResult.Yes)
                {
                    // BtnSave_Click works on the selected tab, so make sure
                    // the tab being closed is the selected one.
                    TabControl.SelectedTab = tp;

                    BtnSave_Click(this, EventArgs.Empty);

                    // If the * is still present, saving either failed
                    // or Save As was cancelled. Do not close the tab.
                    if (tp.Text.Contains("*"))
                        return;
                }

                // No = do not save; simply continue and close.
            }

            TabControl.TabPages.RemoveAt(index);
            tp.Dispose();
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            if (TabControl.SelectedTab == null)
                return;

            FontEditorPane activePane = TabControl.SelectedTab.Controls.Count > 0
                ? TabControl.SelectedTab.Controls[0] as FontEditorPane
                : null;

            if (activePane == null)
                return;

            CFontInfo oldfont = activePane.CurrentFontInfo;
            if (oldfont == null)
                return;

            // fullPath may be null for "New" / unsaved tabs
            string fullPath = TabControl.SelectedTab.Tag as string;

            // Choose output folder:
            // - if we have a source path -> same folder
            // - else -> ask user for a folder
            string paneFilepath = null;

            if (!string.IsNullOrEmpty(fullPath))
            {
                paneFilepath = System.IO.Path.GetDirectoryName(fullPath);
            }
            else
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Select a folder to save the converted font";

                    if (folderDialog.ShowDialog() != DialogResult.OK)
                        return;

                    paneFilepath = folderDialog.SelectedPath;
                }
            }

            // Decide conversion direction from IN-MEMORY type (works for unsaved tabs too)
            bool isWfn = oldfont is CWFNFontInfo;
            bool isSci = oldfont is CSCIFontInfo;

            // Fallback (shouldn't normally happen): try extension if we have a path
            if (!isWfn && !isSci && !string.IsNullOrEmpty(fullPath))
            {
                string ext = System.IO.Path.GetExtension(fullPath).ToLower();
                isWfn = (ext == ".wfn");
                isSci = !isWfn;
            }

            if (isWfn)
            {
                // WFN -> SCI
                DialogResult result = MessageBox.Show(
                    "You're about to convert this WFN font into a SCI font. This will create a new SCI font (FONT.xxx) and open it in a new tab.\n\nContinue?",
                    "Warning",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.OK)
                    return;

                // Find next free FONT.xxx
                int number = -1;
                for (int i = 0; i <= 999; i++)
                {
                    string candidate = System.IO.Path.Combine(paneFilepath, "FONT." + i.ToString("000"));
                    if (!System.IO.File.Exists(candidate))
                    {
                        number = i;
                        break;
                    }
                }

                if (number == -1)
                {
                    MessageBox.Show(
                        "This folder already contains the maximum number of SCI fonts (FONT.000–FONT.999).\n\nConversion aborted.",
                        "Limit Reached",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string newname = System.IO.Path.Combine(paneFilepath, "FONT." + number.ToString("000"));

                int glyphLimit = 256;

                int glyphCount = (oldfont.Character != null)
                    ? oldfont.Character.Length
                    : oldfont.NumberOfCharacters;

                if (glyphCount > glyphLimit)
                {
                    DialogResult limitResult = MessageBox.Show(
                        $"Warning!\n\nSCI fonts are limited to 256 glyphs (0–255).\n\nThe current WFN font glyphs range is {glyphCount}.\n\nAll glyphs beyond index 256 will NOT be saved.\n\nContinue?",
                        "SCI Glyph Limit",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning);

                    if (limitResult != DialogResult.OK)
                        return;
                }

                int finalCount = Math.Min(glyphCount, glyphLimit);

                CFontInfo newfont = new CSCIFontInfo();
                newfont.FontPath = paneFilepath;
                newfont.FontName = "FONT." + number.ToString("000");
                newfont.NumberOfCharacters = finalCount;
                newfont.Character = new CCharInfo[finalCount];

                for (int i = 0; i < finalCount; i++)
                {
                    CCharInfo src = (oldfont.Character != null && i < oldfont.Character.Length)
                        ? oldfont.Character[i]
                        : null;

                    CCharInfo dst = new CCharInfo();
                    dst.Index = i;

                    if (src != null)
                    {
                        dst.Width = src.Width;
                        dst.Height = src.Height;

                        if (src.ByteLines != null)
                        {
                            dst.ByteLines = new byte[src.ByteLines.Length];
                            Array.Copy(src.ByteLines, dst.ByteLines, src.ByteLines.Length);
                        }
                        else
                        {
                            dst.ByteLines = new byte[0];
                        }
                    }
                    else
                    {
                        dst.Width = 0;
                        dst.Height = 0;
                        dst.ByteLines = new byte[0];
                    }

                    newfont.Character[i] = dst;
                }

                using (var fs = System.IO.File.Create(newname)) { }
                newfont.Write(newname);
                OpenFontFileInTab(newname);
            }
            else
            {
                // SCI -> WFN
                DialogResult result = MessageBox.Show(
                    "You're about to convert this SCI font into a WFN font. This will create a new WFN font (AGSFNTx.WFN) and open it in a new tab.\n\nContinue?",
                    "Warning",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.OK)
                    return;

                int number = -1;
                for (int i = 0; i <= 999; i++)
                {
                    string candidate = System.IO.Path.Combine(paneFilepath, "AGSFNT" + i.ToString() + ".WFN");
                    if (!System.IO.File.Exists(candidate))
                    {
                        number = i;
                        break;
                    }
                }

                if (number == -1)
                {
                    MessageBox.Show(
                        "This folder already contains the maximum number of WFN fonts (AGSFNT0–AGSFNT999).\n\nConversion aborted.",
                        "Limit Reached",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string newname = System.IO.Path.Combine(paneFilepath, "AGSFNT" + number.ToString() + ".WFN");

                int finalCount = (oldfont.Character != null)
                    ? oldfont.Character.Length
                    : oldfont.NumberOfCharacters;

                CFontInfo newfont = new CWFNFontInfo();
                newfont.FontPath = paneFilepath;
                newfont.FontName = "AGSFNT" + number.ToString() + ".WFN";
                newfont.NumberOfCharacters = finalCount;
                newfont.Character = new CCharInfo[finalCount];

                for (int i = 0; i < finalCount; i++)
                {
                    CCharInfo src = (oldfont.Character != null && i < oldfont.Character.Length)
                        ? oldfont.Character[i]
                        : null;

                    CCharInfo dst = new CCharInfo();
                    dst.Index = i;

                    if (src != null)
                    {
                        dst.Width = src.Width;
                        dst.Height = src.Height;

                        if (src.ByteLines != null)
                        {
                            dst.ByteLines = new byte[src.ByteLines.Length];
                            Array.Copy(src.ByteLines, dst.ByteLines, src.ByteLines.Length);
                        }
                        else
                        {
                            dst.ByteLines = new byte[0];
                        }
                    }
                    else
                    {
                        dst.Width = 0;
                        dst.Height = 0;
                        dst.ByteLines = new byte[0];
                    }

                    newfont.Character[i] = dst;
                }

                using (var fs = System.IO.File.Create(newname)) { }
                newfont.Write(newname);
                OpenFontFileInTab(newname);
            }
        }
        private void BtnAbout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "WFN-FontEditor (256 ASCII characters) by Rulaman:\nhttps://github.com/Rulaman/WFN-FontEditor\n\nImproved version with full Unicode support by PacketLauncher (Gal Shemesh):\nhttps://github.com/PacketLauncher/WFN-FontEditor",
                "About",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Work backwards because tabs are removed as we process them.
            for (int i = TabControl.TabPages.Count - 1; i >= 0; i--)
            {
                TabPage tab = TabControl.TabPages[i];

                if (tab.Text.Contains("*"))
                {
                    string filename = tab.Text.TrimEnd('*').TrimEnd();

                    DialogResult result = ShowUnsavedChangesDialog(filename);

                    // Cancel / ESC = abort closing the whole program.
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        TabControl.SelectedTab = tab;
                        return;
                    }

                    // Yes = save this font first.
                    if (result == DialogResult.Yes)
                    {
                        TabControl.SelectedTab = tab;

                        BtnSave_Click(this, EventArgs.Empty);

                        // If it is still marked as modified, saving failed
                        // or the user cancelled Save As.
                        if (tab.Text.Contains("*"))
                        {
                            e.Cancel = true;
                            return;
                        }
                    }

                    // Yes successfully saved, or No was chosen.
                    // Close this tab now.
                    TabControl.TabPages.Remove(tab);
                    tab.Dispose();
                }
                else
                {
                    // This font has no unsaved changes, so it can
                    // simply be closed while shutting down.
                    TabControl.TabPages.Remove(tab);
                    tab.Dispose();
                }
            }

            base.OnFormClosing(e);
        }

    }
}
