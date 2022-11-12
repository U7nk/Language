using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Emit;

public class Bytecode
{
    public class Facts
    {
        public const int LAST_ARGUMENT = -3;
        public const int TYPE_INT = 1;
        public const int TYPE_BOOL = 2;
        public const int TYPE_STRING = 2;
    }

    public const int IADD = 1; // int addition
    public const int ISUB = 2; // int subtraction
    public const int IMUL = 3; // int multiplication
    public const int IDIV = 4; // int division
    public const int ILT = 5; // int less than
    public const int IGT = 6; // integer greater than
    public const int IEQ = 7; // int equal
    public const int ICONST = 8; // push int constant

    /// <summary>
    /// Push string constant
    /// </summary>
    public const int SCONST = 9;

    /// <summary>
    /// String concatenation
    /// </summary>
    public const int SADD = 10;

    /// <summary>
    /// String equality
    /// </summary>
    public const int SEQ = 11;

    public const int BCONST = 12;

    /// <summary>
    /// Boolean equality
    /// </summary>
    public const int BEQ = 13;

    /// <summary>
    /// Boolean inequality
    /// </summary>
    public const int BINEQ = 14;

    public const int BR = 15; // branch
    public const int BRT = 16; // branch if true
    public const int BRF = 17; // branch if false
    public const int HLOAD = 18; // load global variable
    public const int HSTORE = 19; // store global variable
    public const int PRINT = 20; // print stack top
    public const int POP = 21; // pop stack top
    public const int HALT = 22; // halt
    public const int CALL = 23; // call
    public const int RET = 24; // call

    /// <summary>
    /// convert int to string
    /// </summary>
    public const int CASTINTSTRING = 25;

    /// <summary>
    /// convert int to bool
    /// </summary>
    public const int CASTINTBOOL = 26;

    /// <summary>
    /// Cast string to int
    /// </summary>
    public const int CASTSTRINGINT = 27;

    /// <summary>
    /// Cast string to bool
    /// </summary>
    public const int CASTSTRINGBOOL = 28;

    /// <summary>
    /// Cast bool to int
    /// </summary>
    public const int CASTBOOLINT = 29;

    /// <summary>
    /// Cast bool to string
    /// </summary>
    public const int CASTBOOLSTRING = 30;

    /// Load function argument to stack
    /// LGARG 0 - load first argument
    public const int LDARG = 31;

    /// Store function argument
    /// STARG 0 - store first argument
    public const int STARG = 32;

    /// Load local variable to stack
    /// LDLOC 0 - load first local variable
    public const int LDLOC = 33;

    /// Take top stack value and store to a local variable
    /// STLOC 0 - store to first local variable
    public const int STLOC = 34;
        
}

enum InstructionKind
{
    MethodCallPlaceholder,
    Instruction,
    LabelPlaceholder,
    ConditionalGotoPlaceholder,
    GotoPlaceholder,
}

class Instruction
{
    public int Opcode { get; set; }
    public InstructionKind Kind { get; }

    protected Instruction(int opcode, InstructionKind kind = InstructionKind.Instruction)
    {
        Opcode = opcode;
        Kind = kind;
    }

    public Instruction(int opcode)
    {
        Opcode = opcode;
        Kind = InstructionKind.Instruction;
    }
}

class MethodCallPlaceholder : Instruction
{
    public MethodSymbol Method { get; }

    public MethodCallPlaceholder(MethodSymbol method) : base(-1, InstructionKind.MethodCallPlaceholder)
    {
        Method = method;
    }
}

class LabelPlaceholder : Instruction
{
    public LabelSymbol Label { get; }

    public LabelPlaceholder(LabelSymbol label) : base(-1, InstructionKind.LabelPlaceholder)
    {
        Label = label;
    }
}

class ConditionalGotoPlaceholder : Instruction
{
    public LabelSymbol Label { get; }
    public bool JumpIfTrue { get; }

    public ConditionalGotoPlaceholder(LabelSymbol label, bool jumpIfTrue) : base(-1, InstructionKind.ConditionalGotoPlaceholder)
    {
        Label = label;
        JumpIfTrue = jumpIfTrue;
    }
}

