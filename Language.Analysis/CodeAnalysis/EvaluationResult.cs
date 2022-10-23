using System.Collections.Immutable;

namespace Language.Analysis.CodeAnalysis;

public class EvaluationResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public object? Result { get; }

    public EvaluationResult(ImmutableArray<Diagnostic> diagnostics, object? result)
    {
        Diagnostics = diagnostics;
        Result = result;
    }
}