using System;
using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class VariableSymbol(Option<SyntaxNode> declarationSyntax,
                            string name, TypeSymbol type, bool isReadonly) : Symbol(declarationSyntax, name), ITypedSymbol
{
    
    public TypeSymbol Type { get; } = type;
    public bool IsReadonly { get; } = isReadonly;

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