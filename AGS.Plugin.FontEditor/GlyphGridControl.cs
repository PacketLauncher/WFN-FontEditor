
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AGS.Plugin.FontEditor
{
    public class GlyphGridControl : ScrollableControl
    {
        private readonly List<PictureBox> _glyphs =
            new List<PictureBox>();

        private const int GlyphMargin = 3;

        public Func<PictureBox, bool> IsGlyphSelected { get; set; }

        public GlyphGridControl()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Selectable,
                true);

            TabStop = true;
            BackColor = SystemColors.Control;
            AutoScroll = true;
        }

        public void SetGlyphs(IEnumerable<PictureBox> glyphs)
        {
            _glyphs.Clear();

            if (glyphs != null)
                _glyphs.AddRange(glyphs);

            UpdateScrollSize();
            Invalidate();
        }

        public PictureBox GetGlyphAt(Point location)
        {
            int x = GlyphMargin;
            int y = GlyphMargin + AutoScrollPosition.Y;
            int rowHeight = 0;
            int availableWidth = Math.Max(1, ClientSize.Width);

            foreach (PictureBox glyph in _glyphs)
            {
                int width = glyph.Width;
                int height = glyph.Height;

                if (x > GlyphMargin &&
                    x + width + GlyphMargin > availableWidth)
                {
                    x = GlyphMargin;
                    y += rowHeight;
                    rowHeight = 0;
                }

                Rectangle glyphBounds = new Rectangle(
                    x, y, width, height);

                if (glyphBounds.Contains(location))
                    return glyph;

                x += width + GlyphMargin * 2;
                rowHeight = Math.Max(
                    rowHeight,
                    height + GlyphMargin * 2);
            }

            return null;
        }

        public PictureBox GetVerticalNeighbor(PictureBox current, bool moveUp)
        {
            if (current == null || !_glyphs.Contains(current))
                return null;

            List<Rectangle> bounds = new List<Rectangle>();

            int x = GlyphMargin;
            int y = GlyphMargin;
            int rowHeight = 0;
            int availableWidth = Math.Max(1, ClientSize.Width);

            foreach (PictureBox glyph in _glyphs)
            {
                int width = glyph.Width;
                int height = glyph.Height;

                if (x > GlyphMargin &&
                    x + width + GlyphMargin > availableWidth)
                {
                    x = GlyphMargin;
                    y += rowHeight;
                    rowHeight = 0;
                }

                bounds.Add(new Rectangle(x, y, width, height));

                x += width + GlyphMargin * 2;
                rowHeight = Math.Max(rowHeight, height + GlyphMargin * 2);
            }

            int currentIndex = _glyphs.IndexOf(current);
            Rectangle source = bounds[currentIndex];
            int sourceX = source.Left + source.Width / 2;

            int targetRowY = moveUp ? int.MinValue : int.MaxValue;

            // Find the immediately preceding or following visual row.
            foreach (Rectangle rect in bounds)
            {
                if (moveUp)
                {
                    if (rect.Top < source.Top && rect.Top > targetRowY)
                        targetRowY = rect.Top;
                }
                else
                {
                    if (rect.Top > source.Top && rect.Top < targetRowY)
                        targetRowY = rect.Top;
                }
            }

            if (targetRowY == int.MinValue || targetRowY == int.MaxValue)
                return current;

            // Find the glyph closest to our horizontal position in that row.
            int bestIndex = currentIndex;
            int bestDistance = int.MaxValue;

            for (int i = 0; i < bounds.Count; i++)
            {
                Rectangle rect = bounds[i];

                if (rect.Top != targetRowY)
                    continue;

                int candidateX = rect.Left + rect.Width / 2;
                int distance = Math.Abs(candidateX - sourceX);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            return _glyphs[bestIndex];
        }

        private void UpdateScrollSize()
        {
            int x = GlyphMargin;
            int y = GlyphMargin;
            int rowHeight = 0;
            int availableWidth = Math.Max(1, ClientSize.Width);

            foreach (PictureBox glyph in _glyphs)
            {
                int width = glyph.Width;
                int height = glyph.Height;

                if (x > GlyphMargin &&
                    x + width + GlyphMargin > availableWidth)
                {
                    x = GlyphMargin;
                    y += rowHeight;
                    rowHeight = 0;
                }

                x += width + GlyphMargin * 2;
                rowHeight = Math.Max(
                    rowHeight,
                    height + GlyphMargin * 2);
            }

            AutoScrollMinSize = new Size(0, y + rowHeight + GlyphMargin);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollSize();
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate();
        }

        public event MouseEventHandler GlyphNavigationWheel;

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Send wheel movement to glyph navigation only.
            // Do not call base.OnMouseWheel(), because ScrollableControl
            // would also move the vertical scrollbar.
            if (GlyphNavigationWheel != null)
                GlyphNavigationWheel(this, e);
        }

        public void ScrollGlyphIntoView(PictureBox target)
        {
            if (target == null || !_glyphs.Contains(target))
                return;

            int x = GlyphMargin;
            int y = GlyphMargin;
            int rowHeight = 0;
            int availableWidth = Math.Max(1, ClientSize.Width);

            foreach (PictureBox glyph in _glyphs)
            {
                int width = glyph.Width;
                int height = glyph.Height;

                if (x > GlyphMargin &&
                    x + width + GlyphMargin > availableWidth)
                {
                    x = GlyphMargin;
                    y += rowHeight;
                    rowHeight = 0;
                }

                if (glyph == target)
                {
                    int scrollY = -AutoScrollPosition.Y;
                    int visibleHeight = ClientSize.Height;

                    int targetTop = y - GlyphMargin;
                    int targetBottom = y + height + GlyphMargin;

                    int newScrollY = scrollY;

                    if (targetTop < scrollY)
                    {
                        newScrollY = targetTop;
                    }
                    else if (targetBottom > scrollY + visibleHeight)
                    {
                        newScrollY = targetBottom - visibleHeight;
                    }

                    if (newScrollY != scrollY)
                    {
                        AutoScrollPosition = new Point(0, newScrollY);
                        Invalidate();
                    }

                    return;
                }

                x += width + GlyphMargin * 2;
                rowHeight = Math.Max(
                    rowHeight,
                    height + GlyphMargin * 2);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int x = GlyphMargin;
            int y = GlyphMargin + AutoScrollPosition.Y;
            int rowHeight = 0;
            int availableWidth = Math.Max(1, ClientSize.Width);

            foreach (PictureBox glyph in _glyphs)
            {
                int width = glyph.Width;
                int height = glyph.Height;

                if (x > GlyphMargin &&
                    x + width + GlyphMargin > availableWidth)
                {
                    x = GlyphMargin;
                    y += rowHeight;
                    rowHeight = 0;
                }

                if (glyph.Image != null)
                {
                    e.Graphics.DrawImageUnscaled(
                        glyph.Image, x, y);
                }

                // Draw the selection outline.
                if (IsGlyphSelected != null && IsGlyphSelected(glyph))
                {
                    using (Pen pen = new Pen(Color.DeepSkyBlue, 2))
                    {
                        Rectangle rect = new Rectangle(x, y, width, height);
                        rect.Inflate(1, 1);
                        e.Graphics.DrawRectangle(pen, rect);
                    }
                }

                x += width + GlyphMargin * 2;
                rowHeight = Math.Max(
                    rowHeight,
                    height + GlyphMargin * 2);
            }
        }
    }
}
