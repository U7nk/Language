using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class FunctionSymbol : Symbol
{
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType { get; }
    public FunctionDeclarationSyntax? Declaration { get; }
    public override SymbolKind Kind => SymbolKind.Function;

    public FunctionSymbol(string name,
        ImmutableArray<ParameterSymbol> parameters,
        TypeSymbol returnType, FunctionDeclarationSyntax? declaration)
        : base(name)
    {
        Parameters = parameters;
        ReturnType = returnType;
        Declaration = declaration;
    }
}