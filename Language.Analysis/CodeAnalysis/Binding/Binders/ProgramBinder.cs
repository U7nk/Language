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
            .ToList();
        DiagnoseMainMethodAndGlobalStatementsUsage(globalStatements);
        
        MethodSymbol? mainMethod = null;
        MethodSymbol? scriptMainMethod = null;
        if (IsScript)
        {
            if (globalStatements.Any())
            {
                var mainMethodGeneration = GenerateMainMethod(_scope, IsScript);
                if (mainMethodGeneration.IsFail)
                {
                    _diagnostics.MergeWith(mainMethodGeneration.Fail);
                }
                else
                {
                    scriptMainMethod = mainMethodGeneration.Success.Function;
                    var mainFunctionType = mainMethodGeneration.Success.Type;
                    BindTopMethods(mainFunctionType);
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
                var mainFunctionGeneration = GenerateMainMethod(_scope, IsScript);
                if (mainFunctionGeneration.IsFail)
                {
                    _diagnostics.AddRange(mainFunctionGeneration.Fail);
                }
                else
                {
                    mainMethod = mainFunctionGeneration.Success.Function;
                    var mainFunctionType = mainFunctionGeneration.Success.Type;
                    BindTopMethods(mainFunctionType);
                }
            }
        }


        var globalStatementMethod = mainMethod ?? scriptMainMethod;
        var programType = _scope.GetDeclaredTypes().SingleOrDefault(x => x.Name == SyntaxFacts.StartTypeName);
        
        if (globalStatementMethod is not null)
        {
            var methodBinder = new MethodBinder(_scope, 
                                                IsScript,
                                                new MethodBinderLookup(
                                                    _lookup.Declarations,
                                                    programType.NG(),
                                                    _scope.GetDeclaredTypes(),
                                                    globalStatementMethod));
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var globalStatement in globalStatements)
            {
                var s = methodBinder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(s);
            }

            globalStatementMethod.ContainingType.NG().MethodTable.SetMethodBody(
                globalStatementMethod,
                new BoundBlockStatement(null, statements.ToImmutableArray()));
            _diagnostics.AddRange(methodBinder.Diagnostics);
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

    void BindTopMethods(TypeSymbol mainFunctionType)
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

    Result<(MethodSymbol Function, TypeSymbol Type), DiagnosticBag> GenerateMainMethod(BoundScope scope,
        bool isScript)
    {
        var programType = TypeSymbol.New(SyntaxFacts.StartTypeName, Option<SyntaxNode>.None,
                                         inheritanceClauseSyntax: null , 
                                         new MethodTable(), new FieldTable());
        
        var main = new MethodSymbol(
            Option<SyntaxNode>.None,
            isStatic: true,
            name: SyntaxFacts.MainMethodName,
            parameters: ImmutableArray<ParameterSymbol>.Empty,
            returnType: BuiltInTypeSymbols.Void, 
            containingType: programType);
        
        if (isScript)
        {
            main = new MethodSymbol(
                Option<SyntaxNode>.None, 
                isStatic: true,
                name: SyntaxFacts.ScriptMainMethodName,
                parameters: ImmutableArray<ParameterSymbol>.Empty,
                returnType: BuiltInTypeSymbols.Object,
                containingType: programType);
        }

        programType.MethodTable.SetMethodBody(main, null);
        
        if (!scope.TryDeclareType(programType))
        {
            var diagnostics = new DiagnosticBag();
            var alreadyDeclaredSymbol = scope.GetDeclaredTypes()
                .Single(x => x.Name == SyntaxFacts.StartTypeName);
            
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
            if (function.Name == SyntaxFacts.MainMethodName)
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
        
        var mainMethod = globalScope.ScriptMainMethod ?? globalScope.MainMethod;
        
        if (mainMethod is not null && IsScript)
        {
            var statements = mainMethod.ContainingType.NG().MethodTable[mainMethod].NG().Statements;
            var expressionStatement = statements[^1] as BoundExpressionStatement;
            var needsReturn = expressionStatement is not null
                              && !Equals(expressionStatement.Expression.Type, BuiltInTypeSymbols.Void);
            if (needsReturn)
            {
                Debug.Assert(expressionStatement != null, nameof(expressionStatement) + " != null");
                statements = statements.SetItem(statements.Length - 1,
                    new BoundReturnStatement(null, expressionStatement.Expression));
            }
            else if (!ControlFlowGraph.AllPathsReturn(new BoundBlockStatement(null, statements)))
            {
                var nullValue = new BoundLiteralExpression(null, "null", BuiltInTypeSymbols.String);
                statements = statements.Add(new BoundReturnStatement(null, nullValue));
            }

            var properReturnBody = Lowerer.Lower(new BoundBlockStatement(null, statements));
            mainMethod.ContainingType.MethodTable[mainMethod] = properReturnBody;
            
            ControlFlowGraph.AllVariablesInitializedBeforeUse(properReturnBody, diagnostics);
        }
        else if (mainMethod is not null)
        {
            var statements = mainMethod.ContainingType.NG().MethodTable[mainMethod]?.Statements;
            if (statements?.Any() is true)
            {
                var body = Lowerer.Lower(new BoundBlockStatement(null, statements.NG()));
                mainMethod.ContainingType.MethodTable[mainMethod] = body;
                ControlFlowGraph.AllVariablesInitializedBeforeUse(body, diagnostics);
            }
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