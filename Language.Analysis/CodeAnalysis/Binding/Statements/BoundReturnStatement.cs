using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundReturnStatement : BoundStatement
{
    public BoundReturnStatement(Option<SyntaxNode> syntax, Option<BoundExpression> expression) : base(syntax)
    {
        Expression = expression;
    }
    
    internal override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
    public Option<BoundExpression> Expression { get; }
}