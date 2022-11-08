using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public sealed class ParameterSymbol : VariableSymbol
{
    public override SymbolKind Kind => SymbolKind.Parameter;

    internal ParameterSymbol(ImmutableArray<SyntaxNode> declarationSyntax, string name, TypeSymbol? containingType,
                             TypeSymbol? parameterType)
        : base(declarationSyntax, name, containingType, parameterType, isReadonly: true)
    {
    }
}