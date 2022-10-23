using System.Collections.Immutable;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Symbols;

public class FunctionSymbol : MemberSymbol
{
    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol ReturnType => this.Type;
    public FunctionDeclarationSyntax? Declaration { get; }
    public override SymbolKind Kind => SymbolKind.Function;

    public FunctionSymbol(string name,
        ImmutableArray<ParameterSymbol> parameters,
        TypeSymbol returnType, FunctionDeclarationSyntax? declaration)
        : base(name, returnType)
    {
        Parameters = parameters;
        Declaration = declaration;
    }
}