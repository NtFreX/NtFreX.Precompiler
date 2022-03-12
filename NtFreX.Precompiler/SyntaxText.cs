namespace NtFreX.Precompiler;

internal class SyntaxText : SyntaxTreeNode
{
    private readonly string text;

    public SyntaxText(string text)
    {
        this.text = text;
    }

    public override string Precompile()
        => text;

    public override string ToString()
        => "{ Text: " + text + " }";
}
