using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

static class BinderHelp
{
    public static TypeSymbol? BindTypeClause(TypeClauseSyntax? syntax, DiagnosticBag diagnostics, BinderLookup typesLookup)
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
