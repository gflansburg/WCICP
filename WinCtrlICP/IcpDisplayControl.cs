using WinCtrlICP.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace WinCtrlICP
{
    public class IcpDisplayControl : Control
    {
        public const int GlyphW = 34;
        public const int GlyphH = 46;

        public static readonly string[] BlankIcpLines = { "", "", "", "", "" };


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int LeftMargin { get; set; } = 13;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int TopMargin { get; set; } = 2;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int LineCount { get; set; } = 5;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ShowSelectionBorder { get; set; } = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] Lines { get; private set; }

        private readonly Dictionary<char, Rectangle> _glyphMap = new Dictionary<char, Rectangle>();

        public IcpDisplayControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            Lines = new string[LineCount];
            BuildGlyphMap();
        }

        public void SetLines(string[] lines)
        {
            // Normalize to exactly 5 lines, no nulls
            var normalized = new string[LineCount];
            for (int i = 0; i < LineCount; i++)
            {
                normalized[i] = (lines != null && i < lines.Length && lines[i] != null) ? lines[i] : string.Empty;
            }

            Lines = normalized;
            Invalidate(); // full repaint is fine for now
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            int glyphH = (ClientSize.Height - (TopMargin * 2)) / LineCount;
            if (glyphH <= 0) return;

            int glyphW = ((glyphH + 1) * (GlyphW - 2)) / GlyphH;   // preserve atlas aspect ratio
            if (glyphW <= 0) return;

            int y = TopMargin;
            for (int i = 0; i < LineCount; i++)
            {
                DrawIcpText(e.Graphics, Lines[i], LeftMargin, y, glyphW, glyphH);
                y += glyphH + 1;
            }

            if (ShowSelectionBorder)
            {
                using var pen = new Pen(Color.Red, 1);
                Rectangle r = new Rectangle(
                    1,
                    1,
                    Width - 1,
                    Height - 1
                );
                e.Graphics.DrawRectangle(pen, r);
            }
        }

        private void DrawIcpText(Graphics g, string text, int x, int y, int glyphW, int glyphH)
        {
            if (string.IsNullOrEmpty(text)) return;

            int maxChars = (ClientSize.Width - x) / (glyphW - 2);
            if (maxChars <= 0) return;

            // Markers for inverted runs
            const char InvStart = '⟦';
            const char InvEnd = '⟧';

            bool inverted = false;
            int drawn = 0;

            foreach (char raw in text)
            {
                if (drawn >= maxChars) break;

                // Toggle inversion; markers do NOT consume a glyph cell
                if (raw == InvStart) { inverted = true; continue; }
                if (raw == InvEnd) { inverted = false; continue; }

                char c = char.ToUpperInvariant(raw);

                if (!_glyphMap.TryGetValue(c, out var src))
                    src = _glyphMap.TryGetValue(' ', out var sp) ? sp : Rectangle.Empty;

                if (src != Rectangle.Empty)
                {
                    var fontBmp = inverted ? Resources.ICP_font_inv : Resources.ICP_font;
                    g.DrawImage(fontBmp, new Rectangle(x, y, glyphW, glyphH), src, GraphicsUnit.Pixel);
                }

                x += glyphW - 1;
                drawn++;
            }
        }

        private void BuildGlyphMap()
        {
            const int charsPerRow = 8;

            string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890↕{}<>[]+-*/=°|↓↑.,!?:;&_'\"%#@ ";

            for (int i = 0; i < charset.Length; i++)
            {
                int row = i / charsPerRow;
                int col = i % charsPerRow;

                _glyphMap[charset[i]] = new Rectangle(
                    (col * GlyphW) + 1,
                    (row * GlyphH) + 1,
                    GlyphW - 1,
                    GlyphH - 1
                );
            }
        }
    }
}
