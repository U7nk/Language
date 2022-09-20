namespace compiler;

internal static class ConsoleEx
{
    public static void WriteLine(string format = "", params object[] args)
    {
        Console.WriteLine(format, args);
    }
    
    public static void Write(string text, ConsoleColor? color = null)
    {
        if (color is null)
        {
            color = Console.ForegroundColor;
        }
        
        var colorBefore = Console.ForegroundColor;
        Console.ForegroundColor = (ConsoleColor)color;
        Console.Write(text);
        Console.ForegroundColor = colorBefore;
    }
}