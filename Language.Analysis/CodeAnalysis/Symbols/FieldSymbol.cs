using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class FieldSymbol : MemberSymbol
{
    public FieldSymbol(Option<SyntaxNode> declarationSyntax, bool isStatic, string name,
                       TypeSymbol containingType, TypeSymbol parameterType)
        : base(declarationSyntax, name, containingType: containingType, parameterType)
    {
        IsStatic = isStatic;
    }

    public bool IsStatic { get; }
    public override SymbolKind Kind => SymbolKind.Field;
}