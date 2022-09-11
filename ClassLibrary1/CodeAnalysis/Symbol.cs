namespace Wired.CodeAnalysis;

public abstract class Symbol
{
    private protected Symbol(string name)
    {
        
        this.Name = name;
    }

    public string Name { get; }

    public override string ToString() => this.Name;
}