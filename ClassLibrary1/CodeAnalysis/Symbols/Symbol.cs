using System;

namespace Wired.CodeAnalysis.Symbols;

public abstract class Symbol
{
    private protected Symbol(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public abstract SymbolKind Kind { get; }

    public override string ToString() => Name;
    
    bool Equals(Symbol other)
    {
        return Name == other.Name && Kind == other.Kind;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) 
            return false;
            
        if (ReferenceEquals(this, obj)) 
            return true;
            
        if (obj.GetType() != GetType()) 
            return false;
        
        return Equals((Symbol)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, (int)Kind);
    }
}