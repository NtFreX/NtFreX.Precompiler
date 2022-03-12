using System.Text;

namespace NtFreX.Precompiler;

internal class SyntaxIf : SyntaxTreeNode
{
    internal record IfCondition(bool ConditionValue, List<SyntaxTreeNode> Children);

    public List<IfCondition> conditions;

    public SyntaxIf(List<IfCondition> conditions)
    {
        this.conditions = conditions;
    }

    public override string Precompile()
    {
        var firstTrueCondition = conditions.FirstOrDefault(x => x.ConditionValue);
        if (firstTrueCondition == null)
            return string.Empty;

        var text = new StringBuilder();
        foreach (var content in firstTrueCondition.Children)
        {
            text.Append(content.Precompile());
        }
        return text.ToString();
    }

    public override string ToString()
        => "{ If: " + string.Join(", ", conditions) + " }";
}