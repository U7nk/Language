using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.CodeAnalysis;

public sealed class Diagnostic
{
    public TextLocation TextLocation { get; }
    public string Message { get; }

    public Diagnostic(TextLocation textLocation, string message)
    {
        TextLocation = textLocation;
        Message = message;
    }

    public override string ToString() => Message;
}