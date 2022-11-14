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

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;
    public bool IsScript { get; }

    internal ProgramBinder(bool isScript,
        BoundGlobalScope? previous,
        ImmutableArray<SyntaxTree> syntaxTrees)
    {
        _previous = previous;
        if (_previous is not null)
            _diagnostics.InsertRange(0, _previous.Diagnostics);
        _scope = CreateParentScope(_previous);
        _syntaxTrees = syntaxTrees;
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

        var classDeclarationTypeMap = new Dictionary<ClassDeclarationSyntax, TypeSymbol>();
        
        var typeSignatureBinder = new TypeSignatureBinder(_scope);
        classDeclarations.ForEach(classDeclaration =>
        {
            var bindingResult = typeSignatureBinder.BindClassDeclaration(classDeclaration); 
            bindingResult.Diagnostics.AddRangeTo(_diagnostics);
            classDeclarationTypeMap.Add(classDeclaration, bindingResult.TypeSymbol);
        });

        var typeMembersSignaturesBinder =
            new TypeMembersSignaturesBinder(new BinderLookup(_scope.GetDeclaredTypes()), _scope, IsScript);
        foreach (var (declaration, typeSymbol) in classDeclarationTypeMap)
        {
            typeMembersSignaturesBinder.BindMembersSignatures(declaration, typeSymbol).AddRangeTo(_diagnostics);
        }
        
        DiagnoseGlobalStatementsUsage();
        
        var globalStatements = _syntaxTrees
            .Only<GlobalStatementDeclarationSyntax>()
            .ToList();
        DiagnoseMainMethodAndGlobalStatementsUsage(globalStatements);
        
        MethodSymbol? mainFunction = null;
        MethodSymbol? scriptMainFunction = null;
        if (IsScript)
        {
            if (globalStatements.Any())
            {
                var mainFunctionGeneration = GenerateMainMethod(_scope, IsScript);
                if (mainFunctionGeneration.IsFail)
                {
                    _diagnostics.AddRange(mainFunctionGeneration.Fail);
                }
                else
                {
                    scriptMainFunction = mainFunctionGeneration.Success.Function;
                    var mainFunctionType = mainFunctionGeneration.Success.Type;
                    BindTopMethods(mainFunctionType);
                }
            }
            else
            {
                var foundMain = TryFindMainMethod();
                if (foundMain is { })
                {
                    _diagnostics.ReportNoMainMethodAllowedInScriptMode(
                        foundMain.DeclarationSyntax.Cast<MethodDeclarationSyntax>().First().Identifier
                        .Location);
                }
            }
        }
        else
        {
            mainFunction = TryFindMainMethod();
            if (mainFunction is null && globalStatements.Any())
            {
                var mainFunctionGeneration = GenerateMainMethod(_scope, IsScript);
                if (mainFunctionGeneration.IsFail)
                {
                    _diagnostics.AddRange(mainFunctionGeneration.Fail);
                }
                else
                {
                    mainFunction = mainFunctionGeneration.Success.Function;
                    var mainFunctionType = mainFunctionGeneration.Success.Type;
                    BindTopMethods(mainFunctionType);
                }
            }
        }


        var programType = _scope.GetDeclaredTypes().SingleOrDefault(x => x.Name == SyntaxFacts.StartTypeName);

        var globalStatementFunction = mainFunction ?? scriptMainFunction;
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        if (globalStatementFunction is not null)
        {
            var functionBinder = new MethodBinder(_scope, IsScript, new MethodBinderLookup(
                programType.NG(),
                _scope.GetDeclaredTypes(),
                globalStatementFunction));

            foreach (var globalStatement in globalStatements)
            {
                var s = functionBinder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(s);
            }

            _diagnostics.AddRange(functionBinder.Diagnostics);
        }

        if (mainFunction is null && scriptMainFunction is null)
        {
            _diagnostics.ReportMainMethodShouldBeDeclared(_syntaxTrees.First().SourceText);
        }

        return new BoundGlobalScope(
            _previous, _diagnostics.ToImmutableArray(),
            mainFunction, scriptMainFunction,
            _scope.GetDeclaredTypes(), _scope.GetDeclaredVariables(),
            new BoundBlockStatement(null, statements.ToImmutable()));
    }

    void BindTopMethods(TypeSymbol mainFunctionType)
    {
        var topMethodDeclarations = _syntaxTrees
            .Only<MethodDeclarationSyntax>()
            .ToImmutableArray();
        var methodSignatureBinder = new MethodSignatureBinder(
            new MethodSignatureBinderLookup(_scope.GetDeclaredTypes(), mainFunctionType, isTopMethod: true), 
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

    static Result<(MethodSymbol Function, TypeSymbol Type), DiagnosticBag> GenerateMainMethod(BoundScope scope,
        bool isScript)
    {
        var programType = TypeSymbol.New(SyntaxFacts.StartTypeName, ImmutableArray<SyntaxNode>.Empty,
                                         inheritanceClauseSyntax: null , 
                                         new MethodTable(), new FieldTable());
        
        var main = new MethodSymbol(
            ImmutableArray<SyntaxNode>.Empty,
            isStatic: true,
            name: SyntaxFacts.MainMethodName,
            parameters: ImmutableArray<ParameterSymbol>.Empty,
            returnType: BuiltInTypeSymbols.Void, 
            containingType: programType);
        
        if (isScript)
        {
            main = new MethodSymbol(
                ImmutableArray<SyntaxNode>.Empty,
                isStatic: true,
                name: SyntaxFacts.ScriptMainMethodName,
                parameters: ImmutableArray<ParameterSymbol>.Empty,
                returnType: BuiltInTypeSymbols.Object,
                containingType: programType);
        }

        programType.MethodTable.Declare(main, null);
        
        if (!scope.TryDeclareType(programType))
        {
            var diagnostics = new DiagnosticBag();
            var alreadyDeclared = scope.GetDeclaredTypes()
                .Single(x => x.Name == SyntaxFacts.StartTypeName);
            
            foreach (var syntax in alreadyDeclared.DeclarationSyntax.Cast<ClassDeclarationSyntax>())
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
                var identifierLocation = method.DeclarationSyntax
                    .Cast<MethodDeclarationSyntax>()
                    .First().Identifier.Location;
                
                _diagnostics.ReportMainCannotBeUsedWithGlobalStatements(identifierLocation);
            }
        }

        foreach (var function in methods.Where(x => x.Name == "main"))
        {
            if (function.Parameters.Any()
                || !Equals(function.ReturnType, BuiltInTypeSymbols.Void) 
                || !function.IsStatic )
            {
                var identifierLocation = function.DeclarationSyntax
                    .Cast<MethodDeclarationSyntax>()
                    .First().Identifier.Location;
                
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


    public static BoundProgram BindProgram(
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
            var binder = new TypeBinder(parentScope, isScript, new TypeBinderLookup(type, availableTypes));
            binder.BindClassBody();
            diagnostics.AddRange(binder.Diagnostics);
        }

        if (globalScope.ScriptMainMethod is not null)
        {
            var statements = globalScope.Statement.Statements;
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

            var type = globalScope.Types.Single(x => x.MethodTable.ContainsKey(globalScope.ScriptMainMethod));
            var properReturnBody = Lowerer.Lower(new BoundBlockStatement(null, statements));
            type.MethodTable[globalScope.ScriptMainMethod] = properReturnBody;
            
            ControlFlowGraph.AllVariablesInitializedBeforeUse(properReturnBody, diagnostics);
        }
        else if (globalScope.MainMethod is not null && globalScope.Statement.Statements.Any())
        {
            var body = Lowerer.Lower(new BoundBlockStatement(null, globalScope.Statement.Statements));
            var type = globalScope.Types.Single(x => x.MethodTable.ContainsKey(globalScope.MainMethod));
            type.MethodTable[globalScope.MainMethod] = body;
            
            ControlFlowGraph.AllVariablesInitializedBeforeUse(body, diagnostics);
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