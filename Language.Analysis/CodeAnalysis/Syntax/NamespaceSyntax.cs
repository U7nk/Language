using System.Collections.Immutable;
using System.Linq;

namespace Language.Analysis.CodeAnalysis.Syntax;

public sealed class NamespaceSyntax : SyntaxNode
{
    public NamespaceSyntax(SyntaxTree syntaxTree, SyntaxToken namespaceKeyword, SeparatedSyntaxList<SyntaxToken> nameTokens, SyntaxToken openBrace, ImmutableArray<ClassDeclarationSyntax> members, SyntaxToken closeBrace) : base(syntaxTree)
    {
        NamespaceKeyword = namespaceKeyword;
        NameTokens = nameTokens;
        OpenBrace = openBrace;
        Members = members;
        CloseBrace = closeBrace;
    }

    public override SyntaxKind Kind => SyntaxKind.NamespaceSyntax;
    public SyntaxToken NamespaceKeyword { get; }
    public SeparatedSyntaxList<SyntaxToken> NameTokens { get; }
    public string FullName => string.Join("", NameTokens.SeparatorsAndNodes.Cast<SyntaxToken>().Select(x => x.Text));
    public SyntaxToken OpenBrace { get; }
    public ImmutableArray<ClassDeclarationSyntax> Members { get; }
    public SyntaxToken CloseBrace { get; }
    
}