namespace Language.CodeAnalysis.Text;

public struct TextSpan
{
    public int Start { get; }
    public int Length { get; }
    public int End => Start + Length;
    public TextSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public override string ToString() => $"{Start}..{End}";

    public static TextSpan FromBounds(int firstStart, int lastEnd) => new TextSpan(firstStart, lastEnd - firstStart);
}