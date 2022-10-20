using System.Collections.Immutable;
using Wired.CodeAnalysis.Symbols;

namespace Wired.CodeAnalysis.Binding;

class BoundProgram
{
    public BoundProgram(
        BoundProgram? previous, ImmutableArray<Diagnostic> diagnostics,
        FunctionSymbol? mainFunction, FunctionSymbol? scriptMainFunction,
        ImmutableArray<TypeSymbol> types)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainFunction = mainFunction;
        ScriptMainFunction = scriptMainFunction;
        Types = types;
    }
    public ImmutableArray<TypeSymbol> Types { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public FunctionSymbol? MainFunction { get; }
    public FunctionSymbol? ScriptMainFunction { get; }
    public BoundProgram? Previous { get; }
}