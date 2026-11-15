using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DarkModeForms;
using SuperPutty.Data;
using SuperPutty.Gui;

namespace SuperPutty
{
    public partial class LayoutsList : ToolWindow
    {
        public LayoutsList()
        {
            InitializeComponent();

            this.listBoxLayouts.DataSource = SuperPuTTY.Layouts;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            this.listBoxLayouts.DataSource = null;
            base.OnFormClosed(e);
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            int idx = IndexAtCursor();
            e.Cancel = idx == -1;

            if (e.Cancel)
                return;

            LayoutData layout = (LayoutData)this.listBoxLayouts.Items[idx];

            loadInNewInstanceToolStripMenuItem.Enabled = !SuperPuTTY.Settings.SingleInstanceMode;
            renameToolStripMenuItem.Enabled = !layout.IsReadOnly;
            deleteToolStripMenuItem.Enabled = !layout.IsReadOnly;
            setAsDefaultLayoutToolStripMenuItem.Enabled = !layout.IsDefault;
        }

        private void listBoxLayouts_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // select item under mouse
                int idx = this.listBoxLayouts.IndexFromPoint(e.X, e.Y);
                if (idx != -1)
                {
                    this.listBoxLayouts.SelectedIndex = idx;
                }
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutData layout = (LayoutData) this.listBoxLayouts.SelectedItem;
            if (layout != null)
            {
                SuperPuTTY.LoadLayout(layout);
            }
        }

        private void loadInNewInstanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutData layout = (LayoutData)this.listBoxLayouts.SelectedItem;
            if (layout != null)
            {
                SuperPuTTY.LoadLayoutInNewInstance(layout);
            }
        }

        private void setAsDefaultLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutData layout = (LayoutData)this.listBoxLayouts.SelectedItem;
            if (layout != null)
            {
                SuperPuTTY.SetLayoutAsDefault(layout.Name);
            }
        }

        private void listBoxLayouts_DoubleClick(object sender, EventArgs e)
        {
            int idx = IndexAtCursor();
            if (idx != -1)
            {
                LayoutData layout = (LayoutData)this.listBoxLayouts.Items[idx];
                if (layout != null)
                {
                    SuperPuTTY.LoadLayout(layout);
                }
            }
            
        }

        int IndexAtCursor()
        {
            Point p = this.listBoxLayouts.PointToClient(Cursor.Position);
            return this.listBoxLayouts.IndexFromPoint(p.X, p.Y);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutData layout = (LayoutData)this.listBoxLayouts.SelectedItem;
            if (layout != null)
            {
                if (layout.IsDefault)
                {
                    Messenger.MessageBox("Cannot delete the default layout", "Delete layout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (layout.IsReadOnly)
                {
                    Messenger.MessageBox("Cannot delete automatic layout", "Delete layout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (DialogResult.Yes == Messenger.MessageBox("Are you sure you want to delete layout \"" + layout.Name + "\"?", "Delete layout", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                {
                    SuperPuTTY.RemoveLayout(layout.Name, true);
                }
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutData layout = (LayoutData)this.listBoxLayouts.SelectedItem;
            if (layout != null)
            {
                dlgRenameItem renameDialog = new dlgRenameItem
                {
                    DetailName = String.Empty,
                    ItemName = layout.Name,
                    ItemNameValidator = this.ValidateLayoutName,
                    Text = "Rename layout"
                };
                if (DialogResult.OK == renameDialog.ShowDialog(this))
                {
                    SuperPuTTY.RenameLayout(layout, renameDialog.ItemName);
                }
            }
            
        }

        bool ValidateLayoutName(string name, out string error)
        {
            LayoutData layout = SuperPuTTY.FindLayout(name);
            if (layout != null)
            {
                error = "A layout with the same name already exists";
                return false;
            }

            error = null;
            return true;
        }

        private void listBoxLayouts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                deleteToolStripMenuItem_Click(deleteToolStripMenuItem, e);
                e.Handled = true;
            }
        }
    }
}
