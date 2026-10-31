
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AGS.Plugin.FontEditor
{
    public class PngExportDialog : Form
    {
        // Change this value to adjust the default everywhere.
        public const int DefaultGlyphsPerRow = 16;

        private RadioButton radioFixed;
        private RadioButton radioOriginal;
        private CheckBox checkPlaceholders;
        private NumericUpDown numericGlyphsPerRow;
        private Button buttonSeparatorColor;
        private CheckBox checkTransparent;

        private Color separatorColor = Color.Magenta;

        public bool FixedSize
        {
            get { return radioFixed.Checked; }
        }

        public bool IncludePlaceholders
        {
            get { return checkPlaceholders.Checked; }
        }

        public int GlyphsPerRow
        {
            get { return (int)numericGlyphsPerRow.Value; }
        }

        public Color SeparatorColor
        {
            get
            {
                return checkTransparent.Checked
                    ? Color.Transparent
                    : separatorColor;
            }
        }

        public Color SelectedSeparatorColor
        {
            get { return separatorColor; }
        }

        public bool TransparentSeparator
        {
            get { return checkTransparent.Checked; }
        }

        public PngExportDialog(Settings settings)
        {
            separatorColor = settings.PngSeparatorColor;
            Text = "Export PNG Glyph Sheet";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(365, 289);
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            // Glyphs per row - top of dialog.
            Label labelGlyphsPerRow = new Label();
            labelGlyphsPerRow.Text = "Glyphs Per Row:";
            labelGlyphsPerRow.SetBounds(17, 17, 110, 22);

            numericGlyphsPerRow = new NumericUpDown();
            numericGlyphsPerRow.Minimum = 1;
            numericGlyphsPerRow.Maximum = 256;
            numericGlyphsPerRow.Value = Math.Max(1, Math.Min(256, settings.PngGlyphsPerRow));
            numericGlyphsPerRow.SetBounds(130, 14, 65, 23);

            // Glyph layout.
            GroupBox groupLayout = new GroupBox();
            groupLayout.Text = "Glyph Layout";
            groupLayout.SetBounds(12, 48, 341, 75);

            radioOriginal = new RadioButton();
            radioOriginal.Text = "Original Size (1-pixel separators)";
            radioOriginal.SetBounds(12, 20, 310, 22);

            radioFixed = new RadioButton();
            radioFixed.Text = "Fixed Size (auto cell dimensions based on highest glyph)";
            radioFixed.SetBounds(12, 45, 310, 22);

            // Restore the saved selection.
            radioFixed.Checked = settings.PngFixedSize;
            radioOriginal.Checked = !settings.PngFixedSize;

            groupLayout.Controls.Add(radioOriginal);
            groupLayout.Controls.Add(radioFixed);

            // Separator settings.
            GroupBox groupSeparator = new GroupBox();
            groupSeparator.Text = "Pick a Separator Color";
            groupSeparator.SetBounds(12, 133, 341, 70);

            buttonSeparatorColor = new Button();
            buttonSeparatorColor.BackColor = separatorColor;
            buttonSeparatorColor.FlatStyle = FlatStyle.Flat;
            buttonSeparatorColor.FlatAppearance.BorderColor = Color.Gray;
            buttonSeparatorColor.SetBounds(15, 28, 25, 25);
            buttonSeparatorColor.Click += ButtonSeparatorColor_Click;
            buttonSeparatorColor.Paint += ButtonSeparatorColor_Paint;

            checkTransparent = new CheckBox();
            checkTransparent.Checked = settings.PngTransparent;
            checkTransparent.Text = "Transparent";
            checkTransparent.SetBounds(53, 29, 220, 23);
            checkTransparent.CheckedChanged += (sender, e) =>
            {
                UpdateSeparatorControls();
            };

            groupSeparator.Controls.Add(buttonSeparatorColor);
            groupSeparator.Controls.Add(checkTransparent);

            radioFixed.CheckedChanged += (sender, e) =>
            {
                UpdateSeparatorControls();
            };

            // Placeholder export settings.
            checkPlaceholders = new CheckBox();
            checkPlaceholders.Checked = settings.PngIncludePlaceholders;
            checkPlaceholders.Text = "Export Placeholder Glyphs";
            checkPlaceholders.SetBounds(17, 216, 340, 23);

            checkPlaceholders.CheckedChanged += (sender, e) =>
            {
                UpdateExportModeLabel();
            };

            // Dialog buttons.
            Button buttonExport = new Button();
            buttonExport.Text = "Export";
            buttonExport.DialogResult = DialogResult.OK;
            buttonExport.SetBounds(197, 252, 75, 25);

            Button buttonCancel = new Button();
            buttonCancel.Text = "Cancel";
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.SetBounds(278, 252, 75, 25);

            Controls.Add(labelGlyphsPerRow);
            Controls.Add(numericGlyphsPerRow);
            Controls.Add(groupLayout);
            Controls.Add(groupSeparator);
            Controls.Add(checkPlaceholders);
            Controls.Add(buttonExport);
            Controls.Add(buttonCancel);

            AcceptButton = buttonExport;
            CancelButton = buttonCancel;

            UpdateSeparatorControls();
            UpdateExportModeLabel();
        }

        private void UpdateSeparatorControls()
        {
            checkTransparent.Enabled = true;
            buttonSeparatorColor.Enabled = !checkTransparent.Checked;

            buttonSeparatorColor.Invalidate();
        }

        private void UpdateExportModeLabel()
        {
            checkPlaceholders.Text = checkPlaceholders.Checked
                ? "Export placeholder glyphs (single file with black placeholders)"
                : "Export placeholder glyphs (multiple files with actual glyphs only)";
        }

        private void ButtonSeparatorColor_Paint(object sender, PaintEventArgs e)
        {
            if (!checkTransparent.Checked)
                return;

            Rectangle area = buttonSeparatorColor.ClientRectangle;

            // Leave one pixel around the edges for the border.
            area.Inflate(-1, -1);

            const int squareSize = 6;

            using (SolidBrush light = new SolidBrush(Color.White))
            using (SolidBrush dark = new SolidBrush(Color.LightGray))
            {
                for (int y = area.Top; y < area.Bottom; y += squareSize)
                {
                    for (int x = area.Left; x < area.Right; x += squareSize)
                    {
                        bool isDark =
                            (((x - area.Left) / squareSize) +
                             ((y - area.Top) / squareSize)) % 2 == 0;

                        e.Graphics.FillRectangle(
                            isDark ? dark : light,
                            x,
                            y,
                            Math.Min(squareSize, area.Right - x),
                            Math.Min(squareSize, area.Bottom - y));
                    }
                }
            }

            // Preserve the swatch's visible border.
            ControlPaint.DrawBorder(
                e.Graphics,
                buttonSeparatorColor.ClientRectangle,
                Color.Gray,
                ButtonBorderStyle.Solid);
        }

        private void ButtonSeparatorColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = separatorColor;
                dialog.FullOpen = true;
                dialog.AnyColor = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                Color selected = dialog.Color;

                if (IsForbiddenSeparatorColor(selected))
                {
                    MessageBox.Show(
                        this,
                        "This color is one of the 4-bit shading colors and can't be used for a separator - please select another.",
                        "Invalid Separator Color",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                separatorColor = selected;
                buttonSeparatorColor.BackColor = separatorColor;
            }
        }

        private static bool IsForbiddenSeparatorColor(Color color)
        {
            if (color.A == 0)
                return false;

            return color.R == color.G &&
                   color.G == color.B &&
                   color.R % 17 == 0;
        }
    }
}
