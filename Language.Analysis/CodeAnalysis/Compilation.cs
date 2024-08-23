using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Binding.Binders;
using Language.Analysis.CodeAnalysis.Emit;
using Language.Analysis.CodeAnalysis.Interpretation;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis;

public sealed class Compilation
{
    Compilation(Compilation? previous, SyntaxTree[] syntaxTree)
    {
        SyntaxTrees = syntaxTree.ToImmutableArray();
        Previous = previous;
    }
    public static Compilation Create(params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(null, syntaxTrees);
    }


    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public Compilation? Previous { get; }


    static readonly object Lock = new();
    Option<FullBoundProgram> _fullBoundProgram;

    FullBoundProgram GetFullBoundProgram()
    {
        lock (Lock)
        {
            if (_fullBoundProgram.IsSome)
                return _fullBoundProgram.Unwrap();   
        }

        var previous = Previous?.GetFullBoundProgram();

        var declarations = new DeclarationsBag();
        var programBinder = new ProgramBinder(previous?.GlobalScope, SyntaxTrees, declarations);
        var boundGlobalScope = programBinder.BindGlobalScope();

        var program = programBinder.BindProgram(previous?.Program, boundGlobalScope);
        var fullBoundProgram = Option.Some(new FullBoundProgram(boundGlobalScope, program));
        lock (Lock)
        {
            _fullBoundProgram = fullBoundProgram;
        }
        
        return _fullBoundProgram.Unwrap();
    }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, ObjectInstance?> variables)
    {
        var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
        var program = GetFullBoundProgram();
        var diagnostics = parseDiagnostics
            .Concat(program.GlobalScope.Diagnostics)
            .Concat(program.Program.Diagnostics)
            .ToImmutableArray();
        
        if (diagnostics.Any())
            return new EvaluationResult(diagnostics, null);

        // var cfgPath = "control_flow.dot";
        // var cfg = ControlFlowGraph.Create(
        //     !GlobalScope.Statement.Statements.Any() && GlobalScope.Functions.Any()
        //         ? program.FunctionBodies.Last().Value
        //         : Lowerer.Lower(GlobalScope.Statement));        // using (var streamWriter = new StreamWriter(cfgPath)) 
        //     cfg.WriteTo(streamWriter);

        var evaluator = new Evaluator(program.Program, variables);
        var result = evaluator.Evaluate();
        return new(ImmutableArray<Diagnostic>.Empty, result);
    }

    public void EmitTree(IndentedTextWriter writer)
    {
        if (GetFullBoundProgram().GlobalScope.MainMethod.IsSome)
            EmitTree(GetFullBoundProgram().GlobalScope.MainMethod.Unwrap(), writer);

        foreach (var type in GetFullBoundProgram().GlobalScope.Types)
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
        var program = GetFullBoundProgram();
        method.WriteTo(writer);
        writer.WriteLine();
        var type = program.Program.Types.SingleOrNone(x => x.MethodTable.FirstOrNone(x => x.MethodSymbol.Equals(method)).IsSome);
        if (type.IsNone)
            return;

        type.Unwrap().MethodTable
            .SingleOrNone(x => x.MethodSymbol.Equals(method))
            .Unwrap().MethodSymbol.WriteTo(writer);
    }

    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] refernces, string outputPaths)
    {
        var program = GetFullBoundProgram();
        return new Emitter().Emit(program.Program, moduleName, refernces, outputPaths);
    }
}