using System.Collections.Generic;
using System.Linq;
using Wired.CodeAnalysis.Text;

namespace Wired.CodeAnalysis.Syntax;

public class SyntaxToken : SyntaxNode
{
    public override SyntaxKind Kind { get; }

    public int Position { get; }
    public string Text { get; }
    public object? Value { get; }
    public override TextSpan Span => new(Position, Text.Length);
    
    public SyntaxToken(SyntaxKind kind,int position, string text, object? value)
    {
        Kind = kind;
        Position = position;
        Text = text;
        Value = value;
    }
}