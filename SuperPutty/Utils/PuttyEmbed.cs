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

        /// <summary>
        /// true if the embedded PuTTY is shipped next to the exe and can be used: right
        /// bitness, intact image, and the exports we call. A file that is only there is
        /// not enough, see Probe.
        /// </summary>
        public static bool IsAvailable { get; } = Probe();

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

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string lpLibFileName, IntPtr hResFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // maps the file and reads its exports, but does not resolve its imports and does not run
        // its DllMain, so checking the dll costs nothing and changes nothing
        private const uint DONT_RESOLVE_DLL_REFERENCES = 0x1;

        /// <summary>
        /// Checks the dll the way the runtime is about to use it. A file that is only present is
        /// not enough: it can be the wrong bitness, corrupt, or from a build without the exports
        /// we call, and then calling it throws DllNotFoundException or
        /// EntryPointNotFoundException instead of reporting anything.
        /// </summary>
        private static bool Probe()
        {
            IntPtr lib = LoadLibraryEx(DllPath, IntPtr.Zero, DONT_RESOLVE_DLL_REFERENCES);
            if (lib == IntPtr.Zero)
                return false;

            try
            {
                return GetProcAddress(lib, "putty_session_create") != IntPtr.Zero &&
                       GetProcAddress(lib, "putty_last_error") != IntPtr.Zero &&
                       GetProcAddress(lib, "putty_show_config") != IntPtr.Zero;
            }
            finally
            {
                FreeLibrary(lib);
            }
        }

        /// <summary>
        /// Start a session from PuTTY command line arguments in a new child window of
        /// <paramref name="parent"/>. The callbacks must stay referenced while the window lives.
        /// </summary>
        /// <returns>the terminal window handle</returns>
        /// <exception cref="InvalidOperationException">the dll is not usable, or PuTTY rejected the command line</exception>
        public static IntPtr CreateSession(IntPtr parent, string args, TitleCallback onTitle, ClosedCallback onClosed)
        {
            if (!IsAvailable)
            {
                // the protocols are external again rather than an unusable dll crashing the tab
                throw new InvalidOperationException(DllName + " could not be loaded");
            }

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
