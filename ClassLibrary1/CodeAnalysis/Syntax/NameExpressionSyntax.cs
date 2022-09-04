using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public sealed class NameExpressionSyntax : ExpressionSyntax
{
    public NameExpressionSyntax(SyntaxToken identifierToken)
    {
        this.IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind => SyntaxKind.NameExpression;
    public SyntaxToken IdentifierToken { get; }
}