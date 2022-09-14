using System.Collections.Immutable;

namespace Wired.CodeAnalysis.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken)
    {
        Members = members;
        EndOfFileToken = endOfFileToken;
    }

    public ImmutableArray<MemberSyntax> Members { get; }
    public SyntaxToken EndOfFileToken { get; }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
}