class GotoPlaceholder : Instruction
{
    public LabelSymbol Label { get; }
    public GotoPlaceholder(LabelSymbol label) : base(-1, InstructionKind.GotoPlaceholder)
    {
        Label = label;
    }
}

class Linker
{
    readonly List<Instruction> _mainMethod;
    readonly Dictionary<MethodSymbol, List<Instruction>> _methodBodies;
    readonly Dictionary<MethodSymbol, int> _methodAddresses = new();
    readonly Dictionary<LabelSymbol, int> _labelAddresses = new();
    readonly List<Instruction> _gotoAddresses = new();
    readonly List<Instruction> _methodCallAdresses = new();
    int _counter = 0;
    
    public Linker(List<Instruction> mainMethod, Dictionary<MethodSymbol, List<Instruction>> methodBodies)
    {
        _mainMethod = mainMethod;
        _methodBodies = methodBodies;
    }

    public List<Instruction> Link()
    {
        var instructions = new List<Instruction>();
        instructions.AddRange(_mainMethod);
        
        REPEAT_FIRST_LOOP:
        foreach (var instruction in instructions)
        {
            if (instruction.Kind == InstructionKind.MethodCallPlaceholder)
            {
                var functionCallPlaceholder = (MethodCallPlaceholder)instruction;
                var function = functionCallPlaceholder.Method;
                var functionBody = _methodBodies[function];
                
                var functionAddress = instructions.Count;
                if (!_methodAddresses.ContainsKey(function))
                {
                    _methodAddresses.Add(function, functionAddress);
                    instructions.AddRange(functionBody);
                }
                
                var placeholderIndex = instructions.IndexOf(functionCallPlaceholder);
                UpdateAddresses(placeholderIndex, InstructionKind.MethodCallPlaceholder);
                instructions.Remove(functionCallPlaceholder);
                var functionCallAddress = new Instruction(_methodAddresses[function]); 
                _methodCallAdresses.Add(functionCallAddress);
                var callInstructions = new List<Instruction>
                {
                    new(Bytecode.CALL),
                    functionCallAddress,
                    new(function.Parameters.Length),
                };
                instructions.InsertRange(placeholderIndex, callInstructions);
                goto REPEAT_FIRST_LOOP;
            }

            if (instruction.Kind == InstructionKind.LabelPlaceholder)
            {
                var labelPlaceholder = (LabelPlaceholder)instruction;
                var label = labelPlaceholder.Label;
                var placeholderIndex = instructions.IndexOf(labelPlaceholder);
                UpdateAddresses(placeholderIndex, InstructionKind.LabelPlaceholder);
                instructions.Remove(labelPlaceholder);
                _labelAddresses.Add(label, placeholderIndex);
                goto REPEAT_FIRST_LOOP;
            }
        }
        
        REPEAT_SECOND_LOOP:
        foreach (var instruction in instructions)
        {
        
            if (instruction.Kind == InstructionKind.ConditionalGotoPlaceholder)
            {
                var gotoPlaceholder = (ConditionalGotoPlaceholder)instruction;
                var label = gotoPlaceholder.Label;
                var placeholderAddress = instructions.IndexOf(gotoPlaceholder);
                var gotoAddress = new Instruction(_labelAddresses[label]);
                _gotoAddresses.Add(gotoAddress);
                UpdateAddresses(placeholderAddress, InstructionKind.ConditionalGotoPlaceholder);
                instructions.Remove(gotoPlaceholder);
                var opCode = gotoPlaceholder.JumpIfTrue ? Bytecode.BRT : Bytecode.BRF;
                instructions.Insert(placeholderAddress, new Instruction(opCode));
                instructions.Insert(placeholderAddress + 1, gotoAddress);
                goto REPEAT_SECOND_LOOP;
            }

            if (instruction.Kind == InstructionKind.GotoPlaceholder)
            {
                var gotoPlaceholder = (GotoPlaceholder)instruction;
                var label = gotoPlaceholder.Label;
                var placeholderAddress = instructions.IndexOf(gotoPlaceholder);
                var gotoAddress = new Instruction(_labelAddresses[label]);
                _gotoAddresses.Add(gotoAddress);
                UpdateAddresses(placeholderAddress, InstructionKind.GotoPlaceholder);
                instructions.Remove(gotoPlaceholder);
                instructions.Insert(placeholderAddress, new Instruction(Bytecode.BR));
                instructions.Insert(placeholderAddress + 1, gotoAddress);
                goto REPEAT_SECOND_LOOP;
            }
        }

        return instructions;
    }

