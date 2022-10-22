using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Symbols;

namespace Language.Analysis.CodeAnalysis.Binding;

internal class BoundMethodCallExpression : BoundExpression
{
    public FunctionSymbol FunctionSymbol { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    public BoundMethodCallExpression(FunctionSymbol functionSymbol, ImmutableArray<BoundExpression> arguments)
    {
        FunctionSymbol = functionSymbol;
        Arguments = arguments;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.MethodCallExpression;
    internal override TypeSymbol Type => FunctionSymbol.ReturnType;
}