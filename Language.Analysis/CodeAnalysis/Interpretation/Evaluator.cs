using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Interpretation;

class Evaluator
{
    readonly BoundProgram _program;
    readonly Stack<Dictionary<VariableSymbol, ObjectInstance?>> _stacks;
    readonly Dictionary<TypeSymbol, TypeStaticInstance> _types;
    ObjectInstance? _lastValue;

    public Evaluator(BoundProgram program, Dictionary<VariableSymbol, ObjectInstance?> globalVariables)
    {
        _stacks = new Stack<Dictionary<VariableSymbol, ObjectInstance?>>();
        _stacks.Push(globalVariables);
        _stacks.Push(new Dictionary<VariableSymbol, ObjectInstance?>());
        _program = program;
        _types = new Dictionary<TypeSymbol, TypeStaticInstance>();
    } 

    public ObjectInstance? Evaluate()
    {
        var function = _program.MainMethod ?? _program.ScriptMainMethod;

        if (function is null)
            return null;

        if (!function.IsStatic)
            throw new Exception("Main method must be static.");

        var programType = _program.Types.Single(x => x.MethodTable.ContainsKey(function));
        var programTypeStatic = new TypeStaticInstance(CreateFieldsFromTable(programType.FieldTable),
                                                       programType);
        _types.Add(programType, programTypeStatic);
        
        var body = programType.MethodTable[function].NullGuard();
        return EvaluateStatement(body);
    }

    Dictionary<string, ObjectInstance?> CreateFieldsFromTable(FieldTable table)
    {
        var result = new Dictionary<string, ObjectInstance?>();
        foreach (var field in table)
        {
            result.Add(field.Name, EvaluateDefaultValueObjectCreation(field.Type));
        }
        return result;
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
                    var condition = (bool)(EvaluateExpression(cgs.Condition).As<ObjectInstance>().LiteralValue 
                                           ?? throw new InvalidOperationException());
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
                    _lastValue = value.As<ObjectInstance?>();
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
        _lastValue = Assign(variable, value);
    }
    
    void EvaluateVariableDeclarationAssignmentStatement(BoundVariableDeclarationAssignmentStatement assignmentStatement)
    {
        var value = EvaluateExpression(assignmentStatement.Initializer).As<ObjectInstance>();
        _lastValue = Assign(assignmentStatement.Variable, value);
    }

