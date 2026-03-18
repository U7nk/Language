namespace Language.Analysis.CodeAnalysis.Syntax;
/// <summary>
/// a + b + 5
/// is left associative
///      +
///     / \
///    +   5
///   / \
///  a   b
///
/// a = b = 5
/// assignment is right associative
///      =
///     / \
///    a   =
///       / \
///      b   5 
/// </summary>

public class AssignmentExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Left { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Initializer { get; }

    public AssignmentExpressionSyntax(
        SyntaxTree syntaxTree,
        ExpressionSyntax left,
        SyntaxToken equalsToken,
        ExpressionSyntax initializer) : base(syntaxTree)
    {
        Left = left;
        EqualsToken = equalsToken;
        Initializer = initializer;
    }

    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
}