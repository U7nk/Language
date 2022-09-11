using System.Collections.Immutable;

namespace Wired.CodeAnalysis;

public class FunctionSymbol : Symbol
{
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType { get; }
    public override SymbolKind Kind => SymbolKind.Function;

    public FunctionSymbol(string name,
        ImmutableArray<ParameterSymbol> parameters,
        TypeSymbol returnType)
        : base(name)
    {
        this.Parameters = parameters;
        this.ReturnType = returnType;
    }
}