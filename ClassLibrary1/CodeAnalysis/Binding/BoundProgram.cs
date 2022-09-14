using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

class BoundProgram
{
    public BoundProgram(
        BoundGlobalScope globalScope, ImmutableArray<Diagnostic> diagnostics,
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies)
    {
        GlobalScope = globalScope;
        Diagnostics = diagnostics;
        FunctionBodies = functionBodies;
    }

    public ImmutableDictionary<FunctionSymbol,BoundBlockStatement> FunctionBodies { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundGlobalScope GlobalScope { get; }
}