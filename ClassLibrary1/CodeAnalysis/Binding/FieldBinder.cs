using System.Collections.Immutable;
using Wired.CodeAnalysis.Binding.Lookup;
using Wired.CodeAnalysis.Symbols;
using Wired.CodeAnalysis.Syntax;

namespace Wired.CodeAnalysis.Binding;

public sealed class FieldBinder
{
    readonly FieldBinderLookup? _lookup;
    readonly BoundScope _scope;
    readonly bool _isScript;

    public FieldBinder(BoundScope scope, bool isScript, FieldBinderLookup? lookup)
    {
        _scope = scope;
        _isScript = isScript;
        _lookup = lookup;
    }

    public ImmutableArray<Diagnostic> BindDeclaration(FieldDeclarationSyntax fieldDeclaration)
    {
        var diagnostics = new DiagnosticBag();
        _lookup.Unwrap();
        var type = _lookup.LookupType(fieldDeclaration.TypeClause.Identifier.Text);
        if (type == null)
        {
            diagnostics.ReportUndefinedType(
                fieldDeclaration.TypeClause.Location,
                fieldDeclaration.TypeClause.Identifier.Text);
        }

        // if diagnostics are reported field should not be used later in binding
        // so we just let it be null and try to gain more diagnostics
        var field = new FieldSymbol(fieldDeclaration.Identifier.Text, type!);  
        if (!_scope.TryDeclareField(field))
        {
            diagnostics.ReportFieldAlreadyDeclared(
                fieldDeclaration.Identifier.Location,
                fieldDeclaration.Identifier.Text);
        }

        return diagnostics.ToImmutableArray();
    }
}