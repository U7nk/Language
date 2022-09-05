using System;
using System.Collections.Generic;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

internal class Evaluator
{
    private readonly BoundStatement root;
    private readonly Dictionary<VariableSymbol, object> variables;
    private object lastValue;

    public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
    {
        this.root = root;
        this.variables = variables;
    }

    public object Evaluate()
    {
        this.EvaluateStatement(this.root);
        return this.lastValue;
    }

    public void EvaluateStatement(BoundStatement statement)
    {
        switch (statement.Kind)
        {
            case BoundNodeKind.BlockStatement:
                this.EvaluateBlockStatement((BoundBlockStatement)statement);
                break;
            case BoundNodeKind.ExpressionStatement:
                this.EvaluateExpressionStatement((BoundExpressionStatement)statement);
                break;
            default:
                throw new Exception($"Unexpected node  {statement.Kind}");
        }
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        this.lastValue = this.EvaluateExpression(expressionStatement.Expression);
    }

    private void EvaluateBlockStatement(BoundBlockStatement blockStatement)
    {
        foreach (var statement in blockStatement.Statements) 
            this.EvaluateStatement(statement);
    }

    public object EvaluateExpression(BoundExpression node)
    {
        return node.Kind switch
        {
            BoundNodeKind.LiteralExpression =>
                this.EvaluateLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.AssignmentExpression =>
                this.EvaluateAssignmentExpression((BoundAssignmentExpression)node),
            BoundNodeKind.VariableExpression =>
                this.EvaluateVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.UnaryExpression =>
                this.EvaluateUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression =>
                this.EvaluateBinaryExpression((BoundBinaryExpression)node),
            _ =>
                throw new Exception($"Unexpected node  {node.Kind}")
        };
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression b)
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

    private object EvaluateUnaryExpression(BoundUnaryExpression unary)
    {
        var operand = this.EvaluateExpression(unary.Operand);
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

    private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
    {
        var value = this.EvaluateExpression(a.Expression);
        this.variables[a.Variable] = value;
        return value;
    }

    private object EvaluateVariableExpression(BoundVariableExpression v)
    {
        return this.variables[v.Variable];
    }

    private object EvaluateLiteralExpression(BoundLiteralExpression l)
    {
        return l.Value;
    }
}