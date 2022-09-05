using System.Collections.Generic;

namespace Wired.CodeAnalysis.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
  public CompilationUnitSyntax(ExpressionSyntax expressionSyntax, SyntaxToken endOfFileToken)
  {
    ExpressionSyntax = expressionSyntax;
    EndOfFileToken = endOfFileToken;
  }

  public ExpressionSyntax ExpressionSyntax { get; }
  public SyntaxToken EndOfFileToken { get; }

  public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}
public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
    public AssignmentExpressionSyntax(SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression)
    {
        this.IdentifierToken = identifierToken;
        this.EqualsToken = equalsToken;
        this.Expression = expression;
    }

    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    public SyntaxToken IdentifierToken { get; }
    public SyntaxToken EqualsToken { get; }
    public ExpressionSyntax Expression { get; }

   
}