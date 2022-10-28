using System;
using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Binding.Binders;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis;

public class ObjectInstance
{
    public Dictionary<string, ObjectInstance?> Fields { get; }
    public TypeSymbol Type { get; }
    public object? LiteralValue { get; }

    public static ObjectInstance Object(TypeSymbol type, Dictionary<string, ObjectInstance?> fields) => new(type, fields);
    public static ObjectInstance Literal(TypeSymbol type, object value) => new(type, new Dictionary<string, ObjectInstance?>(), value);

    private ObjectInstance(TypeSymbol type, Dictionary<string, ObjectInstance?> fields, object? literalValue = null)
    {
        if (fields.Any() && literalValue is { })
        {
            throw new ArgumentException("Cannot have both fields and literal value");
        }

        LiteralValue = literalValue;
        Type = type;
        Fields = fields;
    }
}

class Evaluator
{
    readonly BoundProgram _program;
    readonly Stack<Dictionary<VariableSymbol, ObjectInstance?>> _stacks;
    ObjectInstance? _lastValue;

    public Evaluator(BoundProgram program, Dictionary<VariableSymbol, ObjectInstance?> globalVariables)
    {
        _stacks = new Stack<Dictionary<VariableSymbol, ObjectInstance?>>();
        
        _stacks.Push(globalVariables);
        _stacks.Push(new Dictionary<VariableSymbol, ObjectInstance?>());
        _program = program;
    } 

    public ObjectInstance? Evaluate()
    {
        var function = _program.MainMethod ?? _program.ScriptMainMethod;
        if (function is null)
            return null;
        
        var programType = _program.Types.Single(x => x.MethodTable.ContainsKey(function));
        var body = programType.MethodTable[function].Unwrap();
        var programInstance = EvaluateObjectCreationExpression(new BoundObjectCreationExpression(programType));
        Assign(new VariableSymbol("this", programType, true), programInstance);
        return EvaluateStatement(body);
    }

    ObjectInstance? EvaluateStatement(BoundBlockStatement body)
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
                {
                    var declarationStatement = (BoundVariableDeclarationStatement)statement;
                    EvaluateVariableDeclarationStatement(declarationStatement);
                    i++;
                    break;
                }
                case BoundNodeKind.VariableDeclarationAssignmentStatement:
                {
                    var declarationAssignmentStatement = (BoundVariableDeclarationAssignmentStatement)statement;
                    EvaluateVariableDeclarationAssignmentStatement(declarationAssignmentStatement);
                    i++;
                    break;
                }
                case BoundNodeKind.ConditionalGotoStatement:
                    var cgs = (BoundConditionalGotoStatement)statement;
                    var condition = (bool)(EvaluateExpression(cgs.Condition).Unwrap().LiteralValue ?? throw new InvalidOperationException());
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

