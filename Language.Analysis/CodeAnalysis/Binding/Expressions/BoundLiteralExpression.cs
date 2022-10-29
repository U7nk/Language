using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundLiteralExpression : BoundExpression
{
    internal override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    internal override TypeSymbol Type { get; }
    internal object? Value { get; }
    internal BoundLiteralExpression(SyntaxNode? syntax, object? value, TypeSymbol type) : base(syntax)
    {
        Value = value;
        Type = type;
    }
}