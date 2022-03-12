namespace NtFreX.Precompiler;

internal record SyntaxToken(SyntaxTokenType TokenType, string Text, int LineNumber, string FilePath);
