using System;
using System.Drawing;
using System.Windows.Forms;

namespace AGS.Plugin.FontEditor
{
    public class GlyphFlowLayoutPanel : FlowLayoutPanel
    {
        public GlyphFlowLayoutPanel()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
        }

        public event MouseEventHandler GlyphMouseWheel;

        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x020A;

            if (m.Msg == WM_MOUSEWHEEL)
            {
                Point cursor = PointToClient(Cursor.Position);

                // Allow normal scrolling when hovering over
                // the vertical scrollbar.
                if (VerticalScroll.Visible &&
                    cursor.X >= ClientSize.Width)
                {
                    base.WndProc(ref m);
                    return;
                }

                // Otherwise, navigate between glyphs.
                int delta = unchecked((short)(m.WParam.ToInt64() >> 16));

                GlyphMouseWheel?.Invoke(
                    this,
                    new MouseEventArgs(MouseButtons.None, 0, 0, 0, delta));

                m.Result = IntPtr.Zero;
                return;
            }

            base.WndProc(ref m);
        }
    }
}