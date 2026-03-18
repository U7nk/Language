using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Emit;


public class LLVMIRBlock
{
    public List<TextPiece> Contents = new List<TextPiece>();
    public void AddLine(string str, bool indent = true, int indx = -1)
    {
        if (indent)
        {
            str = "    " + str;
        }
        if (indx == -1)
            Contents.Add(new TextPiece(str + "\n"));
        else
            Contents.Insert(indx, new TextPiece(str + "\n"));
    }
    
    public void AddLine(TextPiece textPiece, bool indent = true, int indx = -1)
    {
        if (indent)
        {
            textPiece = new TextPiece("    ") + textPiece;
        }
        
        if (indx == -1)
            Contents.Add(textPiece + "\n");
        else
            Contents.Insert(indx, textPiece + "\n");
    }
    
    
    public void Add(string str)
    {
        Contents.Add(new TextPiece(str));
    }

    public void Add(TextPiece textPiece)
    {
        Contents.Add(textPiece);
    }
}
public class TextPiece
{
    public TextPiece(string text, bool isVariable = false)
    {
        Text = text;
        IsVariable = isVariable;
    }

    public TextPiece(List<TextPiece> textPieces)
    {
        TextPieces = textPieces;
    }

    public Option<List<TextPiece>> TextPieces { get; set; }

    public string Text { get; set; } = string.Empty;
    public bool IsVariable { get; set; }
        
    public override string ToString()
    {
            
        if (TextPieces.IsSome)
        {
            (Text == string.Empty).EnsureTrue();
            return string.Join("", TextPieces.Unwrap().Select(x => x.ToString()));
        }
            
        return Text;
    }
        
    public static TextPiece operator+(TextPiece a, string b) 
    {
        return new TextPiece([a, new TextPiece(b)]);
    }
    public static TextPiece operator+(TextPiece a, TextPiece b) 
    {
        return new TextPiece([a, b]);
    }
}

public class MethodEmitter
{
    
    private MethodDeclaration _methodDeclaration;
    private MethodSymbol _methodSymbol => _methodDeclaration.MethodSymbol;
    private Dictionary<VariableSymbol, TextPiece> _parameters = new();
    private Dictionary<VariableSymbol, TextPiece> _localVariables = new();
    private Dictionary<BoundStatement, (TextPiece TextPiece, VariableSymbol VariableSymbol)> _variablesDeclarations = new();

    private Option<LLVMIRBlock> _entryBlock;
    private List<TextPiece> _stringConstants = [ ];
    private LLVMIRBlock _currentBlock;
    private readonly List<LLVMIRBlock> _blocks = [ ];
    
    /// <summary>
    /// %tmp1 = phi i32 [ %tmp2, %block1 ], [ %tmp3, %block2 ] - filled phi function <br/>
    /// %tmp2 = phi i32 - unfilled phi function <br/>
    /// we will fill it at the end
    /// </summary>
    private List<(BoundPhiFunctionExpression PhiNode, TextPiece TextPiece)> _unfilledPhiFunctions = new();
    
    
    public MethodEmitter(MethodDeclaration methodDeclaration)
    {
        _methodDeclaration = methodDeclaration;
        _cfg = ControlFlowGraph.Create(_methodDeclaration.Body.Unwrap());
        _cfg.TransformToSSA();
    }

    private int _nextNumberCounter = 0;
    private readonly ControlFlowGraph _cfg;

    private string NextNumber()
    {
        return (++_nextNumberCounter).ToString();
    }

