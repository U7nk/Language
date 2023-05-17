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
            Option.None,
            isStatic: true,
            isVirtual: false,
            isOverriding: false,
            name: "print",
            parameters: ImmutableArray.Create(new ParameterSymbol(Option.None, "text", 
                                                                  containingType: null,
                                                                  BuiltInTypeSymbols.String)),
            returnType: BuiltInTypeSymbols.Void,
            containingType: null,
            isGeneric: false,
            genericParameters: Option.None);

    public static readonly MethodSymbol
        Input = new(
            Option.None,
            isStatic: true,
            isVirtual: false,
            isOverriding: false,
            name: "input",
            parameters: ImmutableArray<ParameterSymbol>.Empty,
            returnType: BuiltInTypeSymbols.String, 
            containingType: null,
            isGeneric: false,
            genericParameters: Option.None);

    public static IEnumerable<MethodSymbol> GetAll() =>
        typeof(BuiltInMethods).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(MethodSymbol))
            .Select(f => f.GetValue(null))
            .Cast<MethodSymbol>();
}