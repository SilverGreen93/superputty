﻿/*
 * Copyright (c) 2009 Jim Radford http://www.jimradford.com
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

using DarkModeForms;
using log4net;
using Microsoft.VisualBasic;
using SuperPutty.Data;
using SuperPutty.Gui;
using SuperPutty.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using static System.Collections.Specialized.BitVector32;


namespace SuperPutty
{
    public delegate void SelectionChangedHandler(SessionData Session);

    public partial class SessionTreeview : ToolWindow, IComparer
    {
        public event SelectionChangedHandler SelectionChanged;
        private static readonly ILog Log = LogManager.GetLogger(typeof(SessionTreeview));

        private static int MaxSessionsToOpen = Convert.ToInt32(ConfigurationManager.AppSettings["SuperPuTTY.MaxSessionsToOpen"] ?? "10");

        public const string SessionIdDelim = "/";
        public const string ImageKeySession = "computer";
        public const string ImageKeyFolder = "folder";

        private DockPanel m_DockPanel;
        private bool isRenamingNode;
        TreeNode nodeRoot;
        ImageList imgIcons = new ImageList();
        Func<SessionData, bool> filter;

        public SessionData SelectedSession
        {
            get
            {
                if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag is SessionData)
                {
                    return treeView1.SelectedNode.Tag as SessionData;
                }
                return null;
            }
        }

        /// <summary>
        /// Instantiate the treeview containing the sessions
        /// </summary>
        /// <param name="dockPanel">The DockPanel container</param>
        /// <remarks>Having the dockpanel container is necessary to allow us to
        /// dock any terminal or file transfer sessions from within the treeview class</remarks>
        public SessionTreeview(DockPanel dockPanel)
        {
            m_DockPanel = dockPanel;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            InitializeComponent();
            this.treeView1.TreeViewNodeSorter = this;
            this.treeView1.HideSelection = false;
            if (SuperPuTTY.Images != null)
            {
                this.treeView1.ImageList = SuperPuTTY.Images;
            }
            this.ApplySettings();

            // populate sessions in the treeview from the registry
            this.LoadSessions();
            this.ExpandInitialTree();
            SuperPuTTY.Sessions.ListChanged += new ListChangedEventHandler(Sessions_ListChanged);
            SuperPuTTY.Settings.SettingsSaving += new SettingsSavingEventHandler(Settings_SettingsSaving);
        }

        void ExpandInitialTree()
        {
            if (SuperPuTTY.Settings.ExpandSessionsTreeOnStartup)
            {
                nodeRoot.ExpandAll();
                this.treeView1.SelectedNode = nodeRoot;
            }
            else
            {
                // start with semi-collapsed view
                nodeRoot.Expand();
                foreach (TreeNode node in this.nodeRoot.Nodes)
                {
                    if (!IsSessionNode(node))
                    {
                        node.Collapse();
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (SuperPuTTY.Settings.InterfaceTheme < (int)InterfaceTheme.LightTheme)
            {
                // Assign custom renderer for context menus
                contextMenuStripFolder.Renderer = new MyRenderer(new CustomColorTable(new OSThemeColors()), false)
                {
                    MyColors = DarkModeCS.GetSystemColors(null, 0)
                };
                contextMenuStripAddTreeItem.Renderer = new MyRenderer(new CustomColorTable(new OSThemeColors()), false)
                {
                    MyColors = DarkModeCS.GetSystemColors(null, 0)
                };
            }
        }

        void Settings_SettingsSaving(object sender, CancelEventArgs e)
        {
            this.ApplySettings();
        }

        void ApplySettings()
        {
            this.treeView1.ShowLines = SuperPuTTY.Settings.SessionsTreeShowLines;
            this.treeView1.Font = SuperPuTTY.Settings.SessionsTreeFont;
            this.panelSearch.Visible = SuperPuTTY.Settings.SessionsShowSearch;
            this.ResortNodes();
        }

        protected override void OnClosed(EventArgs e)
        {
            SuperPuTTY.Sessions.ListChanged -= new ListChangedEventHandler(Sessions_ListChanged);
            SuperPuTTY.Settings.SettingsSaving -= new SettingsSavingEventHandler(Settings_SettingsSaving);
            base.OnClosed(e);
        }

        /// <summary>
        /// Load the sessions from the registry and populate the treeview control
        /// </summary>
        void LoadSessions()
        {
            treeView1.Nodes.Clear();

            this.nodeRoot = treeView1.Nodes.Add("root", "Saved sessions", ImageKeyFolder, ImageKeyFolder);
            this.nodeRoot.ContextMenuStrip = this.contextMenuStripFolder;

            foreach (SessionData session in SuperPuTTY.GetAllSessions())
            {
                TryAddSessionNode(session);
            }
            ResortNodes();
        }

        private void TryAddSessionNode(SessionData session)
        {
            TreeNode nodeParent = this.nodeRoot;
            if (this.filter == null || this.filter(session))
            {
                if (session.SessionId != null && session.SessionId != session.SessionName)
                {
                    // take session id and create folder nodes
                    nodeParent = FindOrCreateParentNode(session.SessionId);
                }
                AddSessionNode(nodeParent, session, true);
            }
        }

        public void SessionPropertyChanged(SessionData Session, String PropertyName)
        {
            if (Session == null)
                return;

            if (PropertyName == "SessionName" || PropertyName == "ImageKey")
            {
                TreeNode Node = FindSessionNode(Session.SessionId);
                if (Node == null)
                {
                    // It is possible that the session id was changed before the
                    // session name. In this case, we check to see if we
                    // can find a node with the old session id that is also associated
                    // to the session data.
                    Node = FindSessionNode(Session.OldSessionId);
                    if (Node == null || Node.Tag != Session)
                        return;
                }

                Node.Text = Session.SessionName;
                Node.Name = Session.SessionName;
                Node.ImageKey = Session.ImageKey;
                Node.SelectedImageKey = Session.ImageKey;

                bool IsSelectedSession = treeView1.SelectedNode == Node;
                ResortNodes();
                if (IsSelectedSession)
                {
                    // Re-selecting the node since it will be unselected when sorting.
                    treeView1.SelectedNode = Node;
                }
            }
            else if (PropertyName == "SessionId")
            {
                TreeNode Node = FindSessionNode(Session.OldSessionId);
                if (Node == null)
                {
                    // It is possible that the session name was changed before the
                    // session id. In this case, we check to see if we
                    // can find a node with the current session id that is also associated
                    // to the session data.
                    Node = FindSessionNode(Session.SessionId);
                    if (Node == null || Node.Tag != Session)
                        return;
                }

                try
                {
                    this.isRenamingNode = true;
                    SuperPuTTY.RemoveSession(Session.OldSessionId);
                    SuperPuTTY.AddSession(Session);
                }
                finally
                {
                    this.isRenamingNode = false;
                }
            }
        }

        public void SessionPropertyChanging(SessionData Session, String AttributeName, Object NewValue, ref bool CancelChange)
        {
            if (Session == null)
                return;

            if (AttributeName == "SessionName")
            {
                TreeNode Node = FindSessionNode(Session.SessionId);
                if (Node == null)
                    return;

                String Error = "";
                bool IsValid = ValidateSessionNameChange(Node.Parent, Node, NewValue as String, out Error);
                if (!IsValid)
                {
                    CancelChange = true;
                    Messenger.MessageBox(Error);
                }
            }
        }

        void Sessions_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (isRenamingNode)
            {
                return;
            }
            BindingList<SessionData> sessions = (BindingList<SessionData>) sender;
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                SessionData session = sessions[e.NewIndex];
                TryAddSessionNode(session);
            }
            else if (e.ListChangedType == ListChangedType.Reset)
            {
                // clear
                List<TreeNode> nodesToRemove = nodeRoot.Nodes.Cast<TreeNode>().ToList();

                foreach (TreeNode node in nodesToRemove)
                {
                    node.Remove();
                }
            }
            // @TODO: implement more later, note delete will be tricky...need a copy of the list
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this.SelectedSession);
            }
        }

        /// <summary>
        /// Opens the selected session when the node is double clicked in the treeview
        /// </summary>
        /// <param name="sender">The treeview control that was double clicked</param>
        /// <param name="e">An Empty EventArgs object</param>
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // e is null if this method is called from connectToolStripMenuItem_Click
            TreeNode node = e != null ? e.Node : treeView1.SelectedNode;

            if (IsSessionNode(node) && node == treeView1.SelectedNode)
            {
                SessionData sessionData = (SessionData)node.Tag;
                SuperPuTTY.OpenProtoSession(sessionData);
            }
        }


        /// <summary>
        /// Create/Update a session entry
        /// </summary>
        /// <param name="sender">The toolstripmenuitem control that was clicked</param>
        /// <param name="e">An Empty EventArgs object</param>
        private void CreateOrEditSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem))
            {
                return;
            }

            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;

            if (menuItem == newSessionToolStripMenuItem)
            {
                // create new session
                CreateOrEditSession(0, null);
            }
            else if (menuItem == createLikeToolStripMenuItem)
            {
                // copy as
                SessionData session = (SessionData)((SessionData)treeView1.SelectedNode.Tag).Clone();
                CreateOrEditSession(1, session);
            }
            else
            {
                // edit, session node selected
                // We make a clone of the session since we do not want to directly edit the real object.
                SessionData session = (SessionData)((SessionData)treeView1.SelectedNode.Tag).Clone();
                CreateOrEditSession(2, session);
            }
        }


        public void CreateOrEditSession(int mode, SessionData existingSession)
        { 
            SessionData session = existingSession;
            TreeNode node = null;
            TreeNode nodeRef = nodeRoot;
            string title = null;

            if (treeView1.SelectedNode == null)
            {
                treeView1.SelectedNode = nodeRoot;
            }

            bool isFolderNode = IsFolderNode(treeView1.SelectedNode);

            if (mode == 0)
            {
                // create new session
                session = new SessionData();
                nodeRef = isFolderNode ? treeView1.SelectedNode : treeView1.SelectedNode.Parent;
                title = "Create new session";
            }
            else if (mode == 1)
            {
                // copy as
                if (session == null)
                {
                    Log.DebugFormat("session null");
                    return;
                }
                session.SessionId = SuperPuTTY.MakeUniqueSessionId(session.SessionId);
                session.SessionName = SessionData.GetSessionNameFromId(session.SessionId);
                nodeRef = isFolderNode ? treeView1.SelectedNode : treeView1.SelectedNode.Parent;
                title = "Create new session based on " + session.OldName;
            }
            else if (mode == 2)
            {
                if (session == null)
                {
                    Log.DebugFormat("session null");
                    return;
                }
                // edit, session node selected
                node = treeView1.SelectedNode;
                nodeRef = node.Parent;
                title = "Edit session: " + session.SessionName;
            }

            dlgEditSession form = new dlgEditSession(session, treeView1.ImageList) {Text = title};
            form.SessionNameValidator += delegate(string txt, out string error)
            {
                bool IsValid = ValidateSessionNameChange(nodeRef, node, txt, out error);
                return IsValid;
            };
            
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                /* "node" will only be assigned if we're editing an existing session entry */
                if (node == null)
                {
                    // get the path up to the ref (parent) node
                    if (nodeRoot != nodeRef)
                    {
                        UpdateSessionId(nodeRef, session);
                        session.SessionId = SessionData.CombineSessionIds(session.SessionId, session.SessionName);
                    }
                    SuperPuTTY.AddSession(session);

                    // find new node and select it
                    TreeNode nodeNew = nodeRef.Nodes[session.SessionName];
                    if (nodeNew != null)
                    {
                        treeView1.SelectedNode = nodeNew;
                    }
                }
                else
                {

                    SessionData RealSession = (SessionData)treeView1.SelectedNode.Tag;
                    RealSession.CopyFrom(session);
                    RealSession.SessionName = session.SessionName;
                    treeView1.SelectedNode = node;
                }

                //treeView1.ExpandAll();
                SuperPuTTY.SaveSessions();
            }
            
        }

        private bool ValidateSessionNameChange(TreeNode ParentNode, TreeNode Node, String NewName, out String Error)
        {
            bool IsEdit = Node != null;
            Error = String.Empty;
            bool IsDupeNode;
            if (IsEdit)
                IsDupeNode = ParentNode.Nodes.ContainsKey(NewName) && ParentNode.Nodes[NewName] != Node;
            else
                IsDupeNode = ParentNode.Nodes.ContainsKey(NewName);

            if (IsDupeNode)
            {
                Error = "Session with same name exists";
            }
            else if (NewName.Contains(SessionIdDelim))
            {
                Error = "Invalid character ( " + SessionIdDelim + " ) in name";
            }
            else if (string.IsNullOrEmpty(NewName) || NewName.Trim() == String.Empty)
            {
                Error = "Empty name";
            }
            return string.IsNullOrEmpty(Error);
        }

        /// <summary>
        /// Forces a node to be selected when right clicked, this assures the context menu will be operating
        /// on the proper session entry.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
            }          
        }

        /// <summary>
        /// Delete a session entry from the treeview and the registry
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SessionData session = (SessionData)treeView1.SelectedNode.Tag;
            if (Messenger.MessageBox("Are you sure you want to delete session \"" + session.SessionName + "\"?", "Delete session", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                //session.RegistryRemove(session.SessionName);
                treeView1.SelectedNode.Remove();
                SuperPuTTY.RemoveSession(session.SessionId);
                SuperPuTTY.SaveSessions();
                //m_SessionsById.Remove(session.SessionId);
            }
        }

        /// <summary>
        /// Open a directory listing on the selected nodes host to allow dropping files
        /// for drag + drop copy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SessionData session = (SessionData)treeView1.SelectedNode.Tag;
            SuperPuTTY.OpenScpSession(session);
        }

        /// <summary>
        /// Shortcut for double clicking an entries node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1_NodeMouseDoubleClick(null, null);
        }

        /// <summary>
        /// Open putty with args but as external process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectExternalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.SelectedNode;
            if (IsSessionNode(node))
            {
                SessionData sessionData = (SessionData)node.Tag;
                PuttyStartInfo startInfo = new PuttyStartInfo(sessionData);
                startInfo.StartStandalone();
            }
        }

        private void connectInNewSuperPuTTYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.SelectedNode;
            if (IsSessionNode(node))
            {
                SuperPuTTY.LoadSessionInNewInstance(((SessionData)node.Tag).SessionId);
            }
        }

        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.SelectedNode;
            if (node != null)
            {
                dlgRenameItem dialog = new dlgRenameItem
                {
                    Text = "New Folder",
                    ItemName = "New Folder",
                    DetailName = "",
                    ItemNameValidator = delegate(string txt, out string error)
                    {
                        error = String.Empty;
                        if (node.Nodes.ContainsKey(txt))
                        {
                            error = "Node with same name exists";
                        }
                        else if (txt.Contains(SessionIdDelim))
                        {
                            error = "Invalid character ( " + SessionIdDelim + " ) in name";
                        }
                        else if (string.IsNullOrEmpty(txt) || txt.Trim() == String.Empty)
                        {
                            error = "Empty folder name";
                        }

                        return string.IsNullOrEmpty(error);
                    }
                };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    AddFolderNode(node, dialog.ItemName);
                }
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.SelectedNode;
            if (node != null)
            {
                dlgRenameItem dialog = new dlgRenameItem
                {
                    Text = "Rename Folder",
                    ItemName = node.Text,
                    DetailName = "",
                    ItemNameValidator = delegate(string txt, out string error)
                    {
                        error = String.Empty;
                        if (node.Parent.Nodes.ContainsKey(txt) && txt != node.Text)
                        {
                            error = "Node with same name exists";
                        }
                        else if (txt.Contains(SessionIdDelim))
                        {
                            error = "Invalid character ( " + SessionIdDelim + " ) in name";
                        }
                        return string.IsNullOrEmpty(error);
                    }
                };
                if (dialog.ShowDialog(this) == DialogResult.OK && node.Text != dialog.ItemName)
                {
                    node.Text = dialog.ItemName;
                    node.Name = dialog.ItemName;
                    UpdateSessionId(node);
                    SuperPuTTY.SaveSessions();
                    ResortNodes();
                }
            }
        }

        private void removeFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.SelectedNode;
            if (node != null)
            {
                if (node.Nodes.Count > 0)
                {
                    List<SessionData> sessions = new List<SessionData>();
                    GetAllSessions(node, sessions);
                    if (DialogResult.Yes == Messenger.MessageBox(
                        "Are you sure you want to remove folder \"" + node.Text + "\" and its " + sessions.Count + " sessions?",
                        "Remove folder", 
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question))
                    {
                        foreach (SessionData session in sessions)
                        {
                            SuperPuTTY.RemoveSession(session.SessionId);
                        }
                        node.Remove();
                        SuperPuTTY.ReportStatus("Removed Folder, {0} and {1} sessions", node.Text, sessions.Count);
                        SuperPuTTY.SaveSessions();
                    }
                }
                else
                {
                    node.Remove();
                    SuperPuTTY.ReportStatus("Removed Folder, {0}", node.Text);
                }
            }
        }

        private void connectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.SelectedNode;
            if (node != null && !IsSessionNode(node))
            {
                List<SessionData> sessions = new List<SessionData>();
                GetAllSessions(node, sessions);
                Log.InfoFormat("Found {0} sessions", sessions.Count);

                if (sessions.Count > MaxSessionsToOpen)
                {
                    if (DialogResult.Cancel == Messenger.MessageBox(
                        "Open All " + sessions.Count + " sessions?", 
                        "WARNING", 
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                    {
                        // bug out...too many sessions to open
                        return;
                    }
                }
                foreach (SessionData session in sessions)
                {
                    SuperPuTTY.OpenProtoSession(session);
                }
            }
        }

        private void contextMenuStripFolder_Opening(object sender, CancelEventArgs e)
        {
            bool isRootNode = this.treeView1.SelectedNode != this.nodeRoot;
            this.renameToolStripMenuItem.Enabled = isRootNode;
            // TODO: handle removing folder and nodes in it recursively
            this.removeFolderToolStripMenuItem.Enabled = isRootNode;// && this.treeView1.SelectedNode.Nodes.Count == 0;
        }

        private void contextMenuStripAddTreeItem_Opening(object sender, CancelEventArgs e)
        {
            // disable file transfers if pscp isn't configured.
            fileBrowserToolStripMenuItem.Enabled = SuperPuTTY.IsScpEnabled;
            fileZillaToolStripMenuItem.Enabled = SuperPuTTY.IsFilezillaEnabled;
            winSCPToolStripMenuItem.Enabled = SuperPuTTY.IsWinSCPEnabled;
            fileZillaToolStripMenuItem.Visible = SuperPuTTY.IsFilezillaEnabled;
            winSCPToolStripMenuItem.Visible = SuperPuTTY.IsWinSCPEnabled;

            connectInNewSuperPuTTYToolStripMenuItem.Enabled = !SuperPuTTY.Settings.SingleInstanceMode;
        }

        #region Node helpers

        TreeNode AddSessionNode(TreeNode parentNode, SessionData session, bool isInitializing)
        {
            TreeNode addedNode = null;
            if (parentNode.Nodes.ContainsKey(session.SessionName))
            {
                SuperPuTTY.ReportStatus("Node with the same name exists.  New node ({0}) NOT added", session.SessionName);
            }
            else
            {
                addedNode = parentNode.Nodes.Add(session.SessionName, session.SessionName, ImageKeySession, ImageKeySession);
                addedNode.Tag = session;
                addedNode.ContextMenuStrip = this.contextMenuStripAddTreeItem;
                addedNode.ToolTipText = session.ToString();

                // Override with custom icon if valid
                if (IsValidImage(session.ImageKey))
                {
                    addedNode.ImageKey = session.ImageKey;
                    addedNode.SelectedImageKey = session.ImageKey;
                }
            }

            session.OnPropertyChanged += SessionPropertyChanged;
            session.OnPropertyChanging += SessionPropertyChanging;

            return addedNode;
        }

        TreeNode AddFolderNode(TreeNode parentNode, String nodeName)
        {
            TreeNode nodeNew = null;
            if (parentNode.Nodes.ContainsKey(nodeName))
            {
                SuperPuTTY.ReportStatus("Node with the same name exists.  New node ({0}) NOT added", nodeName);
            }
            else
            {
                SuperPuTTY.ReportStatus("Adding new folder, {1}.  parent={0}", parentNode.Text, nodeName);
                nodeNew = parentNode.Nodes.Add(nodeName, nodeName, ImageKeyFolder, ImageKeyFolder);
                nodeNew.ContextMenuStrip = this.contextMenuStripFolder;
            }
            return nodeNew;
        }

        bool IsSessionNode(TreeNode node)
        {
            return node != null && node.Tag is SessionData;
        }

        bool IsFolderNode(TreeNode node)
        {
            return !IsSessionNode(node);
        }

        private void UpdateSessionId(TreeNode parentNode)
        {
            foreach (TreeNode node in parentNode.Nodes)
            {
                if (IsSessionNode(node))
                {
                    UpdateSessionId(node, (SessionData)node.Tag);
                }
                else
                {
                    UpdateSessionId(node);
                }
            }
        }
        private void UpdateSessionId(TreeNode addedNode, SessionData session)
        {
            // set session id as node path
            List<string> parentNodeNames = new List<string>();
            for (TreeNode node = addedNode; node.Parent != null; node = node.Parent)
            {
                parentNodeNames.Add(node.Text);
            }
            parentNodeNames.Reverse();
            String sessionId = String.Join(SessionIdDelim, parentNodeNames.ToArray());
            //Log.InfoFormat("sessionId={0}", sessionId);
            //session.SessionId = sessionId;
            if (session != null) session.SessionId = sessionId;
            //SuperPuTTY.SaveSessions();
            //session.SaveToRegistry();
        }

        TreeNode FindOrCreateParentNode(string sessionId)
        {
            Log.DebugFormat("Finding Parent Node for sessionId ({0})", sessionId);
            TreeNode nodeParent = this.nodeRoot;

            string[] parts = sessionId.Split(SessionIdDelim.ToCharArray());
            if (parts.Length > 1)
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    TreeNode node = nodeParent.Nodes[part];
                    if (node == null)
                    {
                        nodeParent = this.AddFolderNode(nodeParent, part);
                    }
                    else if (IsFolderNode(node))
                    {
                        nodeParent = node;
                    }
                }
            }

            Log.DebugFormat("Returning node ({0})", nodeParent.Text);
            return nodeParent;
        }

        TreeNode FindSessionNode(String SessionId)
        {
            Log.DebugFormat("Finding Node for sessionId ({0})", SessionId);
            TreeNode CurrentNode = this.nodeRoot;
            TreeNode NodeToReturn = null;

            string[] Parts = SessionId.Split(SessionIdDelim.ToCharArray());
            if (Parts.Length > 0)
            {
                for (int i = 0; i < Parts.Length; i++)
                {
                    string Part = Parts[i];
                    CurrentNode = CurrentNode.Nodes[Part];
                    if (CurrentNode == null)
                    {
                        return null;
                    }
                    else if (i == Parts.Length - 1 && !IsFolderNode(CurrentNode))
                    {
                        NodeToReturn = CurrentNode;
                    }
                }
            }

            Log.DebugFormat("Returning node ({0})", NodeToReturn != null ? NodeToReturn.Text : "null");
            return NodeToReturn;
        }

        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (SuperPuTTY.Settings.SessiontreeShowFoldersFirst) { 
                if (IsFolderNode(tx) && IsSessionNode(ty)) {
                    return -1;
                }
            }
            return string.Compare(tx.Text, ty.Text);

        }

        void ResortNodes()
        {
            this.treeView1.TreeViewNodeSorter = null;
            this.treeView1.TreeViewNodeSorter = this;
        }


        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
              TreeNode node = this.treeView1.SelectedNode;
              if (node != null)
              {
                  node.ExpandAll();
              }
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.treeView1.SelectedNode;
            if (node != null)
            {
                node.Collapse();
                if (node == this.nodeRoot)
                {
                    nodeRoot.Expand();
                }
            }
        }

        void GetAllSessions(TreeNode nodeFolder, List<SessionData> sessions)
        {
            if (nodeFolder != null)
            {
                foreach (TreeNode child in nodeFolder.Nodes)
                {
                    if (IsSessionNode(child))
                    {
                        SessionData session = (SessionData) child.Tag;
                        sessions.Add(session);
                    }
                    else
                    {
                        GetAllSessions(child, sessions);
                    }
                }
            }
        }

        void GetAllNodes(TreeNode node, List<TreeNode> nodes)
        {
            if (node != null)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Nodes.Count == 0)
                    {
                        nodes.Add(child);
                    }
                    else
                    {
                        GetAllNodes(child, nodes);
                    }
                }
            }
        }
        #endregion

        #region Drag Drop

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            // Get the tree
            TreeView tree = (TreeView)sender;

            // Get the node underneath the mouse.
            TreeNode node = e.Item as TreeNode;

            // Start the drag-and-drop operation with a cloned copy of the node.
            //if (node != null && IsSessionNode(node))
            if (node != null && tree.Nodes[0] != node)
            {
                this.treeView1.DoDragDrop(node, DragDropEffects.Copy);
            }
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            // Get the tree.
            TreeView tree = (TreeView)sender;

            // Drag and drop denied by default.
            e.Effect = DragDropEffects.None;

            // Is it a valid format?
            TreeNode nodePayload = (TreeNode) e.Data.GetData(typeof(TreeNode));
            if (nodePayload != null)
            {
                // Get the screen point.
                Point pt = new Point(e.X, e.Y);

                // Convert to a point in the TreeView's coordinate system.
                pt = tree.PointToClient(pt);

                TreeNode node = tree.GetNodeAt(pt);
                // Is the mouse over a valid node?
                if (node != null && node != nodePayload && nodePayload.Nodes.Find(node.Text, true).Length == 0)
                {
                    tree.SelectedNode = node;
                    // folder that is not the same parent and new node name is not already present
                    if (IsFolderNode(node) && node != nodePayload.Parent && !node.Nodes.ContainsKey(nodePayload.Text))
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                }
            }
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            // Get the tree.
            TreeView tree = (TreeView)sender;

            // Get the screen point.
            Point pt = new Point(e.X, e.Y);

            // Convert to a point in the TreeView's coordinate system.
            pt = tree.PointToClient(pt);

            // Get the node underneath the mouse.
            TreeNode node = tree.GetNodeAt(pt);

            if (IsFolderNode(node))
            {
                Log.DebugFormat("Drag drop");

                TreeNode nodePayload = (TreeNode)e.Data.GetData(typeof(TreeNode));
                TreeNode nodeNew = (TreeNode)nodePayload.Clone();

                // If node was expanded before, ensure new node is also expanded when moved
                if (nodePayload.IsExpanded)
                {
                    nodeNew.Expand();
                }

                // remove old
                nodePayload.Remove();

                // add new
                node.Nodes.Add(nodeNew);
                UpdateSessionId(nodeNew, (SessionData)nodeNew.Tag); //

                // If this a folder, reset it's childrens sessionIds
                if (IsFolderNode(nodeNew))
                {
                    resetFoldersChildrenPaths(nodeNew);
                    
                }

                // remove old
                nodePayload.Remove();

                // Show the newly added node if it is not already visible.
                node.Expand();

                // auto save settings...use timer to prevent excessive saves while dragging and dropping nodes
                timerDelayedSave.Stop();
                timerDelayedSave.Start();
                //SuperPuTTY.SaveSessions();
            }
        }

        #endregion

        public void resetFoldersChildrenPaths(TreeNode nodePayload)
        {
            // Reset folders children nodes sessionId (path)
            foreach (TreeNode node in nodePayload.Nodes)
            {
                UpdateSessionId(node, (SessionData)node.Tag);
            }
        }

        private void timerDelayedSave_Tick(object sender, EventArgs e)
        {
            // stop timer
            timerDelayedSave.Stop();

            // do save
            SuperPuTTY.SaveSessions();
            SuperPuTTY.ReportStatus("Saved Sessions after Drag-Drop @ {0}", DateTime.Now);
        }

        #region Icon
        bool IsValidImage(string imageKey)
        {
            bool valid = false;
            if (!string.IsNullOrEmpty(imageKey))
            {
                valid = this.treeView1.ImageList.Images.ContainsKey(imageKey);
                if (!valid)
                {
                    Log.WarnFormat("Missing icon, {0}", imageKey);
                }
            }
            return valid;
        }


        #endregion

        #region Search
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (SuperPuTTY.Settings.FilterSessionsOnChange)
                    {
                        treeView1.Focus();
                    }
                    else
                    {
                        this.ApplySearch(this.txtSearch.Text);
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Escape:
                    this.ClearSearch();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            this.ApplySearch(this.txtSearch.Text);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.ClearSearch();
        }

        private void ClearSearch()
        {
            this.txtSearch.Text = "";
            this.ApplySearch("");
        }
        private void ApplySearch(string txt)
        {
            Log.InfoFormat("Applying Search: txt={0}.", txt);

            // define filter
            SearchFilter searchFilter = new SearchFilter(SuperPuTTY.Settings.SessionsSearchMode, txt);
            this.filter = searchFilter.IsMatch;

            // reload
            this.LoadSessions();

            // if "clear" show init state otherwise expand all to show all matches
            if (string.IsNullOrEmpty(txt))
            {
                this.ExpandInitialTree();
            }
            else
            {
                this.treeView1.ExpandAll();
            }
        }

        public enum SearchMode
        {
            CaseSensitive, CaseInSensitive, Regex
        }

        public class SearchFilter
        {
            public SearchFilter(string mode, string filter)
            {
                this.Mode = FormUtils.SafeParseEnum(mode, true, SearchMode.CaseSensitive);
                this.Filter = filter;
                if (this.Mode == SearchMode.Regex)
                {
                    try
                    {
                        this.Regex = new Regex(filter);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not parse pattern: " + filter, ex);
                    }
                }
            }
            public bool IsMatch(SessionData s)
            {
                if (this.Mode == SearchMode.CaseInSensitive)
                {
                    return
                        (s.SessionName.ToLower().Contains(this.Filter.ToLower())) ||
                        (s.Host.ToLower().Contains(this.Filter.ToLower()));
                }
                else if (this.Mode == SearchMode.Regex)
                {
                    return this.Regex != null ? this.Regex.IsMatch(s.SessionName) : true;
                }
                else
                {
                    // case sensitive
                    return
                        s.SessionName.Contains(this.Filter) ||
                        s.Host.Contains(this.Filter);
                }
            }
            public SearchMode Mode { get; set; }
            public string Filter { get; set; }
            public Regex Regex { get; set; }
        }
        #endregion

        #region Key Handling
        private void treeView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13 && IsSessionNode(this.treeView1.SelectedNode))
            {
                if (Control.ModifierKeys == Keys.None)
                {
                    treeView1_NodeMouseDoubleClick(null, null);
                    e.Handled = true;
                }
                else if (Control.ModifierKeys == Keys.Shift)
                {
                    CreateOrEditSessionToolStripMenuItem_Click(this.settingsToolStripMenuItem, e);
                    e.Handled = true;
                }
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (IsFolderNode(treeView1.SelectedNode))
                {
                    removeFolderToolStripMenuItem_Click(removeFolderToolStripMenuItem, e);
                }
                else
                {
                    deleteToolStripMenuItem_Click(deleteToolStripMenuItem, e);
                }
                e.Handled = true;
            }
        }
        #endregion

        private void winSCPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SessionData session = (SessionData)treeView1.SelectedNode.Tag;
            ExternalApplications.openWinSCP(session);
        }

        private void fileZillaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SessionData session = (SessionData)treeView1.SelectedNode.Tag;
            ExternalApplications.openFileZilla(session); 
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (SuperPuTTY.Settings.FilterSessionsOnChange)
            {
                this.ApplySearch(this.txtSearch.Text);
            }
        }

        private void copyHostNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SessionData session;
            session = (SessionData)treeView1.SelectedNode.Tag;
            if (session.Port != 0)
            {
                Clipboard.SetText(session.Host + " " + session.Port);
            }
            else
            {
                Clipboard.SetText(session.Host);
            }
        }
    }

}
