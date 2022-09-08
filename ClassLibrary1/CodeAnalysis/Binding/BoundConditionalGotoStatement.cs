namespace Wired.CodeAnalysis.Binding;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfTrue)
    {
        this.Label = label;
        this.Condition = condition;
        this.JumpIfTrue = jumpIfTrue;
    }

    public bool JumpIfTrue { get; }
    public BoundExpression Condition { get; }
    public LabelSymbol Label { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
}