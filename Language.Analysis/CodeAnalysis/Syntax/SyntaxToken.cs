using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.CodeAnalysis.Syntax;

public class SyntaxToken : SyntaxNode
{
    public override SyntaxKind Kind { get; }

    public int Position { get; }
    public string Text { get; }
    public object? Value { get; }
    public override TextSpan Span => new(Position, Text.Length);
    
    public SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind,int position, string text, object? value) 
        : base(syntaxTree)
    {
        Kind = kind;
        Position = position;
        Text = text;
        Value = value;
    }
}