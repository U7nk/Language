using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

class BoundProgram
{
    public BoundProgram(
        BoundProgram previous,
        BoundGlobalScope globalScope, ImmutableArray<Diagnostic> diagnostics,
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies)
    {
        Previous = previous;
        GlobalScope = globalScope;
        Diagnostics = diagnostics;
        FunctionBodies = functionBodies;
    }

    public ImmutableDictionary<FunctionSymbol,BoundBlockStatement> FunctionBodies { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundProgram Previous { get; }
    public BoundGlobalScope GlobalScope { get; }
}