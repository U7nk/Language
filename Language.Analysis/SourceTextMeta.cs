using System.Runtime.CompilerServices;

namespace Language.Analysis;

public static class SourceTextMeta
{
    
    public static string GetCurrentInvokeLocation(
        [CallerLineNumber] int lineNumber = 0, 
        [CallerFilePath] string caller = "") 
        => $"{caller} at line {lineNumber}";
}