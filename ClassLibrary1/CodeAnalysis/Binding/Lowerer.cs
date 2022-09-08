namespace Wired.CodeAnalysis.Binding;

internal sealed class Lowerer : BoundTreeRewriter
{
    private Lowerer()
    {
    }

    public static BoundStatement Lower(BoundStatement statement)
    {
        var lowerer = new Lowerer();
        return lowerer.RewriteStatement(statement);
    }
}