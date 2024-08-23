using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.Extensions;


namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundScope
{
    public Option<BoundScope> Parent { get; }

    readonly Dictionary<string, List<Symbol>> _symbols = new();
    readonly List<BoundScope> _children = new();
    readonly List<NamespaceSymbol> _namespaces = new();
    readonly List<TypeSymbol> _allTypes = new();

    public static BoundScope CreateRootScope(Option<BoundScope> parent)
    {
        return new BoundScope(parent);
    }
    
    private BoundScope(Option<BoundScope> parent)
    {
        Parent = parent;
        parent.OnSome(x => x._children.Add(this));
    }
    
    private BoundScope(BoundScope parent, List<TypeSymbol> allTypes)
    {
        _allTypes = allTypes;
        Parent = parent;
        parent._children.Add(this);
    }

    public BoundScope CreateChild()
    {
        return new BoundScope(this, _allTypes);
    }

    public Option<BoundScope> GetScopeFor(Symbol symbol) => GetScopeForInternal(symbol, new List<BoundScope>());

    private Option<BoundScope> GetScopeForInternal(Symbol symbol, List<BoundScope> alreadyChecked)
    {
        if (alreadyChecked.Contains(this))
        {
            return Option.None;
        }

        var sym = _symbols.Values.SelectMany(x => x).SingleOrNone(x => Equals(x, symbol));
        if (sym.IsSome)
            return this;
        alreadyChecked.Add(this);

        var parentSearch = Parent.OnSome(x => x.GetScopeForInternal(symbol, alreadyChecked));
        if (parentSearch.IsSome)
            return parentSearch;
        foreach (var child in _children)
        {
            var childSearch = child.GetScopeForInternal(symbol, alreadyChecked);
            if (childSearch.IsSome)
                return childSearch;
        }

        return Option.None;
    }

    public bool TryDeclareVariable(VariableSymbol variable)
    {
        if (Parent.IsSome && Parent.Unwrap().TryLookupVariable(variable.Name, out _))
            return false;

        if (_symbols.TryGetValue(variable.Name, out var sameNamers))
        {
            var parameters = sameNamers.Where(x => x.Kind == SymbolKind.Parameter);
            if (parameters.Any())
                return false;

            var variables = sameNamers.Where(x => x.Kind == SymbolKind.Variable);
            if (variables.Any())
                return false;

            sameNamers.Add(variable);
        }
        else
        {
            _symbols.Add(variable.Name, new List<Symbol> { variable });
        }

        return true;
    }

    public bool TryLookupVariable(string name, [NotNullWhen(true)] out VariableSymbol? variable)
    {
        variable = null;
        if (_symbols.TryGetValue(name, out var sameNamers))
        {
            var variables = sameNamers.Where(x => x.Kind is SymbolKind.Variable or SymbolKind.Parameter).ToList();
            Debug.Assert(variables.Count <= 1);
            if (variables.Any())
            {
                variable = (VariableSymbol)variables.First();
                return true;
            }
        }

        return Parent.IsSome && Parent.Unwrap().TryLookupVariable(name, out variable);
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
        => _symbols.Values.SelectMany(x => x)
            .OfType<VariableSymbol>()
            .ToImmutableArray();

    
    public bool TryDeclareType(TypeSymbol typeSymbol, Option<NamespaceSymbol> containingNamespace, bool isScopeTied = false)
    {
        if (Parent.OnSome(x => x.TryLookupType(typeSymbol.Name, containingNamespace)).IsSome)
            return false;

        if (_symbols.TryGetValue(typeSymbol.Name, out var sameNamers))
        {
            var functionsAndFieldsAndTypes = sameNamers.Where(
                x => x.Kind
                    is SymbolKind.Method
                    or SymbolKind.Field
                    or SymbolKind.Type);

            if (functionsAndFieldsAndTypes.Any())
                return false;

            sameNamers.Add(typeSymbol);
        }
        else
        {
            _symbols.Add(typeSymbol.Name, new List<Symbol> { typeSymbol });
        }

        if (!isScopeTied)
        {
            _allTypes.Add(typeSymbol);
        }

        return true;
    }

    public Option<TypeSymbol> TryLookupType(string name, Option<NamespaceSymbol> containingNamespace)
    {
        var isTypeMatchByName = new Func<TypeSymbol, bool> ((x) => x.GetFullName() == name || x.GetFullName() == containingNamespace.Unwrap().FullName + "." + name);
        
        var scoped = _symbols.Values.SelectMany(x=>x).Where(x=> x.Kind is SymbolKind.Type).Cast<TypeSymbol>().SingleOrNone(isTypeMatchByName);
        if (scoped.IsSome)
            return scoped;
        
        if (containingNamespace.IsSome)
        {
            var type = _allTypes.SingleOrNone(isTypeMatchByName);
            if (type.IsSome)
                return type;
        }
        
        foreach (var type in _allTypes)
        {
            if (isTypeMatchByName(type))
            {
                return type;
            }
        }

        return Parent.OnSome(x => x.TryLookupType(name, containingNamespace));
    }

    public ImmutableArray<TypeSymbol> GetDeclaredTypes()
    {
        if (Parent.IsSome)
        {
            var symbols = Parent.Unwrap().GetDeclaredTypes();
            _namespaces
                .SelectMany(x => x.GetAllTypesFromNamespaces())
                .ToImmutableArray()
                .AddRangeTo(symbols);
            return symbols;
        }

        return _namespaces
            .SelectMany(x => x.GetAllTypesFromNamespaces())
            .Distinct()
            .ToImmutableArray();
    }

    public ImmutableArray<TypeSymbol> GetDeclaredTypesCurrentScope()
    {
        return _symbols.Values.SelectMany(x => x)
            .OfType<TypeSymbol>()
            .ToImmutableArray();
    }

    public Option<NamespaceSymbol> TryLookupNamespace(string name)
    {
        if (_namespaces.SingleOrNone(x => x.FullName == name) is { IsSome: true } ns)
        {
            return ns;
        }

        return Parent.OnSome(parent => parent.TryLookupNamespace(name));
    }

    public bool TryDeclareNamespace(NamespaceSymbol namespaceSymbol)
    {
        var sameNamers = _namespaces.Where(x => x.FullName == namespaceSymbol.FullName).ToList();
        if (sameNamers.Any())
        {
            return false;
        }

        _namespaces.Add(namespaceSymbol);
        return true;
    }
}