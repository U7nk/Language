using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

internal class Evaluator
{
    private readonly BoundExpression root;
    private readonly  Dictionary<VariableSymbol, object> variables;

    public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables)
    {
        this.root = root;
        this.variables = variables;
    }

    public object Evaluate()
    {
        return this.EvaluateExpression(this.root);
    }

    private object EvaluateExpression(BoundExpression node)
    {
        if (node is BoundLiteralExpression l)
        {
            return l.Value;
        }

        if (node is BoundAssignmentExpression a)
        {
            var value = this.EvaluateExpression(a.Expression);
            this.variables[a.Variable] = value;
            return value;
        }
        
        if (node is BoundVariableExpression v)
        {
            return this.variables[v.Variable];
        }
        
        if (node is BoundUnaryExpression unary)
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

        if (node is BoundBinaryExpression b)
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

        throw new Exception($"Unexpected node  {node.Kind}");
    }
}