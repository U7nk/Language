using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;


internal class BoundErrorExpression : BoundExpression
{
    public BoundErrorExpression(SyntaxNode? syntax) : base(syntax)
    {
        
    }
    internal override TypeSymbol Type => BuiltInTypeSymbols.Error;
    internal override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
}