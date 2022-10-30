using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class FieldSymbol : MemberSymbol
{
    public FieldSymbol(ImmutableArray<SyntaxNode> declarationSyntax, string name, TypeSymbol type) 
        : base(declarationSyntax,name, type)
    { }
    
    public override SymbolKind Kind => SymbolKind.Field;
}