using System.Text;
using System.Text.RegularExpressions;

namespace NtFreX.Precompiler
{
    public class Precompiler
    {
        private readonly List<(SyntaxTokenType TokenType, Regex Regex)> TokenMatchers = new()
        {
            { (SyntaxTokenType.If, new Regex(@"#if")) },
            { (SyntaxTokenType.ElseIf, new Regex(@"#elseif")) },
            { (SyntaxTokenType.Else, new Regex(@"#else")) },
            { (SyntaxTokenType.EndIf, new Regex(@"#endif")) },
            { (SyntaxTokenType.Not, new Regex(@"not")) },
            { (SyntaxTokenType.Not, new Regex(@"!")) },
            { (SyntaxTokenType.Include, new Regex(@"#include")) },
            { (SyntaxTokenType.Variable, new Regex(@"#{\w*}")) },
        };

        private readonly Dictionary<string, bool> flags;
        private readonly Dictionary<string, string> values;

        public Precompiler(Dictionary<string, bool> flags, Dictionary<string, string> values)
        {
            this.flags = flags;
            this.values = values;
        }

        public static string PrecompileText(Dictionary<string, bool> flags, Dictionary<string, string> values, string value)
        {
            var precomipler = new Precompiler(flags, values);
            return precomipler.Precompile(value, value);
        }

        public static string PrecompileFile(Dictionary<string, bool> flags, Dictionary<string, string> values, string path)
        {
            var precomipler = new Precompiler(flags, values);
            var rawValue = File.ReadAllText(path);
            return precomipler.Precompile(rawValue, path);
        }

        private static bool TryGetNotToken(List<SyntaxToken> tokens, ref int tokenIndex)
        {
            for (; tokenIndex < tokens.Count; tokenIndex++)
            {
                if (tokens[tokenIndex].TokenType == SyntaxTokenType.Not)
                {
                    tokenIndex++;
                    return true;
                }
                else if (tokens[tokenIndex].TokenType != SyntaxTokenType.Text || !string.IsNullOrWhiteSpace(tokens[tokenIndex].Text.Replace(Environment.NewLine, "")))
                    return false;
            }
            return false;
        }

        private static List<SyntaxToken> GetIfContent(List<SyntaxToken> tokens, ref int tokenIndex)
        {
            var items = new List<SyntaxToken>();
            var depth = 0;
            for (; tokenIndex < tokens.Count; tokenIndex++)
            {
                var type = tokens[tokenIndex].TokenType;
                if (type == SyntaxTokenType.If)
                    depth++;
                else if ((type == SyntaxTokenType.Else || type == SyntaxTokenType.ElseIf || type == SyntaxTokenType.EndIf) && depth == 0)
                    break;
                else if (type == SyntaxTokenType.EndIf)
                    depth--;

                items.Add(tokens[tokenIndex]);
            }
            return items;
        }

        private (Match Match, SyntaxTokenType TokenType)? TryMatchToken(string token)
        {
            foreach (var matcher in TokenMatchers)
            {
                var match = matcher.Regex.Match(token);
                if (match.Success)
                    return (match, matcher.TokenType);
            }
            return null;
        }

        private List<SyntaxToken> ParseTokens(int lineNumber, Span<string> tokens, string filePath)
        {
            var parsedTokens = new List<SyntaxToken>();

            if (tokens.Length > 0)
            {
                var tokenValue = tokens[0];
                var tokenDefinitionMatch = TryMatchToken(tokenValue);
                var tokenDefintion = tokenDefinitionMatch == null ? SyntaxTokenType.Text : tokenDefinitionMatch.Value.TokenType;
                var match = tokenDefinitionMatch?.Match;

                if (match != null && match.Index != 0)
                {
                    parsedTokens.Add(new SyntaxToken(SyntaxTokenType.Text, tokenValue[..match.Index], lineNumber, filePath));
                }

                parsedTokens.Add(new SyntaxToken(tokenDefintion, match != null ? tokenValue.Substring(match.Index, match.Length) : tokenValue, lineNumber, filePath));

                if (match != null && match.Index + match.Length != tokenValue.Length)
                {
                    parsedTokens.Add(new SyntaxToken(SyntaxTokenType.Text, tokenValue.Substring(match.Index + match.Length, tokenValue.Length - match.Index - match.Length), lineNumber, filePath));
                }
            }

            if (tokens.Length > 1)
            {
                parsedTokens.AddRange(ParseTokens(lineNumber, tokens[1..], filePath));
            }

            return parsedTokens;
        }

