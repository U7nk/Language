namespace Language.Analysis.CodeAnalysis.Symbols;

public abstract class MemberSymbol : Symbol
{
    protected MemberSymbol(string name, TypeSymbol type) : base(name)
    {
        Type = type;
    }
    public TypeSymbol Type { get; }
}

public class FieldSymbol : MemberSymbol
{
    public FieldSymbol(string name, TypeSymbol type) : base(name, type)
    { }
    
    public override SymbolKind Kind => SymbolKind.Field;
}