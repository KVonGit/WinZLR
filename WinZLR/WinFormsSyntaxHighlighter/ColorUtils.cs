using System;
using System.Drawing;

namespace winZLR.WinFormsSyntaxHighlighter
{
    public class ColorUtils
    {
        public static string ColorToRtfTableEntry(Color color)
        {
            return string.Format(@"\red{0}\green{1}\blue{2}", color.R, color.G, color.B);
        }
    }
}
