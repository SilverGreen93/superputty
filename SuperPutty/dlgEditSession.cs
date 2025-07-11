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

using DarkModeForms;
using log4net;
using SuperPutty.Data;
using SuperPutty.Gui;
using SuperPutty.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace SuperPutty
{
    public partial class dlgEditSession : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(dlgEditSession));

        public delegate bool SessionNameValidationHandler(string name, out string error);

        private SessionData Session;
        private String OldHostname;
        private bool isInitialized = false;
        private ImageListPopup imgPopup = null;
        private System.Collections.Hashtable protoTypesMap;

        public dlgEditSession(SessionData session, ImageList iconList)
        {
            Session = session;
            InitializeComponent();

            // get putty saved settings from the registry to populate
            // the dropdown
            PopulatePuttySettings();
            PopulateProtoList();

            if (!String.IsNullOrEmpty(Session.SessionName))
            {
                this.Text = "Edit session: " + session.SessionName;
                this.textBoxSessionName.Text = Session.SessionName;
                this.textBoxHostname.Text = Session.Host;
                this.textBoxPort.Text = Session.Port.ToString();
                this.textBoxExtraArgs.Text = Session.ExtraArgs;
                this.textBoxUsername.Text = Session.Username;
                this.textBoxSPSLScriptFile.Text = Session.SPSLFileName;
                this.textBoxRemotePathSesion.Text = Session.RemotePath;
                this.textBoxLocalPathSesion.Text = Session.LocalPath;
                this.textBoxNote.Text = Session.Note;

                foreach (System.Collections.DictionaryEntry protoEntry in this.protoTypesMap)
                {
                    if ((int)protoEntry.Value == (int)Session.Proto)
                        comboBoxProto.SelectedItem = (string)protoEntry.Key;
                }

                comboBoxPuttyProfile.DropDownStyle = ComboBoxStyle.DropDownList;
                foreach(String settings in this.comboBoxPuttyProfile.Items){
                    if (settings == session.PuttySession)
                    {
                        this.comboBoxPuttyProfile.SelectedItem = settings;
                        break;
                    }
                }

                this.buttonSave.Enabled = true;
            }
            else
            {
                this.Text = "Create new session";
                comboBoxProto.SelectedItem = "SSH";
                this.buttonSave.Enabled = false;
            }


            // Setup icon chooser
            this.buttonImageSelect.ImageList = iconList;
            this.buttonImageSelect.ImageKey = string.IsNullOrEmpty(Session.ImageKey)
                ? SessionTreeview.ImageKeySession
                : Session.ImageKey;
            this.toolTip.SetToolTip(this.buttonImageSelect, buttonImageSelect.ImageKey);

            this.isInitialized = true;

            if (SuperPuTTY.Settings.InterfaceTheme < (int)InterfaceTheme.LightTheme)
            {
                new DarkModeCS(this)
                {
                    ColorMode = DarkModeCS.DisplayMode.DarkMode,
                    ColorizeIcons = false
                };
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.BeginInvoke(new MethodInvoker(delegate { this.textBoxSessionName.Focus(); }));
        }

        private void PopulatePuttySettings()
        {
            foreach (String sessionName in PuttyDataHelper.GetSessionNames())
            {
                comboBoxPuttyProfile.Items.Add(sessionName);
            }
            comboBoxPuttyProfile.SelectedItem = PuttyDataHelper.SessionDefaultSettings;
        }

        private void PopulateProtoList()
        {
            this.protoTypesMap = new System.Collections.Hashtable();
            this.protoTypesMap["SSH"] = ConnectionProtocol.SSH;
            this.protoTypesMap["Telnet"] = ConnectionProtocol.Telnet;
            this.protoTypesMap["Rlogin"] = ConnectionProtocol.Rlogin;
            this.protoTypesMap["Raw"] = ConnectionProtocol.Raw;
            this.protoTypesMap["Serial"] = ConnectionProtocol.Serial;
            this.protoTypesMap["VNC"] = ConnectionProtocol.VNC;
            this.protoTypesMap["RDP"] = ConnectionProtocol.RDP;
            this.protoTypesMap["CygTerm"] = ConnectionProtocol.Cygterm;
            this.protoTypesMap["MinTTY"] = ConnectionProtocol.Mintty;
            this.protoTypesMap["WSL"] = ConnectionProtocol.WSL;
            this.protoTypesMap["Win CMD"] = ConnectionProtocol.WINCMD;
            this.protoTypesMap["PowerShell"] = ConnectionProtocol.PS;


            foreach (System.Collections.DictionaryEntry protoEntry in this.protoTypesMap)
                comboBoxProto.Items.Add(protoEntry.Key);
            comboBoxProto.SelectedItem = "SSH";
            comboBoxProto.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

            int val = 0;
            if (!string.IsNullOrEmpty(CommandLineOptions.getcommand(textBoxExtraArgs.Text, "-pw")))
            {
                if (Messenger.MessageBox("SuperPutty saves the extra arguments Sessions.xml file in plain text.\n" +
                                         "Use of -pw password in 'Extra arguments' is very insecure.\n" +
                                         "For a secure connection use SSH authentication with Pageant.\n" +
                                         "Alternatively, use -pwfile to specify a password file or enable -pw in Tools->Options->Advanced.\n" +
                                         "\nSelect Yes, if you still want save the password.",
                                         "Are you sure that you want to save the password?",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }
            }
            Session.SessionName  = textBoxSessionName.Text.Trim();
            Session.PuttySession = comboBoxPuttyProfile.Text.Trim();
            Session.Host         = textBoxHostname.Text.Trim();
            Session.ExtraArgs    = textBoxExtraArgs.Text.Trim();
            if (!Int32.TryParse(this.textBoxPort.Text, out val))
                Session.Port     = 0;
            else
                Session.Port     = int.Parse(textBoxPort.Text.Trim());
            Session.Username     = textBoxUsername.Text.Trim();
            Session.SessionId    = SessionData.CombineSessionIds(SessionData.GetSessionParentId(Session.SessionId), Session.SessionName);
            Session.ImageKey     = buttonImageSelect.ImageKey;
            Session.SPSLFileName = textBoxSPSLScriptFile.Text.Trim();
            Session.RemotePath = textBoxRemotePathSesion.Text.Trim();
            Session.LocalPath = textBoxLocalPathSesion.Text.Trim();
            Session.Note = textBoxNote.Text.Trim();

            if (this.protoTypesMap.ContainsKey(comboBoxProto.SelectedItem))
                Session.Proto = (ConnectionProtocol)this.protoTypesMap[comboBoxProto.SelectedItem];
            
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Special UI handling for cygterm or mintty sessions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxProto_SelectedIndexChanged(object sender, EventArgs e)
        {
            string host = this.textBoxHostname.Text;
            if (!this.isInitialized)
                return;

            ConnectionProtocol proto = this.protoTypesMap.ContainsKey(comboBoxProto.SelectedItem) ? (ConnectionProtocol)this.protoTypesMap[comboBoxProto.SelectedItem] : ConnectionProtocol.SSH;
            if (proto == ConnectionProtocol.Cygterm || proto == ConnectionProtocol.Mintty || proto == ConnectionProtocol.WINCMD || proto == ConnectionProtocol.PS || proto == ConnectionProtocol.WSL)
            {
                this.textBoxPort.Enabled = false;
                this.textBoxUsername.Enabled = false;
                lblHostname.Text = "Host address:";
                if (String.IsNullOrEmpty(host) || !host.StartsWith(CygtermStartInfo.LocalHost))
                {
                    OldHostname = this.textBoxHostname.Text;
                    this.textBoxHostname.Text = CygtermStartInfo.LocalHost;
                }
            }
            else if (proto == ConnectionProtocol.Serial)
            {
                textBoxPort.Enabled = false;
                textBoxUsername.Enabled = false;
                lblHostname.Text = "Serial port:";
                textBoxExtraArgs.Text = "-sercfg 115200,8,n,1,N";
            }
            else
            {
                this.textBoxPort.Enabled = true;
                this.textBoxUsername.Enabled = true;
                lblHostname.Text = "Host address:";
                if (!string.IsNullOrEmpty(OldHostname))
                {
                    this.textBoxHostname.Text = OldHostname;
                    OldHostname = null;
                }
                if (proto != ConnectionProtocol.Raw)
                    this.textBoxPort.Text = dlgEditSession.GetDefaultPort(proto).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            // Default icons
            switch (proto)
            {
                case ConnectionProtocol.SSH:
                case ConnectionProtocol.Rlogin:
                    buttonImageSelect.ImageKey = "computer";
                    break;
                case ConnectionProtocol.Telnet:
                case ConnectionProtocol.Serial:
                case ConnectionProtocol.Raw:
                    buttonImageSelect.ImageKey = "application_osx_terminal";
                    break;
                case ConnectionProtocol.VNC:
                case ConnectionProtocol.RDP:
                    buttonImageSelect.ImageKey = "map";
                    break;
                case ConnectionProtocol.WINCMD:
                case ConnectionProtocol.PS:
                case ConnectionProtocol.Cygterm:
                case ConnectionProtocol.Mintty:
                    buttonImageSelect.ImageKey = "application_xp_terminal";
                    break;
                case ConnectionProtocol.WSL:
                    buttonImageSelect.ImageKey = "ubuntu";
                    break;
            }
        }

        public static int GetDefaultPort(ConnectionProtocol protocol)
        {
            int port = 22;
            switch (protocol)
            {
                case ConnectionProtocol.Raw:
                    break;
                case ConnectionProtocol.Rlogin:
                    port = 513;
                    break;
                case ConnectionProtocol.Serial:
                    break;
                case ConnectionProtocol.Telnet:
                    port = 23;
                    break;
                case ConnectionProtocol.VNC:
                    port = 5900;
                    break;
                case ConnectionProtocol.RDP:
                    port = 3389;
                    break;
                case ConnectionProtocol.WINCMD:
                case ConnectionProtocol.PS:
                case ConnectionProtocol.WSL:
                    port = 0;
                    break;
            }
            return port;
        }

        #region Icon
        private void buttonImageSelect_Click(object sender, EventArgs e)
        {
            if (this.imgPopup == null)
            {
                // TODO: ImageList is null on initial installation and will throw a nullreference exception when creating a new session and trying to select an image.

                int n = buttonImageSelect.ImageList.Images.Count;
                int x = (int) Math.Floor(Math.Sqrt(n)) + 1;
                int cols = x;
                int rows = x;

                imgPopup = new ImageListPopup();

                if (SuperPuTTY.Settings.InterfaceTheme < (int)InterfaceTheme.LightTheme)
                {
                    imgPopup.BackgroundColor = Color.FromArgb(55, 55, 55);
                    imgPopup.BackgroundOverColor = Color.FromArgb(102, 154, 204);
                }
                else
                {
                    imgPopup.BackgroundColor = Color.FromArgb(241, 241, 241);
                    imgPopup.BackgroundOverColor = Color.FromArgb(102, 154, 204);
                }

                imgPopup.Init(this.buttonImageSelect.ImageList, 8, 8, cols, rows);
                imgPopup.ItemClick += new ImageListPopupEventHandler(this.OnItemClicked);
            }

            Point pt = PointToScreen(new Point(buttonImageSelect.Left, buttonImageSelect.Bottom));
            imgPopup.Show(pt.X + 2, pt.Y);
        }


        private void OnItemClicked(object sender, ImageListPopupEventArgs e)
        {
            if (imgPopup == sender)
            {
                buttonImageSelect.ImageKey = e.SelectedItem;
                this.toolTip.SetToolTip(this.buttonImageSelect, buttonImageSelect.ImageKey);
            }
        } 
        #endregion

        #region Validation Logic

        public SessionNameValidationHandler SessionNameValidator { get; set; }

        private void textBoxSessionName_Validating(object sender, CancelEventArgs e)
        {
            if (this.SessionNameValidator != null)
            {
                string error;
                if (!this.SessionNameValidator(this.textBoxSessionName.Text, out error))
                {
                    e.Cancel = true;
                    this.SetError(this.textBoxSessionName, error ?? "Invalid Session Name");
                }
            }
        }

        private void textBoxSessionName_Validated(object sender, EventArgs e)
        {
            this.SetError(this.textBoxSessionName, String.Empty);
        }

        private void textBoxPort_Validating(object sender, CancelEventArgs e)
        {
            int val;
            ConnectionProtocol proto = this.protoTypesMap.ContainsKey(comboBoxProto.SelectedItem) ? (ConnectionProtocol)this.protoTypesMap[comboBoxProto.SelectedItem] : ConnectionProtocol.SSH;
            if (!Int32.TryParse(this.textBoxPort.Text, out val))
            {
                if (this.textBoxPort.Text == "")
                    if (proto == ConnectionProtocol.Mintty || proto == ConnectionProtocol.Cygterm || proto == ConnectionProtocol.RDP || proto == ConnectionProtocol.WINCMD || proto == ConnectionProtocol.PS || proto == ConnectionProtocol.WSL)
                        return;

                e.Cancel = true;
                this.SetError(this.textBoxPort, "Invalid Port");
            }
        }

        private void textBoxPort_Validated(object sender, EventArgs e)
        {
            this.SetError(this.textBoxPort, String.Empty);
        }

        private void textBoxHostname_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty((string)this.comboBoxPuttyProfile.SelectedItem) &&
                string.IsNullOrEmpty(this.textBoxHostname.Text.Trim()))
            {
                if (sender == this.textBoxHostname)
                {
                    this.SetError(this.textBoxHostname, "A host name must be specified if a Putty Session Profile is not selected");
                }
                else if (sender == this.comboBoxPuttyProfile)
                {
                    this.SetError(this.comboBoxPuttyProfile, "A Putty Session Profile must be selected if a Host Name is not provided");
                }
            }
            else
            {
                this.SetError(this.textBoxHostname, String.Empty);
                this.SetError(this.comboBoxPuttyProfile, String.Empty);
            }
        }

        private void comboBoxPuttyProfile_Validating(object sender, CancelEventArgs e)
        {
            this.textBoxHostname_Validating(sender, e);
        }

        private void comboBoxPuttyProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ValidateChildren(ValidationConstraints.ImmediateChildren);    
        }

        void SetError(Control control, string error)
        {
            this.errorProvider.SetError(control, error);
            this.EnableDisableSaveButton();
        }

        void EnableDisableSaveButton()
        {
            this.buttonSave.Enabled = this.errorProvider.GetError(this.textBoxSessionName) == String.Empty &&
                                      this.errorProvider.GetError(this.textBoxHostname) == String.Empty &&
                                      this.errorProvider.GetError(this.textBoxPort) == String.Empty &&
                                      this.errorProvider.GetError(this.comboBoxPuttyProfile) == String.Empty &&
                                      this.errorProvider.GetError(this.comboBoxProto) == String.Empty;
        }

        #endregion

        private void buttonBrowse_Click(object sender, EventArgs e)
        {

            DialogResult dlgResult = this.openFileDialog1.ShowDialog();
            if (dlgResult == DialogResult.OK)
            {
                textBoxSPSLScriptFile.Text = this.openFileDialog1.FileName;
            }
        }

        private void buttonClearSPSLFile_Click(object sender, EventArgs e)
        {
            Session.SPSLFileName = textBoxSPSLScriptFile.Text = String.Empty;
            
        }

        private void buttonBrowseLocalPath_Click(object sender, EventArgs e)
        {            
            if (Directory.Exists(textBoxLocalPathSesion.Text))
            {
                folderBrowserDialog1.SelectedPath = textBoxLocalPathSesion.Text;
            }
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                if (!String.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
                    textBoxLocalPathSesion.Text = folderBrowserDialog1.SelectedPath;
            }


        }

        private void textBoxExtraArgs_Validating(object sender, CancelEventArgs e)
        {
            if (!string.IsNullOrEmpty(CommandLineOptions.getcommand(textBoxExtraArgs.Text, "-pw")))
            {
                e.Cancel = true;
                this.SetError(this.textBoxExtraArgs,  "Password set in clear text");
            }
        }

        private void textBoxExtraArgs_Validated(object sender, EventArgs e)
        {
            this.SetError(this.textBoxExtraArgs, String.Empty);
        }
    }
}
