using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.CodeAnalysis;

public sealed class Diagnostic
{
    public string Code { get; }
    public TextLocation TextLocation { get; }
    public string Message { get; }

    public Diagnostic(TextLocation textLocation, string message, string code)
    {
        TextLocation = textLocation;
        Message = message;
        Code = code;
    }

    public override string ToString() => Message;
}