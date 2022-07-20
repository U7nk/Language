using System.Collections.Generic;

namespace Wired.CodeAnalysis;

public sealed class NumberExpressionSyntax : ExpressionSyntax
{
    public NumberExpressionSyntax(SyntaxToken numberToken)
    {
        this.NumberToken = numberToken;
    }

    public SyntaxToken NumberToken { get; }

    public override SyntaxKind Kind => SyntaxKind.NumberExpression;
    public override IEnumerable<SyntaxToken> GetChildren()
    {
        yield return this.NumberToken;
    }
}