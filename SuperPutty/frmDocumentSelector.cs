/*
 * Copyright (c) 2009 - 2015 Jim Radford http://www.jimradford.com
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions: 
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using log4net;
using SuperPutty.Utils;
using DarkModeForms;

namespace SuperPutty
{
    public partial class frmDocumentSelector : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(frmDocumentSelector));

        private DockPanel dockPanel;

        public frmDocumentSelector(DockPanel dockPanel)
        {
            this.dockPanel = dockPanel;
            InitializeComponent();
            this.checkSendToVisible.Checked = SuperPuTTY.Settings.SendCommandsToVisibleOnly;

            if (SuperPuTTY.Settings.InterfaceTheme < (int)InterfaceTheme.LightTheme)
            {
                new DarkModeCS(this)
                {
                    ColorMode = DarkModeCS.DisplayMode.DarkMode,
                    ColorizeIcons = false
                };
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            var d = this.dockPanel;
            base.OnVisibleChanged(e);
            if (this.Visible)
            {
                // load docs into the ListView
                this.listViewDocs.Items.Clear();
                int i = 0;
                foreach (IDockContent doc in VisualOrderTabSwitchStrategy.GetDocuments(this.dockPanel))
                {
                    i++;
                    ctlPuttyPanel pp = doc as ctlPuttyPanel;
                    if (pp != null)
                    {
                        string tabNum = pp == this.dockPanel.ActiveDocument ? i + "*" : i.ToString();
                        ListViewItem item = this.listViewDocs.Items.Add(tabNum);
                        item.SubItems.Add(new ListViewItem.ListViewSubItem(item, pp.Text));
                        item.SubItems.Add(new ListViewItem.ListViewSubItem(item, pp.Session.SessionId));
                        item.SubItems.Add(new ListViewItem.ListViewSubItem(item, pp.GetHashCode().ToString()));

                        item.Selected = IsDocumentSelected(pp);
                        item.Tag = pp;
                    }

                }
                this.BeginInvoke(new Action(delegate { this.listViewDocs.Focus(); }));
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Log.Debug("Cancel");
            this.Hide();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Log.Debug("OK");
            foreach (ListViewItem item in this.listViewDocs.Items)
            {
                ((ctlPuttyPanel)item.Tag).AcceptCommands = item.Selected;
            }
            this.DialogResult = DialogResult.OK;
            this.Hide();

            SuperPuTTY.Settings.SendCommandsToVisibleOnly = this.checkSendToVisible.Checked;
        }

        // WFDEV004: OnClosing is obsolete. This overrides rather than calls base first on purpose: calling
        // base.OnFormClosing(e) would run the FormClosing handlers, which may set e.Cancel, and the
        // unconditional "e.Cancel = true" below would then overwrite it.
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // this dialog is only hidden, so the same code runs each time it opens: closing it with Esc,
            // Alt+F4 or the X must not destroy it. Restricted to UserClosing - on app shutdown
            // (Application.Exit, the main form Close, Windows shutdown) it has to close with the rest,
            // otherwise the message loop never ends and the process stays alive.
            if (e.CloseReason != CloseReason.UserClosing)
            {
                base.OnFormClosing(e);
                return;
            }
            e.Cancel = true;
            this.Hide();
        }

        public bool IsDocumentSelected(ctlPuttyPanel document)
        {
            bool selected = false;
            if (document != null && document.Session != null)
            {
                selected = this.checkSendToVisible.Checked ? document.Visible : document.AcceptCommands;
            }
            return selected;
        }

        private void checkSendToVisible_CheckedChanged(object sender, EventArgs e)
        {
            this.listViewDocs.Enabled = !this.checkSendToVisible.Checked;
        }
    }
}
