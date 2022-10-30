using System.Collections.Immutable;

namespace Language.Analysis.CodeAnalysis.Text;

public class SourceText
{
    public string FileName { get; }
    readonly string _text;
    public ImmutableArray<TextLine> Lines { get; }

    SourceText(string text, string fileName)
    {
        FileName = fileName;
        _text = text;
        Lines = ParseLines(this, text);
    }

    public int Length => _text.Length;
    public char this[int index]
        => _text[index];

    public static SourceText From(string text, string fileName = "") 
        => new SourceText(text, fileName);
    
    

    public int GetLineIndex(int position)
    {
        for (var i = 0; i < Lines.Length; i++)
        {
            var line = Lines[i];
            if (position.InRange(line.Start, line.End))
            {
                return i;
            }
        }

        return -1;
    }

    public override string ToString() 
        => _text;

    public string ToString(int start, int length) 
        => _text.Substring(start, length);
    public string ToString(TextSpan span) 
        => _text.Substring(span.Start, span.Length);

    static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
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

    static void AddLine(ImmutableArray<TextLine>.Builder result,SourceText text, int lineStart, int position,
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