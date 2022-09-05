using System.Collections.Immutable;
using System.Linq;

namespace Wired.CodeAnalysis.Text;

public class SourceText
{
    private readonly string text;
    public  ImmutableArray<TextLine> Lines { get; set; }

    private SourceText(string text)
    {
        this.text = text;
        this.Lines = ParseLines(this, text);
    }

    public int Length => this.text.Length;
    public char this[int index]
        => this.text[index];

    public int GetLineIndex(int position)
    {
        for (var i = 0; i < this.Lines.Length; i++)
        {
            var line = this.Lines[i];
            if (position.InRange(line.Start, line.End))
            {
                return i;
            }
        }

        return -1;
    }

    public override string ToString() 
        => this.text;

    public string ToString(int start, int length) 
        => this.text.Substring(start, length);
    public string ToString(TextSpan span) 
        => this.text.Substring(span.Start, span.Length);

    public static SourceText From(string text)
    {
        return new SourceText(text);
    }
    private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
    {
        var result = ImmutableArray.CreateBuilder<TextLine>();
        var lineStart = 0;
        var position = 0;
        while (position < text.Length)
        {
            var lineBreakLength = GetLineBreakLength(text, position);
            if (lineBreakLength is 0)
            {
                position++;
            }
            else
            {
                AddLine(result, sourceText, lineStart, position, lineBreakLength);
                position += lineBreakLength;
                lineStart = position;
            }
        }
        if (position >= lineStart)
        {
            AddLine(result, sourceText, lineStart, position, 0);
        }

        return result.ToImmutable();
    }

    private static void AddLine(ImmutableArray<TextLine>.Builder result,SourceText text, int lineStart, int position,
        int lineBreakLength)
    {
        var lineLength = position - lineStart;
        var lineLengthWithLineBreak = lineLength + lineBreakLength;
        result.Add(new TextLine(text, lineStart,lineLength, lineLengthWithLineBreak));
    }

    public static int GetLineBreakLength(string text, int i)
    {
        var c = text[i];
        var l = i + 1 >= text.Length ? '\0' : text[i + 1];

        return c switch
        {
            '\r' when l is '\n' => 2,
            '\r' or '\n' => 1,
            _ => 0
        };
    }
}

public sealed class TextLine
{
    public TextLine(SourceText sourceText, int start, int length, int lengthWithLineBreak)
    {
        this.SourceText = sourceText;
        this.Start = start;
        this.Length = length;
        this.LengthWithLineBreak = lengthWithLineBreak;
    }

    public SourceText SourceText { get; }
    public TextSpan Span => new TextSpan(this.Start, this.Length);
    public TextSpan SpanWithLineBreak => new TextSpan(this.Start, this.LengthWithLineBreak);
    public int Start { get; }
    public int End => this.Start + this.Length;

    public int Length { get; }

    public int LengthWithLineBreak { get; }
}