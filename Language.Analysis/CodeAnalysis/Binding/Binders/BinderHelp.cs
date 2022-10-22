using System.Linq;
using Language.CodeAnalysis.Binding.Lookup;
using Language.CodeAnalysis.Symbols;
using Language.CodeAnalysis.Syntax;

namespace Language.CodeAnalysis.Binding.Binders;

static class BinderHelp
{
    public static TypeSymbol? BindTypeClause(TypeClauseSyntax? syntax, DiagnosticBag diagnostics, BaseBinderLookup typesLookup)
    {
        if (syntax is null)
            return null;
        
        var type = typesLookup.AvailableTypes.SingleOrDefault(x=> x.Name == syntax.Identifier.Text);
        if (type != null)
            return type;

        diagnostics.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);
        return type;
    }
}