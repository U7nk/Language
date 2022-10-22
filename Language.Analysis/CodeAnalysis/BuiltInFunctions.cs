using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis;

internal static class BuiltInFunctions
{
    public static readonly FunctionSymbol Print = new("print",
        ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.String)), TypeSymbol.Void, null);

    public static readonly FunctionSymbol
        Input = new("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String, null);

    public static IEnumerable<FunctionSymbol> GetAll() =>
        typeof(BuiltInFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(FunctionSymbol))
            .Select(f => f.GetValue(null))
            .Cast<FunctionSymbol>();
}