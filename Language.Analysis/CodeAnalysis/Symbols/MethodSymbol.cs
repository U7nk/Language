using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class MethodSymbol : MemberSymbol
{
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType => Type;
    public MethodDeclarationSyntax? Declaration { get; }
    public override SymbolKind Kind => SymbolKind.Method;

    public MethodSymbol(string name,
        ImmutableArray<ParameterSymbol> parameters,
        TypeSymbol returnType, MethodDeclarationSyntax? declaration)
        : base(name, returnType)
    {
        Parameters = parameters;
        Declaration = declaration;
    }
}