    /// <summary>
    /// Emitting necessary code for expression evaluation and returns local variable name that contains resulting value 
    /// </summary>
    /// <param name="boundExpression"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    TextPiece EmitExpression(BoundExpression boundExpression)
    {
        switch (boundExpression.Kind)
        {
            case BoundNodeKind.MethodCallExpression:
                return EmitCallExpression((BoundMethodCallExpression)boundExpression);
            case BoundNodeKind.LiteralExpression:
                return EmitLiteralExpression((BoundLiteralExpression)boundExpression);
            case BoundNodeKind.BinaryExpression:
                return EmitBinaryExpression((BoundBinaryExpression)boundExpression);
            case BoundNodeKind.VariableExpression:
                return EmitVariableExpression((BoundVariableExpression)boundExpression);
            case BoundNodeKind.ConversionExpression:
                return EmitConversionExpression((BoundConversionExpression)boundExpression);
            case BoundNodeKind.AssignmentExpression:
                return EmitAssignmentExpression((BoundAssignmentExpression)boundExpression);
            case BoundNodeKind.MemberAccessExpression:
                return EmitMemberAccessExpression(boundExpression.As<BoundMemberAccessExpression>());
            case BoundNodeKind.PhiFunctionExpression:
                return EmitPhiFunctionExpression(boundExpression.As<BoundPhiFunctionExpression>());
            case BoundNodeKind.ObjectCreationExpression:
                return EmitObjectCreationExpression(boundExpression.As<BoundObjectCreationExpression>());
        }
        throw new Exception($"Unexpected expression {boundExpression.Kind}");
    }

    private ControlFlowGraph.BasicBlock FindBlockThatContainsVariableDeclaration(ControlFlowGraph cfg, VariableSymbol variable)
    {
        var result = new List<ControlFlowGraph.BasicBlock>();
        cfg.Start.TraverseBlocksBreadthFirst((block, traverse) =>
        {
            var variables = block.Statements.SelectMany(x => x.GetChildren(true))
                .OfType<BoundVariableDeclarationStatement>()
                .ToList();
            
            if (variables.Any(x => x.Variable.Equals(variable)))
            {
                result.Add(block);
            }
        });
        (result.Count == 1).EnsureTrue();
        return result.Single();
    }

    private TextPiece EmitObjectCreationExpression(BoundObjectCreationExpression expression)
    {
        var varName = new TextPiece("%size_ptr_" + expression.Type.Name + "_" + NextNumber());
        var size_ptr = varName + " = getelementptr %\"" + expression.Type.GetFullName() + "\", ptr null, i32 1";
        
        var sizeName = new TextPiece("%int_size_ptr_" + expression.Type.Name + "_" + NextNumber());
        var size = sizeName + " = ptrtoint ptr " + varName + " to i64";
        
        var objectPointerName = new TextPiece("%malloc_" + expression.Type.Name + "_" + NextNumber(), true);
        var objectPointer = objectPointerName + " = call ptr @malloc(i64 " + sizeName + ") ";
        _currentBlock.AddLine(size_ptr);
        _currentBlock.AddLine(size);
        _currentBlock.AddLine(objectPointer);
        return objectPointerName;
    }
    
    private TextPiece EmitPhiFunctionExpression(BoundPhiFunctionExpression phiFunctionExpression)
    {
        phiFunctionExpression.VariableSymbols
            .All(x=> x.Type.Equals(phiFunctionExpression.VariableSymbols.First().Type))
            .EnsureTrue();

        var varName = new TextPiece("%phiRes" + NextNumber());
        var phiFunction = new TextPiece(" = phi ");
        _unfilledPhiFunctions.Add((phiFunctionExpression, phiFunction));
        _currentBlock.Add(new TextPiece([varName, phiFunction]));
        return varName;
        
        // foreach (var variableSymbol in phiFunctionExpression.VariableSymbols)
        // {
        //     var containingBlock = FindBlockThatContainsVariableDeclaration(_cfg, variableSymbol);
        //     var variableName = _localVariables[variableSymbol];
        //     // first statement of block should always be its label
        //     var blockName = containingBlock.Statements.First().As<BoundLabelStatement>().Label.Name;
        //     res += $"[{variableName}, %{blockName}]";
        // }
        // 
        // return res;
    }