    void UpdateAddresses(int position, int offset)
    {
        foreach (var functionSymbol in _methodAddresses.Keys)
        {
            if (_methodAddresses[functionSymbol] > position)
            {
                _methodAddresses[functionSymbol] += offset;
            }
        }

        foreach (var labelSymbol in _labelAddresses.Keys)
        {
            if (_labelAddresses[labelSymbol] > position)
            {
                _labelAddresses[labelSymbol] += offset;
            }
        }

        foreach (var gotoAddress in _gotoAddresses)
        {
            if (gotoAddress.Opcode > position)
            {
                gotoAddress.Opcode += offset;
            }
        }
        
        foreach (var functionCallAddress in _methodCallAdresses)
        {
            if (functionCallAddress.Opcode > position)
            {
                functionCallAddress.Opcode += offset;
            }
        }
    }
    void UpdateAddresses(int position, InstructionKind kind)
    {
        if (kind == InstructionKind.Instruction)
            throw new Exception("Cannot update addresses for instruction");
        
        if (kind == InstructionKind.MethodCallPlaceholder)
        {
            UpdateAddresses(position, offset: 2);
        }

        if (kind == InstructionKind.LabelPlaceholder)
        {
            UpdateAddresses(position, offset: -1);
        }
        
        if (kind == InstructionKind.ConditionalGotoPlaceholder)
        {
            UpdateAddresses(position, offset: 1);
        }

        if (kind == InstructionKind.GotoPlaceholder)
        {
            UpdateAddresses(position, offset: 1);
        }
    }
}

class BytecodePrettyPrinter
{
    List<Instruction> _instructions;
    int _counter = -1;

    public BytecodePrettyPrinter(List<Instruction> instructions)
    {
        _instructions = instructions;
    }

    Instruction Current => _instructions[_counter];
    void Next() => _counter++;
    