    void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement assignmentStatement)
    {
        var variable = assignmentStatement.Variable;
        var value = EvaluateDefaultValueObjectCreation(variable.Type);
        Assign(variable, value);
    }
    
    void EvaluateVariableDeclarationAssignmentStatement(BoundVariableDeclarationAssignmentStatement assignmentStatement)
    {
        var value = EvaluateExpression(assignmentStatement.Initializer);
        Assign(assignmentStatement.Variable, value);
    }

    void EvaluateExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        _lastValue = EvaluateExpression(expressionStatement.Expression);
    }

    public ObjectInstance? EvaluateExpression(BoundExpression node)
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
            BoundNodeKind.MethodCallExpression =>
                EvaluateMethodCallExpression((BoundMethodCallExpression)node,
                    _stacks.Peek().Single(x => x.Key.Name == "this").Value.Unwrap().Type,
                    _stacks.Peek().Single(x => x.Key.Name == "this").Value),
            BoundNodeKind.FieldAccessExpression =>
                EvaluateFieldAccessExpression((BoundFieldAccessExpression)node),
            _ => /* default */
                throw new($"Unexpected node  {node.Kind}")
        };
    }

    ObjectInstance? EvaluateFieldAccessExpression(BoundFieldAccessExpression node)
    {
        var instance = _stacks.Peek().Single(x => x.Key.Name == "this").Value;
        if (instance is null)
            throw new InvalidOperationException("Cannot access field on null instance");
        return instance.Fields[node.FieldSymbol.Name];
    }
    ObjectInstance? EvaluateMemberAssignmentExpression(BoundMemberAssignmentExpression node)
    {
        // should return object.
        // objects is represented as Dictionary<string, object>
        
        if (node.MemberAccess.Kind is BoundNodeKind.FieldAccessExpression)
        {
            var instance = _stacks.Peek().Single(x => x.Key.Name == "this").Value.Unwrap();
            var value = EvaluateExpression(node.RightValue).Unwrap();
        
            var member = node.MemberAccess.Unwrap<BoundFieldAccessExpression>();
            return instance.Fields[member.FieldSymbol.Name] = value;
        }
        else if (node.MemberAccess.Kind is BoundNodeKind.VariableExpression)
        {
            var variable = node.MemberAccess.Unwrap<BoundVariableExpression>().Variable;
            var rightValue = EvaluateExpression(node.RightValue).Unwrap();
            return Assign(variable, rightValue);
        }
        else
        {
            var memberAccess = (BoundMemberAccessExpression)node.MemberAccess;
            var instance = EvaluateExpression(memberAccess.Left).Unwrap();
            var value = EvaluateExpression(node.RightValue).Unwrap();

            var member = memberAccess.Member.Unwrap<BoundFieldAccessExpression>();

            return instance.Fields[member.FieldSymbol.Name] = value;
        }
    }

    ObjectInstance? EvaluateMethodCallExpression(BoundMethodCallExpression methodCallExpression, TypeSymbol type, ObjectInstance? typeInstance)
    {
        typeInstance.Unwrap();
        
        var parameters = methodCallExpression.MethodSymbol.Parameters;
        var locals = _stacks.Peek();
        foreach (var i in 0..methodCallExpression.Arguments.Length)
        {
            locals.Add(parameters[i], EvaluateExpression(methodCallExpression.Arguments[i]));
        }
        
        _stacks.Push(new Dictionary<VariableSymbol, ObjectInstance?>());
        Assign(new VariableSymbol("this", type, true), typeInstance);
        var methodBody = type.MethodTable[methodCallExpression.MethodSymbol].Unwrap();
        var result = EvaluateStatement(methodBody);
        _stacks.Pop();
        return result;
    }
    
    ObjectInstance? EvaluateMemberAccessExpression(BoundMemberAccessExpression node)
    {
        // should return object.
        // objects is represented as Dictionary<string, object>
        var leftValue = EvaluateExpression(node.Left).Unwrap();

        if (node.Member is BoundMethodCallExpression methodCall)
            return EvaluateMethodCallExpression(methodCall, node.Left.Type, leftValue);
        
        if (node.Member is BoundFieldAccessExpression fieldAccess)
        {
            var field = fieldAccess.FieldSymbol;
            return leftValue.Fields[field.Name];
        }
        
        throw new Exception("Unexpected member access");
    }

    ObjectInstance EvaluateObjectCreationExpression(BoundObjectCreationExpression node)
    {
        return EvaluateDefaultValueObjectCreation(node.Type);
    }
    
    ObjectInstance EvaluateDefaultValueObjectCreation(TypeSymbol typeSymbol)
    {
        var fieldsValues = new Dictionary<string, ObjectInstance?>();
        foreach (var field in typeSymbol.FieldTable)
        {
            //                   BUG: we should use Rust like Option<T> instead of null
            //                        compiler should check that all fields are initialized
            fieldsValues.Add(field.Name, null!);
        }
        
        var instance = ObjectInstance.Object(typeSymbol, fieldsValues);
        return instance;
    }

    ObjectInstance? EvaluateThisExpression(BoundThisExpression node)
    {
        return _stacks.Peek()[new VariableSymbol("this", node.Type, true)];
    }

    ObjectInstance EvaluateConversionExpression(BoundConversionExpression node)
    {
        var value = EvaluateExpression(node.Expression).Unwrap();

        if (Equals(node.Type, TypeSymbol.Bool)) 
            return ObjectInstance.Literal(TypeSymbol.Bool, Convert.ToBoolean(value));

        if (Equals(node.Type, TypeSymbol.Int))
            return ObjectInstance.Literal(TypeSymbol.Int, Convert.ToInt32(value));

        if (Equals(node.Type, TypeSymbol.String))
            return ObjectInstance.Literal(TypeSymbol.String, Convert.ToString(value).Unwrap());
        
        if (Equals(node.Type, TypeSymbol.Any))
            return value;

        throw new Exception($"Unexpected type {node.Type}");
        
    }

    ObjectInstance EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);
        
        if (Equals(b.Left.Type, TypeSymbol.Int))
        {
            if (Equals(b.Right.Type, TypeSymbol.Int))
            {
                var intRight = right.Unwrap().LiteralValue.Unwrap<int>();
                var intLeft = left.Unwrap().LiteralValue.Unwrap<int>();
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => ObjectInstance.Literal(TypeSymbol.Int, intLeft + intRight),
                    BoundBinaryOperatorKind.Subtraction => ObjectInstance.Literal(TypeSymbol.Int, intLeft - intRight),
                    BoundBinaryOperatorKind.Multiplication => ObjectInstance.Literal(TypeSymbol.Int, intLeft * intRight),
                    BoundBinaryOperatorKind.Division => ObjectInstance.Literal(TypeSymbol.Int, intLeft / intRight),

                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(TypeSymbol.Int, intLeft == intRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(TypeSymbol.Int, intLeft != intRight),
                    BoundBinaryOperatorKind.LessThan => ObjectInstance.Literal(TypeSymbol.Int, intLeft < intRight),
                    BoundBinaryOperatorKind.LessThanOrEquals => ObjectInstance.Literal(TypeSymbol.Int, intLeft <= intRight),
                    BoundBinaryOperatorKind.GreaterThan => ObjectInstance.Literal(TypeSymbol.Int, intLeft > intRight),
                    BoundBinaryOperatorKind.GreaterThanOrEquals => ObjectInstance.Literal(TypeSymbol.Int, intLeft >= intRight),

                    BoundBinaryOperatorKind.BitwiseAnd => ObjectInstance.Literal(TypeSymbol.Int, intLeft & intRight),
                    BoundBinaryOperatorKind.BitwiseOr => ObjectInstance.Literal(TypeSymbol.Int, intLeft | intRight),
                    BoundBinaryOperatorKind.BitwiseXor => ObjectInstance.Literal(TypeSymbol.Int, intLeft ^ intRight),
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }

        if (Equals(b.Left.Type, TypeSymbol.String))
        {
            if (Equals(b.Right.Type, TypeSymbol.String))
            {
                var stringRight = right.Unwrap().LiteralValue.Unwrap<string>();
                var stringLeft = left.Unwrap().LiteralValue.Unwrap<string>();
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => ObjectInstance.Literal(TypeSymbol.String, stringLeft + stringRight),
                    
                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(TypeSymbol.String, stringLeft == stringRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(TypeSymbol.String, stringLeft != stringRight),
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }

        if (Equals(b.Left.Type, TypeSymbol.Bool))
        {
            if (Equals(b.Right.Type, TypeSymbol.Bool))
            {
                var boolRight = right.Unwrap().LiteralValue.Unwrap<bool>();
                var boolLeft = left.Unwrap().LiteralValue.Unwrap<bool>();
                return b.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(TypeSymbol.String, boolLeft == boolRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(TypeSymbol.String, boolLeft != boolRight),
                    
                    BoundBinaryOperatorKind.BitwiseAnd => ObjectInstance.Literal(TypeSymbol.String, boolLeft & boolRight),
                    BoundBinaryOperatorKind.BitwiseOr => ObjectInstance.Literal(TypeSymbol.String, boolLeft | boolRight),
                    BoundBinaryOperatorKind.BitwiseXor => ObjectInstance.Literal(TypeSymbol.String, boolLeft ^ boolRight),
                    
                    BoundBinaryOperatorKind.LogicalAnd => ObjectInstance.Literal(TypeSymbol.String, boolLeft && boolRight),
                    BoundBinaryOperatorKind.LogicalOr => ObjectInstance.Literal(TypeSymbol.String, boolLeft || boolRight),
                    _ => throw new($"Unexpected binary operator {b.Op.Kind}")
                };
            }
        }
        
        throw new($"Unexpected binary operator {b.Op.Kind}");
    }

    ObjectInstance EvaluateUnaryExpression(BoundUnaryExpression unary)
    {
        var operand = EvaluateExpression(unary.Operand).Unwrap();
        if (Equals(unary.Type, TypeSymbol.Int))
        {
            var intOperand = operand.LiteralValue.Unwrap<int>();
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.Negation => ObjectInstance.Literal(TypeSymbol.Int,  -intOperand),
                BoundUnaryOperatorKind.Identity => ObjectInstance.Literal(TypeSymbol.Int,  +intOperand),
                BoundUnaryOperatorKind.BitwiseNegation => ObjectInstance.Literal(TypeSymbol.Int,  ~intOperand),
                _ => throw new($"Unexpected unary operator {unary.Op}")
            };
        }

        if (Equals(unary.Type, TypeSymbol.Bool))
        {
            var boolOperand = operand.LiteralValue.Unwrap<bool>();
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.LogicalNegation => ObjectInstance.Literal(TypeSymbol.Bool, !boolOperand),
                _ => throw new($"Unexpected unary operator {unary.Op}")
            };
        }

        throw new Exception($"Unexpected unary operator {unary.Op}");
    }

    ObjectInstance? EvaluateAssignmentExpression(BoundAssignmentExpression a)
    {
        var value = EvaluateExpression(a.Expression);
        Assign(a.Variable, value);
        return value;
    }
    
    ObjectInstance? EvaluateVariableExpression(BoundVariableExpression v)
    {
        return _stacks.Peek()[v.Variable];
    }

    ObjectInstance EvaluateLiteralExpression(BoundLiteralExpression l) =>
        ObjectInstance.Literal(l.Type, l.Value.Unwrap());

    ObjectInstance? Assign(VariableSymbol variableSymbol, ObjectInstance? value)
    { 
        var currentStack = _stacks.Peek();
        return currentStack[variableSymbol] = value;
    }
}