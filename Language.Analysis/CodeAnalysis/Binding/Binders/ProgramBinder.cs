using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Lowering;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Common;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

public class TypeBindingUnit
{
    public TypeBindingUnit(TypeSymbol type, BoundScope scope)
    {
        Type = type;
        Scope = scope;
    }

    public TypeSymbol Type { get; }
    public BoundScope Scope { get; }
}

internal sealed class ProgramBinder
{
    readonly BoundGlobalScope? _previous;
    readonly BoundScope _scope;
    readonly ImmutableArray<SyntaxTree> _syntaxTrees;
    readonly DeclarationsBag _allDeclarations;
    readonly DiagnosticBag _diagnostics = new();

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;
    public bool IsScript { get; }

    internal ProgramBinder(bool isScript,
        BoundGlobalScope? previous,
        ImmutableArray<SyntaxTree> syntaxTrees, DeclarationsBag allDeclarations)
    {
        _previous = previous;
        if (_previous is not null)
            _diagnostics.InsertRange(0, _previous.Diagnostics);
        _scope = CreateParentScope(_previous);
        _syntaxTrees = syntaxTrees;
        _allDeclarations = allDeclarations;
        IsScript = isScript;
    }

    void DeclareBuiltInTypes()
    {
        _scope.TryDeclareType(BuiltInTypeSymbols.Object);
        _scope.TryDeclareType(BuiltInTypeSymbols.Bool);
        _scope.TryDeclareType(BuiltInTypeSymbols.Int);
        _scope.TryDeclareType(BuiltInTypeSymbols.String);
        _scope.TryDeclareType(BuiltInTypeSymbols.Void);
    }

    List<FullTypeBinder> _typeBinders = new();
    public BoundGlobalScope BindGlobalScope()
    {
        DeclareBuiltInTypes();

        var classDeclarations = _syntaxTrees
            .Only<ClassDeclarationSyntax>()
            .ToImmutableArray();
        
        
        foreach (var classDeclaration in classDeclarations)
        {
            var typeBinder = new FullTypeBinder(_scope, _allDeclarations, IsScript);
            var typeSignatureBindResult = typeBinder.BindClassDeclaration(classDeclaration, _diagnostics);
            if (typeSignatureBindResult.IsOk)
            {
                _typeBinders.Add(typeBinder);   
            }
        }
        
        
        foreach (var typeBinder in _typeBinders)
        {
            typeBinder.BindInheritanceClause(_diagnostics);
        }
        foreach (var typeBinder in _typeBinders)
        {
            typeBinder.DiagnoseTypeDontInheritFromItself(_diagnostics);
        }
        foreach (var typeBinder in _typeBinders)
        {
            typeBinder.BindMembersSignatures(_diagnostics);
        }
        foreach (var typeBinder in _typeBinders)
        {
            typeBinder.DiagnoseDiamondProblem(_diagnostics);
        }

        DiagnoseGlobalStatementsUsage();
        
        var globalStatements = _syntaxTrees
            .Only<GlobalStatementDeclarationSyntax>()
            .ToImmutableArray();
        DiagnoseMainMethodAndGlobalStatementsUsage(globalStatements);
        
        MethodSymbol? mainMethod = null;
        MethodSymbol? scriptMainMethod = null;
        
        
        if (globalStatements.Any())
        {
            BindGlobalStatements(globalStatements, ref scriptMainMethod, ref mainMethod);
        }
        else
        {
            if (IsScript)
            {
                scriptMainMethod = TryFindMainMethod();
                if (scriptMainMethod is { })
                {
                    _diagnostics.ReportNoMainMethodAllowedInScriptMode(
                        scriptMainMethod.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>().Identifier.Location);
                }
            }

            mainMethod = TryFindMainMethod();
        }

        if (mainMethod is null && scriptMainMethod is null)
        {
            _diagnostics.ReportMainMethodShouldBeDeclared(_syntaxTrees.First().SourceText);
        }

        return new BoundGlobalScope(
            _previous, _diagnostics.ToImmutableArray(),
            mainMethod, scriptMainMethod,
            _scope.GetDeclaredTypes(), _scope.GetDeclaredVariables(),
            _allDeclarations,
            _typeBinders.ToImmutableArray());
    }

