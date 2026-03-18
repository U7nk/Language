using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Language.Analysis.CodeAnalysis.Binding;
using Language.Analysis.CodeAnalysis.Symbols;
using Language.Analysis.CodeAnalysis.Syntax;
using Language.Analysis.Extensions;

namespace Language.Analysis.CodeAnalysis.Emit;

class Emitter
{
    BoundProgram? _program;

    public ImmutableArray<Diagnostic> Emit(
        BoundProgram program,
        string moduleName,
        string[] references,
        string outputPath)
    {
        _program = program;
        var types = _program.Types;

        var stringWriter = new StringWriter();
        // add c printf
        stringWriter.WriteLine(CoreFunctionsLLVMIR.Malloc);
        stringWriter.WriteLine(CoreFunctionsLLVMIR.RuntimeDeclarations);
        stringWriter.WriteLine(CoreFunctionsLLVMIR.PrintF);
        stringWriter.WriteLine(CoreFunctionsLLVMIR.SPrintF);
        stringWriter.WriteLine(CoreFunctionsLLVMIR.MyStringType);
        stringWriter.WriteLine(CoreFunctionsLLVMIR.Int32ToStringFunction);
        stringWriter.WriteLine(CoreFunctionsLLVMIR.MyPrintFunction);
        stringWriter.WriteLine(CoreFunctionsLLVMIR.MyStringConcatFunction);
        
        EmitTypes(types, stringWriter);

        EmitMethods(types, stringWriter);
        Console.WriteLine(stringWriter.ToString());
        var file = File.CreateText(@"D:\codes\Language\llvm-compiler\sample.ll");
        file.Write(stringWriter.ToString());
        file.Flush();
        return ImmutableArray<Diagnostic>.Empty;
    }
    
    private void EmitMethods(ICollection<TypeSymbol> types, TextWriter writer)
    {
        // find program type
        var programType = types.Single(x => x.Name == SyntaxFacts.PROGRAM_TYPE_NAME);
        var mainMethod = programType.MethodTable.Single(x => x.MethodSymbol.Name == SyntaxFacts.MAIN_METHOD_NAME);
        
        // emit main method of LLVM IR
        writer.WriteLine("define i32 @main() {");
        writer.WriteLine("entry:");
        writer.WriteLine($"%main_result = call i32 {EmittingShared.GetMethodName(mainMethod.MethodSymbol, true)}()");
        writer.WriteLine("ret i32 %main_result");
        writer.WriteLine("}");
        
        foreach (var type in types)
        {
            foreach (var methodDeclaration in type.MethodTable.Select(x => x))
            {
                var methodEmitter = new MethodEmitter(methodDeclaration);
                var res = methodEmitter.Emit();
                foreach (var line in res)
                {
                    writer.Write(line.ToString());
                }
            }
        }
    }
    
    private void EmitTypes(ICollection<TypeSymbol> types, StringWriter writer)
    {
        foreach (var type in types)
        {
            var typeFields = type.FieldTable.Symbols.ToList();
            List<string> str = [ ];
            foreach (var fieldSymbol in typeFields)
            {
                EmittingShared.GetIRForType(fieldSymbol.Type).AddTo(str);
            }

            if (typeFields.Empty())
                str.Add("i1");
            
            writer.Write($"%\"{type.GetFullName()}\" = type {{ {string.Join(", ", str)} }}\n");
        }
    }
}