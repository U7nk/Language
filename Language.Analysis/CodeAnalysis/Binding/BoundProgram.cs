using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundProgram
{
    public BoundProgram(
        BoundProgram? previous, ImmutableArray<Diagnostic> diagnostics,
        MethodSymbol? mainMethod, MethodSymbol? scriptMainMethod,
        ImmutableArray<TypeSymbol> types)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainMethod = mainMethod;
        ScriptMainMethod = scriptMainMethod;
        Types = types;
    }
    public ImmutableArray<TypeSymbol> Types { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public MethodSymbol? MainMethod { get; }
    public MethodSymbol? ScriptMainMethod { get; }
    public BoundProgram? Previous { get; }
}