    void BindGlobalStatements(ImmutableArray<GlobalStatementDeclarationSyntax> globalStatements, ref MethodSymbol? scriptMainMethod, ref MethodSymbol? mainMethod)
    {
        Result<(MethodSymbol Function, TypeSymbol Type), DiagnosticBag> mainMethodGeneration;
        if (IsScript)
        {
            mainMethodGeneration = GenerateMainMethod(_scope, IsScript, globalStatements);
            if (mainMethodGeneration.IsError)
            {
                _diagnostics.MergeWith(mainMethodGeneration.Error);
            }
            
            scriptMainMethod = mainMethodGeneration.Ok.Function;
        }
        else
        {
            mainMethodGeneration = GenerateMainMethod(_scope, IsScript, globalStatements);
            if (mainMethodGeneration.IsError)
            {
                _diagnostics.AddRange(mainMethodGeneration.Error);
            }
            mainMethod = mainMethodGeneration.Ok.Function;
        }
        
        Option<TypeSymbol> programType = mainMethodGeneration.Ok.Type;
        BindTopMethodsDeclarations(programType.Unwrap());
        
        var programTypeBinder = new FullTypeBinder(_scope, _allDeclarations, IsScript, programType.Unwrap(), isTopMethod: true);
        _typeBinders.Add(programTypeBinder);
    }

    void BindTopMethodsDeclarations(TypeSymbol mainFunctionType)
    {
        var topMethodDeclarations = _syntaxTrees
            .Only<MethodDeclarationSyntax>()
            .ToImmutableArray();
        var methodSignatureBinder = new MethodDeclarationBinder(_scope, mainFunctionType, isTopMethod:true, _allDeclarations);
        
        TypeSymbol typeSymbol = _scope.GetDeclaredTypes().Single(x=> x.Name == SyntaxFacts.START_TYPE_NAME);
        foreach (var topMethodDeclaration in topMethodDeclarations)
        {
            var methodSymbol = methodSignatureBinder.BindMethodDeclaration(topMethodDeclaration, _diagnostics);
            typeSymbol.TryDeclareMethod(methodSymbol, _diagnostics, _allDeclarations);
        }
    }

    void DiagnoseGlobalStatementsUsage()
    {
        var firstGlobalStatementPerSyntaxTree = _syntaxTrees
            .Select(x => x.Root.Members.OfType<GlobalStatementDeclarationSyntax>().FirstOrDefault())
            .Where(x => x is not null)
            .Cast<GlobalStatementDeclarationSyntax>()
            .ToList();
        if (firstGlobalStatementPerSyntaxTree.Count > 1)
        {
            foreach (var globalStatementSyntax in firstGlobalStatementPerSyntaxTree)
                _diagnostics.ReportGlobalStatementsShouldOnlyBeInASingleFile(globalStatementSyntax.Location);
        }
    }

    Result<(MethodSymbol Function, TypeSymbol Type), DiagnosticBag> GenerateMainMethod(
        BoundScope scope,
        bool isScript,
        Option<ImmutableArray<GlobalStatementDeclarationSyntax>> globalStatements)
    {
        var programType = TypeSymbol.New(SyntaxFacts.START_TYPE_NAME, Option.None,
                                         inheritanceClauseSyntax: null , 
                                         methodTable: new MethodTable(), fieldTable: new FieldTable(),
                                         baseTypes: new SingleOccurenceList<TypeSymbol>(), 
                                         isGenericMethodParameter: false, 
                                         isGenericClassParameter: false,
                                         genericParameters: Option.None, genericParameterTypeConstraints: Option.None);


        var mainMethodDeclarationSyntax = globalStatements.IsSome
            ? Option.Some(new CompilerGeneratedGlobalStatementsDeclarationsBlockStatementSyntax(globalStatements.Unwrap()))
            : Option.None;
        
        var main = new MethodSymbol(
            mainMethodDeclarationSyntax.UnwrapOrNull(),
            isStatic: true,
            isVirtual: false,
            isOverriding: false,
            name: SyntaxFacts.MAIN_METHOD_NAME,
            parameters: ImmutableArray<ParameterSymbol>.Empty,
            returnType: BuiltInTypeSymbols.Void, 
            containingType: programType,
            isGeneric: false, 
            genericParameters: Option.None);
        
        if (isScript)
        {
            main = new MethodSymbol(
                mainMethodDeclarationSyntax.UnwrapOrNull(),
                isStatic: true,
                isVirtual: false,
                isOverriding: false,
                name: SyntaxFacts.SCRIPT_MAIN_METHOD_NAME,
                parameters: ImmutableArray<ParameterSymbol>.Empty,
                returnType: BuiltInTypeSymbols.Object,
                containingType: programType, 
                isGeneric: false, 
                genericParameters: Option.None);
        }

        programType.MethodTable.AddMethodDeclaration(main, new List<TypeSymbol>());
        programType.MethodTable.SetMethodBody(main, null);
        
        if (!scope.TryDeclareType(programType))
        {
            var diagnostics = new DiagnosticBag();
            var alreadyDeclaredSymbol = scope.GetDeclaredTypes()
                .Single(x => x.Name == SyntaxFacts.START_TYPE_NAME);
            
            var existingDeclarationSyntax = _allDeclarations.LookupDeclarations<ClassDeclarationSyntax>(alreadyDeclaredSymbol);
            
            foreach (var syntax in existingDeclarationSyntax)
            {
                diagnostics.ReportCannotEmitGlobalStatementsBecauseTypeAlreadyExists(
                    programType.Name,
                    syntax.Identifier.Location);
            }
            
            return diagnostics;
        }


        return (main, programType);
    }

