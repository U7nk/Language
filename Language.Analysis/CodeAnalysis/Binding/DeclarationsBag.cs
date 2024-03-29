using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding;


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
    
    /// <summary>
    /// Retrieves all declarations(including redeclaration) for the given bound node. <br/>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>List of declarations</returns>
    public ImmutableArray<SyntaxNode> LookupDeclarations(Symbol symbol) =>
        this.TryGetValue(symbol, out var declarations) 
            ? declarations.ToImmutableArray() 
            : ImmutableArray<SyntaxNode>.Empty;

    /// <summary>
    /// Retrieves all declarations(including redeclaration) for the given bound node. And casts them to T.<br/>
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns>List of declarations</returns>
    public ImmutableArray<T> LookupDeclarations<T>(Symbol symbol) where T : SyntaxNode =>
        this.TryGetValue(symbol, out var declarations) 
            ? declarations.Cast<T>().ToImmutableArray() 
            : ImmutableArray<T>.Empty;

    public void AddDeclaration(Symbol boundNode, SyntaxNode declaration)
    {
        if (!this.TryGetValue(boundNode, out var declarations))
        {
            declarations = new List<SyntaxNode>();
            this.Add(boundNode, declarations);
        }
        
        declarations.Add(declaration);
    }
}