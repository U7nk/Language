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
        _lookup.NG();
        var fieldType = _lookup.LookupType(fieldDeclaration.TypeClause.Identifier.Text);
        if (fieldType == null)
        {
            diagnostics.ReportUndefinedType(
                fieldDeclaration.TypeClause.Location,
                fieldDeclaration.TypeClause.Identifier.Text);
        }

        // if diagnostics are reported field should not be used later in binding
        // so we just let type to be null and try to gain more diagnostics
        var field = new FieldSymbol(ImmutableArray.Create<SyntaxNode>(fieldDeclaration),
                                    fieldDeclaration.StaticKeyword is { },
                                    fieldDeclaration.Identifier.Text,
                                    _lookup.ContainingType, 
                                    fieldType!);
        
        if (!_scope.TryDeclareField(field, _lookup.ContainingType))
        {
            if (field.Name == _lookup.ContainingType.Name)
                diagnostics.ReportClassMemberCannotHaveNameOfClass(fieldDeclaration.Identifier);
            
            
            var sameNameFields = _lookup.ContainingType.FieldTable.Symbols.Where(x => x.Name == field.Name).ToList();
            if (sameNameFields.Any())
            {
                foreach (var sameNameField in sameNameFields)
                    diagnostics.ReportFieldAlreadyDeclared(
                        sameNameField.DeclarationSyntax.Cast<FieldDeclarationSyntax>().First().Identifier);
                
                diagnostics.ReportFieldAlreadyDeclared(fieldDeclaration.Identifier);
            }

            
            var sameNameMethods = _lookup.ContainingType.MethodTable.Symbols.Where(x => x.Name == field.Name).ToList();
            if (sameNameMethods.Any())
            {
                foreach (var sameNameMethod in sameNameMethods)
                    diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(
                        sameNameMethod.DeclarationSyntax.Cast<MethodDeclarationSyntax>().First().Identifier);
                
                diagnostics.ReportClassMemberWithThatNameAlreadyDeclared(fieldDeclaration.Identifier);
            }
        }

        return diagnostics.ToImmutableArray();
    }
}