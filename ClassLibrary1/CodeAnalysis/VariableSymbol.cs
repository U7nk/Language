using System;

namespace Wired.CodeAnalysis;

public class VariableSymbol : Symbol
{
    public TypeSymbol Type { get; }
    public bool IsReadonly { get; }
    public VariableSymbol(string name, TypeSymbol type, bool isReadonly) : base(name)
    {
        this.Type = type;
        this.IsReadonly = isReadonly;
    }

    public override SymbolKind Kind => SymbolKind.Variable;
    public override string ToString() => $"{Type}:{Name}";
}