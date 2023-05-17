using System;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

interface ITypedSymbol
{ 
    TypeSymbol Type { get; }
}
public abstract class Symbol
{
    private protected Symbol(Option<SyntaxNode> declarationSyntax, string name, Option<TypeSymbol> containingType)
    {
        DeclarationSyntax = declarationSyntax;
        Name = name;
        ContainingType = containingType;
    }

    public Option<SyntaxNode> DeclarationSyntax { get; private set; }
    public Option<TypeSymbol> ContainingType { get; private set; }
    public string Name { get; }
    public abstract SymbolKind Kind { get; }
    public override string ToString() => Name;

    public virtual bool DeclarationEquals(Symbol other) => Equals(other);
    public virtual int DeclarationHashCode() => GetHashCode();
    
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
        => HashCode.Combine(Name, (int)Kind);
}