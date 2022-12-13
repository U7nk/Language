using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundMethodCallExpression : BoundExpression
{
    public MethodSymbol MethodSymbol { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    public BoundMethodCallExpression(Option<SyntaxNode> syntax, MethodSymbol methodSymbol, ImmutableArray<BoundExpression> arguments) : base(syntax)
    {
        MethodSymbol = methodSymbol;
        Arguments = arguments;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.MethodCallExpression;
    internal override TypeSymbol Type => MethodSymbol.ReturnType;
}