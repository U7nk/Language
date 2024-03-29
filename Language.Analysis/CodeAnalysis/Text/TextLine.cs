namespace Language.Analysis.CodeAnalysis.Text;

public sealed class TextLine
{
    public TextLine(SourceText sourceText, int start, int length, int lengthWithLineBreak)
    {
        SourceText = sourceText;
        Start = start;
        Length = length;
        LengthWithLineBreak = lengthWithLineBreak;
    }

    public SourceText SourceText { get; }
    public TextSpan Span => new TextSpan(Start, Length);
    public TextSpan SpanWithLineBreak => new TextSpan(Start, LengthWithLineBreak);
    public int Start { get; }
    public int End => Start + Length;

    public int Length { get; }

    public int LengthWithLineBreak { get; }
    public override string ToString() => SourceText.ToString(Span);
}