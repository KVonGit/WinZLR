using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace winZLR.WinFormsSyntaxHighlighter
{
    public static class TextBoxBaseExtensions
    {
        /// <summary>
        /// In order to make flicker free changes to the TextBox's text, it will 
        /// first disable the TextBox using some Win32 API stuff, applies the changes
        /// passed through the <c>Action</c> argument, and then re-enables the TextBox.
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="action"></param>
        public static void DisableThenDoThenEnable(this TextBoxBase textBox, Action action)
        {
            nint stateLocked = nint.Zero;

            textBox.Lock(ref stateLocked);

            int hscroll = textBox.GetHScrollPos();
            int vscroll = textBox.GetVScrollPos();

            int selstart = textBox.SelectionStart;
            int sellength = textBox.SelectionLength;

            action();

            textBox.Select(selstart, sellength);

            textBox.SetHScrollPos(hscroll);
            textBox.SetVScrollPos(vscroll);

            textBox.Unlock(ref stateLocked);
        }

        public static int GetHScrollPos(this TextBoxBase textBox)
        {
            return GetScrollPos((int)textBox.Handle, SB_HORZ);
        }

        public static void SetHScrollPos(this TextBoxBase textBox, int value)
        {
            _ = SetScrollPos(textBox.Handle, SB_HORZ, value, true);
            PostMessageA(textBox.Handle, WM_HSCROLL, SB_THUMBPOSITION + 0x10000 * value, 0);
        }

        public static int GetVScrollPos(this TextBoxBase textBox)
        {
            return GetScrollPos((int)textBox.Handle, SB_VERT);
        }

        public static void SetVScrollPos(this TextBoxBase textBox, int value)
        {
            _ = SetScrollPos(textBox.Handle, SB_VERT, value, true);
            PostMessageA(textBox.Handle, WM_VSCROLL, SB_THUMBPOSITION + 0x10000 * value, 0);
        }

        private static void Lock(this TextBoxBase textBox, ref nint stateLocked)
        {
            // Stop redrawing:  
            SendMessage(textBox.Handle, WM_SETREDRAW, 0, nint.Zero);
            // Stop sending of events:  
            stateLocked = SendMessage(textBox.Handle, EM_GETEVENTMASK, 0, nint.Zero);
            // change colors and stuff in the RichTextBox  
        }

        private static void Unlock(this TextBoxBase textBox, ref nint stateLocked)
        {
            // turn on events  
            SendMessage(textBox.Handle, EM_SETEVENTMASK, 0, stateLocked);
            // turn on redrawing  
            SendMessage(textBox.Handle, WM_SETREDRAW, 1, nint.Zero);

            stateLocked = nint.Zero;
            textBox.Invalidate();
        }

        #region Win API Stuff

        // Windows APIs
        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static nint SendMessage(nint hWnd, int msg, int wParam, nint lParam);

        [DllImport("user32.dll")]
        private static extern bool PostMessageA(nint hWnd, int nBar, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetScrollPos(int hWnd, int nBar);

        [DllImport("user32.dll")]
        private static extern int SetScrollPos(nint hWnd, int nBar, int nPos, bool bRedraw);

        private const int WM_SETREDRAW = 0x000B;
        private const int WM_USER = 0x400;
        private const int EM_GETEVENTMASK = WM_USER + 59;
        private const int EM_SETEVENTMASK = WM_USER + 69;
        private const int SB_HORZ = 0x0;
        private const int SB_VERT = 0x1;
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int SB_THUMBPOSITION = 4;
        private const int UNDO_BUFFER = 100;

        #endregion
    }
}
