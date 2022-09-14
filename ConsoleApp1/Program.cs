// See https://aka.ms/new-console-template for more information

using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Syntax;

var st = SyntaxTree.Parse($$"""

function fibonacci(count : int)
{ 
    var x = count; 
    if x == 0
    {
        print("Done"); 
    }
    else
    {  
        fibonacci(x - 1); 
        print(string(x));
        x = 42;
     }
}
fibonacci(3);
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
