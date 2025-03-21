﻿namespace SuperPutty
{
    partial class LayoutsList
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
            this.listBoxLayouts = new System.Windows.Forms.ListBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadInNewInstanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.setAsDefaultLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxLayouts
            // 
            this.listBoxLayouts.ContextMenuStrip = this.contextMenuStrip;
            this.listBoxLayouts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLayouts.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxLayouts.FormattingEnabled = true;
            this.listBoxLayouts.IntegralHeight = false;
            this.listBoxLayouts.ItemHeight = 20;
            this.listBoxLayouts.Location = new System.Drawing.Point(0, 0);
            this.listBoxLayouts.Margin = new System.Windows.Forms.Padding(13, 4, 13, 4);
            this.listBoxLayouts.Name = "listBoxLayouts";
            this.listBoxLayouts.Size = new System.Drawing.Size(313, 375);
            this.listBoxLayouts.Sorted = true;
            this.listBoxLayouts.TabIndex = 0;
            this.listBoxLayouts.DoubleClick += new System.EventHandler(this.listBoxLayouts_DoubleClick);
            this.listBoxLayouts.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxLayouts_KeyDown);
            this.listBoxLayouts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBoxLayouts_MouseDown);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.loadInNewInstanceToolStripMenuItem,
            this.toolStripMenuItem1,
            this.deleteToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.toolStripMenuItem2,
            this.setAsDefaultLayoutToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.ShowImageMargin = false;
            this.contextMenuStrip.Size = new System.Drawing.Size(192, 136);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // loadInNewInstanceToolStripMenuItem
            // 
            this.loadInNewInstanceToolStripMenuItem.Name = "loadInNewInstanceToolStripMenuItem";
            this.loadInNewInstanceToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.loadInNewInstanceToolStripMenuItem.Text = "Load in new instance";
            this.loadInNewInstanceToolStripMenuItem.Click += new System.EventHandler(this.loadInNewInstanceToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(188, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(188, 6);
            // 
            // setAsDefaultLayoutToolStripMenuItem
            // 
            this.setAsDefaultLayoutToolStripMenuItem.Name = "setAsDefaultLayoutToolStripMenuItem";
            this.setAsDefaultLayoutToolStripMenuItem.Size = new System.Drawing.Size(191, 24);
            this.setAsDefaultLayoutToolStripMenuItem.Text = "Set as default layout";
            this.setAsDefaultLayoutToolStripMenuItem.Click += new System.EventHandler(this.setAsDefaultLayoutToolStripMenuItem_Click);
            // 
            // LayoutsList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(313, 375);
            this.Controls.Add(this.listBoxLayouts);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "LayoutsList";
            this.Text = "Layouts";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxLayouts;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadInNewInstanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem setAsDefaultLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
    }
}