    private TextPiece EmitMemberAccessExpression(BoundMemberAccessExpression memberAccess)
    {
        if (memberAccess.Member.Kind is BoundNodeKind.MethodCallExpression)
        {
            if (memberAccess.Member.As<BoundMethodCallExpression>().MethodSymbol.IsStatic)
            {
                return EmitExpression(memberAccess.Member);
            }
        }
        else if (memberAccess.Member.Kind is BoundNodeKind.FieldExpression)
        {
            var field = memberAccess.Member.As<BoundFieldExpression>();
            var i = memberAccess.Left.Type.FieldTable.IndexOf(field.FieldSymbol);
            var obj = EmitExpression(memberAccess.Left);
            var fieldPtrName = new TextPiece("%field_access_" + field.FieldSymbol.Name + "_" + NextNumber(), true);
            var fieldPtr = fieldPtrName + " = getelementptr %\"" + field.FieldSymbol.ContainingType.Unwrap().GetFullName() + "\", ptr " + obj + ", i32 0, i32 " + i.ToString();
            
            var loadName = new TextPiece("%load_of_" + field.FieldSymbol.Name + "_" + NextNumber());
            var load = loadName + " = load " + EmittingShared.GetIRForType(field.FieldSymbol.Type) + ", ptr " + fieldPtrName;

            _currentBlock.AddLine(fieldPtr);
            _currentBlock.AddLine(load);
            return loadName;
        }
        return new TextPiece(EmitNotImplemented());
    }

    private TextPiece EmitAssignmentExpression(BoundAssignmentExpression boundExpression)
    {
        var right = EmitExpression(boundExpression.Initializer); // e.g., "1" for literal 1
        var left = boundExpression.Left;

        if (left is BoundVariableExpression variableExpr)
        {
            var variableSymbol = variableExpr.Variable;
            TextPiece variablePtr;
            if (_localVariables.ContainsKey(variableSymbol))
                variablePtr = _localVariables[variableSymbol];
            else if (_parameters.ContainsKey(variableSymbol))
                variablePtr = _parameters[variableSymbol];
            else
                throw new Exception($"Variable {variableSymbol.Name} not found");

            var valueType = EmittingShared.GetIRForType(boundExpression.Initializer.Type);
            var storeInstruction = $"store {valueType} {right}, ptr {variablePtr}";
            _currentBlock.AddLine(storeInstruction);
            return right;
        }
        else if (left is BoundMemberAccessExpression memberAccess && 
                 memberAccess.Member is BoundFieldExpression fieldExpr)
        {
            var fieldSymbol = fieldExpr.FieldSymbol;
            var i = memberAccess.Left.Type.FieldTable.IndexOf(fieldSymbol);
            var objectPtr = EmitExpression(memberAccess.Left); // e.g., %malloc_Barabas_31

            var fieldPtrName = "%field_ptr" + NextNumber();
            var objectType = "%\"" + memberAccess.Left.Type.GetFullName() + "\""; // e.g., %"HelloWorldSample.Barabas"
            var gepInstruction = $"{fieldPtrName} = getelementptr {objectType}, ptr {objectPtr}, i32 0, i32 {i}";
            _currentBlock.AddLine(gepInstruction);

            var fieldValueType = EmittingShared.GetIRForType(fieldSymbol.Type); // e.g., i32
            var storeInstruction = $"store {fieldValueType} {right}, ptr {fieldPtrName}";
            _currentBlock.AddLine(storeInstruction);

            return right;
        }

        return new TextPiece(EmitNotImplemented());
    }

