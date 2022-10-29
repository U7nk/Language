using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundExpressionStatement : BoundStatement
{
    public BoundExpression Expression { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

    public BoundExpressionStatement(SyntaxNode? syntax, BoundExpression expression) : base(syntax)
    {
        Expression = expression;
    }
}