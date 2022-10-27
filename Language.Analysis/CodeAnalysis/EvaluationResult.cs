using System.Collections.Immutable;

namespace Language.Analysis.CodeAnalysis;

public class EvaluationResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public ObjectInstance? Result { get; }

    public EvaluationResult(ImmutableArray<Diagnostic> diagnostics, ObjectInstance? result)
    {
        Diagnostics = diagnostics;
        Result = result;
    }
}