    private TextPiece EmitConversionExpression(BoundConversionExpression boundExpression)
    {
        // from int to string
        if (boundExpression.Type.Equals(TypeSymbol.BuiltIn.String()) && boundExpression.Expression.Type.Equals(TypeSymbol.BuiltIn.Int()))
        {
            var res = new TextPiece("%int_to_string_res" + NextNumber());
            var resInit = res + " = alloca %" + CoreFunctionsLLVMIR.MyStringTypeName;
            // alloca's should all be in the entry block, to prevent stackoverflow in loops
            _entryBlock.Unwrap().AddLine(resInit, indx: 1);
            _currentBlock.AddLine("call void @" + CoreFunctionsLLVMIR.Int32ToStringFunctionName + "(" 
                               + "ptr " + res + ", "
                               + EmittingShared.GetIRForType(boundExpression.Expression.Type) + " " + EmitExpression(boundExpression.Expression) 
                               + ")");
            return res;
        }
        
        return new TextPiece(EmitNotImplemented());
    }

    private TextPiece EmitVariableExpression(BoundVariableExpression boundExpression)
    {
        if (_localVariables.ContainsKey(boundExpression.Variable))
            return _localVariables[boundExpression.Variable];
        
        if (_parameters.ContainsKey(boundExpression.Variable))
            return _parameters[boundExpression.Variable];

        
        throw new Exception("Variable not found");
    }

    private TextPiece EmitBinaryExpression(BoundBinaryExpression boundExpression)
    {
        var left = EmitExpression(boundExpression.Left);
        var right = EmitExpression(boundExpression.Right);
        var leftType = boundExpression.Left.Type;
        var rightType = boundExpression.Right.Type;
        if (leftType.Equals(TypeSymbol.BuiltIn.Int()))
        {
            if (!Equals(rightType, TypeSymbol.BuiltIn.Int()) && !Equals(rightType, TypeSymbol.BuiltIn.Int()))
                throw new Exception("Left side is int right side is not(" + boundExpression.Right.Type + ")");
            switch (boundExpression.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = add " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.Subtraction:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = sub " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.Multiplication:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = mul " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                 
                }
                case BoundBinaryOperatorKind.Division:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = sdiv " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.LogicalOr:
                    throw new NotImplementedException();
                    break;
                case BoundBinaryOperatorKind.LogicalAnd:
                    throw new NotImplementedException();
                    break;
                case BoundBinaryOperatorKind.Equality:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = icmp eq " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.Inequality:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = icmp ne " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.LessThan:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = icmp slt " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.GreaterThan:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = icmp sgt " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.GreaterThanOrEquals:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = icmp sge " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.LessThanOrEquals:
                {
                    var varName = new TextPiece("%binaryRes" + NextNumber());
                    var res = varName + " = icmp sle " + EmittingShared.GetIRForType(boundExpression.Left.Type) + " " + left + ", " + right;
                    _currentBlock.AddLine(res);
                    return varName;
                }
                case BoundBinaryOperatorKind.BitwiseAnd:
                    throw new NotImplementedException();
                    break;
                case BoundBinaryOperatorKind.BitwiseOr:
                    throw new NotImplementedException();
                    break;
                case BoundBinaryOperatorKind.BitwiseXor:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO should throw exception instead of returning TextPiece(EmitNotImplemented());
            return new TextPiece(EmitNotImplemented());
            // throw new UnreachableException();
        }

        if (leftType.Equals(TypeSymbol.BuiltIn.String()) && rightType.Equals(TypeSymbol.BuiltIn.String()))
        {
            if (boundExpression.Op.Kind == BoundBinaryOperatorKind.Addition)
            {
                //call void @my_string_concat(ptr dead_on_unwind writable sret(%struct.my_string) align 8 %5, ptr noundef %3, ptr noundef %4)
                var varName = new TextPiece("%string_concat" + NextNumber());
                var resAllocaLine = varName + " = alloca %" + CoreFunctionsLLVMIR.MyStringTypeName;
                var concatCallLine = "call void @" + CoreFunctionsLLVMIR.MyStringConcatFunctionName + "("
                          + "ptr " + varName + ", "
                          + "ptr " + left + ", "
                          + "ptr " + right + ")";
                
                // alloca's should all be in the entry block, to prevent stackoverflow in loops
                _entryBlock.Unwrap().AddLine(resAllocaLine, indx: 1);
                _currentBlock.AddLine(concatCallLine);
                return varName;
            }
            throw new UnreachableException();
        }
        
        return new TextPiece(EmitNotImplemented());
    }

