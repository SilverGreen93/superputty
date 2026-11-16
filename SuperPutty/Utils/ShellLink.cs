using System;
using System.Diagnostics;
using System.Windows.Forms;
using DarkModeForms;
using log4net;

namespace SuperPutty.Utils
{
    /// <summary>
    /// Opens a link or a document through the shell, so that a web address goes to the default
    /// browser and a file to whatever is registered for its extension.
    ///
    /// ProcessStartInfo.UseShellExecute defaulted to true under .NET Framework, which is what the
    /// plain Process.Start("http://...") calls in this application relied on. It has been false by
    /// default since .NET Core 3, so the address is handed to CreateProcess as if it were an
    /// executable name and the click dies with Win32Exception (2), "The system cannot find the
    /// file specified".
    /// </summary>
    public static class ShellLink
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ShellLink));

        /// <summary>
        /// Opens the specified web address or file in the application associated with it.
        /// </summary>
        /// <param name="target">A web address (http/https) or a path to a file</param>
        public static void Open(string target)
        {
            if (String.IsNullOrEmpty(target))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // a link click should not take the application down with it
                Log.WarnFormat("Could not open {0}: {1}", target, ex.Message);
                Messenger.MessageBox("Could not open " + target + "\n" + ex.Message,
                    "Error opening link", MessageBoxButtons.OK, MsgIcon.Error);
            }
        }
    }
}
