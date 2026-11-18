using System;
using System.Windows.Forms;

namespace SuperPutty.Utils
{
    /// <summary>
    /// Makes the first click on a toolbar button or menu do its job instead of only
    /// bringing the window to the foreground.
    /// 
    /// When the window is not active, Windows asks the window under the cursor what to do
    /// with the click by sending it WM_MOUSEACTIVATE. WinForms tool strips answer with
    /// MA_ACTIVATEANDEAT: activate the window, then throw the click away, so the button is
    /// never pressed and the menu never opens. Plain Win32 windows - and every other
    /// application on the desktop - answer MA_ACTIVATE instead and then receive the click.
    /// This subclass lets the strip handle the message as usual and corrects the answer, so
    /// one click both activates the window and presses the button.
    /// 
    /// The correction has to happen on the strip's own window. The main form is asked about
    /// the same click and does answer MA_ACTIVATE, but the strip is asked last and wins, so
    /// there is nothing the form can do about it.
    /// </summary>
    public class ToolStripFirstClickHelper : NativeWindow, IDisposable
    {
        private const int MA_ACTIVATE = 1;
        private const int MA_ACTIVATEANDEAT = 2;

        private readonly ToolStrip toolStrip;
        private bool disposed = false;

        public ToolStripFirstClickHelper(ToolStrip toolStrip)
        {
            this.toolStrip = toolStrip;
            this.toolStrip.HandleCreated += HandleCreated;
            this.toolStrip.HandleDestroyed += HandleDestroyed;
            if (this.toolStrip.IsHandleCreated)
            {
                this.AssignHandle(this.toolStrip.Handle);
            }
        }

        void HandleCreated(object sender, EventArgs e)
        {
            this.AssignHandle(this.toolStrip.Handle);
        }

        void HandleDestroyed(object sender, EventArgs e)
        {
            this.ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == (int)NativeMethods.WM.MOUSEACTIVATE && m.Result.ToInt32() == MA_ACTIVATEANDEAT)
            {
                m.Result = (IntPtr)MA_ACTIVATE;
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.toolStrip.HandleCreated -= HandleCreated;
                this.toolStrip.HandleDestroyed -= HandleDestroyed;
                this.ReleaseHandle();
                this.disposed = true;
            }
        }
    }
}
