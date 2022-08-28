using System;

namespace Wired.CodeAnalysis;

public sealed class VariableSymbol
{
    public string Name { get; }
    public Type Type { get; }
    public VariableSymbol(string name, Type type)
    {
        this.Name = name;
        this.Type = type;
    }
}