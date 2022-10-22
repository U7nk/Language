using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Wired.CodeAnalysis.Binding;
using Wired.CodeAnalysis.Binding.Binders;
using Wired.CodeAnalysis.Binding.Lookup;
using Wired.CodeAnalysis.Symbols;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

internal class Evaluator
{
    readonly BoundProgram _program;
    readonly Stack<Dictionary<VariableSymbol, object?>> _stacks;
    object? _lastValue;

    public Evaluator(BoundProgram program, Dictionary<VariableSymbol, object?> globalVariables)
    {
        _stacks = new Stack<Dictionary<VariableSymbol, object?>>();
        
        _stacks.Push(globalVariables);
        _stacks.Push(new Dictionary<VariableSymbol, object?>());
        _program = program;
    } 

    public object? Evaluate()
    {
        var function = _program.MainFunction ?? _program.ScriptMainFunction;
        if (function is null)
            return null;
        
        var programType = _program.Types.Single(x => x.MethodTable.ContainsKey(function));
        var body = programType.MethodTable[function].Unwrap();
        var programInstance = EvaluateObjectCreationExpression(new BoundObjectCreationExpression(programType));
        Assign(new VariableSymbol("this", programType, true), programInstance);
        return EvaluateStatement(body);
    }

    object? EvaluateStatement(BoundBlockStatement body)
    {
        var labelToIndex = new Dictionary<LabelSymbol, int>();

