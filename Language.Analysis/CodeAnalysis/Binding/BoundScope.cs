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

    public bool TryDeclareMethod(MethodSymbol method, TypeSymbol containingType)
    {
        if (containingType.Name == method.Name)
            return false;
        
        if (Parent?.TryLookupField(method.Name, out _) is true)
            return false;
        
        if (Parent?.TryLookupMethod(method.Name, out _) is true)
            return false;   
        
        if (_symbols.TryGetValue(method.Name, out var sameNamers))
        {
            var functionsAndFieldsAndTypes = sameNamers.Where(
                x => x.Kind 
                    is SymbolKind.Method 
                    or SymbolKind.Field 
                    or SymbolKind.Type);
            
            if (functionsAndFieldsAndTypes.Any())
                return false;
            
            sameNamers.Add(method);
        }
        else
        {
            _symbols.Add(method.Name,new List<Symbol>{ method });    
        }
        
        containingType.MethodTable.Add(method, null);
        return true;
    }
    public bool TryLookupMethod(string name, [NotNullWhen(true)] out MethodSymbol? function)
    {
        function = null;
        
        if (_symbols.TryGetValue(name, out var sameNamers))
        {
            var functions = sameNamers.Where(x => x.Kind == SymbolKind.Method).ToList();
            Debug.Assert(functions.Count <= 1);
            if (functions.Any())
            {
                function = (MethodSymbol)functions.First();
                return true;
            }
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (Parent != null)
            return Parent.TryLookupMethod(name, out function);
        
        return false;
    }
    public ImmutableArray<MethodSymbol> GetDeclaredMethods() 
        => _symbols.Values.SelectMany(x => x)
            .OfType<MethodSymbol>()
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
        => _symbols.Values.SelectMany(x=> x)
            .OfType<TypeSymbol>()
            .ToImmutableArray();

    public bool TryDeclareField(FieldSymbol fieldSymbol, TypeSymbol containingType)
    {
        if (containingType.Name == fieldSymbol.Name)
            return false;
        
        if (containingType.MethodTable.Symbols.Any(x => x.Name == fieldSymbol.Name))
            return false;
        
        if (Parent?.TryLookupField(fieldSymbol.Name, out _) is true)
            return false;

        if (_symbols.TryGetValue(fieldSymbol.Name, out var sameNamers))
        {
            var functionsAndFieldsAndTypes = sameNamers.Where(
                x => x.Kind 
                    is SymbolKind.Method 
                    or SymbolKind.Field 
                    or SymbolKind.Type);
            
            if (functionsAndFieldsAndTypes.Any())
                return false;
            
            sameNamers.Add(fieldSymbol);
        }
        else
        {
            _symbols.Add(fieldSymbol.Name,new List<Symbol>{ fieldSymbol });    
        }
        
        containingType.FieldTable.Add(fieldSymbol);
        return true;
    }
    
    public bool TryLookupField(string name, [NotNullWhen(true)] out FieldSymbol? field)
    {
        field = null;
        if (_symbols.TryGetValue(name, out var sameNamers))
        {
            var fields = sameNamers.Where(x => x.Kind == SymbolKind.Field).ToList();
            Debug.Assert(fields.Count <= 1);
            if (fields.Any())
            {
                field = (FieldSymbol)fields.First();
                return true;
            }
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (Parent?.TryLookupField(name, out field) is true)
            return Parent.TryLookupField(name, out field);
        
        return false;
    }
    public ImmutableArray<FieldSymbol> GetDeclaredFields() 
        => _symbols.Values.SelectMany(x => x)
            .OfType<FieldSymbol>()
            .ToImmutableArray();
}