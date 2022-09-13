namespace Wired.CodeAnalysis;

public class ParameterSymbol : VariableSymbol
{
    public TypeSymbol Type { get; }
    public override SymbolKind Kind => SymbolKind.Parameter;

    internal ParameterSymbol(string name, TypeSymbol type)
        : base(name, type, isReadonly: true)
    {
        Type = type;
    }
}