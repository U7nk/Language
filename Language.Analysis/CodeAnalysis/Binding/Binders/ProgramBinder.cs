using System;
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
        _scope.TryDeclareType(TypeSymbol.Any);
        _scope.TryDeclareType(TypeSymbol.Bool);
        _scope.TryDeclareType(TypeSymbol.Int);
        _scope.TryDeclareType(TypeSymbol.String);
        _scope.TryDeclareType(TypeSymbol.Void);
    }

    public BoundGlobalScope BindGlobalScope()
    {
        DeclareBuiltInTypes();

        var classDeclarations = _syntaxTrees
            .Only<ClassDeclarationSyntax>()
            .ToImmutableArray();

        var typeSignatureBinder = new TypeSignatureBinder(_scope);
        classDeclarations.ForEach(classDeclaration =>
            typeSignatureBinder.BindClassDeclaration(classDeclaration).AddRangeTo(_diagnostics));

        var typeMembersSignaturesBinder =
            new TypeMembersSignaturesBinder(new BaseBinderLookup(_scope.GetDeclaredTypes()), _scope, IsScript);
        classDeclarations.ForEach(classDeclaration =>
            typeMembersSignaturesBinder.BindMembersSignatures(classDeclaration).AddRangeTo(_diagnostics));

        var topFunctionDeclarations = _syntaxTrees
            .Only<FunctionDeclarationSyntax>()
            .ToImmutableArray();

        var functionSignatureBinder =
            new FunctionSignatureBinder(new BaseBinderLookup(_scope.GetDeclaredTypes()), _scope);
        topFunctionDeclarations.ForEach(topFunctionDeclaration => 
            functionSignatureBinder.BindFunctionSignature(topFunctionDeclaration).AddRangeTo(_diagnostics));
        

        DiagnoseGlobalStatementsUsage();
        
        var globalStatements = _syntaxTrees
            .Only<GlobalStatementSyntax>()
            .ToList();
        DiagnoseMainFunctionAndGlobalStatementsUsage(globalStatements);

        var topFunctionsSymbols = _scope.GetDeclaredFunctions()
            .Except(BuiltInFunctions.GetAll())
            .ToImmutableArray();
        FunctionSymbol? mainFunction = null;
        FunctionSymbol? scriptMainFunction = null;
        if (IsScript)
        {
            if (globalStatements.Any())
            {
                var mainFunctionGeneration = GenerateMainFunction(_scope, IsScript);
                if (mainFunctionGeneration.IsFail)
                {
                    _diagnostics.AddRange(mainFunctionGeneration.Fail);
                }
                else
                {
                    scriptMainFunction = mainFunctionGeneration.Success.Function;
                    var mainFunctionType = mainFunctionGeneration.Success.Type;
                    foreach (var topFunction in topFunctionsSymbols)
                    {
                        mainFunctionType.MethodTable.Declare(topFunction, null);
                    }
                }
            }
            else
            {
                var foundMain = TryFindMainFunction();
                if (foundMain is { })
                {
                    _diagnostics.ReportNoMainFunctionAllowedInScriptMode(foundMain.Declaration.Unwrap().Identifier
                        .Location);
                }
            }
        }
        else
        {
            mainFunction = TryFindMainFunction();
            if (mainFunction is null && globalStatements.Any())
            {
                var mainFunctionGeneration = GenerateMainFunction(_scope, IsScript);
                if (mainFunctionGeneration.IsFail)
                {
                    _diagnostics.AddRange(mainFunctionGeneration.Fail);
                }
                else
                {
                    mainFunction = mainFunctionGeneration.Success.Function;
                    var mainFunctionType = mainFunctionGeneration.Success.Type;
                    foreach (var topFunction in topFunctionsSymbols)
                    {
                        mainFunctionType.MethodTable.Declare(topFunction, null);
                    }
                }
            }
        }


        var programType = _scope.GetDeclaredTypes().SingleOrDefault(x => x.Name == SyntaxFacts.StartTypeName);

        var globalStatementFunction = mainFunction ?? scriptMainFunction;
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        if (globalStatementFunction is not null)
        {
            var functionBinder = new FunctionBinder(_scope, IsScript, new FunctionBinderLookup(
                programType.Unwrap(),
                _scope.GetDeclaredTypes(),
                globalStatementFunction));

            foreach (var globalStatement in globalStatements)
            {
                var s = functionBinder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(s);
            }

            _diagnostics.AddRange(functionBinder.Diagnostics);
        }


        return new BoundGlobalScope(
            _previous, _diagnostics.ToImmutableArray(),
            mainFunction, scriptMainFunction,
            _scope.GetDeclaredTypes(), _scope.GetDeclaredVariables(),
            new BoundBlockStatement(statements.ToImmutable()));
    }

    void DiagnoseGlobalStatementsUsage()
    {
        var firstGlobalStatementPerSyntaxTree = _syntaxTrees
            .Select(x => x.Root.Members.OfType<GlobalStatementSyntax>().FirstOrDefault())
            .Where(x => x is not null)
            .Cast<GlobalStatementSyntax>()
            .ToList();
        if (firstGlobalStatementPerSyntaxTree.Count > 1)
        {
            foreach (var globalStatementSyntax in firstGlobalStatementPerSyntaxTree)
                _diagnostics.ReportGlobalStatementsShouldOnlyBeInASingleFile(globalStatementSyntax.Location);
        }
    }

    static Result<(FunctionSymbol Function, TypeSymbol Type), DiagnosticBag> GenerateMainFunction(BoundScope scope,
        bool isScript)
    {
        var main = new FunctionSymbol(SyntaxFacts.MainFunctionName,
            ImmutableArray<ParameterSymbol>.Empty,
            TypeSymbol.Void,
            null
        );
        if (isScript)
        {
            main = new FunctionSymbol(SyntaxFacts.ScriptMainFunctionName,
                ImmutableArray<ParameterSymbol>.Empty,
                TypeSymbol.Any,
                null
            );
        }

        var type = TypeSymbol.New(SyntaxFacts.StartTypeName, null, new MethodTable { { main, null } },
            new FieldTable());
        if (!scope.TryDeclareType(type))
        {
            var diagnostics = new DiagnosticBag();
            var alreadyDeclared = scope.GetDeclaredTypes()
                .Single(x => x.Name == SyntaxFacts.StartTypeName);
            diagnostics.ReportCannotEmitGlobalStatementsBecauseTypeAlreadyExists(type.Name,
                alreadyDeclared.Declaration.Unwrap().Location);
            return diagnostics;
        }

        return (main, type);
    }

    FunctionSymbol? TryFindMainFunction()
    {
        foreach (var function in _scope.GetDeclaredTypes().SelectMany(x => x.MethodTable.Symbols))
        {
            if (function.Name == SyntaxFacts.MainFunctionName)
                return function;
        }

        return null;
    }

    void DiagnoseMainFunctionAndGlobalStatementsUsage(
        IEnumerable<GlobalStatementSyntax> statements)
    {
        var functions = _scope.GetDeclaredTypes().SelectMany(x => x.MethodTable.Symbols).ToList();
        if (functions.Any(x => x.Name == "main") && statements.Any())
        {
            foreach (var function in functions.Where(x => x.Name == "main"))
            {
                var identifierLocation = function
                    .Declaration?
                    .Identifier
                    .Location ?? throw new InvalidOperationException();
                _diagnostics.ReportMainCannotBeUsedWithGlobalStatements(identifierLocation);
            }
        }

        foreach (var function in functions.Where(x => x.Name == "main"))
        {
            if (function.Parameters.Any()
                || !Equals(function.ReturnType, TypeSymbol.Void))
            {
                var identifierLocation = function
                    .Declaration?
                    .Identifier
                    .Location ?? throw new InvalidOperationException();
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
        foreach (var functionSymbol in BuiltInFunctions.GetAll())
            result.TryDeclareFunction(functionSymbol);

        return result;
    }


    public static BoundProgram BindProgram(
        bool isScript,
        BoundProgram? previous,
        BoundGlobalScope globalScope)
    {
        var parentScope = CreateParentScope(globalScope);

        var diagnostics = new DiagnosticBag();

        var typesToBind = globalScope.Types;

        foreach (var type in typesToBind)
        {
            var binder = new TypeBinder(parentScope, isScript, new TypeBinderLookup(type, typesToBind));
            var typeBindingDiagnostic = binder.BindBody();
            diagnostics.AddRange(typeBindingDiagnostic);
        }

        if (globalScope.ScriptMainFunction is not null)
        {
            var statements = globalScope.Statement.Statements;
            var expressionStatement = statements[^1] as BoundExpressionStatement;
            var needsReturn = expressionStatement is not null
                              && !Equals(expressionStatement.Expression.Type, TypeSymbol.Void);
            if (needsReturn)
            {
                Debug.Assert(expressionStatement != null, nameof(expressionStatement) + " != null");
                statements = statements.SetItem(statements.Length - 1,
                    new BoundReturnStatement(expressionStatement.Expression));
            }
            else if (!ControlFlowGraph.AllPathsReturn(new BoundBlockStatement(statements)))
            {
                var nullValue = new BoundLiteralExpression("", TypeSymbol.String);
                statements = statements.Add(new BoundReturnStatement(nullValue));
            }

            var type = globalScope.Types.Single(x => x.MethodTable.ContainsKey(globalScope.ScriptMainFunction));
            var properReturnBody = Lowerer.Lower(new BoundBlockStatement(statements));
            type.MethodTable[globalScope.ScriptMainFunction] = properReturnBody;
        }
        else if (globalScope.MainFunction is not null && globalScope.Statement.Statements.Any())
        {
            var body = Lowerer.Lower(new BoundBlockStatement(globalScope.Statement.Statements));
            var type = globalScope.Types.Single(x => x.MethodTable.ContainsKey(globalScope.MainFunction));
            type.MethodTable[globalScope.MainFunction] = body;
        }

        var boundProgram = new BoundProgram(
            previous,
            diagnostics.ToImmutableArray(),
            globalScope.MainFunction,
            globalScope.ScriptMainFunction,
            typesToBind);
        return boundProgram;
    }
}