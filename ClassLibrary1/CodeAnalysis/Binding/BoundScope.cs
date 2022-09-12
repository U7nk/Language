using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Wired.CodeAnalysis.Binding;

internal class BoundScope
{
    public BoundScope? Parent { get; }
    private readonly Dictionary<string, VariableSymbol> variables = new();
    private readonly Dictionary<string, FunctionSymbol> functions = new();

    public BoundScope(BoundScope? parent)
    {
        this.Parent = parent;
    }

    public bool TryDeclareVariable(VariableSymbol variable)
    {
        if (this.variables.ContainsKey(variable.Name))
            return false;
        
        if (this.Parent?.TryLookupVariable(variable.Name, out _) is true)
            return false;
        
        this.variables.Add(variable.Name, variable);
        return true;
    }
    
    public bool TryDeclareFunction(FunctionSymbol function)
    {
        if (this.functions.ContainsKey(function.Name))
            return false;
        
        if (this.Parent?.TryLookupFunction(function.Name, out _) is true)
            return false;
        
        this.functions.Add(function.Name, function);
        return true;
    }
    
    public bool TryLookupVariable(string name, [NotNullWhen(true)] out VariableSymbol? variable)
    {
        if (this.variables.TryGetValue(name, out var result))
        {
            result.ThrowIfNull();
            variable = result;
            return true;
        }

        if (this.Parent != null)
            return this.Parent.TryLookupVariable(name, out variable);

        variable = null;
        return false;
    }
    
    public bool TryLookupFunction(string name, [NotNullWhen(true)] out FunctionSymbol? function)
    {
        if (this.functions.TryGetValue(name, out var result))
        {
            result.ThrowIfNull();
            function = result;
            return true;
        }

        if (this.Parent != null)
            return this.Parent.TryLookupFunction(name, out function);

        function = null;
        return false;
    }
    
    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        return this.variables.Values.ToImmutableArray();
    }
}