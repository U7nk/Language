using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

internal class Evaluator
{
    readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies;
    readonly BoundBlockStatement _root;
    readonly Stack<Dictionary<VariableSymbol, object?>> _stacks;
    object? _lastValue;

    public Evaluator(
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies,
        BoundBlockStatement root, Dictionary<VariableSymbol, object?> globals)
    {
        _functionBodies = functionBodies;
        _root = root;
        _stacks = new Stack<Dictionary<VariableSymbol, object?>>();
        _stacks.Push(globals);
    }

    public object Evaluate() 
        => EvaluateStatement(_root);

    object EvaluateStatement(BoundBlockStatement body)
    {
        var labelToIndex = new Dictionary<LabelSymbol, int>();

        for (var index = 0; index < body.Statements.Length; index++)
        {
            var statement = body.Statements[index];
            if (statement is BoundLabelStatement l)
                labelToIndex.Add(l.Label, index);
        }

        var i = 0;
        while (i < body.Statements.Length)
        {
            var statement = body.Statements[i];
            switch (statement.Kind)
            {
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)statement);
                    i++;
                    break;
                case BoundNodeKind.VariableDeclarationStatement:
                    var gs = (BoundVariableDeclarationStatement)statement;
                    EvaluateVariableDeclarationStatement(gs);
                    i++;
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    var cgs = (BoundConditionalGotoStatement)statement;
                    var condition = (bool)EvaluateExpression(cgs.Condition);
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

        return _lastValue;
    }

    void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement statement)
    {
        var value = EvaluateExpression(statement.Initializer);
        Assign(statement.Variable, value);
    }

    void EvaluateExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        _lastValue = EvaluateExpression(expressionStatement.Expression);
    }

    public object? EvaluateExpression(BoundExpression node)
    {
        return node.Kind switch
        {
            BoundNodeKind.LiteralExpression =>
                EvaluateLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.AssignmentExpression =>
                EvaluateAssignmentExpression((BoundAssignmentExpression)node),
            BoundNodeKind.VariableExpression =>
                EvaluateVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.UnaryExpression =>
                EvaluateUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression =>
                EvaluateBinaryExpression((BoundBinaryExpression)node),
            BoundNodeKind.CallExpression =>
                EvaluateCallExpression((BoundCallExpression)node),
            BoundNodeKind.ConversionExpression =>
                EvaluateConversionExpression((BoundConversionExpression)node),
            _ =>
                throw new($"Unexpected node  {node.Kind}")
        };
    }

    object? EvaluateConversionExpression(BoundConversionExpression node)
    {
        var value = EvaluateExpression(node.Expression);
        if (node.Type == TypeSymbol.Bool) 
            return Convert.ToBoolean(value);

        if (node.Type == TypeSymbol.Int)
            return Convert.ToInt32(value);

        if (node.Type == TypeSymbol.String)
            return Convert.ToString(value);
        
        throw new Exception($"Unexpected type {node.Type}");
        
    }

    object? EvaluateCallExpression(BoundCallExpression node)
    {
        var arguments = node.Arguments.Select(EvaluateExpression).ToList();
        
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

        _stacks.Push(new Dictionary<VariableSymbol, object?>());
        for (int i = 0; i < node.Arguments.Length; i++)
        {
            var parameter = node.FunctionSymbol.Parameters[i];
            var value = arguments[i];
            Assign(parameter, value);
        }

        var result =  EvaluateStatement(_functionBodies[node.FunctionSymbol]);
        _stacks.Pop();
        return result;
    }

    object EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);
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

    object EvaluateUnaryExpression(BoundUnaryExpression unary)
    {
        var operand = EvaluateExpression(unary.Operand);
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

    object? EvaluateAssignmentExpression(BoundAssignmentExpression a)
    {
        var value = EvaluateExpression(a.Expression);
        Assign(a.Variable, value);
        return value;
    }
    
    object? EvaluateVariableExpression(BoundVariableExpression v)
    {
        return _stacks.Peek()[v.Variable];
    }

    object? EvaluateLiteralExpression(BoundLiteralExpression l)
    {
        return l.Value;
    }
    
    void Assign(VariableSymbol variableSymbol, object? value)
    { 
        var currentStack = _stacks.Peek();
        currentStack[variableSymbol] = value;
    }
}