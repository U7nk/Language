using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    public LiteralExpressionSyntax(SyntaxToken literalToken) : this(literalToken, literalToken.Value)
    {
    }

    public LiteralExpressionSyntax(SyntaxToken literalToken, object? value)
    {
        this.LiteralToken = literalToken;
        this.Value = value;
    }

    public SyntaxToken LiteralToken { get; }
    public object? Value { get; }

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    
}