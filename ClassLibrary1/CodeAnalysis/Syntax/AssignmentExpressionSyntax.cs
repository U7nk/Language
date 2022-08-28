using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
    public AssignmentExpressionSyntax(SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression)
    {
        this.IdentifierToken = identifierToken;
        this.EqualsToken = equalsToken;
        this.Expression = expression;
    }

    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Expression { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return this.IdentifierToken;
        yield return this.EqualsToken;
        yield return this.Expression;
    }
}