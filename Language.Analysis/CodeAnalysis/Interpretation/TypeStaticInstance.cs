using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Interpretation;

public class TypeStaticInstance : RuntimeObject
{
    public TypeStaticInstance(Dictionary<string, ObjectInstance?> fields, TypeSymbol type) 
        : base(fields)
    {
        Type = type;
    }
    
    public ImmutableDictionary<MethodSymbol, BoundBlockStatement?> Methods => Type.MethodTable
        .Where(x=> x.Key.IsStatic)
        .ToImmutableDictionary();
    public TypeSymbol Type { get; }
}