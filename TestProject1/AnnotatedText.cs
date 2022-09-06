using System.Collections.Immutable;
using System.Text;
using Wired.CodeAnalysis.Text;

namespace TestProject1;

internal class AnnotatedText
{
    public string Text { get; }
    public ImmutableArray<TextSpan> Spans { get; }

    public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
    {
        this.Text = text;
        this.Spans = spans;
    }

    public static AnnotatedText Parse(string text)
    {
        var textBuilder = new StringBuilder();
        var spansBuilder = ImmutableArray.CreateBuilder<TextSpan>();
        var startStack = new Stack<int>();
        foreach (var c in text.Select((@char, pos)=> new { @char, pos} ))
        {
            if (c.@char == '[')
            {
                startStack.Push(c.pos);
                continue;
            }
            
            if (c.@char is ']')
            {
                if (startStack.Count == 0)
                    throw new Exception("Unmatched ]");
                
                var start = startStack.Pop();
                var end = c.pos - 1;
                spansBuilder.Add(TextSpan.FromBounds(start, end));
                continue;
            }

            textBuilder.Append(c.@char);
        }
        return new AnnotatedText(textBuilder.ToString(), spansBuilder.ToImmutable());
    }
}