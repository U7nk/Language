using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

class BoundProgram
{
    public BoundProgram(
        BoundProgram? previous, ImmutableArray<Diagnostic> diagnostics,
        FunctionSymbol? mainFunction, FunctionSymbol? scriptMainFunction,
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainFunction = mainFunction;
        ScriptMainFunction = scriptMainFunction;
        Functions = functions;
    }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public FunctionSymbol? MainFunction { get; }
    public FunctionSymbol? ScriptMainFunction { get; }
    public BoundProgram? Previous { get; }
}