        foreach(var index in 0..body.Statements.Length)
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
            BoundNodeKind.MethodCallExpression =>
                EvaluateCallExpression((BoundMethodCallExpression)node),
            BoundNodeKind.ConversionExpression =>
                EvaluateConversionExpression((BoundConversionExpression)node),
            BoundNodeKind.ThisExpression =>
                EvaluateThisExpression((BoundThisExpression)node),
            BoundNodeKind.ObjectCreationExpression =>
                EvaluateObjectCreationExpression((BoundObjectCreationExpression)node),
            BoundNodeKind.FieldExpression =>
                EvaluateFieldExpression((BoundFieldExpression)node),
            BoundNodeKind.FieldAssignmentExpression =>
                EvaluateFieldAssignmentExpression((BoundFieldAssignmentExpression)node),
            _ =>
                throw new($"Unexpected node  {node.Kind}")
        };
    }

    object EvaluateFieldAssignmentExpression(BoundFieldAssignmentExpression node)
    {
        var instance = (Dictionary<string, object>?)EvaluateExpression(node.ObjectAccess);
        instance.Unwrap();
        var value = EvaluateExpression(node.Initializer).Unwrap();
        instance[node.Field.Name] = value;
        return value;
    }

    object? EvaluateFieldExpression(BoundFieldExpression node)
    {
        var thisInstance = GetThisInstance();
        var fieldInstance = thisInstance[node.Field.Name];
        return fieldInstance;
    }

    object EvaluateObjectCreationExpression(BoundObjectCreationExpression node)
    {
        return EvaluateDefaultValueObjectCreation(node.Type);
    }
    
    object EvaluateDefaultValueObjectCreation(TypeSymbol typeSymbol)
    {
        var instance = new Dictionary<string, object>();
        foreach (var field in typeSymbol.FieldTable)
        {
            instance.Add(field.Name, EvaluateDefaultValueObjectCreation(field.Type));
        }
        
        return instance;
    }

    object? EvaluateThisExpression(BoundThisExpression node)
    {
        return _stacks.Peek()[new VariableSymbol("this", node.Type, true)];
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

    object? EvaluateCallExpression(BoundMethodCallExpression node)
    {
        var nodeArguments = node.Arguments
            // skip "this" arg
            .Skip(1)
            .ToList();
        var thisArg = node.Arguments[0];
        var evaluatedArguments = node.Arguments
            // skip "this" arg
            .Skip(1)
            .Select(EvaluateExpression).ToList();
        var thisEvaluatedArg = EvaluateExpression(node.Arguments[0]);
        
        if (Equals(node.FunctionSymbol, BuiltInFunctions.Input))
        {
            return Console.ReadLine();
        }

        if (Equals(node.FunctionSymbol, BuiltInFunctions.Print))
        {
            var value = evaluatedArguments[0];
            Console.WriteLine(value);
            return null;
        }

        _stacks.Push(new Dictionary<VariableSymbol, object?>());
        foreach(var i in 0..nodeArguments.Count)
        {
            var parameter = node.FunctionSymbol.Parameters[i];
            var value = evaluatedArguments[i];
            Assign(parameter, value);
        }
        
        // add "this" arg
        Assign(new VariableSymbol("this", thisArg.Type, true), thisEvaluatedArg);
        
        var statement = thisArg.Type.MethodTable[node.FunctionSymbol].Unwrap();
        var result = EvaluateStatement(statement);
        _stacks.Pop();
        return result;
    }

    T NullCheckConvert<T>(object? obj)
    {
        if (obj is null)
            throw new Exception("Null reference exception");

        return (T)obj;
    }

    object? EvaluateMethodCallBinaryExpression(BoundBinaryExpression b)
    {
        var lValue = EvaluateExpression(b.Left);
        var methodCallExpr = (BoundMethodCallExpression)b.Right; 
        b.Left.Type.MethodTable.TryGetValue(methodCallExpr.FunctionSymbol, out var method);
        if (method is null)
            throw new Exception($"Method {methodCallExpr.FunctionSymbol.Name} not found on type {b.Left.Type.Name}");
            
            
        var arguments = methodCallExpr.Arguments
            // skip "this" arg
            .Skip(1)
            .Select(EvaluateExpression)
            .ToList();
        
        _stacks.Push(new Dictionary<VariableSymbol, object?>());
        Assign(new VariableSymbol("this", b.Left.Type, true), lValue);
        
        foreach(var i in 0..arguments.Count)
        {
            var parameter = methodCallExpr.FunctionSymbol.Parameters[i];
            var value = arguments[i];
            Assign(parameter, value);
        }
        
        var result = EvaluateStatement(method);
        _stacks.Pop();
        return result;
    }
    
    object? EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        if (b.Op.Kind == BoundBinaryOperatorKind.MethodCall)
        {
            return EvaluateMethodCallBinaryExpression(b);
        }
        
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);
        
        if (Equals(b.Left.Type, TypeSymbol.Int))
        {
            if (Equals(b.Right.Type, TypeSymbol.Int))
            {
                var intRight = NullCheckConvert<int>(right);
                var intLeft = NullCheckConvert<int>(left);
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => intLeft + intRight,
                    BoundBinaryOperatorKind.Subtraction => intLeft - intRight,
                    BoundBinaryOperatorKind.Multiplication => intLeft * intRight,
                    BoundBinaryOperatorKind.Division => intLeft / intRight,

                    BoundBinaryOperatorKind.Equality => intLeft == intRight,
                    BoundBinaryOperatorKind.Inequality => intLeft != intRight,
                    BoundBinaryOperatorKind.LessThan => intLeft < intRight,
                    BoundBinaryOperatorKind.LessThanOrEquals => intLeft <= intRight,
                    BoundBinaryOperatorKind.GreaterThan => intLeft > intRight,
                    BoundBinaryOperatorKind.GreaterThanOrEquals => intLeft >= intRight,

                    BoundBinaryOperatorKind.BitwiseAnd => intLeft & intRight,
                    BoundBinaryOperatorKind.BitwiseOr => intLeft | intRight,
                    BoundBinaryOperatorKind.BitwiseXor => intLeft ^ intRight,
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }

        if (Equals(b.Left.Type, TypeSymbol.String))
        {
            if (Equals(b.Right.Type, TypeSymbol.String))
            {
                var stringRight = NullCheckConvert<string>(right);
                var stringLeft = NullCheckConvert<string>(left);
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => stringLeft + stringRight,
                    
                    BoundBinaryOperatorKind.Equality => stringLeft == stringRight,
                    BoundBinaryOperatorKind.Inequality => stringLeft != stringRight,
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }

        if (Equals(b.Left.Type, TypeSymbol.Bool))
        {
            if (Equals(b.Right.Type, TypeSymbol.Bool))
            {
                var boolRight = NullCheckConvert<bool>(right);
                var boolLeft = NullCheckConvert<bool>(left);
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Equality => boolLeft == boolRight,
                    BoundBinaryOperatorKind.Inequality => boolLeft != boolRight,
                    
                    BoundBinaryOperatorKind.BitwiseAnd => boolLeft & boolRight,
                    BoundBinaryOperatorKind.BitwiseOr => boolLeft | boolRight,
                    BoundBinaryOperatorKind.BitwiseXor => boolLeft ^ boolRight,
                    
                    BoundBinaryOperatorKind.LogicalAnd => boolLeft && boolRight,
                    BoundBinaryOperatorKind.LogicalOr => boolLeft || boolRight,
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

    Dictionary<string, object> GetThisInstance()
    {
        var thisKey = _stacks.Peek().Keys.Single(x => x.Name == "this");
        return (Dictionary<string, object>)_stacks.Peek()[thisKey].Unwrap();
    }
}