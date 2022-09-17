// See https://aka.ms/new-console-template for more information

using Wired.CodeAnalysis;
using Wired.CodeAnalysis.Syntax;

var st = SyntaxTree.Parse($$"""
var result = 0; 
        
        for (var i = 0; i < 15; i = i + 1) 
        { 
            if (i == 10){
                print(" break " + string(i));                                  
                break;  
            }
            if (i / 2 * 2 == i){
                print(" continue " + string(i));                 
                continue; 
            }
            
            print(" end " + string(i));
        }
        
        result;
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