    public void Print(StringWriter writer, List<Instruction> instructions)
    {
        while (_counter < instructions.Count)
        {
            Next();
            if (_counter >= instructions.Count)
                break;
            writer.Write("{0,4}:\t", _counter);
            switch (Current.Opcode)
            {
                case Bytecode.BR:
                    writer.Write("BR ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.BRT:
                    writer.Write("BRT ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.BRF:
                    writer.Write("BRF ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.ICONST:
                    writer.Write("ICONST ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.IEQ:
                    writer.WriteLine("IEQ");
                    continue;
                case Bytecode.IGT:
                    writer.WriteLine("IGT");
                    continue;
                case Bytecode.ILT:
                    writer.WriteLine("ILT");
                    continue;
                case Bytecode.IADD:
                    writer.WriteLine("IADD");
                    continue;
                case Bytecode.IDIV:
                    writer.WriteLine("IDIV");
                    continue;
                case Bytecode.IMUL:
                    writer.WriteLine("IMUL");
                    continue;
                case Bytecode.ISUB:
                    writer.WriteLine("ISUB");
                    continue;
                case Bytecode.PRINT:
                    writer.WriteLine("PRINT");
                    continue;
                case Bytecode.SEQ:
                    writer.WriteLine("SEQ");
                    continue;
                case Bytecode.SCONST:
                    writer.Write("SCONST ");
                    Next();
                    var length = Current.Opcode;
                    writer.Write( " len:" + length + " \"");
                    Next();
                    foreach(var _ in 0..length)
                    {
                        writer.Write((char)Current.Opcode);
                        Next();
                    }
                    Next();
                    writer.WriteLine("\"");
                    continue;
                case Bytecode.SADD:
                    writer.WriteLine("SADD");
                    continue;
                case Bytecode.CASTBOOLINT:
                    writer.WriteLine("CastBoolToInt");
                    continue;
                case Bytecode.CASTBOOLSTRING:
                    writer.WriteLine("CastBoolToString");
                    continue;
                case Bytecode.CASTINTBOOL:
                    writer.WriteLine("CastIntToBool");
                    continue;
                case Bytecode.CASTINTSTRING:
                    writer.WriteLine("CastIntToString");
                    continue;
                case Bytecode.CASTSTRINGINT:
                    writer.WriteLine("CastStringToInt");
                    continue;
                case Bytecode.CASTSTRINGBOOL:
                    writer.WriteLine("CastStringToBool");
                    continue;
                case Bytecode.POP:
                    writer.WriteLine("POP");
                    continue;
                case Bytecode.HALT:
                    writer.WriteLine("HALT");
                    continue;
                case Bytecode.STARG:
                    writer.Write("STARG ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.LDARG:
                    writer.Write("LDARG ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.STLOC:
                    writer.Write("STLOC ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.LDLOC:
                    writer.Write("LDLOC ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.HSTORE:
                    writer.Write("HSTORE");
                    writer.WriteLine();
                    continue;
                case Bytecode.HLOAD:
                    writer.Write("HLOAD");
                    writer.WriteLine();
                    continue;
                case Bytecode.CALL:
                    writer.Write("CALL address:");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.Write(" args:");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
                case Bytecode.RET:
                    writer.Write("RET");
                    writer.WriteLine();
                    continue;
                case Bytecode.BEQ:
                    writer.Write("BEQ");
                    writer.WriteLine();
                    continue;
                case Bytecode.BINEQ:
                    writer.Write("BINEQ");
                    writer.WriteLine();
                    continue;
                case Bytecode.BCONST:
                    writer.Write("BCONST ");
                    Next();
                    writer.Write(Current.Opcode);
                    writer.WriteLine();
                    continue;
            }
        }
    }
}
class Emitter
{
    Dictionary<MethodSymbol, int> _methodOffsets = new();
    Dictionary<MethodSymbol, int> _methodParameters = new();
    Dictionary<MethodCallPlaceholder, int> _methodCallMarkers = new();
    Dictionary<MethodSymbol, List<Instruction>> _methodBodies = new();
    List<Instruction> _mainMethod = new();
    FileStream? _writer;
    BoundProgram? _program;
    readonly Stack<Dictionary<VariableSymbol, int>> _stack = new();

    public ImmutableArray<Diagnostic> Emit(
        BoundProgram program,
        string moduleName,
        string[] references,
        string outputPath)
    {
        _program = program;
        var mainFunction = _program.MainMethod ?? _program.ScriptMainMethod;
        Debug.Assert(mainFunction != null, nameof(mainFunction) + " != null");
        
        _methodOffsets.Add(mainFunction, 0);
        _methodParameters.Add(mainFunction, 0);

        _mainMethod = EmitMainMethod(mainFunction);
        
        EmitOtherMethodBodies(exceptMainMethod: mainFunction);

        var linker = new Linker(_mainMethod, _methodBodies);
        var programInstructions = linker.Link();

        var stringWriter = new StringWriter();
        var bytecodePrettyPrinter = new BytecodePrettyPrinter(programInstructions);
        bytecodePrettyPrinter.Print(stringWriter, programInstructions);
        File.WriteAllText(@"C:\Users\PC-123\Desktop\C#\Bindings-master\samples\Bytecode.txt", stringWriter.ToString());
        
        _writer = new FileStream("test.bin", FileMode.Create);
        _writer.WriteByte(0);
        _writer.WriteByte(0);
        _writer.WriteByte(0);
        _writer.WriteByte(0);
        foreach (var instruction in programInstructions)
        {
            var @byte = (byte)instruction.Opcode;
            _writer.WriteByte(@byte);
        }
        _writer.Flush();
        return ImmutableArray<Diagnostic>.Empty;
    }

    void EmitOtherMethodBodies(MethodSymbol exceptMainMethod)
    {
        //BUG
        // foreach (var (functionSymbol, functionBody) in _program.Types.Exclude(x => Equals(x.Key, exceptMainFunction)))
        // {
            // _stack.Push(new Dictionary<VariableSymbol, int>());
            // for (var index = 0; index < functionSymbol.Parameters.Length; index++)
            // {
                // var parameter = functionSymbol.Parameters[index];
                // _stack.Peek().Add(parameter, index);
            // }

            // var instructions = EmitBoundBlockStatement(functionBody);
            // _functionBodies.Add(functionSymbol, instructions);
            // _stack.Pop();
        // }
        throw new NotImplementedException();
    }

    List<Instruction> EmitMainMethod(MethodSymbol programMainMethod)
    {
        var instructions = new List<Instruction>();
        throw new NotImplementedException();
        // var body = _program.Types[programMainFunction];
        // _stack.Push(new Dictionary<VariableSymbol, int>());
        // instructions.AddRange(EmitBoundBlockStatement(body));
        // _stack.Pop();
        // instructions.Add(new Instruction(Bytecode.HALT));
        // return instructions;
    }

    List<Instruction> EmitBoundBlockStatement(BoundBlockStatement blockStatement)
    {
        var instructions = new List<Instruction>();
        foreach (var statement in blockStatement.Statements)
        {
            instructions.AddRange(EmitStatement(statement));
        }
        
        return instructions;
    }

    List<Instruction> EmitStatement(BoundStatement statement)
    {
        switch (statement.Kind)
        {
            case BoundNodeKind.ExpressionStatement:
                return EmitExpressionStatement((BoundExpressionStatement)statement);
            case BoundNodeKind.VariableDeclarationAssignmentStatement:
                return EmitVariableDeclarationStatement((BoundVariableDeclarationAssignmentStatement)statement);
            case BoundNodeKind.ConditionalGotoStatement:
                return EmitConditionalGotoStatement((BoundConditionalGotoStatement)statement);
            case BoundNodeKind.LabelStatement:
                return EmitLabelStatement((BoundLabelStatement)statement);
            case BoundNodeKind.ReturnStatement:
                return EmitReturnStatement((BoundReturnStatement)statement);
            case BoundNodeKind.GotoStatement:
                return EmitGotoStatement((BoundGotoStatement)statement);
        }

        throw new Exception("Unexpected statement");
    }

    List<Instruction> EmitGotoStatement(BoundGotoStatement statement)
    {
        var instructions = new List<Instruction>();
        instructions.Add(new GotoPlaceholder(statement.Label));
        return instructions;
    }

    List<Instruction> EmitReturnStatement(BoundReturnStatement statement)
    {
        var instructions = new List<Instruction>();
        if (statement.Expression is not null) 
            instructions.AddRange(EmitExpression(statement.Expression));
        
        instructions.Add(new Instruction(Bytecode.RET));
        return instructions;
    }

    List<Instruction> EmitLabelStatement(BoundLabelStatement statement)
    {
        var instructions = new List<Instruction>();
        instructions.Add(new LabelPlaceholder(statement.Label));
        return instructions;
    }

    List<Instruction> EmitConditionalGotoStatement(BoundConditionalGotoStatement statement)
    {
        var instructions = new List<Instruction>();
        instructions.AddRange(EmitExpression(statement.Condition));
        instructions.Add(new ConditionalGotoPlaceholder(statement.Label, statement.JumpIfTrue));
        return instructions;
    }

    List<Instruction> EmitVariableDeclarationStatement(BoundVariableDeclarationAssignmentStatement assignmentStatement)
    {
        var instructions = new List<Instruction>();
        instructions.AddRange(EmitExpression(assignmentStatement.Initializer));
        instructions.Add(new Instruction(Bytecode.STLOC));
        instructions.Add(new Instruction(_stack.Peek().Count));
        _stack.Peek().Add(assignmentStatement.Variable, _stack.Peek().Count);
        return instructions;
    }

    List<Instruction> EmitExpressionStatement(BoundExpressionStatement statement)
    {
        return EmitExpression(statement.Expression);
    }

    List<Instruction> EmitExpression(BoundExpression boundExpression)
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
        }
        throw new Exception($"Unexpected expression {boundExpression.Kind}");
    }

    List<Instruction> EmitAssignmentExpression(BoundAssignmentExpression boundExpression)
    {
        var instructions = new List<Instruction>();
        instructions.AddRange(EmitExpression(boundExpression.Expression));
        instructions.Add(new Instruction(Bytecode.STLOC));
        instructions.Add(new Instruction(_stack.Peek()[boundExpression.Variable]));
        return instructions;
    }

    List<Instruction> EmitConversionExpression(BoundConversionExpression conversionExpression)
    {

        if (Equals(conversionExpression.Type, conversionExpression.Expression.Type))
            return EmitExpression(conversionExpression.Expression);
        
        var instructions = new List<Instruction>();
        instructions.AddRange(EmitExpression(conversionExpression.Expression));
        if (Equals(conversionExpression.Expression.Type, BuiltInTypeSymbols.Int))
        {
            if (Equals(conversionExpression.Type, BuiltInTypeSymbols.String))
            {
                instructions.Add(new Instruction(Bytecode.CASTINTSTRING));
                return instructions;
            }
            if (Equals(conversionExpression.Type, BuiltInTypeSymbols.Bool))
            {
                instructions.Add(new Instruction(Bytecode.CASTINTBOOL));
                return instructions;
            }
        }
        if (Equals(conversionExpression.Expression.Type, BuiltInTypeSymbols.String))
        {
            if (Equals(conversionExpression.Type, BuiltInTypeSymbols.Int))
            {
                instructions.Add(new Instruction(Bytecode.CASTSTRINGINT));
                return instructions;
            }
            if (Equals(conversionExpression.Type, BuiltInTypeSymbols.Bool))
            {
                instructions.Add(new Instruction(Bytecode.CASTSTRINGBOOL));
                return instructions;
            }
        }
        if (Equals(conversionExpression.Expression.Type, BuiltInTypeSymbols.Bool))
        {
            if (Equals(conversionExpression.Type, BuiltInTypeSymbols.Int))
            {
                instructions.Add(new Instruction(Bytecode.CASTBOOLINT));
                return instructions;
            }
            if (Equals(conversionExpression.Type, BuiltInTypeSymbols.String))
            {
                instructions.Add(new Instruction(Bytecode.CASTBOOLSTRING));
                return instructions;
            }
        }
        
        throw new Exception("Unexpected conversion");
    }

    List<Instruction> EmitVariableExpression(BoundVariableExpression boundExpression)
    {
        var instructions = new List<Instruction>();
        
        var locals = _stack.Peek();
        var localOffset = locals[boundExpression.Variable];
        if (locals.Single(x => Equals(x.Key, boundExpression.Variable)).Key is ParameterSymbol)
        {
            instructions.Add(new Instruction(Bytecode.LDARG));
            instructions.Add(new Instruction(localOffset));
        }
        else
        {
            instructions.Add(new Instruction(Bytecode.LDLOC));
            instructions.Add(new Instruction(localOffset));
        }

        return instructions;
    }

    List<Instruction> EmitBinaryExpression(BoundBinaryExpression boundExpression)
    {
        var instructions = new List<Instruction>();
        instructions.AddRange(EmitExpression(boundExpression.Left));
        instructions.AddRange(EmitExpression(boundExpression.Right));
        if (Equals(boundExpression.Left.Type, BuiltInTypeSymbols.Bool))
        {
            if (Equals(boundExpression.Right.Type, BuiltInTypeSymbols.Bool))
            {
                switch (boundExpression.Op.Kind)
                {
                    case BoundBinaryOperatorKind.Equality:
                        instructions.Add(new Instruction(Bytecode.BEQ));
                        break;
                    case BoundBinaryOperatorKind.Inequality:
                        instructions.Add(new Instruction(Bytecode.BINEQ));
                        break;
                    default:
                        throw new Exception("Unexpected binary operator");
                }

                return instructions;
            }

            throw new Exception("Invalid binary expression");
        }

        if (Equals(boundExpression.Left.Type, BuiltInTypeSymbols.Int))
        {
            if (Equals(boundExpression.Right.Type, BuiltInTypeSymbols.Int))
            {
                switch (boundExpression.Op.Kind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        instructions.Add(new Instruction(Bytecode.IADD));
                        break;
                    case BoundBinaryOperatorKind.Subtraction:
                        instructions.Add(new Instruction(Bytecode.ISUB));
                        break;
                    case BoundBinaryOperatorKind.Multiplication:
                        instructions.Add(new Instruction(Bytecode.IMUL));
                        break;
                    case BoundBinaryOperatorKind.Division:
                        instructions.Add(new Instruction(Bytecode.IDIV));
                        break;
                    case BoundBinaryOperatorKind.LessThan:
                        instructions.Add(new Instruction(Bytecode.ILT));
                        break;
                    case BoundBinaryOperatorKind.GreaterThan:
                        instructions.Add(new Instruction(Bytecode.IGT));
                        break;
                    case BoundBinaryOperatorKind.Equality:
                        instructions.Add(new Instruction(Bytecode.IEQ));
                        break;
                    default:
                        throw new Exception("Unexpected binary operator");
                }

                return instructions;
            }

            throw new Exception("Invalid binary expression");
        }

        if (Equals(boundExpression.Left.Type, BuiltInTypeSymbols.String))
        {
            if (Equals(boundExpression.Right.Type, BuiltInTypeSymbols.String))
            {
                switch (boundExpression.Op.Kind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        instructions.Add(new Instruction(Bytecode.SADD));
                        break;
                    case BoundBinaryOperatorKind.Equality:
                        instructions.Add(new Instruction(Bytecode.SEQ));
                        break;
                    default:
                        throw new Exception("Unexpected binary operator");
                }

                return instructions;
            }

            throw new Exception("Invalid binary expression");
        }

        throw new Exception("Invalid binary expression");
    }

    List<Instruction> EmitLiteralExpression(BoundLiteralExpression boundExpression)
    {
        Debug.Assert(!Equals(boundExpression.Type, BuiltInTypeSymbols.Object));

        var instructions = new List<Instruction>();
        if (Equals(boundExpression.Type, BuiltInTypeSymbols.Int))
        {
            instructions.Add(new Instruction(Bytecode.ICONST));
            instructions.Add(new Instruction((int)boundExpression.Value!));
        }
        else if (Equals(boundExpression.Type, BuiltInTypeSymbols.String))
        {
            instructions.Add(new Instruction(Bytecode.SCONST));
            var str = (string)boundExpression.Value!;
            instructions.Add(new Instruction(str.Length));
            foreach (var c in str)
            {
                instructions.Add(new Instruction(c));
            }
            instructions.Add(new Instruction(0));
            instructions.Add(new Instruction(0));
        }
        else if (Equals(boundExpression.Type, BuiltInTypeSymbols.Bool))
        {
            instructions.Add(new Instruction(Bytecode.BCONST));
            instructions.Add(new Instruction(((bool)boundExpression.Value! ? 1 : 0)));
        }
        else
        {
            throw new Exception($"Unexpected type {boundExpression.Type}");
        }

        return instructions;
    }

    List<Instruction> EmitCallExpression(BoundMethodCallExpression statement)
    {
        var instructions = new List<Instruction>();
        if (statement.MethodSymbol.Name == "print")
        {
            var arg = statement.Arguments.Single();
            instructions.AddRange(EmitExpression(arg));
            instructions.Add(new Instruction(Bytecode.PRINT));
        }
        else
        {
            foreach (var arg in statement.Arguments)
            {
                instructions.AddRange(EmitExpression(arg));
            }

            if (_methodOffsets.ContainsKey(statement.MethodSymbol))
            {
                _writer.WriteByte(Bytecode.CALL);
                _writer.WriteByte((byte)_methodOffsets[statement.MethodSymbol]);
                _writer.WriteByte((byte)_methodParameters[statement.MethodSymbol]);
            }
            else
            {
                var callPlaceHolder = new MethodCallPlaceholder(statement.MethodSymbol);
                instructions.Add(callPlaceHolder);
            }
        }

        return instructions;
    }
}