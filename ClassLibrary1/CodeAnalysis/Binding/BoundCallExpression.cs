using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Binding;

internal class BoundCallExpression : BoundExpression
{
    public FunctionSymbol FunctionSymbol { get; }
    public ImmutableArray<BoundExpression> Arguments { get; }

    public BoundCallExpression(FunctionSymbol functionSymbol, ImmutableArray<BoundExpression> arguments)
    {
        this.FunctionSymbol = functionSymbol;
        this.Arguments = arguments;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.CallExpression;
    internal override TypeSymbol Type => this.FunctionSymbol.ReturnType;
}