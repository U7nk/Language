using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public sealed class ParameterSymbol : VariableSymbol
{
    public override SymbolKind Kind => SymbolKind.Parameter;

    internal ParameterSymbol(Option<SyntaxNode> declarationSyntax, string name, TypeSymbol parameterType)
        : base(declarationSyntax, name, parameterType, isReadonly: true)
    {
    }
}