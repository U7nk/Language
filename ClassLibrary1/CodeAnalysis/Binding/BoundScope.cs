using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

internal class BoundScope
{
    public BoundScope? Parent { get; }
    readonly Dictionary<string, VariableSymbol> _variables = new();
    readonly Dictionary<string, FunctionSymbol> _functions = new();

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
    
    public bool TryDeclareFunction(FunctionSymbol function)
    {
        if (_functions.ContainsKey(function.Name))
            return false;
        
        if (Parent?.TryLookupFunction(function.Name, out _) is true)
            return false;
        
        _functions.Add(function.Name, function);
        return true;
    }
    
    public bool TryLookupVariable(string name, [NotNullWhen(true)] out VariableSymbol? variable)
    {
        if (_variables.TryGetValue(name, out var result))
        {
            result.ThrowIfNull();
            variable = result;
            return true;
        }

        if (Parent != null)
            return Parent.TryLookupVariable(name, out variable);

        variable = null;
        return false;
    }
    
    public bool TryLookupFunction(string name, [NotNullWhen(true)] out FunctionSymbol? function)
    {
        if (_functions.TryGetValue(name, out var result))
        {
            result.ThrowIfNull();
            function = result;
            return true;
        }

        if (Parent != null)
            return Parent.TryLookupFunction(name, out function);

        function = null;
        return false;
    }
    
    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        return _variables.Values.ToImmutableArray();
    }

    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
    {
        return _functions.Values.ToImmutableArray();
    }
}