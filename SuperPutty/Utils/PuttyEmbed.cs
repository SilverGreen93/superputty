using System;
using System.IO;
using System.Runtime.InteropServices;
using SuperPutty.Data;

namespace SuperPutty.Utils
{
    /// <summary>
    /// PuTTY built into the process (putty-embed.dll, from the putty directory).
    /// Each terminal is a child window of an ApplicationPanel on the UI thread,
    /// so SuperXPuTTY stays a single window for Alt+Tab, focus and keyboard.
    /// All calls must be made on the UI thread.
    /// </summary>
    public static class PuttyEmbed
    {
        public const string DllName = "putty-embed.dll";

        public static string DllPath { get; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DllName);

        /// <summary>true if the embedded PuTTY is shipped next to the exe</summary>
        public static bool IsAvailable { get; } = File.Exists(DllPath);

        /// <summary>Protocols the embedded PuTTY runs; the rest stay external programs</summary>
        public static bool Supports(ConnectionProtocol proto)
        {
            switch (proto)
            {
                case ConnectionProtocol.SSH:
                case ConnectionProtocol.Telnet:
                case ConnectionProtocol.Rlogin:
                case ConnectionProtocol.Raw:
                case ConnectionProtocol.Serial:
                    return IsAvailable;
                default:
                    return false;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public delegate void TitleCallback(IntPtr ctx, [MarshalAs(UnmanagedType.LPWStr)] string title);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ClosedCallback(IntPtr ctx, int exitCode);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern IntPtr putty_session_create(IntPtr parent, string args,
            TitleCallback onTitle, ClosedCallback onClosed, IntPtr ctx);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr putty_last_error();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int putty_show_config();

        /// <summary>
        /// Start a session from PuTTY command line arguments in a new child window of
        /// <paramref name="parent"/>. The callbacks must stay referenced while the window lives.
        /// </summary>
        /// <returns>the terminal window handle</returns>
        /// <exception cref="InvalidOperationException">PuTTY rejected the command line</exception>
        public static IntPtr CreateSession(IntPtr parent, string args, TitleCallback onTitle, ClosedCallback onClosed)
        {
            IntPtr hwnd = putty_session_create(parent, args, onTitle, onClosed, IntPtr.Zero);
            if (hwnd == IntPtr.Zero)
            {
                throw new InvalidOperationException(Marshal.PtrToStringUni(putty_last_error()));
            }
            return hwnd;
        }

        /// <summary>PuTTY's configuration dialog, to edit saved PuTTY sessions</summary>
        public static void ShowConfig()
        {
            putty_show_config();
        }
    }
}
