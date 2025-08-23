using System.Collections.Generic;

namespace winZLR.WinFormsSyntaxHighlighter
{
    public class CaseInsensitivePatternDefinition : PatternDefinition
    {
        public CaseInsensitivePatternDefinition(IEnumerable<string> tokens)
            : base(false, tokens)
        {
        }

        public CaseInsensitivePatternDefinition(params string[] tokens)
            : base(false, tokens)
        {
        }
    }
}
