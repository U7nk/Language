using System;
using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.Extensions;

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
        var function = _program.MainMethod.Unwrap();

        if (!function.IsStatic)
            throw new Exception("Main method must be static.");

        var programType = _program.Types.Single(x => x.MethodTable.SingleOrNone(x => x.MethodSymbol.Equals(function)).IsSome);
        var programTypeStatic = new TypeStaticInstance(CreateFieldsFromTable(programType.FieldTable),
                                                       programType);
        _types.Add(programType, programTypeStatic);
        
        var body = programType.MethodTable.Single(x=> x.MethodSymbol.Equals(function)).Body.Unwrap();
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
                EvaluateMethodCallExpression((BoundMethodCallExpression)node, _stacks.Peek().SingleOrDefault(x => x.Key.Name == "this").Value),
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
            instance = GetTypeStaticInstance(node.FieldSymbol.ContainingType.Unwrap());
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
                instance = GetTypeStaticInstance(fieldExpression.FieldSymbol.ContainingType.Unwrap());
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

    ObjectInstance? EvaluateMethodCallExpression(BoundMethodCallExpression methodCallExpression, RuntimeObject? typeInstance)
    {
        if (!methodCallExpression.MethodSymbol.IsStatic)
        {
            var message = $"Method {methodCallExpression.MethodSymbol.Name} is not static";
            typeInstance.NullGuard(message + " and this expression should not be null");
        }
        else
        {
            var type = methodCallExpression.MethodSymbol.ContainingType.Unwrap();
            typeInstance = GetTypeStaticInstance(type);
        }

        
        var parameters = methodCallExpression.MethodSymbol.Parameters;
        var newLocals = new Dictionary<VariableSymbol, ObjectInstance?>();
        foreach (var i in 0..methodCallExpression.Arguments.Length)
        {
            newLocals.Add(parameters[i], EvaluateExpression(methodCallExpression.Arguments[i]).As<ObjectInstance>());
        }
        
        _stacks.Push(newLocals);
        BoundBlockStatement methodBody;
        ObjectInstance? result;
        if (typeInstance is ObjectInstance objectInstance)
        {
            var variableSymbol = new VariableSymbol(
                declarationSyntax: Option.None,
                name: "this",
                objectInstance.Type,
                isReadonly: true);
            Assign(variableSymbol, objectInstance);
            methodBody = objectInstance.Type.LookupMethodBody(methodCallExpression.MethodSymbol);
            result = EvaluateStatement(methodBody);
        }
        else
        {
            methodBody = methodCallExpression.MethodSymbol.ContainingType.Unwrap()
                .LookupMethodBody(methodCallExpression.MethodSymbol);
            result = EvaluateStatement(methodBody);
        }

        _stacks.Pop();
        return result;
    }
    
    ObjectInstance? EvaluateMemberAccessExpression(BoundMemberAccessExpression node)
    {
        // should return object.
        // objects is represented as Dictionary<string, object>
        var leftValue = EvaluateExpression(node.Left).NullGuard();

        if (node.Member is BoundMethodCallExpression methodCall)
            return EvaluateMethodCallExpression(methodCall, leftValue);
        
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
            isReadonly: true)];
    }

    ObjectInstance EvaluateConversionExpression(BoundConversionExpression node)
    {
        var value = EvaluateExpression(node.Expression).As<ObjectInstance>();

        if (Equals(node.Type, TypeSymbol.BuiltIn.Bool())) 
            return ObjectInstance.Literal(TypeSymbol.BuiltIn.Bool(), Convert.ToBoolean(value));

        if (Equals(node.Type, TypeSymbol.BuiltIn.Int()))
            return ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), Convert.ToInt32(value));

        if (Equals(node.Type, TypeSymbol.BuiltIn.String()))
            return ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), Convert.ToString(value).NullGuard());
        
        if (Equals(node.Type, TypeSymbol.BuiltIn.Object()))
            return value;

        if (node.Expression.Type.IsSubClassOf(node.Type))
            return value;

        throw new Exception($"Unexpected type {node.Type}");
        
    }

    ObjectInstance EvaluateBinaryExpression(BoundBinaryExpression binary)
    {
        var left = EvaluateExpression(binary.Left).As<ObjectInstance>();
        var right = EvaluateExpression(binary.Right).As<ObjectInstance>();
        
        if (Equals(binary.Left.Type, TypeSymbol.BuiltIn.Int()))
        {
            if (Equals(binary.Right.Type, TypeSymbol.BuiltIn.Int()))
            {
                var intRight = right.LiteralValue.As<int>();
                var intLeft = left.LiteralValue.As<int>();
                return binary.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft + intRight),
                    BoundBinaryOperatorKind.Subtraction => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft - intRight),
                    BoundBinaryOperatorKind.Multiplication => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft * intRight),
                    BoundBinaryOperatorKind.Division => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft / intRight),

                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft == intRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft != intRight),
                    BoundBinaryOperatorKind.LessThan => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft < intRight),
                    BoundBinaryOperatorKind.LessThanOrEquals => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft <= intRight),
                    BoundBinaryOperatorKind.GreaterThan => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft > intRight),
                    BoundBinaryOperatorKind.GreaterThanOrEquals => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft >= intRight),

                    BoundBinaryOperatorKind.BitwiseAnd => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft & intRight),
                    BoundBinaryOperatorKind.BitwiseOr => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft | intRight),
                    BoundBinaryOperatorKind.BitwiseXor => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(), intLeft ^ intRight),
                    _ => throw new($"Unexpected binary operator {binary.Op.Kind}")
                };
            }
        }

        if (Equals(binary.Left.Type, TypeSymbol.BuiltIn.String()))
        {
            if (Equals(binary.Right.Type, TypeSymbol.BuiltIn.String()))
            {
                right.NullGuard();
                left.NullGuard();
                var stringRight = right.LiteralValue.As<string>();
                var stringLeft = left.LiteralValue.As<string>();
                return binary.Op.Kind switch
                {
                    BoundBinaryOperatorKind.Addition => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), stringLeft + stringRight),
                    
                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), stringLeft == stringRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), stringLeft != stringRight),
                    _ => throw new($"Unexpected binary operator {binary.Op.Kind}")
                };
            }
        }

        if (Equals(binary.Left.Type, TypeSymbol.BuiltIn.Bool()))
        {
            if (Equals(binary.Right.Type, TypeSymbol.BuiltIn.Bool()))
            {
                var boolRight = right.LiteralValue.As<bool>();
                var boolLeft = left.LiteralValue.As<bool>();
                return binary.Op.Kind switch
                {
                    // TODO: this should return bool literal, not string
                    BoundBinaryOperatorKind.Equality => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), boolLeft == boolRight),
                    BoundBinaryOperatorKind.Inequality => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), boolLeft != boolRight),
                    
                    BoundBinaryOperatorKind.BitwiseAnd => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), boolLeft & boolRight), //-V3093
                    BoundBinaryOperatorKind.BitwiseOr => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), boolLeft | boolRight), //-V3093
                    BoundBinaryOperatorKind.BitwiseXor => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), boolLeft ^ boolRight),
                    
                    BoundBinaryOperatorKind.LogicalAnd => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), boolLeft && boolRight),
                    BoundBinaryOperatorKind.LogicalOr => ObjectInstance.Literal(TypeSymbol.BuiltIn.String(), boolLeft || boolRight),
                    _ => throw new($"Unexpected binary operator {binary.Op.Kind}")
                };
            }
        }
        
        // not built in types
        if (binary.Left.Type.IsOutside(TypeSymbol.BuiltIn.All).OrEquals(TypeSymbol.BuiltIn.Object()) 
            && binary.Right.Type.IsOutside(TypeSymbol.BuiltIn.All).OrEquals(TypeSymbol.BuiltIn.Object()))
        {
            if (binary.Op.Kind is BoundBinaryOperatorKind.Equality)
                return ObjectInstance.Literal(TypeSymbol.BuiltIn.Bool(), ReferenceEquals(left, right));

            if (binary.Op.Kind is BoundBinaryOperatorKind.Inequality)
                return ObjectInstance.Literal(TypeSymbol.BuiltIn.Bool(), !ReferenceEquals(left, right));
        }

        throw new($"Unexpected binary operator {binary.Op.Kind}");
    }

    ObjectInstance EvaluateUnaryExpression(BoundUnaryExpression unary)
    {
        var operand = EvaluateExpression(unary.Operand).As<ObjectInstance>();
        if (Equals(unary.Type, TypeSymbol.BuiltIn.Int()))
        {
            var intOperand = operand.LiteralValue.As<int>();
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.Negation => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(),  -intOperand),
                BoundUnaryOperatorKind.Identity => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(),  +intOperand),
                BoundUnaryOperatorKind.BitwiseNegation => ObjectInstance.Literal(TypeSymbol.BuiltIn.Int(),  ~intOperand),
                _ => throw new($"Unexpected unary operator {unary.Op}")
            };
        }

        if (Equals(unary.Type, TypeSymbol.BuiltIn.Bool()))
        {
            var boolOperand = operand.LiteralValue.As<bool>();
            return unary.Op.Kind switch
            {
                BoundUnaryOperatorKind.LogicalNegation => ObjectInstance.Literal(TypeSymbol.BuiltIn.Bool(), !boolOperand),
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