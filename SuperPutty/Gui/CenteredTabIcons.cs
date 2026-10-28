using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace SuperPutty.Gui
{
    /// <summary>
    /// The VS2015 themes draw a document tab's icon 2px above the bottom of the tab but centre
    /// its text, so the icons sit low once the tab font is scaled up for a high DPI.
    /// Wraps the theme's tab strips and moves the icons to the middle after they paint.
    /// </summary>
    internal class CenteredTabIcons : DockPanelExtender.IDockPaneStripFactory
    {
        private readonly DockPanelExtender.IDockPaneStripFactory inner;

        public static void Apply(ThemeBase theme)
        {
            theme.Extender.DockPaneStripFactory = new CenteredTabIcons(theme.Extender.DockPaneStripFactory);
        }

        private CenteredTabIcons(DockPanelExtender.IDockPaneStripFactory inner)
        {
            this.inner = inner;
        }

        public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
        {
            DockPaneStripBase strip = this.inner.CreateDockPaneStrip(pane);
            new IconPainter(strip, pane);
            return strip;
        }

        // ponytail: reads the strip's private layout (DockPanelSuite 3.1.1 VS2013DockPaneStrip) and ignores RTL;
        // recheck the constants and member names when upgrading DockPanelSuite.
        private class IconPainter : NativeWindow
        {
            private const int WM_PAINT = 0x000F;
            private const int IconSize = 16, IconGapLeft = 8, IconGapBottom = 2;
            private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

            private readonly Control strip;
            private readonly DockPane pane;
            private readonly PropertyInfo tabsRectangle;
            private readonly MethodInfo getTabRectangle;

            public IconPainter(Control strip, DockPane pane)
            {
                this.strip = strip;
                this.pane = pane;
                this.tabsRectangle = strip.GetType().GetProperty("TabsRectangle", Private);
                this.getTabRectangle = strip.GetType().GetMethod("GetTabRectangle", Private, null, new[] { typeof(int) }, null);
                if (this.tabsRectangle == null || this.getTabRectangle == null)
                    return; // another strip; leave it alone

                strip.HandleCreated += (s, e) => AssignHandle(strip.Handle);
                if (strip.IsHandleCreated)
                    AssignHandle(strip.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg == WM_PAINT && this.pane.Appearance == DockPane.AppearanceStyle.Document && this.pane.DockPanel.ShowDocumentIcon)
                {
                    using (Graphics g = this.strip.CreateGraphics())
                        PaintIcons(g);
                }
            }

            private void PaintIcons(Graphics g)
            {
                Rectangle tabs = (Rectangle)this.tabsRectangle.GetValue(this.strip);
                DockPanelColorPalette colors = this.pane.DockPanel.Theme.ColorPalette;
                g.SetClip(tabs);
                for (int i = 0; i < this.pane.DisplayingContents.Count; i++)
                {
                    IDockContent content = this.pane.DisplayingContents[i];
                    Rectangle tab = (Rectangle)this.getTabRectangle.Invoke(this.strip, new object[] { i });
                    // same rules as the strip uses to decide whether it drew the icon
                    if (content == this.pane.ActiveContent)
                        tab.Intersect(tabs);
                    else if (!tab.IntersectsWith(tabs))
                        continue;
                    Rectangle drawn = new Rectangle(tab.X + IconGapLeft, tab.Bottom - IconGapBottom - IconSize, IconSize, IconSize);
                    Rectangle centred = new Rectangle(drawn.X, tab.Y + (tab.Height - IconSize) / 2, IconSize, IconSize);
                    if (drawn.Y == centred.Y || !tab.Contains(drawn) || content.DockHandler.Icon == null)
                        continue;

                    Color back = content == this.pane.ActiveContent
                        ? (this.pane.IsActiveDocumentPane ? colors.TabSelectedActive.Background : colors.TabSelectedInactive.Background)
                        : content == this.pane.MouseOverTab ? colors.TabUnselectedHovered.Background : colors.MainWindowActive.Background;
                    using (Brush brush = new SolidBrush(back))
                        g.FillRectangle(brush, drawn);
                    g.DrawIcon(content.DockHandler.Icon, centred);
                }
            }
        }
    }
}
