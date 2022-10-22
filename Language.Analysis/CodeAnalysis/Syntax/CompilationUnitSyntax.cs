using System.Collections.Immutable;

namespace Language.CodeAnalysis.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<ITopMemberSyntax> members, SyntaxToken endOfFileToken) 
        : base(syntaxTree)
    {
        Members = members;
        EndOfFileToken = endOfFileToken;
    }

    public ImmutableArray<ITopMemberSyntax> Members { get; }
    public SyntaxToken EndOfFileToken { get; }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}