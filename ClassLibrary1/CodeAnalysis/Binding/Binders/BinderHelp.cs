using System.Linq;
using Wired.CodeAnalysis.Binding.Lookup;
using Wired.CodeAnalysis.Symbols;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding.Binders;

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