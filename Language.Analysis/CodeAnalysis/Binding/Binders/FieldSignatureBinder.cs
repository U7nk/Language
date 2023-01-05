using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

public sealed class FieldSignatureBinder
{
    readonly FieldBinderLookup? _lookup;
    readonly BoundScope _scope;
    readonly bool _isScript;

    public FieldSignatureBinder(BoundScope scope, bool isScript, FieldBinderLookup? lookup)
    {
        _scope = scope;
        _isScript = isScript;
        _lookup = lookup;
    }

    public ImmutableArray<Diagnostic> BindDeclaration(FieldDeclarationSyntax fieldDeclaration)
    {
        var diagnostics = new DiagnosticBag();
        _lookup.NullGuard();
        var fieldType = _lookup.LookupType(fieldDeclaration.TypeClause.Identifier.Text);
        if (fieldType == null)
        {
            diagnostics.ReportUndefinedType(
                fieldDeclaration.TypeClause.Location,
                fieldDeclaration.TypeClause.Identifier.Text);
        }

        // if diagnostics are reported field should not be used later in binding
        // so we just let type to be null and try to gain more diagnostics
        var fieldSymbol = new FieldSymbol(fieldDeclaration,
                                    fieldDeclaration.StaticKeyword is { },
                                    fieldDeclaration.Identifier.Text,
                                    _lookup.ContainingType, 
                                    fieldType!);
        _lookup.AddDeclaration(fieldSymbol, fieldDeclaration);
        if (!_lookup.ContainingType.TryDeclareField(fieldSymbol))
        {
            if (fieldSymbol.Name == _lookup.ContainingType.Name)
                diagnostics.ReportClassMemberCannotHaveNameOfClass(fieldDeclaration.Identifier);

            var existingFieldDeclarations = _lookup.LookupDeclarations<FieldDeclarationSyntax>(fieldSymbol);
            if (existingFieldDeclarations.Length > 1)
            {
                foreach (var existingFieldDeclaration in existingFieldDeclarations)
                {
                    diagnostics.ReportFieldAlreadyDeclared(existingFieldDeclaration.Identifier);
                }
            }

            
            var sameNameMethods = _lookup.ContainingType.MethodTable.Symbols.Where(x => x.Name == fieldSymbol.Name).ToList();
            if (sameNameMethods.Any())
            {
                diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(fieldDeclaration.Identifier);
                foreach (var sameNameMethod in sameNameMethods)
                {
                    var sameNameMethodDeclarations = _lookup.LookupDeclarations<MethodDeclarationSyntax>(sameNameMethod);
                    foreach (var sameNameMethodDeclaration in sameNameMethodDeclarations)
                    {
                        diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(sameNameMethodDeclaration.Identifier);    
                    }
                }
            }
        }

        return diagnostics.ToImmutableArray();
    }
}