    MethodSymbol? TryFindMainMethod()
    {
        foreach (var function in _scope.GetDeclaredTypes().SelectMany(x => x.MethodTable.Select(declaration=>declaration.MethodSymbol)))
        {
            if (function.Name == SyntaxFacts.MAIN_METHOD_NAME)
                return function;
        }

        return null;
    }

    void DiagnoseMainMethodAndGlobalStatementsUsage(
        IEnumerable<GlobalStatementDeclarationSyntax> statements)
    {
        var methods = _scope.GetDeclaredTypes().SelectMany(x => x.MethodTable.Select(declaration=> declaration.MethodSymbol)).ToList();
        if (methods.Any(x => x.Name == "main") && statements.Any())
        {
            foreach (var method in methods.Where(x => x.Name == "main"))
            {
                var identifierLocation = method.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>()
                    .As<MethodDeclarationSyntax>().Identifier.Location;
                
                _diagnostics.ReportMainCannotBeUsedWithGlobalStatements(identifierLocation);
            }
        }

        foreach (var function in methods.Where(x => x.Name == "main"))
        {
            if (function.Parameters.Any()
                || !Equals(function.ReturnType, BuiltInTypeSymbols.Void) 
                || !function.IsStatic )
            {
                var identifierLocation = function.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>()
                    .Identifier.Location;
                
                _diagnostics.ReportMainMustHaveCorrectSignature(identifierLocation);
            }
        }
    }

    static BoundScope CreateParentScope(BoundGlobalScope? previous)
    {
        var stack = new Stack<BoundGlobalScope>();
        while (previous is not null)
        {
            stack.Push(previous);
            previous = previous.Previous;
        }

        var parent = CreateRootScope();

        while (stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parent);
            foreach (var variable in previous.Variables)
                scope.TryDeclareVariable(variable);

            foreach (var type in previous.Types)
                scope.TryDeclareType(type);

            parent = scope;
        }

        return parent;
    }

    static BoundScope CreateRootScope()
    {
        var result = new BoundScope(null);

        return result;
    }


    public BoundProgram BindProgram(
        bool isScript,
        BoundProgram? previous,
        BoundGlobalScope globalScope)
    {
        var parentScope = CreateParentScope(globalScope);

        var diagnostics = new DiagnosticBag();

        var typesToBind = globalScope.Types.Exclude(
            x => x == BuiltInTypeSymbols.Object
                 || x == BuiltInTypeSymbols.Void 
                 || x == BuiltInTypeSymbols.Bool 
                 || x == BuiltInTypeSymbols.Int 
                 || x == BuiltInTypeSymbols.String 
                 || x == BuiltInTypeSymbols.Error)
            .ToImmutableArray();

        foreach (var typeBinder in _typeBinders)
        {
            typeBinder.BindClassBody(diagnostics);
        }

        var boundProgram = new BoundProgram(
            previous,
            diagnostics.ToImmutableArray(),
            globalScope.MainMethod,
            globalScope.ScriptMainMethod,
            typesToBind);
        return boundProgram;
    }
}