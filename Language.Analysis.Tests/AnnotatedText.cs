using System.Collections.Immutable;
using System.Text;
using Language.Analysis.CodeAnalysis.Text;
using Language.Analysis.Extensions;

namespace Language.Analysis.Tests;

internal class AnnotatedText
{
    public string RawText { get; }
    public ImmutableArray<TextSpan> Spans { get; }

    public AnnotatedText(string rawText, ImmutableArray<TextSpan> spans)
    {
        RawText = rawText;
        Spans = spans;
    }

    public static string Annotate(string text, IEnumerable<TextSpan> spans)
    {
        var spansList = spans.ToList();
        var sb = new StringBuilder();
        var pos = 0;
        foreach (var c in text)
        {
            var matchingStarts = spansList.Where(s => s.Start == pos).ToArray();
            var matchingEnds = spansList.Where(s => s.End == pos).ToArray();

            foreach (var i in 0..matchingStarts.Length) 
                sb.Append('[');

            foreach (var i in 0..matchingEnds.Length) 
                sb.Append(']');
            
            sb.Append(c);
            pos++;
        }
        
        return sb.ToString();
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
