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
            isStatic: true,
            name: "print",
            parameters: ImmutableArray.Create(new ParameterSymbol(ImmutableArray<SyntaxNode>.Empty, "text", 
                                                                  containingType: null,
                                                                  TypeSymbol.String)),
            returnType: TypeSymbol.Void, 
            containingType: null);

    public static readonly MethodSymbol
        Input = new(
            ImmutableArray<SyntaxNode>.Empty,
            isStatic: true,
            name: "input",
            parameters: ImmutableArray<ParameterSymbol>.Empty,
            returnType: TypeSymbol.String, 
            containingType: null);

    public static IEnumerable<MethodSymbol> GetAll() =>
        typeof(BuiltInMethods).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.FieldType == typeof(MethodSymbol))
            .Select(f => f.GetValue(null))
            .Cast<MethodSymbol>();
}