    private TextPiece EmitLiteralExpression(BoundLiteralExpression literalExpression)
    {
        if (literalExpression.Type.Equals(TypeSymbol.BuiltIn.String()))
        {
            var constVar = "@strConst" + NextNumber();
            var literalValue = literalExpression.Value.As<string>();
            _stringConstants.Add(new TextPiece(constVar + " = internal constant [" + (literalValue.Length)+ " x i8] c\"" + literalValue + "\"", true)); 
            
            var resStringPtrName = "%tmp" + NextNumber();
            var resStringPtr = resStringPtrName + " = alloca %" + CoreFunctionsLLVMIR.MyStringTypeName; 
            //  %charPtr = getelementptr %struct.my_string, ptr %mystr, i32 0, i32 1
            // %constPtr = getelementptr [12 x i8], ptr @strConst7, i32 0, i64 0
            // store ptr %constPtr, ptr %charPtr
            // 
            // %leno = getelementptr %struct.my_string, ptr %mystr, i32 0, i32 0
            // store i32 11, ptr %leno
            var charPtrName = "%charPtr" + NextNumber();
            var charPtr = charPtrName + " = getelementptr %" + CoreFunctionsLLVMIR.MyStringTypeName + ", ptr " + resStringPtrName + ", i32 0, i32 1";
            
            var constPtrName = "%constPtr" + NextNumber();
            var constPtr = constPtrName + " = getelementptr [" + (literalValue.Length + 1) + " x i8], ptr " + constVar + ", i32 0, i64 0";
            var storeConstPtr = "store ptr " + constPtrName + ", ptr " + charPtrName;
            
            var lenoName = "%leno" + NextNumber();
            var leno = lenoName + " = getelementptr %" + CoreFunctionsLLVMIR.MyStringTypeName + ", ptr " + resStringPtrName + ", i32 0, i32 0";
            var storeLen = "store i32 " + literalValue.Length + ", ptr " + lenoName;
            
            // alloca's should all be in the entry block, to prevent stackoverflow in loops
            _entryBlock.Unwrap().AddLine(resStringPtr, indx:1);
            
            _currentBlock.AddLine(charPtr);
            _currentBlock.AddLine(constPtr);
            _currentBlock.AddLine(storeConstPtr);
            _currentBlock.AddLine(leno);
            _currentBlock.AddLine(storeLen);
            
            return new TextPiece(resStringPtrName, true);
        }
        if (literalExpression.Type.Equals(TypeSymbol.BuiltIn.Int()))
        {
            return new TextPiece(literalExpression.Value.NullGuard().ToString().NullGuard(), false);
        }
        else
        {
            return new TextPiece(EmitNotImplemented());
        }
    }

    private string EmitNotImplemented([CallerMemberName] string callerMemberName = "")
    {
        return callerMemberName + " not implemented :)";
    }