    void EvaluateExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        _lastValue = EvaluateExpression(expressionStatement.Expression).As<ObjectInstance?>();
    }

    public RuntimeObject? EvaluateExpression(BoundExpression node)
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
                    _stacks.Peek().SingleOrDefault(x => x.Key.Name == "this").Value?.Type,
                    _stacks.Peek().SingleOrDefault(x => x.Key.Name == "this").Value),
            BoundNodeKind.FieldExpression =>
                EvaluateFieldAccessExpression((BoundFieldExpression)node),
            BoundNodeKind.NamedTypeExpression =>
                EvaluateNamedTypeExpression((BoundNamedTypeExpression)node),
            _ => /* default */
                throw new($"Unexpected node  {node.Kind}")
        };
    }

    TypeStaticInstance EvaluateNamedTypeExpression(BoundNamedTypeExpression node)
    {
        if (_types.TryGetValue(node.Type, out var typeStaticInstance))
            return typeStaticInstance;
        
        var type = node.Type;
        var fields = new Dictionary<string, ObjectInstance?>();
        foreach (var field in type.FieldTable.Where(x=> x.IsStatic))
        {
            var value = EvaluateDefaultValueObjectCreation(field.Type);
            fields.Add(field.Name, value);
        }
        var typeStatic = new TypeStaticInstance(fields, type);
        _types.Add(typeStatic.Type, typeStatic);
        return typeStatic;
    }

    ObjectInstance? EvaluateFieldAccessExpression(BoundFieldExpression node)
    {
        RuntimeObject instance;
        if (node.FieldSymbol.IsStatic)
        {
            instance = GetTypeStaticInstance(node.FieldSymbol.ContainingType.NullGuard());
        }
        else
        {
            instance = _stacks.Peek().Single(x => x.Key.Name == "this").Value.NullGuard();
        }
        
        return instance.Fields[node.FieldSymbol.Name];
    }
    RuntimeObject? EvaluateMemberAssignmentExpression(BoundMemberAssignmentExpression node)
    {
        // should return object.
        // objects is represented as Dictionary<string, object>
        
        if (node.MemberAccess.Kind is BoundNodeKind.FieldExpression)
        {
            var fieldExpression = (BoundFieldExpression)node.MemberAccess;
            RuntimeObject instance;
            if (fieldExpression.FieldSymbol.IsStatic)
            {
                instance = GetTypeStaticInstance(fieldExpression.FieldSymbol.ContainingType.NullGuard());
            }
            else
            {
                instance = _stacks.Peek().Single(x => x.Key.Name == "this").Value.NullGuard();    
            }
            
            
            var value = EvaluateExpression(node.RightValue).As<ObjectInstance>();
        
            var member = node.MemberAccess.As<BoundFieldExpression>();
            return _lastValue = instance.Fields[member.FieldSymbol.Name] = value;
        }
        else if (node.MemberAccess.Kind is BoundNodeKind.VariableExpression)
        {
            var variable = node.MemberAccess.As<BoundVariableExpression>().Variable;
            var rightValue = EvaluateExpression(node.RightValue).As<ObjectInstance>();
            return _lastValue = Assign(variable, rightValue);
        }
        else
        {
            var memberAccess = (BoundMemberAccessExpression)node.MemberAccess;
            var instance = EvaluateExpression(memberAccess.Left).NullGuard();
            var value = EvaluateExpression(node.RightValue).As<ObjectInstance>();

            var member = memberAccess.Member.As<BoundFieldExpression>();

            return _lastValue = instance.Fields[member.FieldSymbol.Name] = value;
        }
    }

    TypeStaticInstance GetTypeStaticInstance(TypeSymbol fieldSymbolType)
    {
        if (_types.TryGetValue(fieldSymbolType, out var typeStaticInstance))
            return typeStaticInstance;
        
        
        var fields = CreateFieldsFromTable(fieldSymbolType.FieldTable);
        var typeStatic = new TypeStaticInstance(fields, fieldSymbolType);
        _types.Add(typeStatic.Type, typeStatic);
        return typeStatic;
    }

    ObjectInstance? EvaluateMethodCallExpression(BoundMethodCallExpression methodCallExpression, TypeSymbol? type, RuntimeObject? typeInstance)
    {
        if (!methodCallExpression.MethodSymbol.IsStatic)
        {
            var message = $"Method {methodCallExpression.MethodSymbol.Name} is not static";
            type.NullGuard(message + " and this expression should not be null");
            typeInstance.NullGuard(message + " and this expression should not be null");
        }
        else
        {
            type = methodCallExpression.MethodSymbol.ContainingType.NullGuard();
            typeInstance = GetTypeStaticInstance(type);
        }

        var parameters = methodCallExpression.MethodSymbol.Parameters;
        var locals = _stacks.Peek();
        foreach (var i in 0..methodCallExpression.Arguments.Length)
        {
            locals.Add(parameters[i], EvaluateExpression(methodCallExpression.Arguments[i]).As<ObjectInstance>());
        }
        
        _stacks.Push(new Dictionary<VariableSymbol, ObjectInstance?>());
        if (typeInstance is ObjectInstance objectInstance)
        {
            Assign(new VariableSymbol(Option.None, "this",
                                      containingType: null,
                                      type, isReadonly: true),
                   objectInstance);
        }

        var methodBody = type.LookupMethodBody(methodCallExpression.MethodSymbol);
        var result = EvaluateStatement(methodBody);
        _stacks.Pop();
        return result;
    }
    
    ObjectInstance? EvaluateMemberAccessExpression(BoundMemberAccessExpression node)
    {
        // should return object.
        // objects is represented as Dictionary<string, object>
        var leftValue = EvaluateExpression(node.Left).NullGuard();

        if (node.Member is BoundMethodCallExpression methodCall)
            return EvaluateMethodCallExpression(methodCall, node.Left.Type, leftValue);
        
        if (node.Member is BoundFieldExpression fieldAccess)
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
        return _stacks.Peek()[new VariableSymbol(
            node.Syntax,
            name: "this",
            type: node.Type,
            isReadonly: true, 
            containingType: null)];
    }

    ObjectInstance EvaluateConversionExpression(BoundConversionExpression node)
    {
        var value = EvaluateExpression(node.Expression).As<ObjectInstance>();

        if (Equals(node.Type, BuiltInTypeSymbols.Bool)) 
            return ObjectInstance.Literal(BuiltInTypeSymbols.Bool, Convert.ToBoolean(value));

        if (Equals(node.Type, BuiltInTypeSymbols.Int))
            return ObjectInstance.Literal(BuiltInTypeSymbols.Int, Convert.ToInt32(value));

        if (Equals(node.Type, BuiltInTypeSymbols.String))
            return ObjectInstance.Literal(BuiltInTypeSymbols.String, Convert.ToString(value).NullGuard());
        
        if (Equals(node.Type, BuiltInTypeSymbols.Object))
            return value;

        if (node.Expression.Type.IsSubClassOf(node.Type))
            return value;

        throw new Exception($"Unexpected type {node.Type}");
        
    }

    ObjectInstance EvaluateBinaryExpression(BoundBinaryExpression binary)
    {
        var left = EvaluateExpression(binary.Left).As<ObjectInstance>();
        var right = EvaluateExpression(binary.Right).As<ObjectInstance>();
        
        if (Equals(binary.Left.Type, BuiltInTypeSymbols.Int))
        {
            if (Equals(binary.Right.Type, BuiltInTypeSymbols.Int))
            {
                var intRight = right.LiteralValue.As<int>();
                var intLeft = left.LiteralValue.As<int>();
                return binary.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft + intRight),
                    BoundBinaryOperatorKind.Subtraction => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft - intRight),
                    BoundBinaryOperatorKind.Multiplication => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft * intRight),
                    BoundBinaryOperatorKind.Division => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft / intRight),

                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft == intRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft != intRight),
                    BoundBinaryOperatorKind.LessThan => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft < intRight),
                    BoundBinaryOperatorKind.LessThanOrEquals => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft <= intRight),
                    BoundBinaryOperatorKind.GreaterThan => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft > intRight),
                    BoundBinaryOperatorKind.GreaterThanOrEquals => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft >= intRight),

                    BoundBinaryOperatorKind.BitwiseAnd => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft & intRight),
                    BoundBinaryOperatorKind.BitwiseOr => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft | intRight),
                    BoundBinaryOperatorKind.BitwiseXor => ObjectInstance.Literal(BuiltInTypeSymbols.Int, intLeft ^ intRight),
                    _ => throw new($"Unexpected binary operator {binary.Op.Kind}")
                };
            }
        }

        if (Equals(binary.Left.Type, BuiltInTypeSymbols.String))
        {
            if (Equals(binary.Right.Type, BuiltInTypeSymbols.String))
            {
                right.NullGuard();
                left.NullGuard();
                var stringRight = right.LiteralValue.As<string>();
                var stringLeft = left.LiteralValue.As<string>();
                return binary.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => ObjectInstance.Literal(BuiltInTypeSymbols.String, stringLeft + stringRight),
                    
                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(BuiltInTypeSymbols.String, stringLeft == stringRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(BuiltInTypeSymbols.String, stringLeft != stringRight),
                    _ => throw new($"Unexpected binary operator {binary.Op.Kind}")
                };
            }
        }

        if (Equals(binary.Left.Type, BuiltInTypeSymbols.Bool))
        {
            if (Equals(binary.Right.Type, BuiltInTypeSymbols.Bool))
            {
                var boolRight = right.LiteralValue.As<bool>();
                var boolLeft = left.LiteralValue.As<bool>();
                return binary.Op.Kind switch
                {
                    // TODO: this should return bool literal, not string
                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(BuiltInTypeSymbols.String, boolLeft == boolRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(BuiltInTypeSymbols.String, boolLeft != boolRight),
                    
                    BoundBinaryOperatorKind.BitwiseAnd => ObjectInstance.Literal(BuiltInTypeSymbols.String, boolLeft & boolRight), //-V3093
                    BoundBinaryOperatorKind.BitwiseOr => ObjectInstance.Literal(BuiltInTypeSymbols.String, boolLeft | boolRight), //-V3093
                    BoundBinaryOperatorKind.BitwiseXor => ObjectInstance.Literal(BuiltInTypeSymbols.String, boolLeft ^ boolRight),
                    
                    BoundBinaryOperatorKind.LogicalAnd => ObjectInstance.Literal(BuiltInTypeSymbols.String, boolLeft && boolRight),
                    BoundBinaryOperatorKind.LogicalOr => ObjectInstance.Literal(BuiltInTypeSymbols.String, boolLeft || boolRight),
                    _ => throw new($"Unexpected binary operator {binary.Op.Kind}")
                };
            }
        }
        
        // not built in types
        if (binary.Left.Type.IsOutside(BuiltInTypeSymbols.All).OrEquals(BuiltInTypeSymbols.Object) 
            && binary.Right.Type.IsOutside(BuiltInTypeSymbols.All).OrEquals(BuiltInTypeSymbols.Object))
        {
            if (binary.Op.Kind is BoundBinaryOperatorKind.Equality)
                return ObjectInstance.Literal(BuiltInTypeSymbols.Bool, ReferenceEquals(left, right));

            if (binary.Op.Kind is BoundBinaryOperatorKind.Inequality)
                return ObjectInstance.Literal(BuiltInTypeSymbols.Bool, !ReferenceEquals(left, right));
        }

        throw new($"Unexpected binary operator {binary.Op.Kind}");
    }

    ObjectInstance EvaluateUnaryExpression(BoundUnaryExpression unary)
    {
        var operand = EvaluateExpression(unary.Operand).As<ObjectInstance>();
        if (Equals(unary.Type, BuiltInTypeSymbols.Int))
        {
            var intOperand = operand.LiteralValue.As<int>();
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.Negation => ObjectInstance.Literal(BuiltInTypeSymbols.Int,  -intOperand),
                BoundUnaryOperatorKind.Identity => ObjectInstance.Literal(BuiltInTypeSymbols.Int,  +intOperand),
                BoundUnaryOperatorKind.BitwiseNegation => ObjectInstance.Literal(BuiltInTypeSymbols.Int,  ~intOperand),
                _ => throw new($"Unexpected unary operator {unary.Op}")
            };
        }

        if (Equals(unary.Type, BuiltInTypeSymbols.Bool))
        {
            var boolOperand = operand.LiteralValue.As<bool>();
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.LogicalNegation => ObjectInstance.Literal(BuiltInTypeSymbols.Bool, !boolOperand),
                _ => throw new($"Unexpected unary operator {unary.Op}")
            };
        }

        throw new Exception($"Unexpected unary operator {unary.Op}");
    }

    ObjectInstance? EvaluateAssignmentExpression(BoundAssignmentExpression a)
    {
        var value = EvaluateExpression(a.Expression).As<ObjectInstance>();
        return _lastValue = Assign(a.Variable, value);
    }
    
    ObjectInstance? EvaluateVariableExpression(BoundVariableExpression v)
    {
        return _stacks.Peek()[v.Variable];
    }

    ObjectInstance EvaluateLiteralExpression(BoundLiteralExpression l) =>
        ObjectInstance.Literal(l.Type, l.Value.NullGuard());

    ObjectInstance? Assign(VariableSymbol variableSymbol, ObjectInstance? value)
    { 
        var currentStack = _stacks.Peek();
        return currentStack[variableSymbol] = value;
    }
}