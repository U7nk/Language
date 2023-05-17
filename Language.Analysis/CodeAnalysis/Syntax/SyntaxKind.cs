namespace Language.Analysis.CodeAnalysis.Syntax;

public enum SyntaxKind
{
    // tokens
    IdentifierToken,
    NumberToken,
    StringToken,
    WhitespaceToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    BadToken,
    EndOfFileToken,
    BangToken,
    AmpersandAmpersandToken,
    PipePipeToken,
    EqualsEqualsToken,
    BangEqualsToken,
    EqualsToken,
    OpenBraceToken,
    CloseBraceToken,
    SemicolonToken,
    LessThanOrEqualsToken,
    LessThanToken,
    GreaterThanOrEqualsToken,
    GreaterThanToken,
    PipeToken,
    AmpersandToken,
    HatToken,
    TildeToken,
    CommaToken,   
    DotToken,

    // keywords
    TrueKeyword,
    FalseKeyword,
    VarKeyword,
    LetKeyword,
    WhileKeyword,
    IfKeyword,
    ElseKeyword,
    ForKeyword,
    FunctionKeyword,
    BreakKeyword,
    ContinueKeyword,
    ReturnKeyword,
    ClassKeyword,
    ThisKeyword,
    NewKeyword,
    StaticKeyword,
    VirtualKeyword,
    OverrideKeyword,
    WhereKeyword,
    
    // nodes
    CompilationUnit,
    VariableDeclarationSyntax,
    VariableDeclarationAssignmentSyntax,
    TypeClause,
    InheritanceClause,
    ClassDeclaration,
    MethodDeclaration,
    FieldDeclaration,
    GlobalStatement,
    Parameter,
    GenericArgumentsList,
    GenericConstraintsClause,
    
    // expressions
    LiteralExpression,
    BinaryOperatorExpression,
    ParenthesizedExpression,
    UnaryExpression,
    AssignmentExpression,
    NameExpression,
    MethodCallExpression,
    ObjectCreationExpression,
    MemberAccessExpression,
    MemberAssignmentExpression,
    ThisExpression,
    CastExpression,
    NamedTypeExpression,
    
    
    // statements
    BlockStatement,
    ExpressionStatement,
    WhileStatement,
    VariableDeclarationStatement,
    IfStatement,
    ElseClause,   
    ForStatement,
    ColonToken,
    ContinueStatement,
    BreakStatement,
    ReturnStatement,
    VariableDeclarationAssignmentStatement,
    GlobalStatementsDeclarationsBlockStatement,
}