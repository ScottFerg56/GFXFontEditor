namespace GFXFontEditor
{
	partial class Editor
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Editor));
			toolStrip1 = new ToolStrip();
			toolStripDropDownButtonFile = new ToolStripDropDownButton();
			newToolStripMenuItem = new ToolStripMenuItem();
			openToolStripMenuItem = new ToolStripMenuItem();
			saveToolStripMenuItem = new ToolStripMenuItem();
			saveAsToolStripMenuItem = new ToolStripMenuItem();
			recentFilesToolStripMenuItem = new ToolStripMenuItem();
			aboutToolStripMenuItem = new ToolStripMenuItem();
			toolStripButtonNextFile = new ToolStripButton();
			toolStripSeparator1 = new ToolStripSeparator();
			toolStripLabelHeight = new ToolStripLabel();
			toolStripLabelNumGlyphs = new ToolStripLabel();
			toolStripLabel3 = new ToolStripLabel();
			toolStripNumberControlFirstCode = new ToolStripNumberControl();
			toolStripSeparator2 = new ToolStripSeparator();
			toolStripLabel1 = new ToolStripLabel();
			toolStripNumberControlPPD = new ToolStripNumberControl();
			toolStripLabel2 = new ToolStripLabel();
			toolStripTextBoxFVText = new ToolStripTextBox();
			splitContainer1 = new SplitContainer();
			pictureBoxGlyph = new PictureBox();
			listViewGlyphs = new ListView();
			columnHeader1 = new ColumnHeader();
			columnHeader2 = new ColumnHeader();
			columnHeader3 = new ColumnHeader();
			columnHeader4 = new ColumnHeader();
			columnHeader5 = new ColumnHeader();
			columnHeader6 = new ColumnHeader();
			columnHeader7 = new ColumnHeader();
			contextMenuStripGlyphList = new ContextMenuStrip(components);
			insertToolStripMenuItem = new ToolStripMenuItem();
			deleteToolStripMenuItem = new ToolStripMenuItem();
			cutToolStripMenuItem = new ToolStripMenuItem();
			copyToolStripMenuItem = new ToolStripMenuItem();
			pasteToolStripMenuItem = new ToolStripMenuItem();
			pasteInsertToolStripMenuItem = new ToolStripMenuItem();
			clearToolStripMenuItem = new ToolStripMenuItem();
			setRectToolStripMenuItem = new ToolStripMenuItem();
			splitContainer2 = new SplitContainer();
			pictureBoxFontView = new PictureBox();
			toolStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxGlyph).BeginInit();
			contextMenuStripGlyphList.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
			splitContainer2.Panel1.SuspendLayout();
			splitContainer2.Panel2.SuspendLayout();
			splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pictureBoxFontView).BeginInit();
			SuspendLayout();
			// 
			// toolStrip1
			// 
			toolStrip1.ImageScalingSize = new Size(24, 24);
			toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButtonFile, toolStripButtonNextFile, toolStripSeparator1, toolStripLabelHeight, toolStripLabelNumGlyphs, toolStripLabel3, toolStripNumberControlFirstCode, toolStripSeparator2, toolStripLabel1, toolStripNumberControlPPD, toolStripLabel2, toolStripTextBoxFVText });
			toolStrip1.Location = new Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new Size(1729, 36);
			toolStrip1.TabIndex = 0;
			toolStrip1.Text = "toolStrip1";
			// 
			// toolStripDropDownButtonFile
			// 
			toolStripDropDownButtonFile.DisplayStyle = ToolStripItemDisplayStyle.Text;
			toolStripDropDownButtonFile.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, recentFilesToolStripMenuItem, aboutToolStripMenuItem });
			toolStripDropDownButtonFile.Image = (Image)resources.GetObject("toolStripDropDownButtonFile.Image");
			toolStripDropDownButtonFile.ImageTransparentColor = Color.Magenta;
			toolStripDropDownButtonFile.Name = "toolStripDropDownButtonFile";
			toolStripDropDownButtonFile.Size = new Size(56, 31);
			toolStripDropDownButtonFile.Text = "&File";
			// 
			// newToolStripMenuItem
			// 
			newToolStripMenuItem.Name = "newToolStripMenuItem";
			newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
			newToolStripMenuItem.Size = new Size(271, 34);
			newToolStripMenuItem.Text = "&New";
			newToolStripMenuItem.Click += newToolStripMenuItem_Click;
			// 
			// openToolStripMenuItem
			// 
			openToolStripMenuItem.Name = "openToolStripMenuItem";
			openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
			openToolStripMenuItem.Size = new Size(271, 34);
			openToolStripMenuItem.Text = "&Open";
			openToolStripMenuItem.Click += openToolStripMenuItem_Click;
			// 
			// saveToolStripMenuItem
			// 
			saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
			saveToolStripMenuItem.Size = new Size(271, 34);
			saveToolStripMenuItem.Text = "&Save";
			saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
			// 
			// saveAsToolStripMenuItem
			// 
			saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Alt | Keys.S;
			saveAsToolStripMenuItem.Size = new Size(271, 34);
			saveAsToolStripMenuItem.Text = "Save &As";
			saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
			// 
			// recentFilesToolStripMenuItem
			// 
			recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
			recentFilesToolStripMenuItem.Size = new Size(271, 34);
			recentFilesToolStripMenuItem.Text = "&Recent Files";
			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Size = new Size(271, 34);
			aboutToolStripMenuItem.Text = "About";
			aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
			// 
			// toolStripButtonNextFile
			// 
			toolStripButtonNextFile.Alignment = ToolStripItemAlignment.Right;
			toolStripButtonNextFile.DisplayStyle = ToolStripItemDisplayStyle.Text;
			toolStripButtonNextFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
			toolStripButtonNextFile.Image = (Image)resources.GetObject("toolStripButtonNextFile.Image");
			toolStripButtonNextFile.ImageTransparentColor = Color.Magenta;
			toolStripButtonNextFile.Name = "toolStripButtonNextFile";
			toolStripButtonNextFile.Size = new Size(101, 31);
			toolStripButtonNextFile.Text = "Next File: ";
			toolStripButtonNextFile.Visible = false;
			toolStripButtonNextFile.Click += toolStripButtonNextFile_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(6, 36);
			// 
			// toolStripLabelHeight
			// 
			toolStripLabelHeight.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
			toolStripLabelHeight.Name = "toolStripLabelHeight";
			toolStripLabelHeight.Size = new Size(70, 31);
			toolStripLabelHeight.Text = "Height";
			// 
			// toolStripLabelNumGlyphs
			// 
			toolStripLabelNumGlyphs.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
			toolStripLabelNumGlyphs.Name = "toolStripLabelNumGlyphs";
			toolStripLabelNumGlyphs.Size = new Size(96, 31);
			toolStripLabelNumGlyphs.Text = "# Glyphs: ";
			// 
			// toolStripLabel3
			// 
			toolStripLabel3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
			toolStripLabel3.Name = "toolStripLabel3";
			toolStripLabel3.Size = new Size(106, 31);
			toolStripLabel3.Text = "First Code: ";
			// 
			// toolStripNumberControlFirstCode
			// 
			toolStripNumberControlFirstCode.Name = "toolStripNumberControlFirstCode";
			toolStripNumberControlFirstCode.Size = new Size(66, 31);
			toolStripNumberControlFirstCode.Text = "0";
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new Size(6, 36);
			// 
			// toolStripLabel1
			// 
			toolStripLabel1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
			toolStripLabel1.Name = "toolStripLabel1";
			toolStripLabel1.Size = new Size(139, 31);
			toolStripLabel1.Text = "Pixels Per Dot: ";
			// 
			// toolStripNumberControlPPD
			// 
			toolStripNumberControlPPD.Name = "toolStripNumberControlPPD";
			toolStripNumberControlPPD.Size = new Size(66, 31);
			toolStripNumberControlPPD.Text = "0";
			// 
			// toolStripLabel2
			// 
			toolStripLabel2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
			toolStripLabel2.Name = "toolStripLabel2";
			toolStripLabel2.Size = new Size(53, 31);
			toolStripLabel2.Text = "Text:";
			// 
			// toolStripTextBoxFVText
			// 
			toolStripTextBoxFVText.BackColor = Color.LightGray;
			toolStripTextBoxFVText.Name = "toolStripTextBoxFVText";
			toolStripTextBoxFVText.Size = new Size(300, 36);
			toolStripTextBoxFVText.TextChanged += toolStripTextBoxFVText_TextChanged;
			// 
			// splitContainer1
			// 
			splitContainer1.Dock = DockStyle.Fill;
			splitContainer1.Location = new Point(0, 0);
			splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			splitContainer1.Panel1.Controls.Add(pictureBoxGlyph);
			// 
			// splitContainer1.Panel2
			// 
			splitContainer1.Panel2.Controls.Add(listViewGlyphs);
			splitContainer1.Size = new Size(1729, 862);
			splitContainer1.SplitterDistance = 1104;
			splitContainer1.TabIndex = 1;
			// 
			// pictureBoxGlyph
			// 
			pictureBoxGlyph.BackColor = Color.Gray;
			pictureBoxGlyph.Dock = DockStyle.Fill;
			pictureBoxGlyph.Location = new Point(0, 0);
			pictureBoxGlyph.Name = "pictureBoxGlyph";
			pictureBoxGlyph.Size = new Size(1104, 862);
			pictureBoxGlyph.TabIndex = 0;
			pictureBoxGlyph.TabStop = false;
			pictureBoxGlyph.Paint += pictureBoxGlyph_Paint;
			pictureBoxGlyph.MouseDown += pictureBoxGlyph_MouseDown;
			pictureBoxGlyph.MouseMove += pictureBoxGlyph_MouseMove;
			pictureBoxGlyph.MouseUp += pictureBoxGlyph_MouseUp;
			pictureBoxGlyph.Resize += pictureBoxGlyph_Resize;
			// 
			// listViewGlyphs
			// 
			listViewGlyphs.BackColor = Color.LightGray;
			listViewGlyphs.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5, columnHeader6, columnHeader7 });
			listViewGlyphs.ContextMenuStrip = contextMenuStripGlyphList;
			listViewGlyphs.Dock = DockStyle.Fill;
			listViewGlyphs.FullRowSelect = true;
			listViewGlyphs.GridLines = true;
			listViewGlyphs.Location = new Point(0, 0);
			listViewGlyphs.Name = "listViewGlyphs";
			listViewGlyphs.Size = new Size(621, 862);
			listViewGlyphs.TabIndex = 0;
			listViewGlyphs.UseCompatibleStateImageBehavior = false;
			listViewGlyphs.View = View.Details;
			listViewGlyphs.SelectedIndexChanged += listViewGlyphs_SelectedIndexChanged;
			listViewGlyphs.KeyDown += listViewGlyphs_KeyDown;
			listViewGlyphs.KeyUp += listViewGlyphs_KeyUp;
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Symbol";
			columnHeader1.Width = 75;
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "Code";
			columnHeader2.Width = 110;
			// 
			// columnHeader3
			// 
			columnHeader3.Text = "Width";
			columnHeader3.Width = 75;
			// 
			// columnHeader4
			// 
			columnHeader4.Text = "Height";
			columnHeader4.Width = 75;
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "xAdvance";
			columnHeader5.Width = 90;
			// 
			// columnHeader6
			// 
			columnHeader6.Text = "xOffset";
			columnHeader6.Width = 75;
			// 
			// columnHeader7
			// 
			columnHeader7.Text = "yOffset";
			columnHeader7.Width = 75;
			// 
			// contextMenuStripGlyphList
			// 
			contextMenuStripGlyphList.ImageScalingSize = new Size(24, 24);
			contextMenuStripGlyphList.Items.AddRange(new ToolStripItem[] { insertToolStripMenuItem, deleteToolStripMenuItem, cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, pasteInsertToolStripMenuItem, clearToolStripMenuItem, setRectToolStripMenuItem });
			contextMenuStripGlyphList.Name = "contextMenuStrip1";
			contextMenuStripGlyphList.Size = new Size(237, 260);
			contextMenuStripGlyphList.Opening += contextMenuStripGlyphList_Opening;
			// 
			// insertToolStripMenuItem
			// 
			insertToolStripMenuItem.Name = "insertToolStripMenuItem";
			insertToolStripMenuItem.ShortcutKeyDisplayString = "Ins";
			insertToolStripMenuItem.Size = new Size(236, 32);
			insertToolStripMenuItem.Text = "Insert";
			insertToolStripMenuItem.Click += insertToolStripMenuItem_Click;
			// 
			// deleteToolStripMenuItem
			// 
			deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			deleteToolStripMenuItem.ShortcutKeyDisplayString = "Del";
			deleteToolStripMenuItem.Size = new Size(236, 32);
			deleteToolStripMenuItem.Text = "Delete";
			deleteToolStripMenuItem.Click += deleteToolStripMenuItem_Click;
			// 
			// cutToolStripMenuItem
			// 
			cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			cutToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+X";
			cutToolStripMenuItem.Size = new Size(236, 32);
			cutToolStripMenuItem.Text = "Cut";
			cutToolStripMenuItem.Click += cutToolStripMenuItem_Click;
			// 
			// copyToolStripMenuItem
			// 
			copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			copyToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
			copyToolStripMenuItem.Size = new Size(236, 32);
			copyToolStripMenuItem.Text = "Copy";
			copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
			// 
			// pasteToolStripMenuItem
			// 
			pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			pasteToolStripMenuItem.Size = new Size(236, 32);
			pasteToolStripMenuItem.Text = "Paste";
			pasteToolStripMenuItem.Click += pasteToolStripMenuItem_Click;
			// 
			// pasteInsertToolStripMenuItem
			// 
			pasteInsertToolStripMenuItem.Name = "pasteInsertToolStripMenuItem";
			pasteInsertToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+V";
			pasteInsertToolStripMenuItem.Size = new Size(236, 32);
			pasteInsertToolStripMenuItem.Text = "Paste Insert";
			pasteInsertToolStripMenuItem.Click += pasteInsertToolStripMenuItem_Click;
			// 
			// clearToolStripMenuItem
			// 
			clearToolStripMenuItem.Name = "clearToolStripMenuItem";
			clearToolStripMenuItem.Size = new Size(236, 32);
			clearToolStripMenuItem.Text = "Clear";
			clearToolStripMenuItem.Click += clearToolStripMenuItem_Click;
			// 
			// setRectToolStripMenuItem
			// 
			setRectToolStripMenuItem.Name = "setRectToolStripMenuItem";
			setRectToolStripMenuItem.Size = new Size(236, 32);
			setRectToolStripMenuItem.Text = "Set Rect";
			setRectToolStripMenuItem.Click += setRectToolStripMenuItem_Click;
			// 
			// splitContainer2
			// 
			splitContainer2.Dock = DockStyle.Fill;
			splitContainer2.Location = new Point(0, 36);
			splitContainer2.Name = "splitContainer2";
			splitContainer2.Orientation = Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			splitContainer2.Panel1.Controls.Add(splitContainer1);
			// 
			// splitContainer2.Panel2
			// 
			splitContainer2.Panel2.AutoScroll = true;
			splitContainer2.Panel2.BackColor = Color.FromArgb(50, 50, 50);
			splitContainer2.Panel2.Controls.Add(pictureBoxFontView);
			splitContainer2.Size = new Size(1729, 1088);
			splitContainer2.SplitterDistance = 862;
			splitContainer2.TabIndex = 2;
			// 
			// pictureBoxFontView
			// 
			pictureBoxFontView.Location = new Point(0, 18);
			pictureBoxFontView.Name = "pictureBoxFontView";
			pictureBoxFontView.Size = new Size(150, 75);
			pictureBoxFontView.SizeMode = PictureBoxSizeMode.AutoSize;
			pictureBoxFontView.TabIndex = 0;
			pictureBoxFontView.TabStop = false;
			pictureBoxFontView.Paint += pictureBoxFontView_Paint;
			pictureBoxFontView.MouseDown += pictureBoxFontView_MouseDown;
			// 
			// Editor
			// 
			AllowDrop = true;
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1729, 1124);
			Controls.Add(splitContainer2);
			Controls.Add(toolStrip1);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Name = "Editor";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "GFX Font Editor";
			FormClosing += Editor_FormClosing;
			FormClosed += Editor_FormClosed;
			Shown += Editor_Shown;
			DragDrop += Editor_DragDrop;
			DragOver += Editor_DragOver;
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			splitContainer1.Panel1.ResumeLayout(false);
			splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
			splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxGlyph).EndInit();
			contextMenuStripGlyphList.ResumeLayout(false);
			splitContainer2.Panel1.ResumeLayout(false);
			splitContainer2.Panel2.ResumeLayout(false);
			splitContainer2.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
			splitContainer2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pictureBoxFontView).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ToolStrip toolStrip1;
		private SplitContainer splitContainer1;
		private ListView listViewGlyphs;
		private ColumnHeader columnHeader1;
		private ColumnHeader columnHeader2;
		private ColumnHeader columnHeader3;
		private ColumnHeader columnHeader4;
		private ColumnHeader columnHeader5;
		private ColumnHeader columnHeader6;
		private ColumnHeader columnHeader7;
		private ToolStripLabel toolStripLabelHeight;
		private PictureBox pictureBoxGlyph;
		private ToolStripSeparator toolStripSeparator1;
		private SplitContainer splitContainer2;
		private PictureBox pictureBoxFontView;
		private ToolStripLabel toolStripLabel1;
		private ToolStripNumberControl toolStripNumberControlPPD;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripLabel toolStripLabel2;
		private ToolStripTextBox toolStripTextBoxFVText;
		private ToolStripButton toolStripButtonNextFile;
		private ContextMenuStrip contextMenuStripGlyphList;
		private ToolStripMenuItem copyToolStripMenuItem;
		private ToolStripMenuItem pasteToolStripMenuItem;
		private ToolStripMenuItem clearToolStripMenuItem;
		private ToolStripMenuItem setRectToolStripMenuItem;
		private ToolStripMenuItem deleteToolStripMenuItem;
		private ToolStripMenuItem insertToolStripMenuItem;
		private ToolStripNumberControl toolStripNumberControlFirstCode;
		private ToolStripLabel toolStripLabel3;
		private ToolStripMenuItem cutToolStripMenuItem;
		private ToolStripMenuItem pasteInsertToolStripMenuItem;
		private ToolStripDropDownButton toolStripDropDownButtonFile;
		private ToolStripMenuItem newToolStripMenuItem;
		private ToolStripMenuItem openToolStripMenuItem;
		private ToolStripMenuItem saveToolStripMenuItem;
		private ToolStripMenuItem saveAsToolStripMenuItem;
		private ToolStripLabel toolStripLabelNumGlyphs;
		private ToolStripMenuItem recentFilesToolStripMenuItem;
		private ToolStripMenuItem aboutToolStripMenuItem;
	}
}