using System.Linq;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

static class BinderHelp
{
    public static TypeSymbol? BindTypeClause(TypeClauseSyntax? syntax, DiagnosticBag diagnostics, BoundScope scope)
    {
        if (syntax is null)
            return null;

        var type = TypeSymbol.FromNamedTypeExpression(syntax.NamedTypeExpression, scope, diagnostics);
        
        if (Equals(type, BuiltInTypeSymbols.Error))
            return null;
        
        return type;
    }
}
