using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using SuperPutty.Data;
using SuperPutty.Properties;
using SuperPutty.Utils;
using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;
using SuperPutty.Scp;
using SuperPuTTY.Scripting;
using System.Net.Http;
using DarkModeForms;

namespace SuperPutty
{
    /// <summary>
    /// Represents the SuperPuTTY application itself
    /// </summary>
    public static class SuperPuTTY 
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SuperPuTTY));

        public static event EventHandler<LayoutChangedEventArgs> LayoutChanging;
        public static event EventHandler<LayoutChangedEventArgs> LayoutChanged;

        public static event Action<string> StatusEvent;

        static BindingList<LayoutData> layouts = new BindingList<LayoutData>();
        static SortedList<string, SessionData> sessionsMap = new SortedList<string, SessionData>();
        static BindingList<SessionData> sessionsList = new BindingList<SessionData>();
        static bool? isFirstRun;

        public static void Initialize(string[] args)
        {
            MigrateSettingsFolder();

            // nothing has to be asked of the user before the first run when the embedded PuTTY
            // is used, so the first run setup is done here instead of by the setup dialog
            bool firstRunSetup = PerformFirstRunSetup();

            Log.InfoFormat($"Initializing.  Version={Version}, UserSettings={Settings.SettingsFilePath}, SettingsFolder={Settings.SettingsFolder}");

            Images = LoadImageList("default", false);
            ImagesWithStop = LoadImageList("default", true);

            if (!IsFirstRun)
            {
                // parse command line args
                CommandLine = new CommandLineOptions(args);

                // display help if --help specified
                if (CommandLine.Help)
                {
                    Messenger.MessageBox(CommandLineOptions.Usage(), "SuperPutty CLI Help", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    Environment.Exit(0);
                }

                // load data                
                LoadLayouts();
                LoadSessions();                

                // determine starting layout, if any.  CLI has priority
                if (CommandLine.IsValid)
                {
                    if (CommandLine.Layout != null)
                    {
                        StartingLayout = FindLayout(CommandLine.Layout);
                        if (StartingLayout != null)
                        {
                            Log.InfoFormat($"Starting with layout from command line, {CommandLine.Layout}");
                        }
                    }
                    else
                    {
                        // ad-hoc session specified
                        SessionDataStartInfo sessionStartInfo = CommandLine.ToSessionStartInfo();
                        if (sessionStartInfo != null)
                        {
                            StartingSession = sessionStartInfo;
                            Log.InfoFormat($"Starting adhoc Session from command line, {StartingSession.Session.SessionId}");
                        }
                    }

                }

                // if nothing specified, then try the default layout
                if (!firstRunSetup && StartingLayout == null && StartingSession == null)
                {
                    StartingLayout = FindLayout(Settings.DefaultLayoutName);
                    if (StartingLayout != null)
                    {
                        Log.InfoFormat($"Starting with default layout, {Settings.DefaultLayoutName}");
                    }
                }
            }

            // Register for single instance support
            SingleInstanceHelper.StartPipeServer(SingleInstanceHelper.Run);
            WindowEvents = new GlobalWindowEvents();

            Log.Info("Initialized");
        }

        /// <summary>
        /// Does the setup that used to be done by the setup dialog when it was shown at startup:
        /// chooses the settings folder and takes over the settings and sessions of older versions.
        /// Only the embedded PuTTY can be started without the dialog, an external PuTTY that
        /// cannot be found still needs it and the dialog then does this itself.
        /// </summary>
        /// <returns>true if this run of the application did the setup</returns>
        private static bool PerformFirstRunSetup()
        {
            if (!PuttyEmbed.IsEnabled || !IsFirstRun)
            {
                return false;
            }

            if (String.IsNullOrEmpty(Settings.SettingsFolder))
            {
                string dir = DefaultSettingsFolder;
                try
                {
                    Log.InfoFormat($"First run, using the default settings folder: {dir}");
                    Directory.CreateDirectory(dir);
                    ImportSuperPuttySettings(dir);
                    Settings.SettingsFolder = dir;
                }
                catch (Exception ex)
                {
                    Log.Error("Error creating the settings folder " + dir, ex);
                    return false;
                }
            }

            Settings.ImportFromRegistry();

            // the auto-restore layout saves the windows of the application when it is closed
            if (String.IsNullOrEmpty(Settings.DefaultLayoutName))
            {
                Settings.DefaultLayoutName = LayoutData.AutoRestore;
            }

            Settings.Save();

            if (Sessions.Count == 0)
            {
                ImportSessionsFromSuperPutty1030();
            }

            // the setup is done, IsFirstRun has to be calculated again: it is now false unless
            // an external PuTTY is used and its executable is still missing
            isFirstRun = null;
            return true;
        }

        /// <summary>Called when application is shutting down, sends message to log.</summary>
        public static void Shutdown()
        {
            Log.Info("Shutting down...");
        }

        /// <summary>Send status message to toolstrip</summary>
        /// <param name="status">A string containing the message</param>
        /// <param name="args">optional arguments <seealso>
        ///         <cref>String.Format</cref>
        ///     </seealso>
        /// </param>
        public static void ReportStatus(string status, params object[] args)
        {
            string msg = args.Length > 0 ? string.Format(status, args) : status;
            Log.DebugFormat($"STATUS: {msg}");

            if (StatusEvent != null)
            {
                StatusEvent(msg);
            }
        }

        #region Layouts

        public static bool IsLayoutChanging { get; private set; }

        public static void AddLayout(string file)
        {
            LayoutData layout = new LayoutData(file);
            if (FindLayout(layout.Name) == null)
            {
                layouts.Add(layout);
                LoadLayout(layout, true);
            }
        }

        public static void RemoveLayout(string name, bool deleteFile)
        {
            LayoutData layout = FindLayout(name);
            if (layout != null)
            {
                layouts.Remove(layout);
                if (deleteFile)
                {
                    File.Delete(layout.FilePath);
                }
            }
        }

        public static void RenameLayout(LayoutData layout, string newName)
        {
            if (layout != null)
            {
                LayoutData existing = FindLayout(newName);
                if (existing == null)
                {
                    Log.InfoFormat($"Renaming layout: {layout.Name} -> {newName}");
                    // rename layout and file
                    string fileOld = layout.FilePath;
                    string fileNew = Path.Combine(Path.GetDirectoryName(layout.FilePath), newName) + ".xml";
                    File.Move(fileOld, fileNew);
                    layout.Name = newName;
                    layout.FilePath = fileNew;

                    // Notify
                    layouts.ResetItem(layouts.IndexOf(layout));
                }
                else
                {
                    throw new ArgumentException($"Layout with the same name exists: {newName}");
                }
            }
        }

        public static LayoutData FindLayout(string name)
        {
            return layouts.FirstOrDefault(layout => name == layout.Name);
        }

        /// <summary>Name of the folder holding the session and layout files</summary>
        private const string SettingsFolderName = "SuperXPuTTY";

        /// <summary>Default location of the session and layout files</summary>
        public static string DefaultSettingsFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SettingsFolderName); } }

        /// <summary>Location of the session and layout files used by older versions</summary>
        private static string LegacySettingsFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SettingsFolderName); } }

        /// <summary>Name of the folder holding the files of the even older SuperPutty releases</summary>
        private const string LegacySuperPuttyFolderName = "SuperPutty";

        /// <summary>Location of the session and layout files used by the even older SuperPutty releases</summary>
        private static string LegacySuperPuttySettingsFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LegacySuperPuttyFolderName); } }

        /// <summary>
        /// Older versions kept the session and layout files in the documents folder.  They are copied
        /// to the application data folder only when the settings file itself is moved there, so a
        /// folder chosen by the user afterwards, even the old one, is left alone.
        /// </summary>
        private static void MigrateSettingsFolder()
        {
            // reading a setting first makes sure the settings provider has looked for the old file
            string currentDir = Settings.SettingsFolder;
            string legacyDir = LegacySettingsFolder;
            if (!PortableSettingsProvider.LegacySettingsFileMigrated ||
                String.IsNullOrEmpty(currentDir) ||
                !String.Equals(currentDir, legacyDir, StringComparison.OrdinalIgnoreCase))
            {
                // nothing was moved by this run, or the folder was already chosen by the user
                return;
            }

            string newDir = DefaultSettingsFolder;
            int copiedFiles;
            try
            {
                Log.InfoFormat($"Migrating settings folder from {legacyDir} to {newDir}");
                Directory.CreateDirectory(newDir);
                copiedFiles = CopySettingsFolder(legacyDir, newDir);

                Settings.SettingsFolder = newDir;
                Settings.Save();
            }
            catch (Exception ex)
            {
                Log.Error("Error migrating settings folder from " + legacyDir, ex);
                return;
            }

            if (copiedFiles > 0)
            {
                Messenger.MessageBox(
                    $"Your settings have been moved from\r\n{legacyDir}\r\nto\r\n{newDir}",
                    "Settings Moved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Copy the contents of a settings folder, the app settings file is handled by the settings provider
        /// </summary>
        /// <returns>the number of files that were copied</returns>
        private static int CopySettingsFolder(string sourceDir, string targetDir)
        {
            int copiedFiles = 0;

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                if (file.EndsWith(".settings", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (CopySettingsFile(file, Path.Combine(targetDir, Path.GetFileName(file))))
                {
                    copiedFiles++;
                }
            }

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string subDir = Path.Combine(targetDir, Path.GetFileName(dir));
                Directory.CreateDirectory(subDir);
                copiedFiles += CopySettingsFolder(dir, subDir);
            }

            return copiedFiles;
        }

        private static bool CopySettingsFile(string sourceFile, string targetFile)
        {
            if (File.Exists(sourceFile) && !File.Exists(targetFile))
            {
                File.Copy(sourceFile, targetFile);
                Log.InfoFormat($"Copied {sourceFile} to {targetFile}");
                return true;
            }
            return false;
        }

        /// <summary>Checks whether a folder holds the files of an installation of this application</summary>
        private static bool IsSettingsFolder(string dir)
        {
            return Directory.Exists(dir) &&
                   (File.Exists(Path.Combine(dir, "Sessions.xml")) || Directory.Exists(Path.Combine(dir, "layouts")));
        }

        /// <summary>
        /// The even older SuperPutty releases kept their sessions and layouts in a SuperPutty folder in
        /// the documents folder.  When nothing has been set up yet, the user is asked whether those files
        /// should be copied into the folder that is used from now on.  They are read only after the copy,
        /// so the application does not have to be restarted.
        /// </summary>
        /// <param name="targetDir">folder receiving the copied files</param>
        private static void ImportSuperPuttySettings(string targetDir)
        {
            string sourceDir = LegacySuperPuttySettingsFolder;
            if (String.Equals(sourceDir, targetDir, StringComparison.OrdinalIgnoreCase) ||
                !IsSettingsFolder(sourceDir) || IsSettingsFolder(targetDir))
            {
                return;
            }

            if (Messenger.MessageBox(
                    $"An older SuperPutty installation has left its settings in\r\n{sourceDir}\r\n\r\nWould you like to import them?",
                    "Import SuperPutty Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                Log.InfoFormat($"Not importing the settings from {sourceDir}");
                return;
            }

            try
            {
                Log.InfoFormat($"Importing settings folder from {sourceDir} to {targetDir}");
                int copiedFiles = CopySettingsFolder(sourceDir, targetDir);
                Log.InfoFormat($"Imported {copiedFiles} files from {sourceDir}");
            }
            catch (Exception ex)
            {
                Log.Error("Error importing settings folder from " + sourceDir, ex);
                Messenger.MessageBox(
                    $"The settings from\r\n{sourceDir}\r\ncould not be imported.",
                    "Import SuperPutty Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void LoadLayouts()
        {
            if (!String.IsNullOrEmpty(Settings.SettingsFolder))
            {
                LayoutData autoRestore = new LayoutData(AutoRestoreLayoutPath) { Name = LayoutData.AutoRestore, IsReadOnly = true };
                if (Directory.Exists(LayoutsDir))
                {
                    List<LayoutData> newLayouts = Directory.GetFiles(LayoutsDir).Select(file => new LayoutData(file)).ToList();

                    layouts.Clear();
                    layouts.Add(autoRestore);
                    foreach (LayoutData layout in newLayouts)
                    {
                        layouts.Add(layout);
                    }
                    Log.InfoFormat($"Loaded {newLayouts.Count} layouts");
                }
                else
                {
                    Log.InfoFormat($"Creating layouts directory: {LayoutsDir}");
                    Directory.CreateDirectory(LayoutsDir);
                    layouts.Add(autoRestore);
                }
            }            
        }

        public static void LoadLayout(LayoutData layout)
        {
            LoadLayout(layout, false);
        }

        public static void LoadLayout(LayoutData layout, bool isNewLayoutAlreadyActive)
        {
            Log.InfoFormat("LoadLayout: layout={0}, isNewLayoutAlreadyActive={1}", layout == null ? "NULL" : layout.Name, isNewLayoutAlreadyActive);
            LayoutChangedEventArgs args = new LayoutChangedEventArgs
            {
                New = layout,
                Old = CurrentLayout,
                IsNewLayoutAlreadyActive = isNewLayoutAlreadyActive
            };

            try
            {
                IsLayoutChanging = true;

                if (LayoutChanging != null)
                {
                    LayoutChanging(typeof(SuperPuTTY), args);
                }

            }
            finally
            {
                IsLayoutChanging = false;
            }


            if (LayoutChanged != null)
            {
                CurrentLayout = layout;
                LayoutChanged(typeof(SuperPuTTY), args);
            }
        }

        public static void LoadLayoutInNewInstance(LayoutData layout)
        {
            ReportStatus($"Starting new instance with layout, {layout.Name}");
            Process.Start(Assembly.GetExecutingAssembly().Location, "-layout \"" + layout.Name + "\"");
        }

        public static void LoadSessionInNewInstance(string sessionId)
        {
            ReportStatus($"Starting session in new instance, {sessionId}");
            Process.Start(Assembly.GetExecutingAssembly().Location, "-session \"" + sessionId + "\"");
        }

        public static void SetLayoutAsDefault(string layoutName)
        {
            if (!string.IsNullOrEmpty(layoutName))
            {
                LayoutData layout = FindLayout(layoutName);
                if (layout != null)
                {
                    ReportStatus($"Setting {layoutName} as default layout");
                    SuperPuTTY.Settings.DefaultLayoutName = layoutName;
                    SuperPuTTY.Settings.Save();

                    // so gui change is propagated via events
                    LoadLayouts();
                }
            }
        }

        #endregion

        #region Sessions

        /// <summary>Returns A string containing the path to the saved sessions database on disk</summary>
        private static string SessionsFileName { get { return Path.Combine(Settings.SettingsFolder, "Sessions.xml"); } }

        /// <summary>Load sessions database from file into the application</summary>
        public static void LoadSessions()
        {
            string fileName = SessionsFileName;
            Log.InfoFormat($"Loading all sessions.  file={fileName}");

            try
            {
                if (File.Exists(fileName))
                {
                    List<SessionData> sessions = SessionData.LoadSessionsFromFile(fileName);
                    // remove old
                    sessionsMap.Clear();
                    sessionsList.Clear();

                    foreach (SessionData session in sessions)
                    {
                        AddSession(session);
                    }
                }
                else
                {
                    Log.WarnFormat($"Sessions file does not exist, nothing loaded.  file={fileName}");
                }

            }
            catch (Exception ex)
            {
                Log.Error($"Error while loading sessions from {fileName}", ex);
            }
        }

        /// <summary>Save in-application Session Database to XML File</summary>
        public static void SaveSessions()
        {
            Log.InfoFormat("Saving all sessions");
            SessionData.SaveSessionsToFile(GetAllSessions(), SessionsFileName);
        }

        /// <summary>
        /// Remove a session from the in-application sessions database. 
        /// </summary>
        /// <param name="sessionId">The <seealso cref="SessionData.SessionId"/> of the session to remove</param>
        /// <returns>true on success, or false on failure or if session did not exist</returns>
        public static bool RemoveSession(string sessionId)
        {
            SessionData session = GetSessionById(sessionId);
            if (session != null)
            {
                sessionsMap.Remove(sessionId);
                sessionsList.Remove(session);
                Log.InfoFormat($"Removed Session, id={session}, success=true");
                return true;
            }            
            return false;
        }

        /// <summary>Get a Session by its <seealso cref="SessionData.SessionId"/></summary>
        /// <param name="sessionId">A string which represents a session</param>
        /// <returns>A <seealso cref="SessionData"/> object containing the session details</returns>
        public static SessionData GetSessionById(string sessionId)
        {
            SessionData session = null;
            if (sessionId != null)
            {
                if (!sessionsMap.TryGetValue(sessionId, out session))
                {
                    // no hit by id...so try the list
                    // @TODO: Revisit...this is a work around the sessionId changing in tree
                    foreach (SessionData sd in sessionsList)
                    {
                        if (sd.SessionId == sessionId)
                        {
                            session = sd;
                            // reindex list
                            sessionsMap.Clear();
                            foreach (SessionData s in sessionsList)
                            {
                                sessionsMap[s.SessionId] = s;
                            }
                            break;
                        }
                    }
                }
            }
            return session;
        }

        /// <summary>Add a new session to the in-application session database</summary>
        /// <param name="session">A <seealso cref="SessionData"/> object containing the configuration of a session</param>
        /// <returns>true on success, false on failure</returns>
        public static bool AddSession(SessionData session)
        {
            bool success = false;
            if (GetSessionById(session.SessionId) == null)
            {
                Log.InfoFormat($"Added Session, id={session.SessionId}");
                sessionsMap.Add(session.SessionId, session);
                sessionsList.Add(session);
                success = true;
            }
            else
            {
                Log.InfoFormat($"Failed to Add Session, id={session.SessionId}.  Session already exists");
            }
            return success;
        }

        /// <summary>Get a list of all sessions from the in-application database</summary>
        /// <returns>A List of <seealso cref="SessionData"/> objects</returns>
        public static List<SessionData> GetAllSessions()
        {
            return sessionsMap.Values.ToList();
        }

        /// <summary>Retrieve a <seealso cref="SessionData"/> object and open a new putty window</summary>
        /// <param name="sessionId">A string containing the <seealso cref="SessionData.SessionId"/> of the session</param>
        public static void OpenProtoSession(string sessionId)
        {
            OpenProtoSession(GetSessionById(sessionId));
        }

        /// <summary>Open a new putty window with its settings being passed in a <seealso cref="SessionData"/> object</summary>
        /// <param name="session">The <seealso cref="SessionData"/> object containing the settings</param>
        public static ctlPuttyPanel OpenProtoSession(SessionData session)
        {
            Log.InfoFormat("Opening putty session, id={0}", session == null ? "" : session.SessionId);
            ctlPuttyPanel panel = null;
            if (session != null)
            {
                string Executable = PuttyStartInfo.GetExecutable(session);
                if (string.IsNullOrEmpty(Executable))
                {
                    Messenger.MessageBox("Error trying to create session: " + session.ToString() +
                        "\nExecutable not set for " + session.Proto.ToString() + " protocol." +
                        "\nGo to tools->options->General tab to set the path to the executable."
                        , "Failed to create a session", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
                if (!File.Exists(Executable))
                {
                    Messenger.MessageBox("Error trying to create session: " + session.ToString() +
                        "\nExecutable not found for " + session.Proto.ToString() + " protocol." +
                        "\nThe path for the executable was set as \"" + Executable + "\"." +
                        "\nGo to tools->options->General tab to set the path to the executable."
                        , "Failed to create a session", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                // This is the callback fired when the panel containing the terminal is closed
                // We use this to save the last docking location and to close the panel
                PuttyClosedCallback callback = delegate
                {
                    if (panel != null)
                    {
                        // save the last dockstate (if it has been changed)
                        if (session.LastDockstate != panel.DockState
                            && panel.DockState != DockState.Unknown
                            && panel.DockState != DockState.Hidden)
                        {
                            session.LastDockstate = panel.DockState;
                            SaveSessions();
                        }

                        if (panel.InvokeRequired)
                        {
                            panel.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate {
                                panel.Close();
                            });
                        }
                        else
                        {
                            panel.Close();
                        }
                    }
                };

                try {
                    panel = new ctlPuttyPanel(session, callback);

                    ApplyDockRestrictions(panel);
                    ApplyIconForWindow(panel, session);
                    panel.Show(MainForm.DockPanel, session.LastDockstate);
                    ReportStatus($"Opened session: {session.SessionId} [{session.Proto}]");

                    if (!string.IsNullOrWhiteSpace(session.SPSLFileName))
                    {
                        string fileName = session.SPSLFileName;
                        string script = string.Empty;

                        if(Regex.IsMatch(fileName, @"^https?:\/\/", RegexOptions.IgnoreCase))
                        {
                            try
                            {
                                using (HttpClient client = new HttpClient())
                                {
                                    HttpResponseMessage response = client.GetAsync(fileName).GetAwaiter().GetResult();
                                    response.EnsureSuccessStatusCode();
                                    using (var stream = new StreamReader(response.Content.ReadAsStream()))
                                    {
                                        script = stream.ReadToEnd();
                                    }
                                }
                            }
                            catch(Exception)
                            {
                                script = String.Empty;
                            }
                        }
                        else
                        {
                            if (fileName.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                            {
                                fileName = fileName.Substring("file://".Length);
                            }

                            if (File.Exists(fileName))
                            {
                                script = File.ReadAllText(fileName);
                            }
                        }

                        if (!String.IsNullOrEmpty(script))
                        {
                            ExecuteScriptEventArgs scriptArgs = new ExecuteScriptEventArgs() { Script = script, Handle = panel.AppPanel.AppWindowHandle };
                            SPSL.BeginExecuteScript(scriptArgs);
                        }
                    }
                } catch (InvalidOperationException ex)
                {
                    Messenger.MessageBox("Error trying to create session " + ex.Message, "Failed to create session panel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return panel;
        }

        /// <summary>Retrieve a <seealso cref="SessionData"/> object and open a new putty scp window</summary>
        /// <param name="sessionId">A string containing the <seealso cref="SessionData.SessionId"/> of the session</param>
        public static void OpenScpSession(string sessionId)
        {
            OpenScpSession(GetSessionById(sessionId));
        }

        /// <summary>Open a new putty scp window with its settings being passed in a <seealso cref="SessionData"/> object</summary>
        /// <param name="session">The <seealso cref="SessionData"/> object containing the settings</param>
        public static void OpenScpSession(SessionData session)
        {
            Log.InfoFormat("Opening scp session, id={0}", session == null ? "" : session.SessionId);
            if (!IsScpEnabled)
            {
                ReportStatus($"Could not open session, pscp not found: {session.SessionId} [SCP]");
            }
            else if (session != null)
            {
                var homePrefix = session.Username.ToLower().Equals("root") ? Settings.PscpRootHomePrefix : Settings.PscpHomePrefix;
                PscpBrowserPanel panel = new PscpBrowserPanel(
                    session, new PscpOptions { PscpLocation = Settings.PscpExe, PscpHomePrefix = homePrefix });
                ApplyDockRestrictions(panel);
                ApplyIconForWindow(panel, session);
                panel.Show(MainForm.DockPanel, session.LastDockstate);

                ReportStatus($"Opened session: {session.SessionId} [SCP]");
            }
            else
            {
                Log.Warn("Could not open null session");
            }
        }

        /// <summary>Apply docking restrictions to a <seealso cref="ToolWindowDocument"/> window such as preventing a window opening from outside the tabbed interface</summary>
        /// <param name="panel">The <seealso cref="DockPanel"/> to apply the restrictions to</param>
        public static void ApplyDockRestrictions(DockPanel panel)
        {
            foreach (DockContent doc in panel.Documents)
            {
                if (doc is ToolWindowDocument)
                {
                    ApplyDockRestrictions(doc);
                }
            }
        }

        /// <summary>Apply docking restrictions to a panel, such as restricting a panel from floating</summary>
        /// <param name="panel">The <seealso cref="DockContent"/> panel to apply the restrictions to</param>
        public static void ApplyDockRestrictions(DockContent panel)
        {
            if (Settings.RestrictContentToDocumentTabs)
            {
                panel.DockAreas = DockAreas.Document | DockAreas.Float;
            }

            if (Settings.DockingRestrictFloatingWindows)
            {
                panel.DockAreas = panel.DockAreas ^ DockAreas.Float;
            }
        }

        public static void OpenSession(SessionDataStartInfo ssi)
        {
            if (MainForm.InvokeRequired)
            {
                MainForm.BeginInvoke(new Action<SessionDataStartInfo>(OpenSession), ssi);
                return;
            }

            if (ssi != null)
            {
                if (ssi.UseScp)
                {
                    OpenScpSession(ssi.Session);
                }
                else
                {
                    OpenProtoSession(ssi.Session);
                }
            }
        }

        /// <summary>Import sessions from the specified file into the in-application database</summary>
        /// <param name="fileName">A string containing the path of the filename that holds session configuration</param>
        public static void ImportSessionsFromFile(string fileName)
        {
            if (fileName == null) { return; }
            if (File.Exists(fileName))
            {
                Log.InfoFormat($"Importing sessions from file, path={fileName}");
                List<SessionData> sessions = SessionData.LoadSessionsFromFile(fileName);
                ImportSessions(sessions, "Imported");
            }
        }

        /// <summary>Import sessions from the specified folder into the in-application database</summary>
        /// <param name="folderName">A string containing the path of the folder that holds session configuration files</param>
        public static void ImportSessionsFromFolder(string folderName)
        {
            if (folderName == null) { return; }
            if (Directory.Exists(folderName))
            {
                Log.InfoFormat($"Importing sessions from folder, path={folderName}");
                List<SessionData> sessions = SessionData.LoadSessionsFromFolder(folderName);
                ImportSessions(sessions, "ImportedFromPortablePuTTY");
            }
        }

        /// <summary>Import sessions from Windows Registry which were set by PuTTY or KiTTY and load them into the in-application sessions database</summary>
        public static void ImportSessionsFromPuTTY()
        {
            Log.InfoFormat("Importing sessions from PuTTY/KiTTY");
            List<SessionData> sessions = PuttyDataHelper.GetAllSessionsFromPuTTY();
            ImportSessions(sessions, "ImportedFromPuTTY");
        }

        /// <summary>Import sessions from file exported by PuttYCM and load them into the in-application sessions database</summary>
        public static void ImportSessionsFromPuttyCM(string fileExport)
        {
            Log.InfoFormat("Importing sessions from PuttyCM");
            List<SessionData> sessions = PuttyDataHelper.GetAllSessionsFromPuTTYCM(fileExport);
            ImportSessions(sessions, "ImportedFromPuTTYCM");
        }

        /// <summary>Import sessions from MobaXterm sessions file and load them into the in-application sessions database</summary>
        public static void ImportSessionsFromMoba(string fileExport)
        {
            Log.InfoFormat("Importing sessions from MobaXterm");
            List<SessionData> sessions = MobaDataHelper.GetAllSessionsFromMoba(fileExport);
            ImportSessions(sessions, "ImportedFromMobaXterm");
        }

        /// <summary>Import sessions from Windows Registry which were set by PuTTY or KiTTY and load them into the in-application sessions database</summary>
        public static void ImportRDPSessionsFromWinReg()
        {
            Log.InfoFormat("Importing RDP sessions from Windows registry");
            List<SessionData> sessions = RDPDataHelper.GetAllSessionsFromRegistry();
            ImportSessions(sessions, "ImportRDPSessionsFromWinReg");
        }

        /// <summary>Import sessions from a from a List object into the specified folder</summary>
        /// <param name="sessions">A List of <seealso cref="SessionData"/> objects</param>
        /// <param name="folder">The destination folder name</param>
        public static void ImportSessions(List<SessionData> sessions, string folder)
        {
            foreach (SessionData session in sessions)
            {
                // pre-pend session id with the provided folder to put them
                session.SessionId = MakeUniqueSessionId(SessionData.CombineSessionIds(folder, session.SessionId));
                session.SessionName = SessionData.GetSessionNameFromId(session.SessionId);
                AddSession(session);
            }
            Log.InfoFormat($"Imported {sessions.Count} sessions into {folder}");

            SaveSessions();
        }

        /// <summary>Import sessions from older version of SuperPuTTY from the Windows Registry</summary>
        public static void ImportSessionsFromSuperPutty1030()
        {
            try
            {
                List<SessionData> sessions = SessionData.LoadSessionsFromRegistry();
                if (sessions != null && sessions.Count > 0)
                {
                    foreach (SessionData session in sessions)
                    {
                        AddSession(session);
                    }
                    SaveSessions();

                    Log.InfoFormat($"Imported {sessions.Count} old sessions from registry.");
                }
            }
            catch (Exception ex)
            {
                Log.WarnFormat($"Could not import old sessions, msg={ex.Message}");
            }
        }

        /// <summary>Generate a unique session ID to prevent collisions in the in-application data store, used when importing and merging sessions from
        /// another application or an older versin of SuperPuTTY</summary>
        /// <param name="sessionId">A string containing the sessionID of the session being imported</param>
        /// <returns>A string containing a unique sessionID</returns>
        public static string MakeUniqueSessionId(string sessionId)
        {
            string newSessionId = sessionId;

            for (int i = 1; i < 1000; i++)
            {
                SessionData sessionExisting = GetSessionById(newSessionId);
                if (sessionExisting == null)
                {
                    break;
                }                
                newSessionId = string.Format("{0}-{1}", sessionId, i);
            }

            return newSessionId;
        }

        #endregion

        #region Icons

        /// <summary>Load Images from themes folder</summary>
        /// <param name="theme">the name of the theme folder</param>
        /// <param name="isEnableStopImage">enable special stop image</param>
        public static ImageList LoadImageList(string theme, bool isEnableStopImage)
        {
            ImageList imgIcons = new ImageList();
            imgIcons.ColorDepth = ColorDepth.Depth32Bit;
            
            if (isEnableStopImage)
                imgIcons.Images.Add("stop", Resources.stop);
            // Load the 2 standard icons in case no icons exist in icons directory, these will be used.
            imgIcons.Images.Add(SessionTreeview.ImageKeyFolder, Resources.folder);
            imgIcons.Images.Add(SessionTreeview.ImageKeySession, Resources.computer);

            try
            {
                string themeFolder = Directory.GetCurrentDirectory();
                themeFolder = Path.Combine(themeFolder, "themes");
                themeFolder = Path.Combine(themeFolder, theme);
                themeFolder = Path.Combine(themeFolder, "icons");

                if (Directory.Exists(themeFolder))
                {
                    foreach (FileInfo fi in new DirectoryInfo(themeFolder).GetFiles())
                    {
                        if (Regex.IsMatch(fi.Extension, @"\.(bmp|jpg|jpeg|png)", RegexOptions.IgnoreCase))
                        {
                            Image img = Image.FromFile(fi.FullName);
                            imgIcons.Images.Add(Path.GetFileNameWithoutExtension(fi.Name), img);
                        }
                    }
                    Log.InfoFormat($"Loaded {imgIcons.Images.Count} icons from theme directory.  dir={themeFolder}");
                }
                else
                {
                    Log.WarnFormat($"theme directory not found, no images loaded. dir={theme}");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while loading icons.", ex);
            }


            return imgIcons;
        }

        /// <summary>Get the Icon defined for the specified session</summary>
        /// <param name="session">The session configuration data</param>
        /// <returns>The Icon configured for the session</returns>
        public static Icon GetIconForSession(SessionData session)
        {
            Icon icon = null;
            if (session != null)
            {
                string imageKey = session.ImageKey == null || !Images.Images.ContainsKey(session.ImageKey)
                    ? SessionTreeview.ImageKeySession : session.ImageKey;
                try
                {
                    Image img = Images.Images[imageKey];
                    Bitmap bmp = img as Bitmap;
                    if (bmp != null)
                    {
                        icon = Icon.FromHandle(bmp.GetHicon());
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error getting icon for image", ex);
                }
            }
            return icon;
        }

        private static void ApplyIconForWindow(ToolWindow win, SessionData session)
        {
            win.Icon = GetIconForSession(session);
        }

        #endregion

        #region Properties
        /// <summary>true if the application has not been set up yet</summary>
        public static bool IsFirstRun {
            get
            {
                if (isFirstRun == null)
                {
                    // With the embedded PuTTY no putty.exe is needed; the settings folder, set by
                    // PerformFirstRunSetup or by the setup dialog, marks setup as done.
                    isFirstRun = PuttyEmbed.IsEnabled
                        ? string.IsNullOrEmpty(Settings.SettingsFolder)
                        : string.IsNullOrEmpty(Settings.PuttyExe);
                }
                return isFirstRun.Value;
            }
        }

        /// <summary>true if the application has not defined where the putty scp program is located</summary>
        public static bool IsScpEnabled { get { return File.Exists(SuperPuTTY.Settings.PscpExe); } }

        /// <summary>true if the application has defined where the filezilla program is located</summary>
        public static bool IsFilezillaEnabled { get { return File.Exists(SuperPuTTY.Settings.FileZillaExe); } }

        /// <summary>true if the application has defined where the winSCP program is located</summary>
        public static bool IsWinSCPEnabled { get { return File.Exists(SuperPuTTY.Settings.WinSCPExe); } }

        /// <summary>true if the application has defined where the Xorg program is located</summary>
        public static bool IsXorgEnabled { get { return File.Exists(SuperPuTTY.Settings.XorgExe); } }

        /// <summary>Returns a string containing the current version of SuperPuTTY</summary>
        public static string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        /// <summary>
        /// Returns the current version formatted for display. The trailing zero components are
        /// dropped so that 3.0.0.0 is shown as 3.0, while a real patch level such as 3.0.1.0 is
        /// still shown as 3.0.1. Major and minor are always shown.
        /// </summary>
        public static string DisplayVersion { get { return FormatVersion(Assembly.GetExecutingAssembly().GetName().Version); } }

        /// <summary>Formats a version without its trailing zero components, e.g. 3.0.0.0 is shown as 3.0</summary>
        /// <param name="version">The version to format</param>
        public static string FormatVersion(Version version)
        {
            // undefined components (-1) are as good as zero here
            int[] parts = { Math.Max(version.Major, 0), Math.Max(version.Minor, 0),
                            Math.Max(version.Build, 0), Math.Max(version.Revision, 0) };
            int shown = parts.Length;
            while (shown > 2 && parts[shown - 1] == 0)
            {
                shown--;
            }
            return string.Join(".", parts.Take(shown).Select(part => part.ToString()));
        }

        internal static Settings Settings { get { return Settings.Default; } }
        public static frmSuperPutty MainForm { get; set; }        
        public static string LayoutsDir { get { return Path.Combine(Settings.SettingsFolder, "layouts"); } }
        public static LayoutData CurrentLayout { get; private set; }
        public static LayoutData StartingLayout { get; private set; }
        public static SessionDataStartInfo StartingSession { get; private set; }
        public static BindingList<LayoutData> Layouts { get { return layouts; } }
        public static BindingList<SessionData> Sessions { get { return sessionsList; } }
        public static CommandLineOptions CommandLine { get; private set; }
        public static ImageList Images { get; private set; }
        public static ImageList ImagesWithStop { get; private set; }
        public static GlobalWindowEvents WindowEvents { get; private set; }

        /// <summary>true of KiTTY is being used instead of putty</summary>
        public static bool IsKiTTY
        {
            get
            {
                bool isKitty = false;
                if (File.Exists(Settings.PuttyExe))
                {
                    string exe = Path.GetFileName(Settings.PuttyExe);
                    isKitty = exe != null && exe.ToLower().StartsWith("kitty");
                }
                return isKitty;
            }
        }

        /// <summary>true of PuTTY Plus is being used instead of putty</summary>
        public static bool IsPuTTYPlus
        {
            get
            {
                bool isPuttyPlus = false;
                if (File.Exists(Settings.PuttyExe))
                {
                    string exe = Path.GetFileName(Settings.PuttyExe);
                    isPuttyPlus = exe != null && exe.ToLower().StartsWith("putty-plus");
                }
                return isPuttyPlus;
            }
        }

        public static string PuTTYAppName
        {
            get
            {
                if (IsPuTTYPlus)
                    return "PuTTY Plus";

                if (IsKiTTY)
                    return "KiTTY";

                return "PuTTY";
            }
        }

        /// <summary>The path to the default AutoRestore layout configuration</summary>
        public static string AutoRestoreLayoutPath { get { return Path.Combine(Settings.SettingsFolder, LayoutData.AutoRestoreLayoutFileName); } }

        #endregion
    }

    #region SuperPuttyAction
    public enum SuperPuttyAction
    {
        CloseTab,
        NextTab,
        PrevTab,
        Options,
        FullScreen,
        OpenSession,
        SwitchSession,
        DuplicateSession,
        GotoCommandBar,
        GotoConnectionBar,
        FocusActiveSession,
        /// <summary>Open Script Editor Window</summary>
        OpenScriptEditor,
        /// <summary>Rename active tab</summary>
        RenameTab,
        RestartSession,
        ClearScrollback,
        ResetTerminal,
        CopyAllToClipboard
    }
    #endregion

    public enum InterfaceTheme
    {
        DarkTheme = 0,
        DarkBlueTheme,
        LightTheme
    }

}
