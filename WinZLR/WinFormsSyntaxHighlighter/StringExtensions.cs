using System;

namespace winZLR.WinFormsSyntaxHighlighter
{
    public static class StringExtensions
    {
        public static string NormalizeLineBreaks(this string instance, string preferredLineBreak)
        {
            return instance.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", preferredLineBreak);
        }

        public static string NormalizeLineBreaks(this string instance)
        {
            return instance.NormalizeLineBreaks(Environment.NewLine);
        }
    }
}
