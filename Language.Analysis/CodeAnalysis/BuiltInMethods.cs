using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis;

internal static class BuiltInMethods
{
    public static readonly MethodSymbol 
        Print = new(
            ImmutableArray<SyntaxNode>.Empty,
            "print",
            ImmutableArray.Create(new ParameterSymbol(ImmutableArray<SyntaxNode>.Empty, "text", TypeSymbol.String)),
            TypeSymbol.Void);

    public static readonly MethodSymbol
        Input = new(
            ImmutableArray<SyntaxNode>.Empty, 
            "input",
            ImmutableArray<ParameterSymbol>.Empty,
            TypeSymbol.String);

    public static IEnumerable<MethodSymbol> GetAll() =>
        typeof(BuiltInMethods).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(MethodSymbol))
            .Select(f => f.GetValue(null))
            .Cast<MethodSymbol>();
}