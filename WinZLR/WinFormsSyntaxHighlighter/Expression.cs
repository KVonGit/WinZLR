using System;

namespace winZLR.WinFormsSyntaxHighlighter
{
    public class Expression
    {
        public ExpressionType Type { get; private set; }
        public string Content { get; private set; }
        public string Group { get; private set; }

        public Expression(string content, ExpressionType type, string group)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(group);

            Type = type;
            Content = content;
            Group = group;
        }

        public Expression(string content, ExpressionType type)
            : this(content, type, string.Empty)
        {
        }

        public override string ToString()
        {
            if (Type == ExpressionType.Newline)
                return string.Format("({0})", Type);

            return string.Format("({0} --> {1}{2})", Content, Type, Group.Length > 0 ? " --> " + Group : string.Empty);
        }
    }
}
