namespace WFN_FontEditor
{
	partial class MainWindow
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.BtnNew = new System.Windows.Forms.Button();
            this.BtnOpen = new System.Windows.Forms.Button();
            this.BtnClose = new System.Windows.Forms.Button();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.BtnSave = new System.Windows.Forms.Button();
            this.BtnSaveAs = new System.Windows.Forms.Button();
            this.BtnConvert = new System.Windows.Forms.Button();
            this.BtnAbout = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnNew
            // 
            this.BtnNew.Location = new System.Drawing.Point(12, 12);
            this.BtnNew.Name = "BtnNew";
            this.BtnNew.Size = new System.Drawing.Size(100, 23);
            this.BtnNew.TabIndex = 1;
            this.BtnNew.Text = "New...        Ctrl+N";
            this.BtnNew.UseVisualStyleBackColor = true;
            this.BtnNew.Click += new System.EventHandler(this.BtnNew_Click);
            // 
            // BtnOpen
            // 
            this.BtnOpen.Location = new System.Drawing.Point(118, 12);
            this.BtnOpen.Name = "BtnOpen";
            this.BtnOpen.Size = new System.Drawing.Size(100, 23);
            this.BtnOpen.TabIndex = 1;
            this.BtnOpen.Text = "Open...       Ctrl+O";
            this.BtnOpen.UseVisualStyleBackColor = true;
            this.BtnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
            // 
            // BtnClose
            // 
            this.BtnClose.Location = new System.Drawing.Point(224, 12);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(100, 23);
            this.BtnClose.TabIndex = 2;
            this.BtnClose.Text = "Close...     Ctrl+W";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // TabControl
            // 
            this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControl.Location = new System.Drawing.Point(12, 41);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(957, 527);
            this.TabControl.TabIndex = 2;
            // 
            // BtnSave
            // 
            this.BtnSave.Location = new System.Drawing.Point(330, 12);
            this.BtnSave.Name = "BtnSave";
            this.BtnSave.Size = new System.Drawing.Size(100, 23);
            this.BtnSave.TabIndex = 1;
            this.BtnSave.Text = "Save...       Ctrl+S";
            this.BtnSave.UseVisualStyleBackColor = true;
            this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // BtnSaveAs
            // 
            this.BtnSaveAs.Location = new System.Drawing.Point(436, 12);
            this.BtnSaveAs.Name = "BtnSaveAs";
            this.BtnSaveAs.Size = new System.Drawing.Size(134, 23);
            this.BtnSaveAs.TabIndex = 1;
            this.BtnSaveAs.Text = "Save As...     Ctrl+Shift+S";
            this.BtnSaveAs.UseVisualStyleBackColor = true;
            this.BtnSaveAs.Click += new System.EventHandler(this.BtnSaveAs_Click);
            // 
            // BtnConvert
            // 
            this.BtnConvert.Location = new System.Drawing.Point(576, 12);
            this.BtnConvert.Name = "BtnConvert";
            this.BtnConvert.Size = new System.Drawing.Size(120, 23);
            this.BtnConvert.TabIndex = 1;
            this.BtnConvert.Text = "SCI/WFN conversion";
            this.BtnConvert.UseVisualStyleBackColor = true;
            this.BtnConvert.Click += new System.EventHandler(this.BtnConvert_Click);
            // 
            // BtnAbout
            // 
            this.BtnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAbout.Location = new System.Drawing.Point(890, 12);
            this.BtnAbout.Name = "BtnAbout";
            this.BtnAbout.Size = new System.Drawing.Size(80, 23);
            this.BtnAbout.TabIndex = 1;
            this.BtnAbout.Text = "About       F1";
            this.BtnAbout.UseVisualStyleBackColor = true;
            this.BtnAbout.Click += new System.EventHandler(this.BtnAbout_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(981, 580);
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.BtnConvert);
            this.Controls.Add(this.BtnAbout);
            this.Controls.Add(this.BtnSave);
            this.Controls.Add(this.BtnSaveAs);
            this.Controls.Add(this.BtnNew);
            this.Controls.Add(this.BtnOpen);
            this.Controls.Add(this.BtnClose);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "WFN-FontEditor";
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button BtnNew;
		private System.Windows.Forms.Button BtnOpen;
		private System.Windows.Forms.TabControl TabControl;
		private System.Windows.Forms.Button BtnSave;
		private System.Windows.Forms.Button BtnSaveAs;
		private System.Windows.Forms.Button BtnConvert;
        private System.Windows.Forms.Button BtnAbout;
        private System.Windows.Forms.Button BtnClose;
        //private System.Windows.Forms.TextBox TxtTextHeight; // the TextHeight never seemed to work and hence removed
        //private System.Windows.Forms.Label LblTextHeight; // the TextHeight never seemed to work and hence removed
    }
}

