using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Interpretation;

namespace Language.Analysis.CodeAnalysis;

public class EvaluationResult
{
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public ObjectInstance? Result { get; }
    public ObjectInstance? MainMethodResult { get; }

    public EvaluationResult(ImmutableArray<Diagnostic> diagnostics, ObjectInstance? result, ObjectInstance? mainMethodResult)
    {
        Diagnostics = diagnostics;
        Result = result;
        MainMethodResult = mainMethodResult;
    }
    
    public static implicit operator Result<ObjectInstance?, ImmutableArray<Diagnostic>>(EvaluationResult result) 
        => result.Diagnostics.Any() 
            ? new Result<ObjectInstance?, ImmutableArray<Diagnostic>>(result.Diagnostics) 
            : new Result<ObjectInstance?, ImmutableArray<Diagnostic>>(result.Result);
    
}