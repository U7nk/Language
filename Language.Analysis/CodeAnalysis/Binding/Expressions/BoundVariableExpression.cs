using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding;

internal sealed class BoundVariableExpression : BoundExpression
{
    public VariableSymbol Variable { get; }

    public BoundVariableExpression(SyntaxNode syntax, VariableSymbol variable) : base(syntax)
    {
        Variable = variable;
    }

    internal override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
    internal override TypeSymbol Type => Variable.Type;

    bool Equals(BoundVariableExpression other)
    {
        return Variable.Equals(other.Variable);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) 
            return false;
        
        if (ReferenceEquals(this, obj)) 
            return true;
        
        if (obj.GetType() != GetType()) 
            return false;
        
        return Equals((BoundVariableExpression)obj);
    }

    public override int GetHashCode()
    {
        return Variable.GetHashCode();
    }
}