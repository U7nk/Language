using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfTrue)
    {
        Label = label;
        Condition = condition;
        JumpIfTrue = jumpIfTrue;
    }

    public bool JumpIfTrue { get; }
    public BoundExpression Condition { get; }
    public LabelSymbol Label { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
}