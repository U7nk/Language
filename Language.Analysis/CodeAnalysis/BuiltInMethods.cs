using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis;

internal static class BuiltInMethods
{
    public static readonly MethodSymbol Print = new("print",
        ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.String)), TypeSymbol.Void, null);

    public static readonly MethodSymbol
        Input = new("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String, null);

    public static IEnumerable<MethodSymbol> GetAll() =>
        typeof(BuiltInMethods).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(MethodSymbol))
            .Select(f => f.GetValue(null))
            .Cast<MethodSymbol>();
}