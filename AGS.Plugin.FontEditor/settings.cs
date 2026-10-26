using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace AGS.Plugin.FontEditor
{
	public class Settings
	{
        public const int DefaultPlaceholderWidth = 8;
        public const int DefaultPlaceholderHeight = 9;
        public const int MaxGlyphSize = 64;
        public Color Color;
        public bool Grid;
        public bool GridFix;
        public bool DarkMode;
        public string CustomText;
        public int PlaceholderWidth = DefaultPlaceholderWidth;
        public int PlaceholderHeight = DefaultPlaceholderHeight;
        public bool RenderUnicode = true;

        // PNG export preferences.
        public int PngGlyphsPerRow = 16;
        public bool PngFixedSize = false;
        public Color PngSeparatorColor = Color.Magenta;
        public bool PngTransparent = false;
        public bool PngIncludePlaceholders = false;

        private XmlDocument doc = new XmlDocument();
		private string Filename = @"WFN-FontEditor.xml";
		private string PartOne = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<?xml-stylesheet type=\"text/xsl\" href=\"xsl/installation.xsl\"?>\r\n<FontEditor><Color>-12566464</Color><Text>";
        private string PartTwo = "</Text><Grid>false</Grid><GridFix>false</GridFix><DarkMode>false</DarkMode><RenderUnicode>true</RenderUnicode></FontEditor>";
        private string PartComplete = "";

		private bool CreateFile(string filename)
		{
			doc.LoadXml((PartComplete));
			//Save the document to a file.
			doc.Save(filename);

			return true;
		}
		private bool LoadFile(string filename)
		{
			if ( ! System.IO.File.Exists(filename) )
			{
                string newValue = "The quick brown Fox jumped over the lazy Dog.";

                if (Settings.InputBox(
                    "Set Text to Render",
                    "Set a text string to render:",
                    ref newValue,
                    true) != DialogResult.OK)
                {
                    newValue = "";
                }

                CustomText = newValue;
                PartComplete = PartOne + System.Security.SecurityElement.Escape(CustomText ?? "") + PartTwo;

                CreateFile(filename);
			}

			doc.Load(filename);

			try { Grid = bool.Parse(((XmlNode)doc.SelectSingleNode("FontEditor/Grid")).InnerText); }
			catch { Grid = false; }

			try { GridFix = bool.Parse(((XmlNode)doc.SelectSingleNode("FontEditor/GridFix")).InnerText); }
			catch { GridFix = false; }

            try { DarkMode = bool.Parse(((XmlNode)doc.SelectSingleNode("FontEditor/DarkMode")).InnerText); }
            catch { DarkMode = false; }

            XmlNode renderUnicodeNode = doc.SelectSingleNode("FontEditor/RenderUnicode");

            bool renderUnicodeValue;
            RenderUnicode = renderUnicodeNode != null &&
                            bool.TryParse(renderUnicodeNode.InnerText, out renderUnicodeValue)
                            ? renderUnicodeValue
                            : true;

            try { CustomText = ((XmlNode)doc.SelectSingleNode("FontEditor/Text")).InnerText; }
			catch { CustomText = ""; }

            try { Color = Color.FromArgb(int.Parse(((XmlNode)doc.SelectSingleNode("FontEditor/Color")).InnerText)); }
            catch { Color = Color.Silver; }

            XmlNode widthNode = doc.SelectSingleNode("FontEditor/PlaceholderWidth");
            XmlNode heightNode = doc.SelectSingleNode("FontEditor/PlaceholderHeight");

            if (widthNode == null || heightNode == null)
            {
                string widthText = widthNode != null
                    ? widthNode.InnerText
                    : DefaultPlaceholderWidth.ToString();

                string heightText = heightNode != null
                    ? heightNode.InnerText
                    : DefaultPlaceholderHeight.ToString();

                int dialogWidth = DefaultPlaceholderWidth;
                int dialogHeight = DefaultPlaceholderHeight;

                int.TryParse(widthText, out dialogWidth);
                int.TryParse(heightText, out dialogHeight);

                if (PlaceholderSizeDialog(ref dialogWidth, ref dialogHeight) != DialogResult.OK)
                {
                    dialogWidth = DefaultPlaceholderWidth;
                    dialogHeight = DefaultPlaceholderHeight;
                }

                widthText = dialogWidth.ToString();
                heightText = dialogHeight.ToString();

                int width;
                int height;

                PlaceholderWidth = string.IsNullOrWhiteSpace(widthText)
                    ? DefaultPlaceholderWidth
                    : (int.TryParse(widthText, out width) ? Math.Max(1, width) : DefaultPlaceholderWidth);

                PlaceholderHeight = string.IsNullOrWhiteSpace(heightText)
                    ? DefaultPlaceholderHeight
                    : (int.TryParse(heightText, out height) ? Math.Max(1, height) : DefaultPlaceholderHeight);

                Write();
            }
            else
            {
                int width;
                int height;

                PlaceholderWidth = int.TryParse(widthNode.InnerText, out width)
                    ? Math.Max(1, Math.Min(MaxGlyphSize, width))
                    : DefaultPlaceholderWidth;

                PlaceholderHeight = int.TryParse(heightNode.InnerText, out height)
                    ? Math.Max(1, Math.Min(MaxGlyphSize, height))
                    : DefaultPlaceholderHeight;
            }

            // Load PNG export preferences.
            XmlNode pngRowsNode = doc.SelectSingleNode("FontEditor/PngGlyphsPerRow");
            int pngRows;
            PngGlyphsPerRow =
                pngRowsNode != null &&
                int.TryParse(pngRowsNode.InnerText, out pngRows)
                    ? Math.Max(1, Math.Min(256, pngRows))
                    : 16;

            XmlNode pngFixedNode = doc.SelectSingleNode("FontEditor/PngFixedSize");
            bool pngFixed;
            PngFixedSize =
                pngFixedNode != null &&
                bool.TryParse(pngFixedNode.InnerText, out pngFixed)
                    ? pngFixed
                    : false;

            XmlNode pngColorNode = doc.SelectSingleNode("FontEditor/PngSeparatorColor");
            int pngColor;
            PngSeparatorColor =
                pngColorNode != null &&
                int.TryParse(pngColorNode.InnerText, out pngColor)
                    ? Color.FromArgb(pngColor)
                    : Color.Magenta;

            XmlNode pngTransparentNode = doc.SelectSingleNode("FontEditor/PngTransparent");
            bool pngTransparent;
            PngTransparent =
                pngTransparentNode != null &&
                bool.TryParse(pngTransparentNode.InnerText, out pngTransparent)
                    ? pngTransparent
                    : false;

            XmlNode pngPlaceholdersNode = doc.SelectSingleNode("FontEditor/PngIncludePlaceholders");
            bool pngPlaceholders;
            PngIncludePlaceholders =
                pngPlaceholdersNode != null &&
                bool.TryParse(pngPlaceholdersNode.InnerText, out pngPlaceholders)
                    ? pngPlaceholders
                    : false;

            return true;
		}

		public bool Read()
		{
			//CreateFile(@"C:\testxml.xml");
			LoadFile(Filename);
			return true;
		}

        private void SetXmlSetting(string name, string value)
        {
            XmlNode node = doc.SelectSingleNode("FontEditor/" + name);

            if (node == null)
            {
                node = doc.CreateElement(name);
                doc.DocumentElement.AppendChild(node);
            }

            node.InnerText = value;
        }

        public bool Write()
		{
			if ( doc != null )
			{
				try { ((XmlNode)doc.SelectSingleNode("FontEditor/Grid")).InnerText = Grid.ToString(); }
				catch { ((XmlNode)doc.CreateElement("Grid")).InnerText = Grid.ToString(); }
				
				try { ((XmlNode)doc.SelectSingleNode("FontEditor/GridFix")).InnerText = GridFix.ToString(); }
				catch { ((XmlNode)doc.CreateElement("GridFix")).InnerText = GridFix.ToString(); }

                try
                {
                    ((XmlNode)doc.SelectSingleNode("FontEditor/DarkMode")).InnerText =
                        DarkMode.ToString();
                }
                catch
                {
                    XmlNode node = doc.CreateElement("DarkMode");
                    node.InnerText = DarkMode.ToString();
                    doc.DocumentElement.AppendChild(node);
                }

                try { ((XmlNode)doc.SelectSingleNode("FontEditor/Text")).InnerText = CustomText;}
				catch { ((XmlNode)doc.CreateElement("Text")).InnerText = CustomText; }

				try { ((XmlNode)doc.SelectSingleNode("FontEditor/Color")).InnerText = Color.ToArgb().ToString(); }
				catch { ((XmlNode)doc.CreateElement("Color")).InnerText = Color.ToArgb().ToString(); }

                XmlNode renderModeNode = doc.SelectSingleNode("FontEditor/RenderUnicode");

                if (renderModeNode == null)
                {
                    renderModeNode = doc.CreateElement("RenderUnicode");
                    doc.DocumentElement.AppendChild(renderModeNode);
                }

                renderModeNode.InnerText = RenderUnicode.ToString();

                XmlNode widthNode = doc.SelectSingleNode("FontEditor/PlaceholderWidth");
                if (widthNode == null)
                {
                    widthNode = doc.CreateElement("PlaceholderWidth");
                    doc.DocumentElement.AppendChild(widthNode);
                }
                widthNode.InnerText = Math.Max(1, PlaceholderWidth).ToString();

                XmlNode heightNode = doc.SelectSingleNode("FontEditor/PlaceholderHeight");
                if (heightNode == null)
                {
                    heightNode = doc.CreateElement("PlaceholderHeight");
                    doc.DocumentElement.AppendChild(heightNode);
                }
                heightNode.InnerText = Math.Max(1, PlaceholderHeight).ToString();

                // Save PNG export preferences.
                SetXmlSetting("PngGlyphsPerRow",
                    Math.Max(1, Math.Min(256, PngGlyphsPerRow)).ToString());

                SetXmlSetting("PngFixedSize", PngFixedSize.ToString());

                SetXmlSetting("PngSeparatorColor",
                    PngSeparatorColor.ToArgb().ToString());

                SetXmlSetting("PngTransparent", PngTransparent.ToString());

                SetXmlSetting("PngIncludePlaceholders",
                    PngIncludePlaceholders.ToString());

                doc.Save(Filename);
            }
			return true;
		}

        public static DialogResult PlaceholderSizeDialog(ref int width, ref int height)
        {
            using (Form form = new Form())
            {
                form.Text = "Set Default Placeholder Size";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = false;

                string description =
                    "Set the default glyph index placeholder size:\n(default is 8x9 and max is 64x64)";

                // Fit the window to the description text.
                int dialogWidth = TextRenderer.MeasureText(
                    description,
                    form.Font,
                    Size.Empty,
                    TextFormatFlags.NoPadding).Width + 24;

                form.ClientSize = new Size(dialogWidth, 155);

                Label labelDescription = new Label();
                labelDescription.Text = description;
                labelDescription.SetBounds(10, 12, dialogWidth - 20, 35);
                labelDescription.TextAlign = ContentAlignment.MiddleCenter;

                // Match the editor's Width/Height control proportions.
                int groupWidth = 100;
                int groupLeft = (dialogWidth - groupWidth) / 2;

                Label labelWidth = new Label();
                labelWidth.Text = "Width:";
                labelWidth.SetBounds(groupLeft, 52, 48, 20);

                Label labelHeight = new Label();
                labelHeight.Text = "Height:";
                labelHeight.SetBounds(groupLeft, 82, 48, 20);

                NumericUpDown numWidth = new NumericUpDown();
                numWidth.Minimum = 1;
                numWidth.Maximum = MaxGlyphSize;
                numWidth.Value = Math.Max(1, Math.Min(MaxGlyphSize, width));
                numWidth.SetBounds(groupLeft + 50, 50, 50, 22);

                NumericUpDown numHeight = new NumericUpDown();
                numHeight.Minimum = 1;
                numHeight.Maximum = MaxGlyphSize;
                numHeight.Value = Math.Max(1, Math.Min(MaxGlyphSize, height));
                numHeight.SetBounds(groupLeft + 50, 80, 50, 22);

                // Highlight the entire value when entering either field with Tab or Shift+Tab.
                numWidth.Enter += (sender, e) =>
                {
                    numWidth.Select(0, numWidth.Text.Length);
                };

                numHeight.Enter += (sender, e) =>
                {
                    numHeight.Select(0, numHeight.Text.Length);
                };

                // Reject manually typed minus signs.
                numWidth.KeyPress += (sender, e) =>
                {
                    if (e.KeyChar == '-')
                        e.Handled = true;
                };

                numHeight.KeyPress += (sender, e) =>
                {
                    if (e.KeyChar == '-')
                        e.Handled = true;
                };

                // Center the OK and Cancel buttons.
                int buttonWidth = 75;
                int buttonGap = 10;
                int buttonsLeft =
                    (dialogWidth - (buttonWidth * 2 + buttonGap)) / 2;

                Button buttonOk = new Button();
                buttonOk.Text = "OK";
                buttonOk.DialogResult = DialogResult.OK;
                buttonOk.SetBounds(buttonsLeft, 118, buttonWidth, 25);

                Button buttonCancel = new Button();
                buttonCancel.Text = "Cancel";
                buttonCancel.DialogResult = DialogResult.Cancel;
                buttonCancel.SetBounds(
                    buttonsLeft + buttonWidth + buttonGap,
                    118, buttonWidth, 25);

                form.Controls.AddRange(new Control[]
                {
            labelDescription,
            labelWidth,
            labelHeight,
            numWidth,
            numHeight,
            buttonOk,
            buttonCancel
                });

                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                // Start with the Width value selected and ready for typing.
                form.Shown += (sender, e) =>
                {
                    numWidth.Focus();
                    numWidth.Select(0, numWidth.Text.Length);
                };

                DialogResult result = form.ShowDialog();

                if (result == DialogResult.OK)
                {
                    width = (int)numWidth.Value;
                    height = (int)numHeight.Value;
                }

                return result;
            }
        }

        public static DialogResult InputBox(
			string title,
			string promptText,
			ref string value,
			bool showReverseButton = false)
        {
			Form form = new Form();
			Label label = new Label();
			TextBox textBox = new TextBox();
			Button buttonOk = new Button();
			Button buttonCancel = new Button();
            Button buttonReverse = new Button();

            form.Text = title;
			label.Text = promptText;
			textBox.Text = value;

			buttonOk.Text = "OK";
			buttonCancel.Text = "Cancel";
			buttonOk.DialogResult = DialogResult.OK;
			buttonCancel.DialogResult = DialogResult.Cancel;

            buttonReverse.Text = "Reverse";
            buttonReverse.Size = new System.Drawing.Size(75, 23);
            buttonReverse.Visible = showReverseButton;

            buttonReverse.Click += delegate
            {
                char[] chars = textBox.Text.ToCharArray();
                Array.Reverse(chars);
                textBox.Text = new string(chars);

                // Keep focus in the textbox and select the reversed result.
                textBox.Focus();
                textBox.SelectAll();
            };

            label.SetBounds(9, 20, 372, 13);
			textBox.SetBounds(12, 36, 372, 20);
            buttonReverse.SetBounds(12, 72, 75, 23);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);
            label.AutoSize = true;
			textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonReverse.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[]
			{
				label,
				textBox,
				buttonReverse,
				buttonOk,
				buttonCancel
			});
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.StartPosition = FormStartPosition.CenterScreen;
			form.MinimizeBox = false;
			form.MaximizeBox = false;
			form.AcceptButton = buttonOk;
			form.CancelButton = buttonCancel;

            form.Shown += delegate
            {
                textBox.Focus();
                textBox.SelectAll();
            };

            DialogResult dialogResult = form.ShowDialog();
			value = textBox.Text;
			return dialogResult;
		}
	}
}