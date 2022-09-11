using System;
using System.Collections.Generic;
using System.Linq;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

internal class Evaluator
{
    private readonly BoundBlockStatement root;
    private readonly Dictionary<VariableSymbol, object> variables;
    private object? lastValue;

    public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
    {
        this.root = root;
        this.variables = variables;
    }

    public object Evaluate()
    {
        var labelToIndex = new Dictionary<LabelSymbol, int>();

        for (var index = 0; index < this.root.Statements.Length; index++)
        {
            var statement = this.root.Statements[index];
            if (statement is BoundLabelStatement l) 
                labelToIndex.Add(l.Label, index);
        }

        var i = 0;
        while (i < this.root.Statements.Length)
        {
            var statement = this.root.Statements[i];
            switch (statement.Kind)
            {
                case BoundNodeKind.ExpressionStatement:
                    this.EvaluateExpressionStatement((BoundExpressionStatement)statement);
                    i++;
                    break;
                case BoundNodeKind.VariableDeclarationStatement:
                    var gs = (BoundVariableDeclarationStatement)statement;
                    this.EvaluateVariableDeclarationStatement(gs);
                    i++;
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    var cgs = (BoundConditionalGotoStatement)statement;
                    var condition = (bool)this.EvaluateExpression(cgs.Condition);
                    if (condition == cgs.JumpIfTrue)
                        i = labelToIndex[cgs.Label];
                    else
                        i++;
                    break;
                case BoundNodeKind.GotoStatement:
                    i = labelToIndex[((BoundGotoStatement)statement).Label];
                    break;
                case BoundNodeKind.LabelStatement:
                    i++;
                    break;
                default:
                    throw new Exception($"Unexpected node  {statement.Kind}");
            }
        }
        
        return this.lastValue;
    }

    private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement statement)
        => this.variables[statement.Variable] = this.EvaluateExpression(statement.Initializer);

    private void EvaluateExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        this.lastValue = this.EvaluateExpression(expressionStatement.Expression);
    }

    public object? EvaluateExpression(BoundExpression node)
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
            BoundNodeKind.CallExpression =>
                this.EvaluateCallExpression((BoundCallExpression)node),
            _ =>
                throw new($"Unexpected node  {node.Kind}")
        };
    }

    private object? EvaluateCallExpression(BoundCallExpression node)
    {
        var arguments = node.Arguments.Select(this.EvaluateExpression).ToList();
        if (node.FunctionSymbol == BuiltInFunctions.Input)
        {
            return Console.ReadLine();
        }

        if (node.FunctionSymbol == BuiltInFunctions.Print)
        {
            var value = arguments[0];
            Console.WriteLine(value);
            return null;
        }
        throw new($"Unexpected function {node.FunctionSymbol.Name}");
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = this.EvaluateExpression(b.Left);
        var right = this.EvaluateExpression(b.Right);
        if (b.Left.Type == TypeSymbol.Int)
        {
            if (b.Right.Type == TypeSymbol.Int)
            {
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => (int)left + (int)right,
                    BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
                    BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
                    BoundBinaryOperatorKind.Division => (int)left / (int)right,

                    BoundBinaryOperatorKind.Equality => (int)left == (int)right,
                    BoundBinaryOperatorKind.Inequality => (int)left != (int)right,
                    BoundBinaryOperatorKind.LessThan => (int)left < (int)right,
                    BoundBinaryOperatorKind.LessThanOrEquals => (int)left <= (int)right,
                    BoundBinaryOperatorKind.GreaterThan => (int)left > (int)right,
                    BoundBinaryOperatorKind.GreaterThanOrEquals => (int)left >= (int)right,

                    BoundBinaryOperatorKind.BitwiseAnd => (int)left & (int)right,
                    BoundBinaryOperatorKind.BitwiseOr => (int)left | (int)right,
                    BoundBinaryOperatorKind.BitwiseXor => (int)left ^ (int)right,
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }

        if (b.Left.Type == TypeSymbol.String)
        {
            if (b.Right.Type == TypeSymbol.String)
            {
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => (string)left + (string)right,
                    
                    BoundBinaryOperatorKind.Equality => (string)left == (string)right,
                    BoundBinaryOperatorKind.Inequality => (string)left != (string)right,
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }

        if (b.Left.Type == TypeSymbol.Bool)
        {
            if (b.Right.Type == TypeSymbol.Bool)
            {
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Equality => (bool)left == (bool)right,
                    BoundBinaryOperatorKind.Inequality => (bool)left != (bool)right,
                    
                    BoundBinaryOperatorKind.BitwiseAnd => (bool)left & (bool)right,
                    BoundBinaryOperatorKind.BitwiseOr => (bool)left | (bool)right,
                    BoundBinaryOperatorKind.BitwiseXor => (bool)left ^ (bool)right,
                    
                    BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                    BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }
        
        throw new($"Unexpected binary operator {b.Op.Kind}");
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression unary)
    {
        var operand = this.EvaluateExpression(unary.Operand);
        if (unary.Type == TypeSymbol.Int)
        {
            var intOperand = (int)operand;
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.Negation => -intOperand,
                BoundUnaryOperatorKind.Identity => +intOperand,
                BoundUnaryOperatorKind.BitwiseNegation => ~intOperand,
                _ => throw new($"Unexpected unary operator {unary.Op}")
            };
        }

        if (unary.Type == TypeSymbol.Bool)
        {
            var boolOperand = (bool)operand;
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.LogicalNegation => !boolOperand,
                _ => throw new($"Unexpected unary operator {unary.Op}")
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