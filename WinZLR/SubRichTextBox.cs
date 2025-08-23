/*  MIT License

    Copyright(c) 2021-2025 Henrik Åsman

    Permission Is hereby granted, free Of charge, to any person obtaining a copy
    of this software And associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
    copies of the Software, And to permit persons to whom the Software Is
    furnished to do so, subject to the following conditions:

    The above copyright notice And this permission notice shall be included In all
    copies Or substantial portions of the Software.

    THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
    IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
    LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
    OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
    SOFTWARE.                                                                         */

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace winZLR
{
    internal class SubRichTextBox : RichTextBox
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        private const int WM_USER = 0x0400;
        private const int EM_SETEVENTMASK = (WM_USER + 69);
        private const int WM_SETREDRAW = 0x0b;
        private const int WM_PAINT = 0x0f;

        private IntPtr OldEventMask;
        private int LineColumnWidth = 0;
        private int Indent = -1;
        private double OldZoomFactor = 1;
        private bool CalculatingIndent = false;
        private int bracketStart = -1;
        private int bracketEnd = -1;
        private readonly System.Windows.Forms.Timer? timerBrackets;

        private bool _ShowLineNumbers = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowLineNumbers
        {
            get { return _ShowLineNumbers; }
            set
            {
                _ShowLineNumbers = value;
                if (_ShowLineNumbers)
                {
                    Size sz = TextRenderer.MeasureText("123456", Font);
                    LineColumnWidth = sz.Width;
                }
                else
                {
                    LineColumnWidth = 0;
                }
                Indent = -1;
                UpdateIndent();
            }
        }
        public string Brackets = "()[]{}<>";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowMatchingBrackets
        {
            get { if (timerBrackets != null) return timerBrackets.Enabled; else return false; }
            set
            {
                if (timerBrackets != null) timerBrackets.Enabled = value;
            }
        }

        public SubRichTextBox() : base() 
        {
            SelectionIndent = LineColumnWidth;
            timerBrackets = new();
            timerBrackets.Tick += TimerBrackets_Tick;
            timerBrackets.Interval = 100;
            timerBrackets.Enabled = false;
        }

        private void TimerBrackets_Tick(object? sender, EventArgs e)
        {
            // TODO: This should be handled in OnSelectionChanged, but for some reason I can't get it to work
            if (timerBrackets == null) return;
            timerBrackets.Enabled = false;

            int oldStart = bracketStart;
            int oldEnd = bracketEnd;
            if (!FindMatchingBrackets())
            {
                bracketStart = -1;
                bracketEnd = -1;
            }
            if (oldStart != bracketStart || oldEnd != bracketEnd) Invalidate();

            timerBrackets.Enabled = true;
        }

        public void BeginUpdate()
        {
            SendMessage(Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
            OldEventMask = (IntPtr)SendMessage(this.Handle, EM_SETEVENTMASK, IntPtr.Zero, IntPtr.Zero);
        }

        public void EndUpdate()
        {
            EndUpdate(true);
        }

        private void EndUpdate(bool invalidate)
        {
            SendMessage(Handle, EM_SETEVENTMASK, IntPtr.Zero, OldEventMask);
            SendMessage(Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            if (invalidate) Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PAINT && LineColumnWidth > 0)
            {
                Invalidate();
                base.WndProc(ref m);

                // use local to stop multiple calls
                int lineCount = Lines.Length;

                // get a graphics object
                using Graphics g = Graphics.FromHwnd(Handle);
                
                // draw light gray rectangle
                g.FillRectangle(Brushes.Gainsboro, new Rectangle(0, 0, LineColumnWidth, Height));

                // get Line position of first visible line (only Y is interesting)
                int charTop = GetCharIndexFromPosition(new Point(0, 0));
                int lineTop = GetLineFromCharIndex(charTop);
                int yTop = GetPositionFromCharIndex(charTop).Y;
                int lineHeight = (int)(Font.Height * ZoomFactor);

                // draw visible line numbers in rectangle
                int y = yTop;
                for (int i = lineTop; i < lineCount; i++)
                {
                    if (i - lineTop == 1)
                    {
                        // recalculate height
                        y = GetPositionFromCharIndex(GetFirstCharIndexFromLine(i)).Y;
                        lineHeight = y - yTop;

                    }
                    // unfortunately isn't the height constant, for example, bold increases height by one pixel
                    // y needs to be calibrated regulary to not loose sync, but because getting the position
                    // for a line is relative slow, we don't do it on every line
                    if (i % 3 == 0) y = GetPositionFromCharIndex(GetFirstCharIndexFromLine(i)).Y;
                    TextRenderer.DrawText(g, (i + 1).ToString(), Font, new Rectangle(0, y, LineColumnWidth - 10, lineHeight), Color.Gray, TextFormatFlags.Right | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
                    y += lineHeight;
                    if (y > Height) break;
                }

                // Update Indent if ZoomFactor has changed
                if (ZoomFactor != OldZoomFactor) RefreshIndent();

                // draw matching brackets if there are any
                if (bracketStart != -1 && bracketEnd != -1 && bracketStart < Text.Length && bracketEnd < Text.Length)
                {
                    string s1 = Text[bracketStart].ToString();
                    string s2 = Text[bracketEnd].ToString();
                    Font fontRegular = new(Font.FontFamily, Font.Size * ZoomFactor, Font.Style);
                    Font fontBold = new(Font.FontFamily, Font.Size * ZoomFactor, FontStyle.Bold);
                    Point bracket1pt = GetPositionFromCharIndex(bracketStart);
                    Point bracket2pt = GetPositionFromCharIndex(bracketEnd);
                    Size maxSize = new(50, 50);
                    Size bracket1sz = TextRenderer.MeasureText(g, s1, fontRegular, maxSize, TextFormatFlags.NoPadding);
                    Size bracket2sz = TextRenderer.MeasureText(g, s2, fontRegular, maxSize, TextFormatFlags.NoPadding);
                    g.FillRectangle(Brushes.White, new Rectangle(bracket1pt, bracket1sz));
                    g.FillRectangle(Brushes.White, new Rectangle(bracket2pt, bracket2sz));
                    TextRenderer.DrawText(g, s1, fontBold, bracket1pt, Color.Red, TextFormatFlags.NoPadding);
                    TextRenderer.DrawText(g, s2, fontBold, bracket2pt, Color.Red, TextFormatFlags.NoPadding);
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        public void UpdateIndent()
        {
            // // TODO: I can't get OnSelectionChanged to work, do manual resfresh instead
            if (CalculatingIndent) return;

            if (Indent == -1) 
            {
                Indent = LineColumnWidth;
                RefreshIndent();
            }
            if (SelectionIndent != Indent)
            {
                RefreshIndent();
                Indent = SelectionIndent;
            }
        }

        private void RefreshIndent()
        {
            BeginUpdate();
            CalculatingIndent = true;
            OldZoomFactor = ZoomFactor;
            int selStart = SelectionStart;
            int selLength = SelectionLength;
            SelectAll();
            int left = GetPositionFromCharIndex(0).X - 1;
            if (left > LineColumnWidth)
                while (left > LineColumnWidth)
                {
                    SelectionIndent--;
                    left = GetPositionFromCharIndex(0).X - 1;
                }
            else if (left < LineColumnWidth)
                while (left < LineColumnWidth)
                {
                    SelectionIndent++;
                    left = GetPositionFromCharIndex(0).X - 1;
                }
            SelectionStart = selStart;
            SelectionLength = selLength;   
            CalculatingIndent = false;
            EndUpdate();
        }

        private bool FindMatchingBrackets()
        {
            int textLength = Text.Length;
            if (textLength == 0) return false;    

            // is char before, or under, caret a bracket? if both, priority goes to before
            int brIdx = -1;
            int oppPos = SelectionStart;
            if (oppPos > 0) brIdx = Brackets.IndexOf(Text[oppPos - 1]);
            if (brIdx == -1 && oppPos < textLength) brIdx = Brackets.IndexOf(Text[oppPos]); else oppPos--;
            if (brIdx != -1) // brackets found before or under caret
            {
                char brChar = Brackets[brIdx];
                char oppChar;
                int brCount = 0;
                int pos = oppPos;
                int direction = -1;                 // assume backwards
                if (brIdx % 2 == 0) direction = 1;  // even index, match forward
                oppChar = Brackets[brIdx + direction];
                StringBuilder sb = new(Text);
                while (true)
                {
                    char c = sb[oppPos];
                    if (c == brChar) brCount++;
                    if (c == oppChar) brCount--;
                    if (brCount == 0) break;
                    oppPos += direction;
                    if (oppPos < 0 || oppPos == textLength) break;  
                }
                if (brCount == 0) // found a matching pair
                {
                    bracketStart = pos;
                    bracketEnd = oppPos;
                    return true;
                }
            }
            return false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Without this MouseHoover only appears one time in a control until mouse leaves and re-enters
            ResetMouseEventArgs();
        }
    }
}
