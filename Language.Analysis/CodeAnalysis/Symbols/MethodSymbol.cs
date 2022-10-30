using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class MethodSymbol : MemberSymbol
{
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType => Type;
    public override SymbolKind Kind => SymbolKind.Method;

    public MethodSymbol(ImmutableArray<SyntaxNode> declaration,
        string name,
        ImmutableArray<ParameterSymbol> parameters,
        TypeSymbol returnType) 
        : base(declaration, name, returnType)
    {
        Parameters = parameters;
    }
}