    TextPiece EmitCallExpression(BoundMethodCallExpression statement)
    {
        List<TextPiece> parameters = [ ];
        foreach (var arg in statement.Arguments)
        {
            parameters.Add(new TextPiece([new TextPiece(EmittingShared.GetIRForType(arg.Type)), new TextPiece(" "), EmitExpression(arg)]));
        }

        var resName = new TextPiece("%callRes" + NextNumber(), true);
        
        
        var methodSymbol = statement.MethodSymbol;
        if (methodSymbol.ContainingType.Unwrap().Equals(TypeSymbol.BuiltIn.Console()))
        {
            // TODO create static variables for known names
            if (methodSymbol.Name == "Print")
            {
                (parameters.Count == 1).EnsureTrue();
                var printCall = new TextPiece([ new TextPiece($"call void @{CoreFunctionsLLVMIR.MyPrintFunctionName}("), parameters.Single(), new TextPiece(")") ]);
                _currentBlock.AddLine(printCall);
                return printCall;
            }

            throw new UnreachableException();
        }

        var call = new TextPiece([ ]);
        if (!methodSymbol.ReturnType.Equals(TypeSymbol.BuiltIn.Void()))
        {
            call.TextPieces.Unwrap().AddRange([resName, new TextPiece(" = ")]);
        }
            
        call.TextPieces.Unwrap().AddRange([   new TextPiece("call "),
                                              new TextPiece(EmittingShared.GetIRForType(statement.MethodSymbol.ReturnType)),
                                              new TextPiece(" @\""),
                                              new TextPiece(EmittingShared.GetMethodName(statement.MethodSymbol)),
                                              new TextPiece("\"")]);
            
        var callParameters = new TextPiece([new TextPiece("(")]);
        foreach (var parameter in parameters)
        {
            callParameters.TextPieces.Unwrap().Add(parameter);
            if (parameter != parameters.Last())
                callParameters.TextPieces.Unwrap().Add(new TextPiece(", "));
        }
            
            
        callParameters.TextPieces.Unwrap().Add(new TextPiece(")"));
        _currentBlock.AddLine(new TextPiece([call, callParameters]));
            
        if (!methodSymbol.ReturnType.Equals(TypeSymbol.BuiltIn.Void()))
        {
            return resName;
        }

        return call;
    }
    
