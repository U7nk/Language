// See https://aka.ms/new-console-template for more information

using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Syntax;

var st = SyntaxTree.Parse($$"""
{
print("Hello" + "World" + "!"); 
                let f = 50;
                print(string(f));
}
""");

var result = new Compilation(st)
    .Evaluate(new());

if (result.Diagnostics.Any())
{
    foreach (var resultDiagnostic in result.Diagnostics)
    {
        Console.WriteLine(resultDiagnostic);
    }
}
else
{
    Console.Write("Success: ");
    Console.WriteLine(result.Result ?? "NULL");    
}
