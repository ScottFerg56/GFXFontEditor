namespace GFXFontEditor
{
	partial class PropertyBrowser
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			propertyGrid = new PropertyGrid();
			buttonOk = new Button();
			buttonCancel = new Button();
			contextMenuStrip = new ContextMenuStrip(components);
			resetDefaultsToolStripMenuItem = new ToolStripMenuItem();
			contextMenuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// propertyGrid
			// 
			propertyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			propertyGrid.LineColor = SystemColors.ControlDark;
			propertyGrid.Location = new Point(0, 0);
			propertyGrid.Margin = new Padding(4, 6, 4, 6);
			propertyGrid.Name = "propertyGrid";
			propertyGrid.PropertySort = PropertySort.NoSort;
			propertyGrid.Size = new Size(710, 388);
			propertyGrid.TabIndex = 0;
			propertyGrid.ToolbarVisible = false;
			// 
			// buttonOk
			// 
			buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonOk.DialogResult = DialogResult.OK;
			buttonOk.Location = new Point(242, 400);
			buttonOk.Margin = new Padding(4, 6, 4, 6);
			buttonOk.Name = "buttonOk";
			buttonOk.Size = new Size(223, 44);
			buttonOk.TabIndex = 1;
			buttonOk.Text = "OK";
			buttonOk.UseVisualStyleBackColor = true;
			// 
			// buttonCancel
			// 
			buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonCancel.DialogResult = DialogResult.Cancel;
			buttonCancel.Location = new Point(476, 400);
			buttonCancel.Margin = new Padding(4, 6, 4, 6);
			buttonCancel.Name = "buttonCancel";
			buttonCancel.Size = new Size(223, 44);
			buttonCancel.TabIndex = 2;
			buttonCancel.Text = "Cancel";
			buttonCancel.UseVisualStyleBackColor = true;
			buttonCancel.Click += buttonCancel_Click;
			// 
			// contextMenuStrip
			// 
			contextMenuStrip.ImageScalingSize = new Size(24, 24);
			contextMenuStrip.Items.AddRange(new ToolStripItem[] { resetDefaultsToolStripMenuItem });
			contextMenuStrip.Name = "contextMenuStrip";
			contextMenuStrip.Size = new Size(197, 36);
			// 
			// resetDefaultsToolStripMenuItem
			// 
			resetDefaultsToolStripMenuItem.Name = "resetDefaultsToolStripMenuItem";
			resetDefaultsToolStripMenuItem.Size = new Size(196, 32);
			resetDefaultsToolStripMenuItem.Text = "Reset Defaults";
			// 
			// PropertyBrowser
			// 
			AcceptButton = buttonOk;
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = buttonCancel;
			ClientSize = new Size(710, 457);
			Controls.Add(propertyGrid);
			Controls.Add(buttonCancel);
			Controls.Add(buttonOk);
			Margin = new Padding(4, 6, 4, 6);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "PropertyBrowser";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "PropertyBrowser";
			contextMenuStrip.ResumeLayout(false);
			ResumeLayout(false);
		}

		#endregion

		public System.Windows.Forms.PropertyGrid propertyGrid;
		public System.Windows.Forms.Button buttonCancel;
		public System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem resetDefaultsToolStripMenuItem;
	}
}