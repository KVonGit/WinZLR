using System;

namespace winZLR.WinFormsSyntaxHighlighter
{
    internal class StyleGroupPair
    {
        public int Index { get; set; }
        public SyntaxStyle SyntaxStyle { get; set; }
        public string GroupName { get; set; }

        public StyleGroupPair(SyntaxStyle syntaxStyle, string groupName)
        {
            ArgumentNullException.ThrowIfNull(syntaxStyle);
            ArgumentNullException.ThrowIfNull(groupName);

            SyntaxStyle = syntaxStyle;
            GroupName = groupName;
        }
    }
}
