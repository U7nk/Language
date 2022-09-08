using System;

namespace Wired.CodeAnalysis;

public sealed class VariableSymbol
{
    public string Name { get; }
    public Type Type { get; }
    public bool IsReadonly { get; }
    public VariableSymbol(string name, Type type, bool isReadonly)
    {
        this.Name = name;
        this.Type = type;
        this.IsReadonly = isReadonly;
    }
    
    public override string ToString() => $"{Type}:{Name}";
}