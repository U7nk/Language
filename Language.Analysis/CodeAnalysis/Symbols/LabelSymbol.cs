namespace Language.CodeAnalysis.Symbols;

internal class LabelSymbol
{
    public LabelSymbol(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public override string ToString()
    {
        return Name;
    }
}