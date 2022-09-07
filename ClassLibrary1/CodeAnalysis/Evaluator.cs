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
            case BoundNodeKind.VariableDeclarationStatement:
                this.EvaluateVariableDeclarationStatement((BoundVariableDeclarationStatement)statement);
                break;
            case BoundNodeKind.IfStatement:
                this.EvaluateIfStatement((BoundIfStatement)statement);
                break;
            case BoundNodeKind.WhileStatement:
                this.EvaluateWhileStatement((BoundWhileStatement)statement);
                break;
            case BoundNodeKind.ForStatement:
                this.EvaluateForStatement((BoundForStatement)statement);
                break;
            default:
                throw new Exception($"Unexpected node  {statement.Kind}");
        }
    }

    private void EvaluateForStatement(BoundForStatement statement)
    {
        if (statement.VariableDeclaration is not null)
            this.EvaluateVariableDeclarationStatement(statement.VariableDeclaration);
        else
            this.EvaluateExpression(statement.Expression.ThrowIfNull());

        while (true)
        {
            var condition = (bool)this.EvaluateExpression(statement.Condition);
            if (!condition)
                break;

            this.EvaluateStatement(statement.Body);
            this.EvaluateExpression(statement.Mutation);
        }
    }
    

    private void EvaluateWhileStatement(BoundWhileStatement statement)
    {
        while (true)
        {
            if (this.EvaluateExpression(statement.Condition) is false)
            {
                break;
            }

            this.EvaluateStatement(statement.Body);
        }
    }

    private void EvaluateIfStatement(BoundIfStatement statement)
    {
        var conditionResult = this.EvaluateExpression(statement.Condition);
        if (conditionResult is true)
        {
            this.EvaluateStatement(statement.ThenStatement);
            return;
        }

        if (statement.ElseStatement is not null)
        {
            this.EvaluateStatement(statement.ElseStatement);
            return;
        }
    }

    private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement statement)
        => this.variables[statement.Variable] = this.EvaluateExpression(statement.Initializer);

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

            BoundBinaryOperatorKind.LessThan => (int)left < (int)right,
            BoundBinaryOperatorKind.LessThanOrEquals => (int)left <= (int)right,
            BoundBinaryOperatorKind.GreaterThan => (int)left > (int)right,
            BoundBinaryOperatorKind.GreaterThanOrEquals => (int)left >= (int)right,

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