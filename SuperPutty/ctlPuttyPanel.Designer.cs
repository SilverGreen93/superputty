namespace SuperPutty
{
    partial class ctlPuttyPanel
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
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyHostNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyHostAndPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.renameTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.acceptCommandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripPuttySep1 = new System.Windows.Forms.ToolStripSeparator();
            this.eventLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripPuttySep2 = new System.Windows.Forms.ToolStripSeparator();
            this.changeSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyAllToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearScrollbackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetTerminalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetAndClearScrollbackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutPuttyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.closeSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeOthersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeOthersToTheRightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSessionAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newSessionToolStripMenuItem,
            this.duplicateSessionToolStripMenuItem,
            this.copyHostNameToolStripMenuItem,
            this.copyHostAndPortToolStripMenuItem,
            this.saveSessionAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.renameTabToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.acceptCommandsToolStripMenuItem,
            this.toolStripPuttySep1,
            this.eventLogToolStripMenuItem,
            this.toolStripPuttySep2,
            this.changeSettingsToolStripMenuItem,
            this.copyAllToClipboardToolStripMenuItem,
            this.restartSessionToolStripMenuItem,
            this.clearScrollbackToolStripMenuItem,
            this.resetTerminalToolStripMenuItem,
            this.resetAndClearScrollbackToolStripMenuItem,
            this.toolStripSeparator3,
            this.aboutPuttyToolStripMenuItem,
            this.toolStripSeparator4,
            this.closeSessionToolStripMenuItem,
            this.closeOthersToolStripMenuItem,
            this.closeOthersToTheRightToolStripMenuItem,
            this.closeAllToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(309, 582);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // newSessionToolStripMenuItem
            // 
            this.newSessionToolStripMenuItem.Name = "newSessionToolStripMenuItem";
            this.newSessionToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.newSessionToolStripMenuItem.Text = "New session";
            // 
            // duplicateSessionToolStripMenuItem
            // 
            this.duplicateSessionToolStripMenuItem.Name = "duplicateSessionToolStripMenuItem";
            this.duplicateSessionToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.duplicateSessionToolStripMenuItem.Text = "Duplicate session";
            this.duplicateSessionToolStripMenuItem.Click += new System.EventHandler(this.duplicateSessionToolStripMenuItem_Click);
            // 
            // copyHostNameToolStripMenuItem
            // 
            this.copyHostNameToolStripMenuItem.Name = "copyHostNameToolStripMenuItem";
            this.copyHostNameToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.copyHostNameToolStripMenuItem.Text = "Copy host address";
            this.copyHostNameToolStripMenuItem.Click += new System.EventHandler(this.copyHostNameToolStripMenuItem_Click);
            // 
            // copyHostAndPortToolStripMenuItem
            // 
            this.copyHostAndPortToolStripMenuItem.Name = "copyHostAndPortToolStripMenuItem";
            this.copyHostAndPortToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.copyHostAndPortToolStripMenuItem.Text = "Copy host address and port";
            this.copyHostAndPortToolStripMenuItem.Click += new System.EventHandler(this.copyHostAndPortToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.BackColor = System.Drawing.Color.Red;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(305, 6);
            // 
            // renameTabToolStripMenuItem
            // 
            this.renameTabToolStripMenuItem.Name = "renameTabToolStripMenuItem";
            this.renameTabToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.renameTabToolStripMenuItem.Text = "Rename tab";
            this.renameTabToolStripMenuItem.Click += new System.EventHandler(this.renameTabToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.refreshToolStripMenuItem.Text = "Refresh tab";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // acceptCommandsToolStripMenuItem
            // 
            this.acceptCommandsToolStripMenuItem.Checked = true;
            this.acceptCommandsToolStripMenuItem.CheckOnClick = true;
            this.acceptCommandsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.acceptCommandsToolStripMenuItem.Name = "acceptCommandsToolStripMenuItem";
            this.acceptCommandsToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.acceptCommandsToolStripMenuItem.Text = "Accept commands";
            // 
            // toolStripPuttySep1
            // 
            this.toolStripPuttySep1.Name = "toolStripPuttySep1";
            this.toolStripPuttySep1.Size = new System.Drawing.Size(305, 6);
            // 
            // eventLogToolStripMenuItem
            // 
            this.eventLogToolStripMenuItem.Name = "eventLogToolStripMenuItem";
            this.eventLogToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.eventLogToolStripMenuItem.Tag = "0x0010";
            this.eventLogToolStripMenuItem.Text = "Event log";
            this.eventLogToolStripMenuItem.Click += new System.EventHandler(this.puTTYMenuTSMI_Click);
            // 
            // toolStripPuttySep2
            // 
            this.toolStripPuttySep2.Name = "toolStripPuttySep2";
            this.toolStripPuttySep2.Size = new System.Drawing.Size(305, 6);
            // 
            // changeSettingsToolStripMenuItem
            // 
            this.changeSettingsToolStripMenuItem.Name = "changeSettingsToolStripMenuItem";
            this.changeSettingsToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.changeSettingsToolStripMenuItem.Tag = "0x0050";
            this.changeSettingsToolStripMenuItem.Text = "Change settings...";
            this.changeSettingsToolStripMenuItem.Click += new System.EventHandler(this.puTTYMenuTSMI_Click);
            // 
            // copyAllToClipboardToolStripMenuItem
            // 
            this.copyAllToClipboardToolStripMenuItem.Name = "copyAllToClipboardToolStripMenuItem";
            this.copyAllToClipboardToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.copyAllToClipboardToolStripMenuItem.Tag = "0x0170";
            this.copyAllToClipboardToolStripMenuItem.Text = "Copy all to clipboard";
            this.copyAllToClipboardToolStripMenuItem.Click += new System.EventHandler(this.puTTYMenuTSMI_Click);
            // 
            // restartSessionToolStripMenuItem
            // 
            this.restartSessionToolStripMenuItem.Name = "restartSessionToolStripMenuItem";
            this.restartSessionToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.restartSessionToolStripMenuItem.Tag = "0x0040";
            this.restartSessionToolStripMenuItem.Text = "Restart session";
            this.restartSessionToolStripMenuItem.Click += new System.EventHandler(this.puTTYMenuTSMI_Click);
            // 
            // clearScrollbackToolStripMenuItem
            // 
            this.clearScrollbackToolStripMenuItem.Name = "clearScrollbackToolStripMenuItem";
            this.clearScrollbackToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.clearScrollbackToolStripMenuItem.Tag = "0x0060";
            this.clearScrollbackToolStripMenuItem.Text = "Clear scrollback";
            this.clearScrollbackToolStripMenuItem.Click += new System.EventHandler(this.puTTYMenuTSMI_Click);
            // 
            // resetTerminalToolStripMenuItem
            // 
            this.resetTerminalToolStripMenuItem.Name = "resetTerminalToolStripMenuItem";
            this.resetTerminalToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.resetTerminalToolStripMenuItem.Tag = "0x0070";
            this.resetTerminalToolStripMenuItem.Text = "Reset terminal";
            this.resetTerminalToolStripMenuItem.Click += new System.EventHandler(this.puTTYMenuTSMI_Click);
            // 
            // resetAndClearScrollbackToolStripMenuItem
            // 
            this.resetAndClearScrollbackToolStripMenuItem.Name = "resetAndClearScrollbackToolStripMenuItem";
            this.resetAndClearScrollbackToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.resetAndClearScrollbackToolStripMenuItem.Tag = "0x0070;0x0060";
            this.resetAndClearScrollbackToolStripMenuItem.Text = "Reset terminal and clear scrollback";
            this.resetAndClearScrollbackToolStripMenuItem.Click += new System.EventHandler(this.puTTYMenuTSMI_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(305, 6);
            // 
            // aboutPuttyToolStripMenuItem
            // 
            this.aboutPuttyToolStripMenuItem.Name = "aboutPuttyToolStripMenuItem";
            this.aboutPuttyToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.aboutPuttyToolStripMenuItem.Text = "About Putty";
            this.aboutPuttyToolStripMenuItem.Click += new System.EventHandler(this.aboutPuttyToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(305, 6);
            // 
            // closeSessionToolStripMenuItem
            // 
            this.closeSessionToolStripMenuItem.Name = "closeSessionToolStripMenuItem";
            this.closeSessionToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.closeSessionToolStripMenuItem.Text = "Close";
            this.closeSessionToolStripMenuItem.Click += new System.EventHandler(this.closeSessionToolStripMenuItem_Click);
            // 
            // closeOthersToolStripMenuItem
            // 
            this.closeOthersToolStripMenuItem.Name = "closeOthersToolStripMenuItem";
            this.closeOthersToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.closeOthersToolStripMenuItem.Text = "Close others ";
            this.closeOthersToolStripMenuItem.Click += new System.EventHandler(this.closeOthersToolStripMenuItem_Click);
            // 
            // closeOthersToTheRightToolStripMenuItem
            // 
            this.closeOthersToTheRightToolStripMenuItem.Name = "closeOthersToTheRightToolStripMenuItem";
            this.closeOthersToTheRightToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.closeOthersToTheRightToolStripMenuItem.Text = "Close others to the right";
            this.closeOthersToTheRightToolStripMenuItem.Click += new System.EventHandler(this.closeOthersToTheRightToolStripMenuItem_Click);
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.closeAllToolStripMenuItem.Text = "Close all";
            this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
            // 
            // saveSessionAsToolStripMenuItem
            // 
            this.saveSessionAsToolStripMenuItem.Name = "saveSessionAsToolStripMenuItem";
            this.saveSessionAsToolStripMenuItem.Size = new System.Drawing.Size(308, 26);
            this.saveSessionAsToolStripMenuItem.Text = "Save session as...";
            this.saveSessionAsToolStripMenuItem.Click += new System.EventHandler(this.saveSessionAsToolStripMenuItem_Click);
            // 
            // ctlPuttyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1785, 265);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ctlPuttyPanel";
            this.TabPageContextMenuStrip = this.contextMenuStrip1;
            this.Text = "PuTTY Session";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripPuttySep1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem duplicateSessionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeSessionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutPuttyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newSessionToolStripMenuItem;

        private System.Windows.Forms.ToolStripMenuItem eventLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartSessionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearScrollbackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetTerminalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyAllToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripPuttySep2;
        private System.Windows.Forms.ToolStripMenuItem changeSettingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem acceptCommandsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeOthersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeOthersToTheRightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetAndClearScrollbackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyHostNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyHostAndPortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveSessionAsToolStripMenuItem;
    }
}