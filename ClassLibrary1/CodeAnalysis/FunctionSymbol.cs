using System.Collections.Immutable;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis;

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