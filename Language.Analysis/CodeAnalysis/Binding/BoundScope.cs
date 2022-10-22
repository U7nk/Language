using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

public class BoundScope
{
    public BoundScope? Parent { get; }
    readonly Dictionary<string, VariableSymbol> _variables = new();
    readonly Dictionary<string, FunctionSymbol> _functions = new();
    readonly Dictionary<string, TypeSymbol> _types = new();
    readonly Dictionary<string, FieldSymbol> _fields = new();

    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public bool TryDeclareVariable(VariableSymbol variable)
    {
        if (_variables.ContainsKey(variable.Name))
            return false;
        
        if (Parent?.TryLookupVariable(variable.Name, out _) is true)
            return false;
        
        _variables.Add(variable.Name, variable);
        return true;
    }
    public bool TryLookupVariable(string name, [NotNullWhen(true)] out VariableSymbol? variable)
    {
        if (_variables.TryGetValue(name, out variable))
            return true;

        if (Parent != null)
            return Parent.TryLookupVariable(name, out variable);

        variable = null;
        return false;
    }
    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        return _variables.Values.ToImmutableArray();
    }
    
    public bool TryDeclareFunction(FunctionSymbol function)
    {
        if (_functions.ContainsKey(function.Name))
            return false;
        
        if (Parent?.TryLookupFunction(function.Name, out _) is true)
            return false;
        
        _functions.Add(function.Name, function);
        return true;
    }
    public bool TryLookupFunction(string name, [NotNullWhen(true)] out FunctionSymbol? function)
    {
        if (_functions.TryGetValue(name, out var result))
        {
            result.Unwrap();
            function = result;
            return true;
        }

        if (Parent != null)
            return Parent.TryLookupFunction(name, out function);

        function = null;
        return false;
    }
    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
    {
        return _functions.Values.ToImmutableArray();
    }
    
    public bool TryDeclareType(TypeSymbol typeSymbol)
    {
        if (_types.ContainsKey(typeSymbol.Name))
            return false;
        
        if (Parent?.TryLookupType(typeSymbol.Name, out _) is true)
            return false;
        
        _types.Add(typeSymbol.Name, typeSymbol);
        return true;
    }
    public bool TryLookupType(string name, [NotNullWhen(true)] out TypeSymbol? type)
    {
        if (_types.TryGetValue(name, out type))
            return true;
        
        if (Parent?.TryLookupType(name, out type) is true)
            return Parent.TryLookupType(name, out type);
        
        return false;
    }
    public ImmutableArray<TypeSymbol> GetDeclaredTypes()
    {
        return _types.Values.ToImmutableArray();
    }
    
    public bool TryDeclareField(FieldSymbol fieldSymbol)
    {
        if (_fields.ContainsKey(fieldSymbol.Name))
            return false;
        
        if (Parent?.TryLookupField(fieldSymbol.Name, out _) is true)
            return false;
        
        _fields.Add(fieldSymbol.Name, fieldSymbol);
        return true;
    }
    public bool TryLookupField(string name, [NotNullWhen(true)] out FieldSymbol? field)
    {
        if (_fields.TryGetValue(name, out field))
            return true;
        
        if (Parent?.TryLookupField(name, out field) is true)
            return Parent.TryLookupField(name, out field);
        
        return false;
    }
    public ImmutableArray<FieldSymbol> GetDeclaredFields()
    {
        return _fields.Values.ToImmutableArray();
    }
    
}