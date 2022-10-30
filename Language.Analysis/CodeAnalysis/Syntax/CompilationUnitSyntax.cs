using System.Collections.Immutable;

namespace Language.Analysis.CodeAnalysis.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<ITopMemberDeclarationSyntax> members, SyntaxToken endOfFileToken) 
        : base(syntaxTree)
    {
        Members = members;
        EndOfFileToken = endOfFileToken;
    }

    public ImmutableArray<ITopMemberDeclarationSyntax> Members { get; }
    public SyntaxToken EndOfFileToken { get; }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}