        private SyntaxIf ParseIf(List<SyntaxToken> tokens, IncludeContext includeContext, ref int tokenIndex)
        {
            tokenIndex++;

            var conditions = new List<SyntaxIf.IfCondition>();
            var not = TryGetNotToken(tokens, ref tokenIndex);
            var conditionValue = flags[tokens[tokenIndex++].Text];
            conditions.Add(new SyntaxIf.IfCondition(not ? !conditionValue : conditionValue, ParseSyntaxTree(GetIfContent(tokens, ref tokenIndex), includeContext).ToList()));

            while (tokenIndex < tokens.Count && tokens[tokenIndex].TokenType != SyntaxTokenType.EndIf)
            {
                if (tokens[tokenIndex].TokenType == SyntaxTokenType.Else)
                {
                    tokenIndex++;
                    conditions.Add(new SyntaxIf.IfCondition(true, ParseSyntaxTree(GetIfContent(tokens, ref tokenIndex), includeContext).ToList()));
                }
                else if (tokens[tokenIndex].TokenType == SyntaxTokenType.ElseIf)
                {
                    tokenIndex++;
                    not = TryGetNotToken(tokens, ref tokenIndex);
                    conditionValue = flags[tokens[tokenIndex++].Text];
                    conditions.Add(new SyntaxIf.IfCondition(not ? !conditionValue : conditionValue, ParseSyntaxTree(GetIfContent(tokens, ref tokenIndex), includeContext).ToList()));
                }
                else
                {
                    break;
                }
            }
            return new SyntaxIf(conditions);
        }

        private IEnumerable<SyntaxTreeNode> ParseSyntaxTree(List<SyntaxToken> tokens, IncludeContext includeContext)
        {
            for (var tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
            {
                if (tokens[tokenIndex].TokenType == SyntaxTokenType.If)
                {
                    yield return ParseIf(tokens, includeContext, ref tokenIndex);
                }
                else if (tokens[tokenIndex].TokenType == SyntaxTokenType.Variable)
                {
                    var text = tokens[tokenIndex].Text;
                    var variableName = text[2..^1];
                    yield return new SyntaxText((tokenIndex == 0 ? string.Empty : " ") + values[variableName]);
                }
                else if (tokens[tokenIndex].TokenType == SyntaxTokenType.Include)
                {
                    tokenIndex++;
                    if (tokens[tokenIndex].TokenType != SyntaxTokenType.Text)
                        throw new Exception($"[Line: {tokens[tokenIndex].LineNumber}, File] A file path must follow the include statement");

                    yield return new SyntaxInclude(this, includeContext, basePath: tokens[tokenIndex].FilePath, path: tokens[tokenIndex].Text);
                }
                else
                {
                    yield return new SyntaxText((tokenIndex == 0 ? string.Empty : " ") + tokens[tokenIndex].Text);
                }
            }
        }

        public string Precompile(string text, string filePath)
            => Precompile(text, filePath, new IncludeContext());

        internal string Precompile(string text, string filePath, IncludeContext includeContext)
        {
            var tokens = new List<SyntaxToken>();
            var lineNumber = 1;
            foreach (var line in text.Split(Environment.NewLine))
            {

                var textTokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                tokens.AddRange(ParseTokens(lineNumber, textTokens, filePath));
                tokens.Add(new SyntaxToken(SyntaxTokenType.Text, Environment.NewLine, lineNumber, filePath));

                lineNumber++;
            }

            var syntaxTree = ParseSyntaxTree(tokens, includeContext);
            var output = new StringBuilder();
            foreach (var node in syntaxTree)
            {
                output.Append(node.Precompile());
            }

            return output.ToString();
        }
    }
}