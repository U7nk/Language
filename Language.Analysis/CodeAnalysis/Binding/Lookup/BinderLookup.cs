using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Lookup;


public class DeclarationsBag : Dictionary<Symbol, List<SyntaxNode>>
{
    class DeclarationEqualityComparer : IEqualityComparer<Symbol>
    {
        public bool Equals(Symbol? left, Symbol? right)
        {
            left.NullGuard("left symbol cannot be null");
            right.NullGuard("right symbol cannot be null");
            
            return left.DeclarationEquals(right);
        }

        public int GetHashCode(Symbol obj)
        {
            return obj.DeclarationHashCode();
        }
    }
    public DeclarationsBag() : base(new DeclarationEqualityComparer())
    {
    }
}

public class BinderLookup
{
    public BinderLookup(DeclarationsBag declarationsBag)
    {
    
        Declarations = declarationsBag;
    }
    
    protected internal DeclarationsBag Declarations { get; }

    /// <summary>
    /// Retrieves all declarations(including redeclaration) for the given bound node. <br/>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>List of declarations</returns>
    public ImmutableArray<SyntaxNode> LookupDeclarations(Symbol symbol) =>
        Declarations.TryGetValue(symbol, out var declarations) 
            ? declarations.ToImmutableArray() 
            : ImmutableArray<SyntaxNode>.Empty;

    /// <summary>
    /// Retrieves all declarations(including redeclaration) for the given bound node. And casts them to T.<br/>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>List of declarations</returns>
    public ImmutableArray<T> LookupDeclarations<T>(Symbol symbol) where T : SyntaxNode =>
        Declarations.TryGetValue(symbol, out var declarations) 
            ? declarations.Cast<T>().ToImmutableArray() 
            : ImmutableArray<T>.Empty;

    public void AddDeclaration(Symbol boundNode, SyntaxNode declaration)
    {
        if (!Declarations.TryGetValue(boundNode, out var declarations))
        {
            declarations = new List<SyntaxNode>();
            Declarations.Add(boundNode, declarations);
        }
        
        declarations.Add(declaration);
    }

    public static ImmutableArray<Symbol> LookupSymbols(string name, BoundScope scope, TypeSymbol type)
    {
        var symbols = new List<Symbol>();
        var methods = type.LookupMethod(name);
        symbols.AddRange(methods);

        var field = type.LookupField(name);
        if (field is { })
            symbols.Add(field);
        
        scope.TryLookupVariable(name, out var variable);
        if (variable != null) 
            symbols.Add(variable);

        scope.TryLookupType(name, out var scopeType);
        if (scopeType != null)
            symbols.Add(scopeType);
        
        return symbols.ToImmutableArray();
    }
}
