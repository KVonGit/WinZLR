using System;

namespace winZLR.WinFormsSyntaxHighlighter
{
    internal class PatternStyleMap
    {
        public string Name { get; set; }
        public PatternDefinition PatternDefinition { get; set; }
        public SyntaxStyle SyntaxStyle { get; set; }

        public PatternStyleMap(string name, PatternDefinition patternDefinition, SyntaxStyle syntaxStyle)
        {
            ArgumentNullException.ThrowIfNull(patternDefinition);
            ArgumentNullException.ThrowIfNull(syntaxStyle);
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name must not be null or empty", nameof(name));

            Name = name;
            PatternDefinition = patternDefinition;
            SyntaxStyle = syntaxStyle;
        }
    }
}