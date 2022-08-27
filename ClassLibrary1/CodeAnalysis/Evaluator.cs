using System;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

internal class Evaluator
{
    private readonly BoundExpression root;

    public Evaluator(BoundExpression root)
    {
        this.root = root;
    }

    public object Evaluate()
    {
        return this.EvaluateExpression(this.root);
    }
    private object EvaluateExpression(BoundExpression root)
    {
        if (root is BoundLiteralExpression l)
        {
            return l.Value;
        }

        if (root is BoundUnaryExpression unary)
        {
            var operand = (int)EvaluateExpression(unary.Operand);
            return unary.OperatorKind switch
            {
                BoundUnaryOperatorKind.Negation => -operand,
                BoundUnaryOperatorKind.Identity => +operand,
                _ => throw new Exception($"Unexpected unary operator {unary.OperatorKind}")
            };
        }
        if (root is BoundBinaryExpression b)
        {
            var left = (int)this.EvaluateExpression(b.Left);
            var right = (int)this.EvaluateExpression(b.Right);
            return b.OperatorKind switch
            {
                BoundBinaryOperatorKind.Addition => left + right,
                BoundBinaryOperatorKind.Subtraction => left - right,
                BoundBinaryOperatorKind.Multiplication => left * right,
                BoundBinaryOperatorKind.Division => left / right,
                _ => throw new Exception($"Unknown binary operator {b.OperatorKind}")
            };
        }

        throw new Exception($"Unexpected node  {root.Kind}");
    }
}