using System;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class VariableSymbol : Symbol
{
    public TypeSymbol Type { get; }
    public bool IsReadonly { get; }
    public VariableSymbol(string name, TypeSymbol type, bool isReadonly) : base(name)
    {
        Type = type;
        IsReadonly = isReadonly;
    }

    public override SymbolKind Kind => SymbolKind.Variable;
    public override string ToString() => $"{Type}:{Name}";

    bool Equals(VariableSymbol other)
    {
        return Type.Equals(other.Type) 
               && IsReadonly == other.IsReadonly 
               && base.Equals(other);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) 
            return false;
        
        if (ReferenceEquals(this, obj)) 
            return true;
        
        if (obj.GetType() != GetType())
            return false;
        
        return Equals((VariableSymbol)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, IsReadonly);
    }
}