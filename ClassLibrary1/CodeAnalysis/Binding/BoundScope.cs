using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Wired.CodeAnalysis.Binding;

internal class BoundScope
{
    public BoundScope? Parent { get; }
    private readonly Dictionary<string, VariableSymbol> variables = new();

    public BoundScope(BoundScope? parent)
    {
        this.Parent = parent;
    }

    public bool TryDeclareVariable(VariableSymbol variable)
    {
        if (this.variables.ContainsKey(variable.Name))
            return false;
        
        this.variables.Add(variable.Name, variable);
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
    
    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        return this.variables.Values.ToImmutableArray();
    }
}