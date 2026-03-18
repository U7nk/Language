using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundConditionalGotoStatement(Option<SyntaxNode> syntax, LabelSymbol onTrueLabel, LabelSymbol onFalseLabel, BoundExpression condition) : base(syntax)
    {
        OnTrueLabel = onTrueLabel;
        OnFalseLabel = onFalseLabel;
        Condition = condition;
    }
    
    
    public BoundExpression Condition { get; }
    public LabelSymbol OnTrueLabel { get; }
    public LabelSymbol OnFalseLabel { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
}