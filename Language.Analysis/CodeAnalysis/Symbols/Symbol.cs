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
    
    private protected Symbol(ImmutableArray<SyntaxNode> declarationSyntax, string name, TypeSymbol? containingType)
    {
        DeclarationSyntax = declarationSyntax;
        Name = name;
        ContainingType = containingType;
    }

    public ImmutableArray<SyntaxNode> DeclarationSyntax { get; private set; }
    public TypeSymbol? ContainingType { get; private set; }
    public string Name { get; }
    public abstract SymbolKind Kind { get; }

    public void AddDeclaration(SyntaxNode syntax)
        => DeclarationSyntax = DeclarationSyntax.Add(syntax);
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
        => HashCode.Combine(Name, (int)Kind);

    public static bool operator==(Symbol? left, Symbol? right) 
        => Equals(left, right);

    public static bool operator !=(Symbol? left, Symbol? right) 
        => !(left == right);
}