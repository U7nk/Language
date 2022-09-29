using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

internal class Evaluator
{
    readonly BoundProgram _program;
    readonly Dictionary<FunctionSymbol, BoundBlockStatement> _functionBodies;
    readonly Stack<Dictionary<VariableSymbol, object?>> _stacks;
    object? _lastValue;

    public Evaluator(BoundProgram program,
         Dictionary<VariableSymbol, object?> globals)
    {
        _stacks = new Stack<Dictionary<VariableSymbol, object?>>();
        _functionBodies = new Dictionary<FunctionSymbol, BoundBlockStatement>();
        
        _program = program;
        
        _stacks.Push(globals);
        var current = program;
        while (current != null)
        {
            foreach (var (symbol, functionBody) in current.Functions)
            {
                _functionBodies.Add(symbol, functionBody);
            }
            current = current.Previous;
        }
    } 

    public object? Evaluate()
    {
        var function = _program.MainFunction ?? _program.ScriptMainFunction;
        if (function is null)
            return null;
        
        var body = _functionBodies[function];
        return EvaluateStatement(body);
    }

    object? EvaluateStatement(BoundBlockStatement body)
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
                    var condition = (bool)(EvaluateExpression(cgs.Condition) ?? throw new InvalidOperationException());
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
                case BoundNodeKind.ReturnStatement:
                    var rs = (BoundReturnStatement)statement;
                    var value = rs.Expression == null 
                        ? null 
                        : EvaluateExpression(rs.Expression);
                    _lastValue = value;
                    return _lastValue;
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

        if (Equals(node.Type, TypeSymbol.Bool)) 
            return Convert.ToBoolean(value);

        if (Equals(node.Type, TypeSymbol.Int))
            return Convert.ToInt32(value);

        if (Equals(node.Type, TypeSymbol.String))
            return Convert.ToString(value);
        
        if (Equals(node.Type, TypeSymbol.Any))
            return value;

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

        var statement = _functionBodies[node.FunctionSymbol];
        var result =  EvaluateStatement(statement);
        _stacks.Pop();
        return result;
    }

    T NullCheckConvert<T>(object? obj)
    {
        if (obj is null)
            throw new Exception("Null reference exception");

        return (T)obj;
    }
    
    object EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);
        if (Equals(b.Left.Type, TypeSymbol.Int))
        {
            if (Equals(b.Right.Type, TypeSymbol.Int))
            {
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => 
                        NullCheckConvert<int>(left) + NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.Subtraction => 
                        NullCheckConvert<int>(left) - NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.Multiplication => 
                        NullCheckConvert<int>(left) * NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.Division => 
                        NullCheckConvert<int>(left) / NullCheckConvert<int>(right),

                    BoundBinaryOperatorKind.Equality => 
                        NullCheckConvert<int>(left) == NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.Inequality => 
                        NullCheckConvert<int>(left) != NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.LessThan => 
                        NullCheckConvert<int>(left) < NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.LessThanOrEquals => 
                        NullCheckConvert<int>(left) <= NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.GreaterThan => 
                        NullCheckConvert<int>(left) > NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.GreaterThanOrEquals => 
                        NullCheckConvert<int>(left) >= NullCheckConvert<int>(right),

                    BoundBinaryOperatorKind.BitwiseAnd => 
                        NullCheckConvert<int>(left) & NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.BitwiseOr => 
                        NullCheckConvert<int>(left) | NullCheckConvert<int>(right),
                    BoundBinaryOperatorKind.BitwiseXor => 
                        NullCheckConvert<int>(left) ^ NullCheckConvert<int>(right),
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
                    BoundBinaryOperatorKind.Addition => NullCheckConvert<string>(left) + NullCheckConvert<string>(right),
                    
                    BoundBinaryOperatorKind.Equality => NullCheckConvert<string>(left) == NullCheckConvert<string>(right),
                    BoundBinaryOperatorKind.Inequality => NullCheckConvert<string>(left) != NullCheckConvert<string>(right),
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }

        if (Equals(b.Left.Type, TypeSymbol.Bool))
        {
            if (Equals(b.Right.Type, TypeSymbol.Bool))
            {
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Equality => NullCheckConvert<bool>(left) == NullCheckConvert<bool>(right),
                    BoundBinaryOperatorKind.Inequality => NullCheckConvert<bool>(left) != NullCheckConvert<bool>(right),
                    
                    BoundBinaryOperatorKind.BitwiseAnd => NullCheckConvert<bool>(left) & NullCheckConvert<bool>(right),
                    BoundBinaryOperatorKind.BitwiseOr => NullCheckConvert<bool>(left) | NullCheckConvert<bool>(right),
                    BoundBinaryOperatorKind.BitwiseXor => NullCheckConvert<bool>(left) ^ NullCheckConvert<bool>(right),
                    
                    BoundBinaryOperatorKind.LogicalAnd => NullCheckConvert<bool>(left) && NullCheckConvert<bool>(right),
                    BoundBinaryOperatorKind.LogicalOr => NullCheckConvert<bool>(left) || NullCheckConvert<bool>(right),
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }
        
        throw new($"Unexpected binary operator {b.Op.Kind}");
    }

    object EvaluateUnaryExpression(BoundUnaryExpression unary)
    {
        var operand = EvaluateExpression(unary.Operand);
        if (Equals(unary.Type, TypeSymbol.Int))
        {
            var intOperand = NullCheckConvert<int>(operand);
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.Negation => -intOperand,
                BoundUnaryOperatorKind.Identity => +intOperand,
                BoundUnaryOperatorKind.BitwiseNegation => ~intOperand,
                _ => throw new($"Unexpected unary operator {unary.Op}")
            };
        }

        if (Equals(unary.Type, TypeSymbol.Bool))
        {
            var boolOperand = NullCheckConvert<bool>(operand);
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