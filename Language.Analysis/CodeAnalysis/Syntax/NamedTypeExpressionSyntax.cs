using System.Linq;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Syntax;

public class NamedTypeExpressionSyntax : SyntaxNode
{
    public NamedTypeExpressionSyntax(SyntaxTree syntaxTree, 
                                     Option<SeparatedSyntaxList<SyntaxNode>> namespaceParts,
                                     Option<SyntaxToken> dot,
                                     SyntaxToken identifier, 
                                     Option<GenericClauseSyntax> genericClause) 
        : base(syntaxTree)
    {
        if (dot.IsSome) 
            (namespaceParts.IsSome && namespaceParts.Unwrap().Count > 0).EnsureTrue();

        if (namespaceParts.IsSome && namespaceParts.Unwrap().Count > 0)
            dot.IsSome.EnsureTrue();
        
        GenericClause = genericClause;
        Dot = dot;
        NamespaceParts = namespaceParts;
        Identifier = identifier;
    }

    public Option<SeparatedSyntaxList<SyntaxNode>> NamespaceParts { get; }
    public Option<SyntaxToken> Dot { get; }
    public SyntaxToken Identifier { get; }
    public Option<GenericClauseSyntax> GenericClause { get; }
    public override SyntaxKind Kind => SyntaxKind.NamedTypeExpression;

    public string GetName()
    {
        var name = "";
        if (NamespaceParts.IsNone)
            return Identifier.Text;

        foreach (var token in NamespaceParts.Unwrap().SeparatorsAndNodes)
        {
            name += token.As<SyntaxToken>().Text;
        }

        name += "." + Identifier.Text;

        return name;
    }
}