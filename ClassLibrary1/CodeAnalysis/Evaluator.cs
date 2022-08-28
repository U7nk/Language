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
                return unary.Op.Kind switch
                {
                    BoundUnaryOperatorKind.Negation => -intOperand,
                    BoundUnaryOperatorKind.Identity => +intOperand,
                    _ => throw new Exception($"Unexpected unary operator {unary.Op}")
                };
            }

            if (unary.Type == typeof(bool))
            {
                var boolOperand = (bool)operand;
                return unary.Op.Kind switch
                {
                    BoundUnaryOperatorKind.LogicalNegation => !boolOperand,
                    _ => throw new Exception($"Unexpected unary operator {unary.Op}")
                };
            }

            throw new Exception($"Unexpected unary operator {unary.Op}");
        }

        if (root is BoundBinaryExpression b)
        {
            var left = this.EvaluateExpression(b.Left);
            var right = this.EvaluateExpression(b.Right);
            return b.Op.Kind switch
            {
                BoundBinaryOperatorKind.Addition => (int)left + (int)right,
                BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
                BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
                BoundBinaryOperatorKind.Division => (int)left / (int)right,
                
                BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
                
                BoundBinaryOperatorKind.Equality => Equals(left, right),
                BoundBinaryOperatorKind.Inequality => !Equals(left, right),
                _ => throw new Exception($"Unknown binary operator {b.Op.Kind}")
            };
        }

        throw new Exception($"Unexpected node  {root.Kind}");
    }
}