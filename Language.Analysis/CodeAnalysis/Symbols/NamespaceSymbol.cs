using System.Collections.Generic;
using System.Linq;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class NamespaceSymbol : Symbol
{
    internal NamespaceSymbol(Option<SyntaxNode> declarationSyntax, string name, string fullName, List<TypeSymbol> types, Option<NamespaceSymbol> parent, List<NamespaceSymbol> children) : base(declarationSyntax, name)
    {
        FullName = fullName;
        Types = types;
        Parent = parent;
        Children = children;
    }
    
    public override SymbolKind Kind => SymbolKind.Namespace;
    public new Option<NamespaceSyntax> DeclarationSyntax => base.DeclarationSyntax.IsSome 
        ? base.DeclarationSyntax.UnwrapAs<NamespaceSyntax>() 
        : Option.None;
    public List<TypeSymbol> Types { get; }
    public Option<NamespaceSymbol> Parent { get; }
    public List<NamespaceSymbol> Children { get; }

    public Option<TypeSymbol> LookupType(string name)
    {
        foreach (var type in this.Types)
        {
            if (type.Name == name)
                return type;
        }

        return Parent.OnSome(x => x.LookupType(name));
    }
    
    /// <summary>
    /// get types from current and all children namespaces
    /// </summary>
    public List<TypeSymbol> GetAllTypesFromNamespaces()
    {
        IEnumerable<TypeSymbol> types = Types;
        foreach (var child in Children)
        {
            types = types.Concat(child.GetAllTypesFromNamespaces());
        }

        return types.ToList();
    }
    public string FullName { get; }
}