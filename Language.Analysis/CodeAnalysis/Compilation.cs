using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Binding.Binders;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Emit;
using Language.Analysis.CodeAnalysis.Interpretation;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis;

public sealed class Compilation
{
    Compilation(bool isScript, Compilation? previous, SyntaxTree[] syntaxTree)
    {
        SyntaxTrees = syntaxTree.ToImmutableArray();
        IsScript = isScript;
        Previous = previous;
    }
    public static Compilation Create(params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(isScript: false, null, syntaxTrees);
    }

    public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(isScript: true, previous, syntaxTrees);
    }


    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public bool IsScript { get; }
    public Compilation? Previous { get; }

    
    BoundGlobalScope? _globalScope;
    internal BoundGlobalScope GetGlobalScope()
    {
            if (_globalScope is not null)
                return _globalScope;

            var lookup = new BinderLookup(new DeclarationsBag());
            var programBinder = new ProgramBinder(IsScript, Previous?._globalScope, SyntaxTrees, lookup);
            var boundGlobalScope = programBinder.BindGlobalScope();
            Interlocked.CompareExchange(ref _globalScope, boundGlobalScope, null);

            return _globalScope;
    }
    
    BoundProgram? _boundProgram;
    
    BoundProgram GetProgram()
    {
        var previous = Previous?.GetProgram();
        if (_boundProgram is not null)
            return _boundProgram;
        
        
        var lookup = new BinderLookup(GetGlobalScope().DeclarationsBag);
        var programBinder = new ProgramBinder(IsScript, Previous?._globalScope, SyntaxTrees, lookup);
        var program = programBinder.BindProgram(IsScript, previous, GetGlobalScope());
        Interlocked.CompareExchange(ref _boundProgram, program, null);
        return _boundProgram;
    }

    FullBoundProgram _fullBoundProgram;
    FullBoundProgram GetFullBoundProgram()
    {
        var previous = Previous?.GetProgram();
        if (_fullBoundProgram is not null)
            return _fullBoundProgram;

        return _fullBoundProgram;
    }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, ObjectInstance?> variables)
    {
        var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
        var program = GetProgram();
        var diagnostics = parseDiagnostics
            .Concat(GetGlobalScope().Diagnostics)
            .Concat(program.Diagnostics)
            .ToImmutableArray();
        
        if (diagnostics.Any())
            return new EvaluationResult(diagnostics, null);

        // var cfgPath = "control_flow.dot";
        // var cfg = ControlFlowGraph.Create(
        //     !GlobalScope.Statement.Statements.Any() && GlobalScope.Functions.Any()
        //         ? program.FunctionBodies.Last().Value
        //         : Lowerer.Lower(GlobalScope.Statement));        // using (var streamWriter = new StreamWriter(cfgPath)) 
        //     cfg.WriteTo(streamWriter);

        var evaluator = new Evaluator(program, variables);
        var result = evaluator.Evaluate();
        return new(ImmutableArray<Diagnostic>.Empty, result);
    }

    public void EmitTree(IndentedTextWriter writer)
    {
        if (GetGlobalScope().MainMethod.IsSome)
            EmitTree(GetGlobalScope().MainMethod.Unwrap(), writer);
        else if (GetGlobalScope().ScriptMainMethod is not null)
            EmitTree(GetGlobalScope().ScriptMainMethod, writer);

        foreach (var type in GetGlobalScope().Types)
        {
            EmitTree(type, writer);
        }
    }

    public void EmitTree(TypeSymbol type, IndentedTextWriter writer)
    {
        type.WriteTo(writer);
        writer.WriteLine();
        writer.Write("{");
        foreach (var declaration in type.MethodTable)
        {
            writer.WriteLine();
            writer.Indent++;
            declaration.MethodSymbol.WriteTo(writer);
            declaration.Body.OnSome(b => b.WriteTo(writer));
            writer.Indent--;
        }
        writer.Write("}");
    }
    
    public void EmitTree(MethodSymbol method, TextWriter writer)
    {
        var program = GetProgram();
        method.WriteTo(writer);
        writer.WriteLine();
        var type = program.Types.SingleOrNone(x => x.MethodTable.FirstOrNone(x => x.MethodSymbol.Equals(method)).IsSome);
        if (type.IsNone)
            return;

        type.Unwrap().MethodTable
            .SingleOrNone(x => x.MethodSymbol.Equals(method))
            .Unwrap().MethodSymbol.WriteTo(writer);
    }

    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] refernces, string outputPaths)
    {
        var program = GetProgram();
        return new Emitter().Emit(program, moduleName, refernces, outputPaths);
    }
}

internal class FullBoundProgram
{
    public FullBoundProgram(BoundGlobalScope globalScope, BoundProgram program)
    {
        this.Program = program;
        this.GlobalScope = globalScope;
    }

    public BoundGlobalScope GlobalScope { get; }
    public BoundProgram Program { get; }
    
}