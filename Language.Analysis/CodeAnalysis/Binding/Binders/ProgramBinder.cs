using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding.Lookup;
using Language.Analysis.CodeAnalysis.Lowering;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;

namespace Language.Analysis.CodeAnalysis.Binding.Binders;

sealed class ProgramBinder
{
    readonly BoundGlobalScope? _previous;
    readonly BoundScope _scope;
    readonly ImmutableArray<SyntaxTree> _syntaxTrees;
    readonly DiagnosticBag _diagnostics = new();
    readonly BinderLookup _lookup;

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;
    public bool IsScript { get; }

    internal ProgramBinder(bool isScript,
        BoundGlobalScope? previous,
        ImmutableArray<SyntaxTree> syntaxTrees, BinderLookup lookup)
    {
        _previous = previous;
        if (_previous is not null)
            _diagnostics.InsertRange(0, _previous.Diagnostics);
        _scope = CreateParentScope(_previous);
        _syntaxTrees = syntaxTrees;
        _lookup = lookup;
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

    public BoundGlobalScope BindGlobalScope()
    {
        DeclareBuiltInTypes();

        var classDeclarations = _syntaxTrees
            .Only<ClassDeclarationSyntax>()
            .ToImmutableArray();

        var typeSignatureBinder = new TypeSignatureBinder(_scope, _lookup);
        foreach (var classDeclaration in classDeclarations)
        {
            var typeSignatureBindDiagnostics = typeSignatureBinder.BindClassDeclaration(classDeclaration); 
            _diagnostics.MergeWith(typeSignatureBindDiagnostics);
        }

        var typeMembersSignaturesBinder = new TypeMembersSignaturesBinder(
            new BinderLookup(_scope.GetDeclaredTypes(), _lookup.Declarations),
            _scope,
            IsScript);
        foreach (var typeSymbol in _scope.GetDeclaredTypes().Except(BuiltInTypeSymbols.All))
        {
            var membersSignaturesBindDiagnostics = typeMembersSignaturesBinder.BindMembersSignatures(typeSymbol);
            _diagnostics.MergeWith(membersSignaturesBindDiagnostics);
        }

        DiagnoseGlobalStatementsUsage();
        
        var globalStatements = _syntaxTrees
            .Only<GlobalStatementDeclarationSyntax>()
            .ToImmutableArray();
        DiagnoseMainMethodAndGlobalStatementsUsage(globalStatements);
        
        MethodSymbol? mainMethod = null;
        MethodSymbol? scriptMainMethod = null;
        if (IsScript)
        {
            if (globalStatements.Any())
            {
                var mainMethodGeneration = GenerateMainMethod(_scope, IsScript, globalStatements);
                if (mainMethodGeneration.IsFail)
                {
                    _diagnostics.MergeWith(mainMethodGeneration.Fail);
                }
                else
                {
                    scriptMainMethod = mainMethodGeneration.Success.Function;
                    var mainFunctionType = mainMethodGeneration.Success.Type;
                    BindTopMethodsSignatures(mainFunctionType);
                }
            }
            else
            {
                scriptMainMethod = TryFindMainMethod();
                if (scriptMainMethod is { })
                {
                    _diagnostics.ReportNoMainMethodAllowedInScriptMode(
                        scriptMainMethod.DeclarationSyntax.UnwrapAs<MethodDeclarationSyntax>().Identifier.Location);
                }
            }
        }
        else
        {
            mainMethod = TryFindMainMethod();
            if (mainMethod is null && globalStatements.Any())
            {
                var mainFunctionGeneration = GenerateMainMethod(_scope, IsScript, globalStatements);
                if (mainFunctionGeneration.IsFail)
                {
                    _diagnostics.AddRange(mainFunctionGeneration.Fail);
                }
                else
                {
                    mainMethod = mainFunctionGeneration.Success.Function;
                    var mainFunctionType = mainFunctionGeneration.Success.Type;
                    BindTopMethodsSignatures(mainFunctionType);
                }
            }
        }
        
        if (mainMethod is null && scriptMainMethod is null)
        {
            _diagnostics.ReportMainMethodShouldBeDeclared(_syntaxTrees.First().SourceText);
        }

        return new BoundGlobalScope(
            _previous, _diagnostics.ToImmutableArray(),
            mainMethod, scriptMainMethod,
            _scope.GetDeclaredTypes(), _scope.GetDeclaredVariables(),
            _lookup.Declarations);
    }

    void BindTopMethodsSignatures(TypeSymbol mainFunctionType)
    {
        var topMethodDeclarations = _syntaxTrees
            .Only<MethodDeclarationSyntax>()
            .ToImmutableArray();
        var methodSignatureBinder = new MethodSignatureBinder(
            new MethodSignatureBinderLookup(_scope.GetDeclaredTypes(), mainFunctionType, isTopMethod: true, _lookup.Declarations), 
            _scope);
        foreach (var topMethodDeclaration in topMethodDeclarations)
        {
            methodSignatureBinder.BindMethodSignature(topMethodDeclaration)
                .AddRangeTo(_diagnostics);   
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
                                         new MethodTable(), new FieldTable());


        var mainMethodDeclarationSyntax = globalStatements.IsSome
            ? Option.Some(new CompilerGeneratedGlobalStatementsDeclarationsBlockStatementSyntax(globalStatements.Unwrap()))
            : Option.None;
        
        var main = new MethodSymbol(
            mainMethodDeclarationSyntax.UnwrapOrNull(),
            isStatic: true,
            name: SyntaxFacts.MAIN_METHOD_NAME,
            parameters: ImmutableArray<ParameterSymbol>.Empty,
            returnType: BuiltInTypeSymbols.Void, 
            containingType: programType);
        
        if (isScript)
        {
            main = new MethodSymbol(
                mainMethodDeclarationSyntax.UnwrapOrNull(),
                isStatic: true,
                name: SyntaxFacts.SCRIPT_MAIN_METHOD_NAME,
                parameters: ImmutableArray<ParameterSymbol>.Empty,
                returnType: BuiltInTypeSymbols.Object,
                containingType: programType);
        }

        programType.MethodTable.SetMethodBody(main, null);
        
        if (!scope.TryDeclareType(programType))
        {
            var diagnostics = new DiagnosticBag();
            var alreadyDeclaredSymbol = scope.GetDeclaredTypes()
                .Single(x => x.Name == SyntaxFacts.START_TYPE_NAME);
            
            var existingDeclarationSyntax = _lookup.LookupDeclarations<ClassDeclarationSyntax>(alreadyDeclaredSymbol);
            
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
        foreach (var function in _scope.GetDeclaredTypes().SelectMany(x => x.MethodTable.Symbols))
        {
            if (function.Name == SyntaxFacts.MAIN_METHOD_NAME)
                return function;
        }

        return null;
    }

    void DiagnoseMainMethodAndGlobalStatementsUsage(
        IEnumerable<GlobalStatementDeclarationSyntax> statements)
    {
        var methods = _scope.GetDeclaredTypes().SelectMany(x => x.MethodTable.Symbols).ToList();
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
        var availableTypes = globalScope.Types;

        foreach (var type in typesToBind)
        {
            var binder = new TypeBinder(parentScope, isScript, new TypeBinderLookup(type, availableTypes, _lookup.Declarations));
            binder.BindClassBody();
            diagnostics.AddRange(binder.Diagnostics);
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