    private void EmitMethodControlFlowGraph(ControlFlowGraph cfg)
    {
        cfg.Start.TraverseBlocksBreadthFirst((currentBlock, traverse) =>
        {
            foreach (var statement in currentBlock.Statements)
            {
                switch (statement.Kind)
                {
                    case BoundNodeKind.ExpressionStatement:
                        EmitExpressionStatement(statement.As<BoundExpressionStatement>());
                        break;
                    case BoundNodeKind.VariableDeclarationAssignmentStatement:
                        EmitVariableDeclarationAssignmentStatement(statement.As<BoundVariableDeclarationAssignmentStatement>());
                        break;
                    case BoundNodeKind.IfStatement:
                        EmitIfStatement();
                        break;
                    case BoundNodeKind.WhileStatement:
                        EmitWhileStatement();
                        break;
                    case BoundNodeKind.ForStatement:
                        EmitForStatement();
                        break;
                    case BoundNodeKind.GotoStatement:
                        EmitGotoStatement(statement.As<BoundGotoStatement>());
                        break;
                    case BoundNodeKind.LabelStatement:
                        EmitLabelStatement(statement.As<BoundLabelStatement>());
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        EmitConditionalGotoStatement(statement.As<BoundConditionalGotoStatement>());
                        break;
                    case BoundNodeKind.ReturnStatement:
                        EmitReturnStatement(statement.As<BoundReturnStatement>());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
        });
    }

    private void EmitReturnStatement(BoundReturnStatement statement)
    {
        if (_methodSymbol.ReturnType.Equals(TypeSymbol.BuiltIn.Void()))
        {
            _currentBlock.AddLine("ret void");
            return;
        }
        
        var res = EmitExpression(statement.Expression.Unwrap());
        _currentBlock.AddLine("ret " + EmittingShared.GetIRForType(_methodSymbol.ReturnType) + " " + res);
    }

    private void EmitLabelStatement(BoundLabelStatement statement)
    {
        _currentBlock = new LLVMIRBlock();
        if (_entryBlock.IsNone)
            _entryBlock = _currentBlock;
        
        _blocks.Add(_currentBlock);
        _currentBlock.AddLine(statement.Label.Name + ":", indent: false);
    }

    private void EmitGotoStatement(BoundGotoStatement statement)
    {
        _currentBlock.AddLine("br label %" + statement.Label.Name);
    }

    private void EmitConditionalGotoStatement(BoundConditionalGotoStatement statement)
    {
        var condition = EmitExpression(statement.Condition);
        _currentBlock.AddLine("br i1 " + condition + ", label %" + statement.OnTrueLabel.Name + ", label %" + statement.OnFalseLabel.Name);
    }

    private void EmitForStatement()
    {
        _currentBlock.AddLine(EmitNotImplemented());
    }

    private void EmitWhileStatement()
    {
        _currentBlock.AddLine(EmitNotImplemented());
    }

    private void EmitIfStatement()
    {
        _currentBlock.AddLine(EmitNotImplemented());
    }

    private void EmitVariableDeclarationStatement()
    {
        _currentBlock.AddLine(EmitNotImplemented());
    }

    private void EmitVariableDeclarationAssignmentStatement(BoundVariableDeclarationAssignmentStatement statement)
    {
        var res = EmitExpression(statement.Initializer);
        if (res.IsVariable)
        {
            _localVariables.Add(statement.Variable, res);
            return;
        }

        if (Equals(statement.Variable.Type, TypeSymbol.BuiltIn.Int()))
        {
            // alloca's should all be in the entry block, to prevent stackoverflow in loops
            _entryBlock.Unwrap().AddLine("%" + statement.Variable.Name + " = alloca " + EmittingShared.GetIRForType(statement.Variable.Type), 
                                indx:1);
            _currentBlock.AddLine("store " + EmittingShared.GetIRForType(statement.Variable.Type) + " " + res + ", " + EmittingShared.GetIRForType(statement.Variable.Type) + "* %" + statement.Variable.Name);
        }
        else
        {
            _currentBlock.AddLine(res);
        }
        
        _localVariables.Add(statement.Variable, res);
    }

    private void EmitExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        EmitExpression(expressionStatement.Expression);
        _currentBlock.Add("\n");
    }
    

    public List<TextPiece> Emit()
    {
        List<string> paramList = [ ];
        var methodSymbol = _methodDeclaration.MethodSymbol;
        
        // add parameter for "this" keyword 
        if (!methodSymbol.IsStatic)
            paramList.Add("ptr");

        foreach (var parameter in methodSymbol.Parameters)
        {
            _parameters.Add(parameter, new TextPiece("%" + parameter.Name + NextNumber()));
            paramList.Add(EmittingShared.GetIRForType(parameter.Type) + " " + _parameters[parameter]);
        }
        
        EmitMethodControlFlowGraph(_cfg);
        FillAllPhiFunctions();


        var res = _stringConstants.Select(stringConstant => stringConstant + "\n").ToList();
        res.Add(new TextPiece($"define {EmittingShared.GetIRForType(methodSymbol.ReturnType)} @\"{EmittingShared.GetMethodName(methodSymbol)}\" ( {string.Join(", ", paramList)} )\n"));
        res.Add(new TextPiece("{\n"));
        foreach (var block in _blocks)
        {
            res.AddRange(block.Contents);
        }
        res.Add(new TextPiece("}\n"));
        
        
        var f = new StringWriter();
        _cfg.WriteTo(f);
        Console.WriteLine(methodSymbol.Name + ": ");
        Console.WriteLine(f.ToString());
        return res;
    }

    private void FillAllPhiFunctions()
    {
        foreach (var (phiNode, textPiece) in _unfilledPhiFunctions)
        {
            var res = textPiece.Text;
            res += EmittingShared.GetIRForType(phiNode.Type) + " ";
            foreach (var variableSymbol in phiNode.VariableSymbols)
            {
                var variableName = _localVariables[variableSymbol];
                var block = FindBlockThatContainsVariableDeclaration(_cfg, variableSymbol);
                var blockName = block.Statements.First().As<BoundLabelStatement>().Label.Name;
                res += $"[{variableName}, %{blockName}]";
                if (!Equals(variableSymbol, phiNode.VariableSymbols.Last()))
                    res += ", ";
            }
            textPiece.Text = res + "\n";
        }
    }
}