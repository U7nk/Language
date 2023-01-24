using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundReturnStatement : BoundStatement
{
    public BoundReturnStatement(Option<SyntaxNode> syntax, BoundExpression? expression) : base(syntax)
    {
        Expression = expression;
    }
    
    internal override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
    public BoundExpression? Expression { get; }
}