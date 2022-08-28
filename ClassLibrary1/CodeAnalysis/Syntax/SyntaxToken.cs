using System.Collections.Generic;
using System.Linq;

namespace Wired.CodeAnalysis.Syntax;

public class SyntaxToken : SyntaxNode
{
    public override SyntaxKind Kind { get; }
    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }

    public int Position { get; }
    public string Text { get; }
    public object? Value { get; }
    public TextSpan Span => new(this.Position, this.Text.Length);
    
    public SyntaxToken(SyntaxKind kind,int position, string text, object? value)
    {
        this.Kind = kind;
        this.Position = position;
        this.Text = text;
        this.Value = value;
    }
}