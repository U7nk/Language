using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Lowering;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class TypeBinder
{
    readonly TypeBinderLookup _lookup;
    readonly BoundScope _scope;
    readonly bool _isScript;
    readonly DiagnosticBag _diagnostics;
    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();

    public TypeBinder(BoundScope scope, bool isScript, TypeBinderLookup lookup)
    {
        _scope = scope;
        _isScript = isScript;
        _lookup = lookup;
        _diagnostics = new DiagnosticBag();
    }

    public void BindClassBody()
    {
        var typeScope = new BoundScope(_scope);
        
        var baseTypes = BindInheritanceClause(_lookup.CurrentType.InheritanceClauseSyntax);
        AddBaseTypesToCurrentType(baseTypes);

        foreach (var methodSymbol in _lookup.CurrentType.MethodTable.Symbols)
        {
            var methodBinder = new MethodBinder(typeScope, _isScript, new MethodBinderLookup(
                                                    _lookup.Declarations, 
                                                    _lookup.CurrentType,
                                                    _lookup.AvailableTypes,
                                                    methodSymbol));
            
            var body = methodBinder.BindMethodBody(methodSymbol);
            var loweredBody = Lowerer.Lower(body);

            if (!Equals(methodSymbol.ReturnType, BuiltInTypeSymbols.Void) &&
                !ControlFlowGraph.AllPathsReturn(loweredBody))
            {
                _diagnostics.ReportAllPathsMustReturn(methodSymbol.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>()
                                                          .Identifier.Location);
            }

            ControlFlowGraph.AllVariablesInitializedBeforeUse(loweredBody, _diagnostics);

            _diagnostics.MergeWith(methodBinder.Diagnostics);

            _lookup.CurrentType.MethodTable.SetMethodBody(methodSymbol, loweredBody);
        }
    }

    public void AddBaseTypesToCurrentType(ImmutableArray<TypeSymbol> baseTypes)
    {
        foreach (var baseType in baseTypes)
        {
            CheckTypeDontInheritFromItself(_lookup.CurrentType, baseType);
            _lookup.CurrentType.BaseTypes.Add(baseType);
        }
    }
    
    /// <summary>
    /// Checks if type inherits from itself. <br/>
    /// If so, adds diagnostic to <see cref="_diagnostics"/>.
    /// Diagnostic is added to every <see cref="InheritanceClauseSyntax"/> location.
    /// </summary>
    /// <param name="currentType">Type that is being checked for inheritance from itself.</param>
    /// <param name="baseTypeToInheritFrom">Base type that will be added to <see cref="TypeSymbol.BaseTypes"/> list of <paramref name="currentType"/></param>
    void CheckTypeDontInheritFromItself(TypeSymbol currentType, TypeSymbol? baseTypeToInheritFrom)
    {
        if (baseTypeToInheritFrom is null)
            return;

        var typesToCheck = new List<TypeSymbol>(currentType.BaseTypes) { currentType };

        foreach (var checkedType in typesToCheck)
        {
            if (checkedType == baseTypeToInheritFrom)
            {
                var existingDeclarationSyntax = _lookup.LookupDeclarations<ClassDeclarationSyntax>(currentType);
                foreach (var declarationSyntax in existingDeclarationSyntax)
                {
                    _diagnostics.ReportClassCannotInheritFromSelf(declarationSyntax.Identifier);
                }
                return;
            }
        }

        foreach (var baseType in currentType.BaseTypes)
        {
            CheckTypeDontInheritFromItself(baseType, baseTypeToInheritFrom);
        }
    }

    /// <summary>
    /// <b>NOTE:</b> All types is implicitly inherited from <see cref="BuiltInTypeSymbols.Object"/> type. <br/>
    /// So every return array will contain at least one element - <see cref="BuiltInTypeSymbols.Object"/> type.
    /// </summary>
    /// <param name="syntax">Inheritance clause syntax.</param>
    /// <returns></returns>
    ImmutableArray<TypeSymbol> BindInheritanceClause(InheritanceClauseSyntax? syntax)
    {
        var builder = ImmutableArray.CreateBuilder<TypeSymbol>();
        builder.Add(BuiltInTypeSymbols.Object);
        
        if (syntax is not null)
        {
            foreach (var baseTypeIdentifier in syntax.BaseTypes)
            {
                var baseTypeName = baseTypeIdentifier.Text;
                if (!_scope.TryLookupType(baseTypeName, out var baseTypeSymbol))
                {
                    _diagnostics.ReportUndefinedType(baseTypeIdentifier.Location, baseTypeName);
                    continue;
                }

                if (baseTypeSymbol == BuiltInTypeSymbols.Object)
                    continue;
                
                builder.Add(baseTypeSymbol);
            }
        }
        
        return builder.ToImmutable();
    }
}