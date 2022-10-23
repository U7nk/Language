using System;
using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis;

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
            BoundNodeKind.ConversionExpression =>
                EvaluateConversionExpression((BoundConversionExpression)node),
            BoundNodeKind.ThisExpression =>
                EvaluateThisExpression((BoundThisExpression)node),
            BoundNodeKind.ObjectCreationExpression =>
                EvaluateObjectCreationExpression((BoundObjectCreationExpression)node),
            BoundNodeKind.MemberAccessExpression =>
                EvaluateMemberAccessExpression((BoundMemberAccessExpression)node),
            BoundNodeKind.MemberAssignmentExpression =>
                EvaluateMemberAssignmentExpression((BoundMemberAssignmentExpression)node),
            _ => /* default */
                throw new($"Unexpected node  {node.Kind}")
        };
    }

    object EvaluateMemberAssignmentExpression(BoundMemberAssignmentExpression node)
    {
        // should return object.
        // objects is represented as Dictionary<string, object>
        var instance = EvaluateExpression(node.MemberAccess.Left)
            .Unwrap<Dictionary<string, object>>();
        var value = EvaluateExpression(node.RightValue).Unwrap();
        
        var member = node.MemberAccess.Member.Unwrap<BoundFieldAccessExpression>();
        return instance[member.FieldSymbol.Name] = value;
    }
    
    object? EvaluateMemberAccessExpression(BoundMemberAccessExpression node)
    {
        // should return object.
        // objects is represented as Dictionary<string, object>
        var leftValue = EvaluateExpression(node.Left)
            .Unwrap<Dictionary<string, object>>();
        if (node.Member is BoundMethodCallExpression methodCall)
        {
            var parameters = methodCall.FunctionSymbol.Parameters;
            _stacks.Push(new Dictionary<VariableSymbol, object?>());
            var locals = _stacks.Peek();
            foreach (var i in 0..methodCall.Arguments.Length)
            {
                locals.Add(parameters[i], EvaluateExpression(methodCall.Arguments[i]));
            }
            Assign(new VariableSymbol("this", node.Left.Type, true), leftValue);
            var methodBody = node.Left.Type.MethodTable[methodCall.FunctionSymbol].Unwrap();
            var result = EvaluateStatement(methodBody);
            _stacks.Pop();
            return result;
        }
        if (node.Member is BoundFieldAccessExpression fieldAccess)
        {
            var field = fieldAccess.FieldSymbol;
            return leftValue[field.Name];
        }
        
        throw new Exception("Unexpected member access");
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
            //                   BUG: we should use Rust like Option<T> instead of null
            //                        compiler should check that all fields are initialized
            instance.Add(field.Name, null!);
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

    object? EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);
        
        if (Equals(b.Left.Type, TypeSymbol.Int))
        {
            if (Equals(b.Right.Type, TypeSymbol.Int))
            {
                var intRight = right.Unwrap<int>();
                var intLeft = left.Unwrap<int>();
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
                var stringRight = right.Unwrap<string>();
                var stringLeft = left.Unwrap<string>();
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
                var boolRight = right.Unwrap<bool>();
                var boolLeft = left.Unwrap<bool>();
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
            var intOperand = operand.Unwrap<int>();
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
            var boolOperand = operand.Unwrap<bool>();
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