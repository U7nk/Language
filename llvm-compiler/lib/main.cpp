#include <iostream>
#include <llvm/IR/Module.h>
#include <llvm/IR/LLVMContext.h>
#include <llvm/Support/SourceMgr.h>
#include <llvm/Support/raw_ostream.h>
#include <llvm/IRReader/IRReader.h>
#include <llvm/ExecutionEngine/ExecutionEngine.h>
#include <llvm/ExecutionEngine/GenericValue.h>
#include <llvm/Target/TargetMachine.h>
#include "llvm/ExecutionEngine/JITLink/JITLink.h"
#include "llvm/ADT/StringRef.h"
#include "llvm/ExecutionEngine/Interpreter.h"
#include "llvm/ExecutionEngine/Orc/CompileUtils.h"
#include "llvm/ExecutionEngine/Orc/Core.h"
#include "llvm/ExecutionEngine/Orc/ExecutionUtils.h"
#include "llvm/ExecutionEngine/Orc/IRCompileLayer.h"
#include "llvm/ExecutionEngine/Orc/JITTargetMachineBuilder.h"
#include "llvm/ExecutionEngine/Orc/RTDyldObjectLinkingLayer.h"
#include "llvm/ExecutionEngine/SectionMemoryManager.h"
#include "llvm/IR/DataLayout.h"
#include "llvm/IR/LLVMContext.h"
#include <llvm/IR/IRBuilder.h>
#include <llvm/IR/LLVMContext.h>
#include <llvm/IR/Module.h>
#include <llvm/IR/Verifier.h>
#include <llvm/Support/TargetSelect.h>
#include <llvm/Target/TargetMachine.h>
#include <llvm/ExecutionEngine/ExecutionEngine.h>
#include <llvm/ExecutionEngine/GenericValue.h>
#include <llvm/IR/LegacyPassManager.h>
#include <llvm/Support/raw_os_ostream.h>
#include "llvm/ExecutionEngine/MCJIT.h"


using namespace llvm;

int main(int argc, char** argv)
{
    llvm::InitializeNativeTarget();
    llvm::InitializeNativeTargetAsmParser();
    llvm::InitializeNativeTargetAsmPrinter();

    std::string fileName = "D:\\codes\\Language\\llvm-compiler\\sample.ll";
    LLVMContext context;
    SMDiagnostic err;

    auto machine = std::unique_ptr<llvm::TargetMachine>(llvm::EngineBuilder().selectTarget());
    auto mod = llvm::parseIRFile(fileName, err, context);
    mod->setDataLayout(machine->createDataLayout());

    if (!mod)
    {
        err.print(argv[0], errs());
        return 1;
    }
    mod->dump();



    EngineBuilder eb = EngineBuilder(std::move(mod));
    auto builderErrors = std::string();
    eb.setErrorStr(&builderErrors);
    eb.setEngineKind(llvm::EngineKind::JIT);
    auto engine = eb.create();

    if (engine == nullptr)
    {
        std::cout <<"Failed to create builder: \n" << builderErrors << std::endl;
        return 1;
    }
    
    auto res = engine->runFunction(engine->FindFunctionNamed("main"), ArrayRef<GenericValue>());
}