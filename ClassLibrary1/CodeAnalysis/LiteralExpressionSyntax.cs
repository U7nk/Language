using System.Collections.Generic;

namespace Wired.CodeAnalysis;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    public LiteralExpressionSyntax(SyntaxToken literalToken)
    {
        this.LiteralToken = literalToken;
    }

    public SyntaxToken LiteralToken { get; }

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    public override IEnumerable<SyntaxToken> GetChildren()
    {
        yield return this.LiteralToken;
    }
}