using System.Collections.Generic;

namespace Language.Analysis.CodeAnalysis.Interpretation;

public abstract class RuntimeObject
{
    protected RuntimeObject(Dictionary<string, ObjectInstance?> fields)
    {
        Fields = fields;
    }

    public Dictionary<string, ObjectInstance?> Fields { get; }
}