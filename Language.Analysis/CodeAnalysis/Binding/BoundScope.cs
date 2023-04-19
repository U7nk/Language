using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundScope
{
    public BoundScope? Parent { get; }
    
    readonly Dictionary<string, List<Symbol>> _symbols = new();

    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public bool TryDeclareVariable(VariableSymbol variable)
    {
        if (Parent?.TryLookupVariable(variable.Name, out _) is true)
            return false;
        
        if (_symbols.TryGetValue(variable.Name, out var sameNamers))
        {
            var parameters = sameNamers.Where(x=> x.Kind == SymbolKind.Parameter);
            if (parameters.Any())
                return false;
        
            var variables = sameNamers.Where(x=> x.Kind == SymbolKind.Variable);
            if (variables.Any())
                return false;

            sameNamers.Add(variable);
        }
        else
        {
            _symbols.Add(variable.Name, new List<Symbol> {variable});
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

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (Parent != null)
            return Parent.TryLookupVariable(name, out variable);
        
        return false;
    }
    public ImmutableArray<VariableSymbol> GetDeclaredVariables() 
        => _symbols.Values.SelectMany(x => x)
            .OfType<VariableSymbol>()
            .ToImmutableArray();

    public bool TryDeclareType(TypeSymbol typeSymbol)
    {
        if (Parent?.TryLookupType(typeSymbol.Name, out _) is true)
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
            _symbols.Add(typeSymbol.Name,new List<Symbol>{ typeSymbol });    
        }
        
        return true;
    }
    
    public bool TryLookupType(string name, [NotNullWhen(true)] out TypeSymbol? type)
    {
        type = null;
        if (_symbols.TryGetValue(name, out var sameNamers))
        {
            var types = sameNamers.Where(x => x.Kind == SymbolKind.Type).ToList();
            Debug.Assert(types.Count <= 1);
            if (types.Any())
            {
                type = (TypeSymbol)types.First();
                return true;
            }
        }

        if (Parent?.TryLookupType(name, out type) is true)
            return Parent.TryLookupType(name, out type);
        
        return false;
    }
    public ImmutableArray<TypeSymbol> GetDeclaredTypes()
    {
        if (Parent is { })
        {
            var symbols = Parent.GetDeclaredTypes();
            return symbols.AddRange(_symbols.Values.SelectMany(x => x)
                .OfType<TypeSymbol>()
                .ToImmutableArray());
        }
        
        return _symbols.Values.SelectMany(x => x)
            .OfType<TypeSymbol>()
            .ToImmutableArray();
    }

    public ImmutableArray<TypeSymbol> GetDeclaredTypesCurrentScope()
    {
        return _symbols.Values.SelectMany(x => x)
            .OfType<TypeSymbol>()
            .ToImmutableArray();
    }
}