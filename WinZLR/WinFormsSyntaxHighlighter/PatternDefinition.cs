using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace winZLR.WinFormsSyntaxHighlighter
{
    public class PatternDefinition
    {
        private readonly Regex _regex;
        private ExpressionType _expressionType = ExpressionType.Identifier;
        private readonly bool _isCaseSensitive = true;

        public PatternDefinition(Regex regularExpression)
        {
            _regex = regularExpression ?? throw new ArgumentNullException(nameof(regularExpression));
        }

        public PatternDefinition(string regexPattern)
        {
            if (string.IsNullOrEmpty(regexPattern))
                throw new ArgumentException("regex pattern must not be null or empty", nameof(regexPattern));

            _regex = new Regex(regexPattern, RegexOptions.Compiled);
        }

        public PatternDefinition(string regexPattern, RegexOptions options)
        {
            if (string.IsNullOrEmpty(regexPattern))
                throw new ArgumentException("regex pattern must not be null or empty", nameof(regexPattern));
            
            options |= RegexOptions.Compiled;
            _regex = new Regex(regexPattern, options);
        }

        public PatternDefinition(params string[] tokens)
            : this(true, tokens)
        {
        }

        public PatternDefinition(IEnumerable<string> tokens)
            : this(true, tokens)
        {
        }

        internal PatternDefinition(bool caseSensitive, IEnumerable<string> tokens)
        {
            ArgumentNullException.ThrowIfNull(tokens);

            _isCaseSensitive = caseSensitive;

            var regexTokens = new List<string>();

            foreach (var token in tokens)
            {
                var escaptedToken = Regex.Escape(token.Trim());

                if (escaptedToken.Length > 0)
                {
                    if (char.IsLetterOrDigit(escaptedToken[0]))
                        regexTokens.Add(string.Format(@"\b{0}\b", escaptedToken));
                    else
                        regexTokens.Add(escaptedToken);
                }
            }

            string pattern = string.Join("|", regexTokens);
            var regexOptions = RegexOptions.Compiled;
            if (!caseSensitive)
                regexOptions |= RegexOptions.IgnoreCase;
            _regex = new Regex(pattern, regexOptions);
        }

        internal ExpressionType ExpressionType 
        {
            get { return _expressionType; }
            set { _expressionType = value; }
        }

        internal bool IsCaseSensitive 
        {
            get { return _isCaseSensitive; }
        }

        internal Regex Regex
        {
            get { return _regex; }
        }
    }
}
