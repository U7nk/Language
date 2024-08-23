using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using Language.Analysis.CodeAnalysis.Text;

namespace Language.Analysis.CodeAnalysis.Syntax;

public partial class SyntaxTree
{
    
    delegate void ParseHandler(SyntaxTree tree, 
    out CompilationUnitSyntax root, 
    out ImmutableArray<Diagnostic> diagnostics);

    public SyntaxTree(SourceText sourceText, DiagnosticBag diagnostics)
    {
        SourceText = sourceText;
        Diagnostics = diagnostics;
    }
    public SourceText SourceText { get; }
    public CompilationUnitSyntax CompilationUnitSyntax { get; set; }
    public CompilationUnitSyntax Root { get; set; }
    public DiagnosticBag Diagnostics { get; }
    
    public static SyntaxTree Load(string fileName)
    {
        var text = File.ReadAllText(fileName);
        var sourceText = SourceText.From(text, fileName);
        return Parse(sourceText);
    }
    
    public static SyntaxTree Parse(string source)
    {
        var sourceText = SourceText.From(source);
        return Parse(sourceText);
    }
    
    public static SyntaxTree Parse(SourceText source)
    {
        var parser = new Parser(source);
        var syntaxTree = parser.Parse();
        return syntaxTree;
    }

    public static ICollection<SyntaxToken> ParseTokens(string source) 
        => ParseTokens(SourceText.From(source));

    public static ICollection<SyntaxToken> ParseTokens(SourceText source) 
        => ParseTokens(source, out _);

    public static ICollection<SyntaxToken> ParseTokens(string source, out ImmutableArray<Diagnostic> diagnostics) 
        => ParseTokens(SourceText.From(source), out diagnostics);

    public static ICollection<SyntaxToken> ParseTokens(SourceText source, out ImmutableArray<Diagnostic> diagnostics)
    {
        ICollection<SyntaxToken>? tokens = null;
        var diagnosticBag = new DiagnosticBag();
        var syntaxTree = new SyntaxTree(source, diagnosticBag);
        var lexer = new Lexer(syntaxTree);
        tokens = lexer.Lex();
        var root = new CompilationUnitSyntax(syntaxTree, new List<NamespaceSyntax>{ new (syntaxTree,
                                                                                     new SyntaxToken(syntaxTree, SyntaxKind.NamespaceKeyword, 0, "namespace", null), 
                                                                                     new SeparatedSyntaxList<SyntaxToken>(ImmutableArray<SyntaxNode>.Empty), 
                                                                                     new SyntaxToken(syntaxTree, SyntaxKind.OpenBraceToken, 0,"", null), 
                                                                                     ImmutableArray<ClassDeclarationSyntax>.Empty, 
                                                                                     new SyntaxToken(syntaxTree, SyntaxKind.OpenBraceToken, 0,"", null)) }, 
                                         new SyntaxToken(syntaxTree, SyntaxKind.EndOfFileToken,0, "\0", null));
        var diags = lexer.Diagnostics.ToImmutableArray();
        syntaxTree.CompilationUnitSyntax = root;
        syntaxTree.Diagnostics.AddRange(diags);
        
        
        
        Debug.Assert(tokens != null, nameof(tokens) + " != null");
        diagnostics = syntaxTree.Diagnostics.ToImmutableArray();
        return tokens.ToImmutableArray();
    }
}