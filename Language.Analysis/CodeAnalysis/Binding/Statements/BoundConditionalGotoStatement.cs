using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundConditionalGotoStatement(Option<SyntaxNode> syntax, LabelSymbol label, BoundExpression condition, bool jumpIfTrue) : base(syntax)
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