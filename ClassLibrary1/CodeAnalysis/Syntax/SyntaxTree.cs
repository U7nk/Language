using System.Collections.Generic;
using System.Linq;

namespace Wired.CodeAnalysis.Syntax;

public class SyntaxTree
{
    public ExpressionSyntax Root { get; }
    public SyntaxToken EndOfFileToken { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    public SyntaxTree(
        IEnumerable<Diagnostic> diagnostics,
        ExpressionSyntax root,
        SyntaxToken endOfFileToken)
    {
        this.Root = root;
        this.EndOfFileToken = endOfFileToken;
        this.Diagnostics = diagnostics.ToArray();
    }
    
    public static SyntaxTree Parse(string source)
    {
        var parser = new Parser(source);
        var syntaxTree = parser.Parse();
        return syntaxTree;
    }
}