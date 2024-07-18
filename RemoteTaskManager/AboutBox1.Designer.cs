namespace RemoteTaskManager
{
	partial class AboutBox1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox1));
			tableLayoutPanel = new TableLayoutPanel();
			logoPictureBox = new PictureBox();
			labelProductName = new Label();
			labelVersion = new Label();
			labelCopyright = new Label();
			labelCompanyName = new Label();
			textBoxDescription = new TextBox();
			okButton = new Button();
			linkLabel1 = new LinkLabel();
			tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
			SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			tableLayoutPanel.ColumnCount = 2;
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67F));
			tableLayoutPanel.Controls.Add(logoPictureBox, 0, 0);
			tableLayoutPanel.Controls.Add(labelProductName, 1, 0);
			tableLayoutPanel.Controls.Add(labelVersion, 1, 1);
			tableLayoutPanel.Controls.Add(labelCopyright, 1, 2);
			tableLayoutPanel.Controls.Add(labelCompanyName, 1, 3);
			tableLayoutPanel.Controls.Add(textBoxDescription, 1, 4);
			tableLayoutPanel.Controls.Add(okButton, 1, 7);
			tableLayoutPanel.Controls.Add(linkLabel1, 0, 7);
			tableLayoutPanel.Dock = DockStyle.Fill;
			tableLayoutPanel.Location = new Point(12, 14);
			tableLayoutPanel.Margin = new Padding(4, 5, 4, 5);
			tableLayoutPanel.Name = "tableLayoutPanel";
			tableLayoutPanel.RowCount = 8;
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 57.6601677F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 2.22841215F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel.Size = new Size(588, 408);
			tableLayoutPanel.TabIndex = 0;
			// 
			// logoPictureBox
			// 
			logoPictureBox.Dock = DockStyle.Fill;
			logoPictureBox.Image = (Image)resources.GetObject("logoPictureBox.Image");
			logoPictureBox.Location = new Point(4, 5);
			logoPictureBox.Margin = new Padding(4, 5, 4, 5);
			logoPictureBox.Name = "logoPictureBox";
			tableLayoutPanel.SetRowSpan(logoPictureBox, 6);
			logoPictureBox.Size = new Size(186, 349);
			logoPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
			logoPictureBox.TabIndex = 12;
			logoPictureBox.TabStop = false;
			// 
			// labelProductName
			// 
			labelProductName.Dock = DockStyle.Fill;
			labelProductName.Location = new Point(202, 0);
			labelProductName.Margin = new Padding(8, 0, 4, 0);
			labelProductName.MaximumSize = new Size(0, 26);
			labelProductName.Name = "labelProductName";
			labelProductName.Size = new Size(382, 26);
			labelProductName.TabIndex = 19;
			labelProductName.Text = "Remote Task Manager";
			labelProductName.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// labelVersion
			// 
			labelVersion.Dock = DockStyle.Fill;
			labelVersion.Location = new Point(202, 36);
			labelVersion.Margin = new Padding(8, 0, 4, 0);
			labelVersion.MaximumSize = new Size(0, 26);
			labelVersion.Name = "labelVersion";
			labelVersion.Size = new Size(382, 26);
			labelVersion.TabIndex = 0;
			labelVersion.Text = "Version 1.0.0";
			labelVersion.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// labelCopyright
			// 
			labelCopyright.Dock = DockStyle.Fill;
			labelCopyright.Location = new Point(202, 72);
			labelCopyright.Margin = new Padding(8, 0, 4, 0);
			labelCopyright.MaximumSize = new Size(0, 26);
			labelCopyright.Name = "labelCopyright";
			labelCopyright.Size = new Size(382, 26);
			labelCopyright.TabIndex = 21;
			labelCopyright.Text = "Copyright (c) 2023-2024";
			labelCopyright.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// labelCompanyName
			// 
			labelCompanyName.Dock = DockStyle.Fill;
			labelCompanyName.Location = new Point(202, 108);
			labelCompanyName.Margin = new Padding(8, 0, 4, 0);
			labelCompanyName.MaximumSize = new Size(0, 26);
			labelCompanyName.Name = "labelCompanyName";
			labelCompanyName.Size = new Size(382, 26);
			labelCompanyName.TabIndex = 22;
			labelCompanyName.Text = "A Software";
			labelCompanyName.TextAlign = ContentAlignment.MiddleLeft;
			// 
			// textBoxDescription
			// 
			textBoxDescription.Dock = DockStyle.Fill;
			textBoxDescription.Location = new Point(202, 149);
			textBoxDescription.Margin = new Padding(8, 5, 4, 5);
			textBoxDescription.Multiline = true;
			textBoxDescription.Name = "textBoxDescription";
			textBoxDescription.ReadOnly = true;
			textBoxDescription.ScrollBars = ScrollBars.Both;
			textBoxDescription.Size = new Size(382, 197);
			textBoxDescription.TabIndex = 23;
			textBoxDescription.TabStop = false;
			textBoxDescription.Text = "Remote Task  Manager is intented to assist system admins to get better visibility into their remote Linux machines\r\n";
			// 
			// okButton
			// 
			okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			okButton.DialogResult = DialogResult.Cancel;
			okButton.Location = new Point(484, 377);
			okButton.Margin = new Padding(4, 5, 4, 5);
			okButton.Name = "okButton";
			tableLayoutPanel.SetRowSpan(okButton, 2);
			okButton.Size = new Size(100, 26);
			okButton.TabIndex = 24;
			okButton.Text = "&OK";
			okButton.Click += okButton_Click;
			// 
			// linkLabel1
			// 
			linkLabel1.AutoSize = true;
			linkLabel1.Dock = DockStyle.Fill;
			linkLabel1.Location = new Point(3, 367);
			linkLabel1.Name = "linkLabel1";
			tableLayoutPanel.SetRowSpan(linkLabel1, 2);
			linkLabel1.Size = new Size(188, 41);
			linkLabel1.TabIndex = 25;
			linkLabel1.TabStop = true;
			linkLabel1.Text = "internixsystems.com";
			linkLabel1.TextAlign = ContentAlignment.MiddleCenter;
			linkLabel1.LinkClicked += linkLabel1_LinkClicked;
			// 
			// AboutBox1
			// 
			AcceptButton = okButton;
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(612, 436);
			Controls.Add(tableLayoutPanel);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			Margin = new Padding(4, 5, 4, 5);
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "AboutBox1";
			Padding = new Padding(12, 14, 12, 14);
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "About";
			tableLayoutPanel.ResumeLayout(false);
			tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private TableLayoutPanel tableLayoutPanel;
		private PictureBox logoPictureBox;
		private Label labelProductName;
		private Label labelVersion;
		private Label labelCopyright;
		private Label labelCompanyName;
		private TextBox textBoxDescription;
		private Button okButton;
		private LinkLabel linkLabel1;
	}
}
