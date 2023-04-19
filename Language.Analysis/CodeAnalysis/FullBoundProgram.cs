using Language.Analysis.CodeAnalysis.Binding;

namespace Language.Analysis.CodeAnalysis;

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