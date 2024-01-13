namespace FMBase
{
	partial class AboutBox
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
			ImagePictureBox = new PictureBox();
			AppDateLabel = new Label();
			SysInfoButton = new Button();
			AppCopyrightLabel = new Label();
			AppVersionLabel = new Label();
			AppDescriptionLabel = new Label();
			GroupBox1 = new GroupBox();
			AppTitleLabel = new Label();
			OKButton = new Button();
			MoreRichTextBox = new RichTextBox();
			FWVersionLabel = new Label();
			((System.ComponentModel.ISupportInitialize)ImagePictureBox).BeginInit();
			SuspendLayout();
			// 
			// ImagePictureBox
			// 
			ImagePictureBox.Location = new Point(23, 14);
			ImagePictureBox.Margin = new Padding(4, 6, 4, 6);
			ImagePictureBox.Name = "ImagePictureBox";
			ImagePictureBox.Size = new Size(53, 61);
			ImagePictureBox.TabIndex = 24;
			ImagePictureBox.TabStop = false;
			// 
			// AppDateLabel
			// 
			AppDateLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			AppDateLabel.Location = new Point(10, 152);
			AppDateLabel.Margin = new Padding(4, 0, 4, 0);
			AppDateLabel.Name = "AppDateLabel";
			AppDateLabel.Size = new Size(794, 31);
			AppDateLabel.TabIndex = 23;
			AppDateLabel.Text = "Built on %builddate%";
			// 
			// SysInfoButton
			// 
			SysInfoButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			SysInfoButton.Location = new Point(514, 783);
			SysInfoButton.Margin = new Padding(4, 6, 4, 6);
			SysInfoButton.Name = "SysInfoButton";
			SysInfoButton.Size = new Size(153, 44);
			SysInfoButton.TabIndex = 22;
			SysInfoButton.Text = "&System Info...";
			SysInfoButton.Click += SysInfoButton_Click;
			// 
			// AppCopyrightLabel
			// 
			AppCopyrightLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			AppCopyrightLabel.Location = new Point(10, 190);
			AppCopyrightLabel.Margin = new Padding(4, 0, 4, 0);
			AppCopyrightLabel.Name = "AppCopyrightLabel";
			AppCopyrightLabel.Size = new Size(794, 31);
			AppCopyrightLabel.TabIndex = 21;
			AppCopyrightLabel.Text = "%copyright%";
			// 
			// AppVersionLabel
			// 
			AppVersionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			AppVersionLabel.Location = new Point(10, 114);
			AppVersionLabel.Margin = new Padding(4, 0, 4, 0);
			AppVersionLabel.Name = "AppVersionLabel";
			AppVersionLabel.Size = new Size(794, 31);
			AppVersionLabel.TabIndex = 20;
			AppVersionLabel.Text = "Version %version%";
			// 
			// AppDescriptionLabel
			// 
			AppDescriptionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			AppDescriptionLabel.Location = new Point(97, 52);
			AppDescriptionLabel.Margin = new Padding(4, 0, 4, 0);
			AppDescriptionLabel.Name = "AppDescriptionLabel";
			AppDescriptionLabel.Size = new Size(708, 31);
			AppDescriptionLabel.TabIndex = 19;
			AppDescriptionLabel.Text = "%description%";
			// 
			// GroupBox1
			// 
			GroupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			GroupBox1.Location = new Point(10, 90);
			GroupBox1.Margin = new Padding(4, 6, 4, 6);
			GroupBox1.Name = "GroupBox1";
			GroupBox1.Padding = new Padding(4, 6, 4, 6);
			GroupBox1.Size = new Size(794, 4);
			GroupBox1.TabIndex = 18;
			GroupBox1.TabStop = false;
			GroupBox1.Text = "GroupBox1";
			// 
			// AppTitleLabel
			// 
			AppTitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			AppTitleLabel.Location = new Point(97, 14);
			AppTitleLabel.Margin = new Padding(4, 0, 4, 0);
			AppTitleLabel.Name = "AppTitleLabel";
			AppTitleLabel.Size = new Size(708, 31);
			AppTitleLabel.TabIndex = 17;
			AppTitleLabel.Text = "%title%";
			// 
			// OKButton
			// 
			OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			OKButton.DialogResult = DialogResult.Cancel;
			OKButton.Location = new Point(681, 783);
			OKButton.Margin = new Padding(4, 6, 4, 6);
			OKButton.Name = "OKButton";
			OKButton.Size = new Size(127, 44);
			OKButton.TabIndex = 16;
			OKButton.Text = "OK";
			// 
			// MoreRichTextBox
			// 
			MoreRichTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			MoreRichTextBox.BackColor = SystemColors.ControlLight;
			MoreRichTextBox.Location = new Point(10, 279);
			MoreRichTextBox.Margin = new Padding(4, 6, 4, 6);
			MoreRichTextBox.Name = "MoreRichTextBox";
			MoreRichTextBox.ReadOnly = true;
			MoreRichTextBox.ScrollBars = RichTextBoxScrollBars.Vertical;
			MoreRichTextBox.Size = new Size(792, 484);
			MoreRichTextBox.TabIndex = 26;
			MoreRichTextBox.Text = "%product% is %copyright%, %trademark%";
			MoreRichTextBox.LinkClicked += MoreRichTextBox_LinkClicked;
			// 
			// FWVersionLabel
			// 
			FWVersionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			FWVersionLabel.Location = new Point(10, 232);
			FWVersionLabel.Margin = new Padding(4, 0, 4, 0);
			FWVersionLabel.Name = "FWVersionLabel";
			FWVersionLabel.Size = new Size(794, 31);
			FWVersionLabel.TabIndex = 27;
			FWVersionLabel.Text = "framework version";
			// 
			// AboutBox
			// 
			AutoScaleDimensions = new SizeF(10F, 25F);
			AutoScaleMode = AutoScaleMode.Font;
			CancelButton = OKButton;
			ClientSize = new Size(817, 840);
			Controls.Add(FWVersionLabel);
			Controls.Add(ImagePictureBox);
			Controls.Add(AppDateLabel);
			Controls.Add(SysInfoButton);
			Controls.Add(AppCopyrightLabel);
			Controls.Add(AppVersionLabel);
			Controls.Add(AppDescriptionLabel);
			Controls.Add(GroupBox1);
			Controls.Add(AppTitleLabel);
			Controls.Add(OKButton);
			Controls.Add(MoreRichTextBox);
			Margin = new Padding(4, 6, 4, 6);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "AboutBox";
			ShowInTaskbar = false;
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "About %title%";
			Load += AboutBox_Load;
			Paint += AboutBox_Paint;
			((System.ComponentModel.ISupportInitialize)ImagePictureBox).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private System.Windows.Forms.PictureBox ImagePictureBox;
		private System.Windows.Forms.Label AppDateLabel;
		private System.Windows.Forms.Button SysInfoButton;
		private System.Windows.Forms.Label AppCopyrightLabel;
		private System.Windows.Forms.Label AppVersionLabel;
		private System.Windows.Forms.Label AppDescriptionLabel;
		private System.Windows.Forms.GroupBox GroupBox1;
		private System.Windows.Forms.Label AppTitleLabel;
		private System.Windows.Forms.Button OKButton;
		internal System.Windows.Forms.RichTextBox MoreRichTextBox;
		private System.Windows.Forms.Label FWVersionLabel;
	}
}