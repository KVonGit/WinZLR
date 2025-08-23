using System.Drawing;

namespace winZLR.WinFormsSyntaxHighlighter
{
    public class SyntaxStyle(Color color, bool bold, bool italic)
    {
        public bool Bold { get; set; } = bold;
        public bool Italic { get; set; } = italic;
        public Color Color { get; set; } = color;

        public SyntaxStyle(Color color)
            : this(color, false, false)
        {
        }
    }
}
