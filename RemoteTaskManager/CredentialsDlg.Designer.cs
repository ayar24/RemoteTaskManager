﻿namespace RemoteTaskManager
{
    partial class CredentialsDlg
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
            textBox1 = new TextBox();
            labelUser = new Label();
            labelPassword = new Label();
            textBox2 = new TextBox();
            labelPort = new Label();
            textBox3 = new TextBox();
            buttonCancel = new Button();
            buttonOK = new Button();
            checkBoxShowPassword = new CheckBox();
            labelIP = new Label();
            textBoxIP = new TextBox();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new Point(109, 60);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(212, 27);
            textBox1.TabIndex = 0;
            // 
            // labelUser
            // 
            labelUser.AutoSize = true;
            labelUser.Location = new Point(12, 63);
            labelUser.Name = "labelUser";
            labelUser.Size = new Size(41, 20);
            labelUser.TabIndex = 1;
            labelUser.Text = "User:";
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(12, 105);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(73, 20);
            labelPassword.TabIndex = 3;
            labelPassword.Text = "Password:";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(109, 102);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(212, 27);
            textBox2.TabIndex = 2;
            textBox2.UseSystemPasswordChar = true;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(12, 147);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(38, 20);
            labelPort.TabIndex = 5;
            labelPort.Text = "Port:";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(109, 144);
            textBox3.MaxLength = 5;
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(60, 27);
            textBox3.TabIndex = 4;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(260, 194);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(94, 29);
            buttonCancel.TabIndex = 6;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += button1_Click;
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(160, 194);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(94, 29);
            buttonOK.TabIndex = 7;
            buttonOK.Text = "Ok";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += button2_Click;
            // 
            // checkBoxShowPassword
            // 
            checkBoxShowPassword.AutoSize = true;
            checkBoxShowPassword.Location = new Point(12, 194);
            checkBoxShowPassword.Name = "checkBoxShowPassword";
            checkBoxShowPassword.Size = new Size(132, 24);
            checkBoxShowPassword.TabIndex = 8;
            checkBoxShowPassword.Text = "Show Password";
            checkBoxShowPassword.UseVisualStyleBackColor = true;
            checkBoxShowPassword.CheckedChanged += checkBoxShowPassword_CheckedChanged;
            // 
            // labelIP
            // 
            labelIP.AutoSize = true;
            labelIP.Location = new Point(12, 21);
            labelIP.Name = "labelIP";
            labelIP.Size = new Size(24, 20);
            labelIP.TabIndex = 10;
            labelIP.Text = "IP:";
            // 
            // textBoxIP
            // 
            textBoxIP.Location = new Point(109, 18);
            textBoxIP.Name = "textBoxIP";
            textBoxIP.Size = new Size(97, 27);
            textBoxIP.TabIndex = 9;
            // 
            // CredentialsDlg
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(366, 235);
            Controls.Add(labelIP);
            Controls.Add(textBoxIP);
            Controls.Add(checkBoxShowPassword);
            Controls.Add(buttonOK);
            Controls.Add(buttonCancel);
            Controls.Add(labelPort);
            Controls.Add(textBox3);
            Controls.Add(labelPassword);
            Controls.Add(textBox2);
            Controls.Add(labelUser);
            Controls.Add(textBox1);
            Name = "CredentialsDlg";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Credentials";
            Load += CredentialsDlg_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Label labelUser;
        private Label labelPassword;
        private TextBox textBox2;
        private Label labelPort;
        private TextBox textBox3;
        private Button buttonCancel;
        private Button buttonOK;
        private CheckBox checkBoxShowPassword;
        private Label labelIP;
        private TextBox textBoxIP;
    }
}