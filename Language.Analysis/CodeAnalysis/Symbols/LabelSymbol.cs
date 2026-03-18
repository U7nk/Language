namespace Language.Analysis.CodeAnalysis.Symbols;


public class LabelSymbol
{
    public enum KindEnum
    {
        Unspecified,
        LoopStart,
        LoopBreak,
    }
    private static int _labelCount;

    public static LabelSymbol GenerateLabel(string? name = null, KindEnum kind = KindEnum.Unspecified)
    {
        if (name is not null)
            return new(name + "_" + _labelCount++, kind);
        
        return new(name ?? "Label" + "_" + _labelCount++, kind);
    }
    
    private LabelSymbol(string name, KindEnum kind)
    {
        Name = name;
        Kind = kind;
    }

    public string Name { get; }
    public KindEnum Kind { get; }
    public override string ToString()
    {
        return Name;
    }
}