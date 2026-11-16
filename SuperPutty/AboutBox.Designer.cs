using System.Windows.Forms;

namespace SuperPutty
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
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Close "AboutBox1" when ESC is pressed.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            logoPictureBox = new PictureBox();
            okButton = new Button();
            linkLabelCompany = new LinkLabel();
            linkLabelCompany2 = new LinkLabel();
            tabPage3 = new TabPage();
            richTextBox1 = new RichTextBox();
            tabPage2 = new TabPage();
            textBox2 = new TextBox();
            tabPage1 = new TabPage();
            textBox1 = new TextBox();
            tabControl1 = new TabControl();
            tabPage4 = new TabPage();
            textBoxSupportText = new TextBox();
            labelProductName = new Label();
            labelVersion = new Label();
            labelCopyright = new Label();
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).BeginInit();
            tabPage3.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage4.SuspendLayout();
            SuspendLayout();
            // 
            // logoPictureBox
            // 
            logoPictureBox.Image = (System.Drawing.Image)resources.GetObject("logoPictureBox.Image");
            logoPictureBox.Location = new System.Drawing.Point(16, 19);
            logoPictureBox.Margin = new Padding(4, 5, 4, 5);
            logoPictureBox.Name = "logoPictureBox";
            logoPictureBox.Size = new System.Drawing.Size(101, 96);
            logoPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            logoPictureBox.TabIndex = 12;
            logoPictureBox.TabStop = false;
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.DialogResult = DialogResult.Cancel;
            okButton.Location = new System.Drawing.Point(627, 432);
            okButton.Margin = new Padding(4, 5, 4, 5);
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(100, 38);
            okButton.TabIndex = 24;
            okButton.Text = "&OK";
            // 
            // linkLabelCompany
            // 
            linkLabelCompany.AutoSize = true;
            linkLabelCompany.Location = new System.Drawing.Point(236, 125);
            linkLabelCompany.Margin = new Padding(8, 0, 4, 0);
            linkLabelCompany.Name = "linkLabelCompany";
            linkLabelCompany.Size = new System.Drawing.Size(131, 20);
            linkLabelCompany.TabIndex = 26;
            linkLabelCompany.TabStop = true;
            linkLabelCompany.Text = "linkLabelCompany";
            linkLabelCompany.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            linkLabelCompany.LinkClicked += linkLabelCompany_LinkClicked;
            // 
            // linkLabelCompany2
            // 
            linkLabelCompany2.AutoSize = true;
            linkLabelCompany2.Location = new System.Drawing.Point(236, 158);
            linkLabelCompany2.Margin = new Padding(8, 0, 4, 0);
            linkLabelCompany2.Name = "linkLabelCompany2";
            linkLabelCompany2.Size = new System.Drawing.Size(139, 20);
            linkLabelCompany2.TabIndex = 27;
            linkLabelCompany2.TabStop = true;
            linkLabelCompany2.Text = "linkLabelCompany2";
            linkLabelCompany2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            linkLabelCompany2.LinkClicked += linkLabelCompany2_LinkClicked;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(richTextBox1);
            tabPage3.Location = new System.Drawing.Point(4, 29);
            tabPage3.Margin = new Padding(4, 5, 4, 5);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new System.Drawing.Size(708, 185);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Acknowledgements";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            richTextBox1.BorderStyle = BorderStyle.None;
            richTextBox1.Cursor = Cursors.IBeam;
            richTextBox1.Dock = DockStyle.Fill;
            richTextBox1.Location = new System.Drawing.Point(0, 0);
            richTextBox1.Margin = new Padding(4, 5, 4, 5);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new System.Drawing.Size(708, 185);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = resources.GetString("richTextBox1.Text");
            richTextBox1.LinkClicked += richTextBox1_LinkClicked;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(textBox2);
            tabPage2.Location = new System.Drawing.Point(4, 29);
            tabPage2.Margin = new Padding(4, 5, 4, 5);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(4, 5, 4, 5);
            tabPage2.Size = new System.Drawing.Size(708, 185);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "License";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            textBox2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            textBox2.BorderStyle = BorderStyle.None;
            textBox2.Cursor = Cursors.IBeam;
            textBox2.Dock = DockStyle.Fill;
            textBox2.Location = new System.Drawing.Point(4, 5);
            textBox2.Margin = new Padding(8, 5, 4, 5);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.ScrollBars = ScrollBars.Both;
            textBox2.Size = new System.Drawing.Size(700, 175);
            textBox2.TabIndex = 25;
            textBox2.TabStop = false;
            textBox2.Text = resources.GetString("textBox2.Text");
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(textBox1);
            tabPage1.Location = new System.Drawing.Point(4, 29);
            tabPage1.Margin = new Padding(4, 5, 4, 5);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4, 5, 4, 5);
            tabPage1.Size = new System.Drawing.Size(708, 185);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "About";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Dock = DockStyle.Fill;
            textBox1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textBox1.Location = new System.Drawing.Point(4, 5);
            textBox1.Margin = new Padding(8, 5, 4, 5);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new System.Drawing.Size(700, 175);
            textBox1.TabIndex = 24;
            textBox1.TabStop = false;
            textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            tabControl1.Location = new System.Drawing.Point(16, 200);
            tabControl1.Margin = new Padding(4, 5, 4, 5);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(716, 218);
            tabControl1.TabIndex = 25;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(textBoxSupportText);
            tabPage4.Location = new System.Drawing.Point(4, 29);
            tabPage4.Margin = new Padding(4, 5, 4, 5);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(4, 5, 4, 5);
            tabPage4.Size = new System.Drawing.Size(708, 185);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Diagnostics";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // textBoxSupportText
            // 
            textBoxSupportText.Location = new System.Drawing.Point(8, 9);
            textBoxSupportText.Margin = new Padding(4, 5, 4, 5);
            textBoxSupportText.Multiline = true;
            textBoxSupportText.Name = "textBoxSupportText";
            textBoxSupportText.ScrollBars = ScrollBars.Both;
            textBoxSupportText.Size = new System.Drawing.Size(688, 158);
            textBoxSupportText.TabIndex = 0;
            // 
            // labelProductName
            // 
            labelProductName.AutoSize = true;
            labelProductName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            labelProductName.Location = new System.Drawing.Point(236, 19);
            labelProductName.Margin = new Padding(4, 0, 4, 0);
            labelProductName.Name = "labelProductName";
            labelProductName.Size = new System.Drawing.Size(104, 20);
            labelProductName.TabIndex = 28;
            labelProductName.Text = "Product Name";
            // 
            // labelVersion
            // 
            labelVersion.AutoSize = true;
            labelVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            labelVersion.Location = new System.Drawing.Point(236, 54);
            labelVersion.Margin = new Padding(4, 0, 4, 0);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new System.Drawing.Size(57, 20);
            labelVersion.TabIndex = 29;
            labelVersion.Text = "Version";
            // 
            // labelCopyright
            // 
            labelCopyright.AutoSize = true;
            labelCopyright.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            labelCopyright.Location = new System.Drawing.Point(236, 89);
            labelCopyright.Margin = new Padding(4, 0, 4, 0);
            labelCopyright.Name = "labelCopyright";
            labelCopyright.Size = new System.Drawing.Size(74, 20);
            labelCopyright.TabIndex = 30;
            labelCopyright.Text = "Copyright";
            // 
            // AboutBox
            // 
            AcceptButton = okButton;
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(745, 488);
            Controls.Add(labelCopyright);
            Controls.Add(labelVersion);
            Controls.Add(labelProductName);
            Controls.Add(linkLabelCompany);
            Controls.Add(linkLabelCompany2);
            Controls.Add(okButton);
            Controls.Add(logoPictureBox);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutBox";
            Padding = new Padding(12, 14, 12, 14);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About SuperXPuTTY";
            ((System.ComponentModel.ISupportInitialize)logoPictureBox).EndInit();
            tabPage3.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.LinkLabel linkLabelCompany;
        private System.Windows.Forms.LinkLabel linkLabelCompany2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox textBoxSupportText;
    }
}
