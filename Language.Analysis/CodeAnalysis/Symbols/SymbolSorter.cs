using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class SymbolSorter
{
    /// <summary>
    /// Sorting symbols in this order:
    /// <see cref="VariableSymbol"/>,
    /// <see cref="ParameterSymbol"/>,
    /// <see cref="FieldSymbol"/>,
    /// <see cref="MethodSymbol"/>,
    /// <see cref="TypeSymbol"/>.
    /// </summary>
    public static ImmutableArray<Symbol> GetSorted(IEnumerable<Symbol> symbols)
    {
        var symbolsList = symbols.ToList();
        var variables = symbolsList.OfType<VariableSymbol>().ToList();
        var parameters = symbolsList.OfType<ParameterSymbol>().ToList();
        var fields = symbolsList.OfType<FieldSymbol>().ToList();
        var methods = symbolsList.OfType<MethodSymbol>().ToList();
        var types = symbolsList.OfType<TypeSymbol>().ToList();
        var sorted = variables.Concat<Symbol>(parameters).Concat(fields).Concat(methods).Concat(types).ToList();
        return sorted.ToImmutableArray();
    }
}