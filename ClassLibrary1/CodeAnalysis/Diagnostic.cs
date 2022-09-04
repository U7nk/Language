using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis;

public sealed class Diagnostic
{
    public TextSpan Span { get; }
    public string Message { get; }

    public Diagnostic(TextSpan span, string message)
    {
        this.Span = span;
        this.Message = message;
    }

    public override string ToString() => this.Message;
}