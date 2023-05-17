using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;


internal class BoundErrorExpression : BoundExpression
{
    /// <summary>
    /// </summary>
    /// <param name="syntax">should be null. needed because base class constructor</param>
    public BoundErrorExpression(SyntaxNode? syntax) : base(syntax)
    {
        
    }
    internal override TypeSymbol Type => BuiltInTypeSymbols.Error;
    internal override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
}