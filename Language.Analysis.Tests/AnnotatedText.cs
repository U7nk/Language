using System.Collections.Immutable;
using System.Text;
using Language.Analysis.CodeAnalysis.Text;

namespace TestProject1;

internal class AnnotatedText
{
    public string Text { get; }
    public ImmutableArray<TextSpan> Spans { get; }

    public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
    {
        Text = text;
        Spans = spans;
    }

    public static AnnotatedText Parse(string text)
    {
        var textBuilder = new StringBuilder();
        var spansBuilder = ImmutableArray.CreateBuilder<TextSpan>();
        var startStack = new Stack<int>();
        var position = 0;
        foreach (var c in text)
        {
            if (c == '[')
            {
                startStack.Push(position);
                continue;
            }
            
            if (c is ']')
            {
                if (startStack.Count == 0)
                    throw new Exception("Unmatched ]");
                
                var start = startStack.Pop();
                var end = position;
                spansBuilder.Add(TextSpan.FromBounds(start, end));
                continue;
            }

            position++;
            textBuilder.Append(c);
        }
        return new AnnotatedText(textBuilder.ToString(), spansBuilder.ToImmutable());
    }
}