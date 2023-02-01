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
    
    public ImmutableArray<MethodDeclaration> Methods => Type.MethodTable
        .Where(x=> x.MethodSymbol.IsStatic)
        .ToImmutableArray();
    public TypeSymbol Type { get; }
}