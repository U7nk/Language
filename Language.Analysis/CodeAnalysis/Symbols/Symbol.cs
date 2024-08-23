using System;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

internal interface ISymbol
{
    
}

internal interface ITypedSymbol : ISymbol
{ 
    TypeSymbol Type { get; }
}

internal interface ITypeMemberSymbol
{
    public Option<TypeSymbol> ContainingType { get; }
}

public abstract class Symbol : ISymbol
{
    private protected Symbol(Option<SyntaxNode> declarationSyntax, string name)
    {
        DeclarationSyntax = declarationSyntax;
        Name = name;
    }

    public Option<SyntaxNode> DeclarationSyntax { get; private set; }
    
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