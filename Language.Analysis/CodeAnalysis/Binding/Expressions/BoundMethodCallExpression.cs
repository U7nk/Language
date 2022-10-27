using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundMethodCallExpression : BoundExpression
{
    public MethodSymbol MethodSymbol { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    public BoundMethodCallExpression(MethodSymbol methodSymbol, ImmutableArray<BoundExpression> arguments)
    {
        MethodSymbol = methodSymbol;
        Arguments = arguments;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.MethodCallExpression;
    internal override TypeSymbol Type => MethodSymbol.ReturnType;
}