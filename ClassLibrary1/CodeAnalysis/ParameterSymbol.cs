namespace Wired.CodeAnalysis;

public sealed class ParameterSymbol : VariableSymbol
{
    public override SymbolKind Kind => SymbolKind.Parameter;

    internal ParameterSymbol(string name, TypeSymbol type)
        : base(name, type, isReadonly: true)
    {
    }
}