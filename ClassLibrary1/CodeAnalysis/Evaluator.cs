using System;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

public class Evaluator
{
    private ExpressionSyntax root;

    public Evaluator(ExpressionSyntax root)
    {
        this.root = root;
    }

    public int Evaluate()
    {
        return this.EvaluateExpression(this.root);
    }
    private int EvaluateExpression(ExpressionSyntax root)
    {
        if (root is LiteralExpressionSyntax l)
        {
            return (int)l.LiteralToken.Value.ThrowIfNull();
        }

        if (root is UnaryExpressionSyntax unary)
        {
            var operand = EvaluateExpression(unary.Operand);
            return unary.OperatorToken.Kind switch
            {
                SyntaxKind.MinusToken => -operand,
                SyntaxKind.PlusToken => +operand,
                _ => throw new Exception($"Unexpected unary operator {unary.OperatorToken.Kind}")
            };
        }
        if (root is BinaryExpressionSyntax b)
        {
            var left = this.EvaluateExpression(b.Left);
            var right = this.EvaluateExpression(b.Right);
            return b.OperatorToken.Kind.ThrowIfNull() switch
            {
                SyntaxKind.PlusToken => left + right,
                SyntaxKind.MinusToken => left - right,
                SyntaxKind.StarToken => left * right,
                SyntaxKind.SlashToken => left / right,
                _ => throw new Exception($"Unknown binary operator {b.OperatorToken.Kind}")
            };
        }
        if (root is ParenthesizedExpressionSyntax p)
        {
            return this.EvaluateExpression(p.Expression);
        }
        
        throw new Exception($"Unexpected node  {root.Kind}");
    }
}