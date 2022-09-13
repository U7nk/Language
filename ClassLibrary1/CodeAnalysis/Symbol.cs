namespace Wired.CodeAnalysis;

public abstract class Symbol
{
    private protected Symbol(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public abstract SymbolKind Kind { get; }

    public override string ToString() => Name;
}