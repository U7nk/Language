namespace Wired.CodeAnalysis.Binding;

internal class LabelSymbol
{
    public LabelSymbol(string name)
    {
        this.Name = name;
    }

    public string Name { get; }

    public override string ToString()
    {
        return this.Name;
    }
}