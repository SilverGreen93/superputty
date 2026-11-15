using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SuperPutty.Gui
{
    public class DpiUtils
    {
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern int GetDpiForWindow(IntPtr hWnd);

        /// <summary>A 96 DPI length in pixels at <paramref name="dpi"/>.</summary>
        public static int Scale(int logical, int dpi)
        {
            return (int)Math.Round(logical * dpi / 96.0);
        }

        /// <summary>
        /// A font of <paramref name="points"/> for a window at <paramref name="dpi"/>. Sized in pixels:
        /// a point size is converted at the DPI the process started with, which is wrong after a
        /// runtime DPI change (e.g. an RDP reconnect from a client with another scale).
        /// </summary>
        public static Font FontForDpi(string family, float points, int dpi)
        {
            return new Font(family, points * dpi / 72f, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Taken from https://oliversturm.com/blog/2010/09/02/the-crime-that-is-windows-dpi-handling/
        /// </summary>
        /// <returns>The current scren DPI</returns>
        public static Size GetScreenDPI()
        {
            // no error checking here - being lazy
            var dc = GetDC(IntPtr.Zero);
            try
            {
                return new Size(
                  GetDeviceCaps(dc, (int)DeviceCap.LOGPIXELSX),
                  GetDeviceCaps(dc, (int)DeviceCap.LOGPIXELSY)
                );
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, dc);
            }
        }

        public static int ScaleWidth(int originalWidth)
        {
            return (int)(originalWidth / 96f * GetScreenDPI().Width);
        }

        public static int ScaleHeight(int originalHeight)
        {
            return (int)(originalHeight / 96f * GetScreenDPI().Height);
        }

        public static Size ScaleSize(Size originalSize)
        {
            return new Size(ScaleWidth(originalSize.Width), ScaleHeight(originalSize.Height));
        }
    }

    enum DeviceCap
    {
        /// <summary>
        /// Logical pixels inch in X
        /// </summary>
        LOGPIXELSX = 88,
        /// <summary>
        /// Logical pixels inch in Y
        /// </summary>
        LOGPIXELSY = 90
    }
}
