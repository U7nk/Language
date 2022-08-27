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
            var operand = EvaluateExpression(unary.Operand);
            if (unary.Type == typeof(int))
            {
                var intOperand = (int)operand;
                return unary.OperatorKind switch
                {
                    BoundUnaryOperatorKind.Negation => -intOperand,
                    BoundUnaryOperatorKind.Identity => +intOperand,
                    _ => throw new Exception($"Unexpected unary operator {unary.OperatorKind}")
                };
            }

            if (unary.Type == typeof(bool))
            {
                var boolOperand = (bool)operand;
                return unary.OperatorKind switch
                {
                    BoundUnaryOperatorKind.LogicalNegation => !boolOperand,
                    _ => throw new Exception($"Unexpected unary operator {unary.OperatorKind}")
                };
            }
            throw new Exception($"Unexpected unary operator {unary.OperatorKind}");
        }
        if (root is BoundBinaryExpression b)
        {
            var left = this.EvaluateExpression(b.Left);
            var right = this.EvaluateExpression(b.Right);
            if (b.Type == typeof(int))
            {
                var intLeft = (int)left;
                var intRight = (int)right;
                return b.OperatorKind switch
                {
                    BoundBinaryOperatorKind.Addition => intLeft + intRight,
                    BoundBinaryOperatorKind.Subtraction => intLeft - intRight,
                    BoundBinaryOperatorKind.Multiplication => intLeft * intRight,
                    BoundBinaryOperatorKind.Division => intLeft / intRight,
                    _ => throw new Exception($"Unknown binary operator {b.OperatorKind}")
                };
            }

            if (b.Type == typeof(bool))
            {
                var boolLeft = (bool)left;
                var boolRight = (bool)right;
                return b.OperatorKind switch
                {
                    BoundBinaryOperatorKind.LogicalAnd => boolLeft && boolRight,
                    BoundBinaryOperatorKind.LogicalOr => boolLeft || boolRight,
                    // BoundBinaryOperatorKind.Equality => boolLeft == boolRight,
                    // BoundBinaryOperatorKind.Inequality => boolLeft != boolRight,
                    // BoundBinaryOperatorKind.GreaterThan => boolLeft > boolRight,
                    // BoundBinaryOperatorKind.LessThan => boolLeft < boolRight,
                    // BoundBinaryOperatorKind.GreaterThanOrEqual => boolLeft >= boolRight,
                    // BoundBinaryOperatorKind.LessThanOrEqual => boolLeft <= boolRight,
                    _ => throw new Exception($"Unknown binary operator {b.OperatorKind}")
                };
            }
            throw new Exception($"Unknown binary operator {b.OperatorKind}");
        }

        throw new Exception($"Unexpected node  {root.Kind}");
    }
}