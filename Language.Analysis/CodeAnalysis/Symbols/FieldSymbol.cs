namespace Language.CodeAnalysis.Symbols;

public class FieldSymbol : Symbol
{
    public FieldSymbol(string name, TypeSymbol type) : base(name)
    {
        Type = type;
    }

    public TypeSymbol Type { get; }
    public override SymbolKind Kind => SymbolKind.Field;
}