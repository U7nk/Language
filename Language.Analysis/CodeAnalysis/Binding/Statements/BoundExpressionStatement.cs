using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundExpressionStatement : BoundStatement
{
    internal BoundExpression Expression { get; }
    internal override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

    internal BoundExpressionStatement(SyntaxNode? syntax, BoundExpression expression) : base(syntax)
    {
        Expression = expression;
    }
}
