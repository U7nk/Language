using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

class BoundProgram
{
    public BoundProgram(
        BoundProgram? previous, ImmutableArray<Diagnostic> diagnostics,
        Option<MethodSymbol> mainMethod,
        ImmutableArray<TypeSymbol> types)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainMethod = mainMethod;
        Types = types;
    }
    public ImmutableArray<TypeSymbol> Types { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public Option<MethodSymbol> MainMethod { get; }
    public BoundProgram? Previous { get; }
}