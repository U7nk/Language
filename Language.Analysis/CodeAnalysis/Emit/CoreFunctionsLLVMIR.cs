using System;

namespace Language.Analysis.CodeAnalysis.Emit;

public class CoreFunctionsLLVMIR
{
      public static string PrintF = "declare i32 @printf(ptr, ...)";
      public static string SPrintF
            = $$$$$$$""""""
                 
                 $sprintf = comdat any
                 
                 $vsprintf = comdat any
                 
                 $_snprintf = comdat any
                 
                 $_vsnprintf = comdat any
                 
                 $_vsprintf_l = comdat any
                 
                 $_vsnprintf_l = comdat any
                 
                 $__local_stdio_printf_options = comdat any
                 
                 @__local_stdio_printf_options._OptionsStorage = internal global i64 0, align 8
                 
                 ; Function Attrs: noinline nounwind optnone uwtable
                 define linkonce_odr dso_local i32 @sprintf(ptr noundef %0, ptr noundef %1, ...) #0 comdat {
                   %3 = alloca ptr, align 8
                   %4 = alloca ptr, align 8
                   %5 = alloca i32, align 4
                   %6 = alloca ptr, align 8
                   store ptr %1, ptr %3, align 8
                   store ptr %0, ptr %4, align 8
                   call void @llvm.va_start.p0(ptr %6)
                   %7 = load ptr, ptr %6, align 8
                   %8 = load ptr, ptr %3, align 8
                   %9 = load ptr, ptr %4, align 8
                   %10 = call i32 @_vsprintf_l(ptr noundef %9, ptr noundef %8, ptr noundef null, ptr noundef %7)
                   store i32 %10, ptr %5, align 4
                   call void @llvm.va_end.p0(ptr %6)
                   %11 = load i32, ptr %5, align 4
                   ret i32 %11
                 }
                 
                 ; Function Attrs: noinline nounwind optnone uwtable
                 define linkonce_odr dso_local i32 @vsprintf(ptr noundef %0, ptr noundef %1, ptr noundef %2) #0 comdat {
                   %4 = alloca ptr, align 8
                   %5 = alloca ptr, align 8
                   %6 = alloca ptr, align 8
                   store ptr %2, ptr %4, align 8
                   store ptr %1, ptr %5, align 8
                   store ptr %0, ptr %6, align 8
                   %7 = load ptr, ptr %4, align 8
                   %8 = load ptr, ptr %5, align 8
                   %9 = load ptr, ptr %6, align 8
                   %10 = call i32 @_vsnprintf_l(ptr noundef %9, i64 noundef -1, ptr noundef %8, ptr noundef null, ptr noundef %7)
                   ret i32 %10
                 }
                 
                 ; Function Attrs: noinline nounwind optnone uwtable
                 define linkonce_odr dso_local i32 @_snprintf(ptr noundef %0, i64 noundef %1, ptr noundef %2, ...) #0 comdat {
                   %4 = alloca ptr, align 8
                   %5 = alloca i64, align 8
                   %6 = alloca ptr, align 8
                   %7 = alloca i32, align 4
                   %8 = alloca ptr, align 8
                   store ptr %2, ptr %4, align 8
                   store i64 %1, ptr %5, align 8
                   store ptr %0, ptr %6, align 8
                   call void @llvm.va_start.p0(ptr %8)
                   %9 = load ptr, ptr %8, align 8
                   %10 = load ptr, ptr %4, align 8
                   %11 = load i64, ptr %5, align 8
                   %12 = load ptr, ptr %6, align 8
                   %13 = call i32 @_vsnprintf(ptr noundef %12, i64 noundef %11, ptr noundef %10, ptr noundef %9)
                   store i32 %13, ptr %7, align 4
                   call void @llvm.va_end.p0(ptr %8)
                   %14 = load i32, ptr %7, align 4
                   ret i32 %14
                 }
                 
                 ; Function Attrs: noinline nounwind optnone uwtable
                 define linkonce_odr dso_local i32 @_vsnprintf(ptr noundef %0, i64 noundef %1, ptr noundef %2, ptr noundef %3) #0 comdat {
                   %5 = alloca ptr, align 8
                   %6 = alloca ptr, align 8
                   %7 = alloca i64, align 8
                   %8 = alloca ptr, align 8
                   store ptr %3, ptr %5, align 8
                   store ptr %2, ptr %6, align 8
                   store i64 %1, ptr %7, align 8
                   store ptr %0, ptr %8, align 8
                   %9 = load ptr, ptr %5, align 8
                   %10 = load ptr, ptr %6, align 8
                   %11 = load i64, ptr %7, align 8
                   %12 = load ptr, ptr %8, align 8
                   %13 = call i32 @_vsnprintf_l(ptr noundef %12, i64 noundef %11, ptr noundef %10, ptr noundef null, ptr noundef %9)
                   ret i32 %13
                 }
                 
                 ; Function Attrs: nocallback nofree nosync nounwind willreturn
                 declare void @llvm.va_start.p0(ptr) #1
                 
                 ; Function Attrs: noinline nounwind optnone uwtable
                 define linkonce_odr dso_local i32 @_vsprintf_l(ptr noundef %0, ptr noundef %1, ptr noundef %2, ptr noundef %3) #0 comdat {
                   %5 = alloca ptr, align 8
                   %6 = alloca ptr, align 8
                   %7 = alloca ptr, align 8
                   %8 = alloca ptr, align 8
                   store ptr %3, ptr %5, align 8
                   store ptr %2, ptr %6, align 8
                   store ptr %1, ptr %7, align 8
                   store ptr %0, ptr %8, align 8
                   %9 = load ptr, ptr %5, align 8
                   %10 = load ptr, ptr %6, align 8
                   %11 = load ptr, ptr %7, align 8
                   %12 = load ptr, ptr %8, align 8
                   %13 = call i32 @_vsnprintf_l(ptr noundef %12, i64 noundef -1, ptr noundef %11, ptr noundef %10, ptr noundef %9)
                   ret i32 %13
                 }
                 
                 ; Function Attrs: nocallback nofree nosync nounwind willreturn
                 declare void @llvm.va_end.p0(ptr) #1
                 
                 ; Function Attrs: noinline nounwind optnone uwtable
                 define linkonce_odr dso_local i32 @_vsnprintf_l(ptr noundef %0, i64 noundef %1, ptr noundef %2, ptr noundef %3, ptr noundef %4) #0 comdat {
                   %6 = alloca ptr, align 8
                   %7 = alloca ptr, align 8
                   %8 = alloca ptr, align 8
                   %9 = alloca i64, align 8
                   %10 = alloca ptr, align 8
                   %11 = alloca i32, align 4
                   store ptr %4, ptr %6, align 8
                   store ptr %3, ptr %7, align 8
                   store ptr %2, ptr %8, align 8
                   store i64 %1, ptr %9, align 8
                   store ptr %0, ptr %10, align 8
                   %12 = load ptr, ptr %6, align 8
                   %13 = load ptr, ptr %7, align 8
                   %14 = load ptr, ptr %8, align 8
                   %15 = load i64, ptr %9, align 8
                   %16 = load ptr, ptr %10, align 8
                   %17 = call ptr @__local_stdio_printf_options()
                   %18 = load i64, ptr %17, align 8
                   %19 = or i64 %18, 1
                   %20 = call i32 @__stdio_common_vsprintf(i64 noundef %19, ptr noundef %16, i64 noundef %15, ptr noundef %14, ptr noundef %13, ptr noundef %12)
                   store i32 %20, ptr %11, align 4
                   %21 = load i32, ptr %11, align 4
                   %22 = icmp slt i32 %21, 0
                   br i1 %22, label %23, label %24
                 
                 23:                                               ; preds = %5
                   br label %26
                 
                 24:                                               ; preds = %5
                   %25 = load i32, ptr %11, align 4
                   br label %26
                 
                 26:                                               ; preds = %24, %23
                   %27 = phi i32 [ -1, %23 ], [ %25, %24 ]
                   ret i32 %27
                 }
                 
                 declare dso_local i32 @__stdio_common_vsprintf(i64 noundef, ptr noundef, i64 noundef, ptr noundef, ptr noundef, ptr noundef) #2
                 
                 ; Function Attrs: noinline nounwind optnone uwtable
                 define linkonce_odr dso_local ptr @__local_stdio_printf_options() #0 comdat {
                   ret ptr @__local_stdio_printf_options._OptionsStorage
                 }                 
                 """""";
      
      public static string MyStringTypeName = "my_string_type";
      public static string MyStringType = $"%" + MyStringTypeName+ " = type { i32, ptr }";
      
      public static string Malloc = "declare noalias i8* @malloc(i64)"; 
      
      public static string Int32ToStringFunctionName = "int_to_string";
      public static string Int32ToStringFunction 
        =$$$$""""
             @__to_string_constant = linkonce_odr dso_local unnamed_addr constant [3 x i8] c"%d\00"
             define dso_local void @{{{{Int32ToStringFunctionName}}}}(ptr dead_on_unwind noalias writable sret(%{{{{MyStringTypeName}}}}) %0, i32 %1) #0 {
               %3 = alloca ptr
               %4 = alloca i32
               %5 = alloca i32
               %6 = alloca i32
               %7 = alloca ptr
               store ptr %0, ptr %3
               store i32 %1, ptr %4
               %8 = load i32, ptr %4
               store i32 %8, ptr %5
               store i32 0, ptr %6
               br label %9
             
             9:                                                ; preds = %12, %2
               %10 = load i32, ptr %5
               %11 = icmp sge i32 %10, 10
               br i1 %11, label %12, label %17
             
             12:                                               ; preds = %9
               %13 = load i32, ptr %5
               %14 = sdiv i32 %13, 10
               store i32 %14, ptr %5
               %15 = load i32, ptr %6
               %16 = add nsw i32 %15, 1
               store i32 %16, ptr %6
               br label %9
             
             17:                                               ; preds = %9
               %18 = load i32, ptr %6
               %19 = add nsw i32 %18, 1
               store i32 %19, ptr %6
               %20 = load i32, ptr %6
               %21 = sext i32 %20 to i64
               %22 = call noalias ptr @malloc(i64 %21) #4
               store ptr %22, ptr %7
               %23 = load i32, ptr %4
               %24 = load ptr, ptr %7
               %25 = call i32 (ptr, ptr, ...) @sprintf(ptr %24, ptr @__to_string_constant, i32 %23) #5
               %26 = load i32, ptr %6
               %27 = getelementptr %{{{{MyStringTypeName}}}}, ptr %0, i32 0, i32 0
               store i32 %26, ptr %27
               %28 = load ptr, ptr %7
               %29 = getelementptr %{{{{MyStringTypeName}}}}, ptr %0, i32 0, i32 1
               store ptr %28, ptr %29
               ret void
             }
             """";
      
      public static string MyPrintFunctionName = "my_print";
      public static string MyPrintFunction
        = $$$"""
             @__my_print_constant = linkonce_odr dso_local unnamed_addr constant [5 x i8] c"%.*s\00"
             define void @{{{MyPrintFunctionName}}}(%{{{MyStringTypeName}}}* %0) 
             {
               %char = getelementptr %{{{MyStringTypeName}}}, %{{{MyStringTypeName}}}* %0, i32 0, i32 1
               %loadChar = load ptr, ptr %char
               %len = getelementptr %{{{MyStringTypeName}}}, %{{{MyStringTypeName}}}* %0, i32 0, i32 0
               %len_val = load i32, i32* %len
               %printres = call i32 (i8*, ...) @printf(ptr @__my_print_constant, i32 %len_val, ptr %loadChar)
               ret void
             }
             """;
      public static string MyStringConcatFunctionName = "my_string_concat";

      public static string MyStringConcatFunction
        = $$$"""
              @__my_string_concat_constant = linkonce_odr dso_local unnamed_addr constant [5 x i8] c"%.*s\00"
              
              define dso_local void @{{{MyStringConcatFunctionName}}}(ptr dead_on_unwind noalias writable sret(%{{{MyStringTypeName}}}) %0, ptr noundef %1, ptr noundef %2) {
                ; Get lengths of input strings
                %8 = getelementptr inbounds %{{{MyStringTypeName}}}, ptr %1, i32 0, i32 0
                %9 = load i32, ptr %8
                %11 = getelementptr inbounds %{{{MyStringTypeName}}}, ptr %2, i32 0, i32 0
                %12 = load i32, ptr %11
              
                ; Compute and store total length
                %13 = add nsw i32 %9, %12
                %14 = getelementptr inbounds %{{{MyStringTypeName}}}, ptr %0, i32 0, i32 0
                store i32 %13, ptr %14
              
                ; Allocate memory (with +1 for null terminator)
                %16 = load i32, ptr %14
                %17 = sext i32 %16 to i64
                %18 = add i64 %17, 1
                %19 = call noalias ptr @malloc(i64 noundef %18)
                %20 = getelementptr inbounds %{{{MyStringTypeName}}}, ptr %0, i32 0, i32 1
                store ptr %19, ptr %20
              
                ; Copy first string
                %21 = getelementptr inbounds %{{{MyStringTypeName}}}, ptr %1, i32 0, i32 1
                %22 = load ptr, ptr %21
                %25 = load i32, ptr %8
                %27 = load ptr, ptr %20
                %28 = call i32 (ptr, ptr, ...) @sprintf(ptr noundef %27, ptr noundef @__my_string_concat_constant, i32 noundef %25, ptr noundef %22)
              
                ; Copy second string
                %30 = getelementptr inbounds %{{{MyStringTypeName}}}, ptr %2, i32 0, i32 1
                %31 = load ptr, ptr %30
                %34 = load i32, ptr %11
                %39 = sext i32 %25 to i64
                %41 = getelementptr inbounds i8, ptr %27, i64 %39
                call i32 (ptr, ptr, ...) @sprintf(ptr noundef %41, ptr noundef @__my_string_concat_constant, i32 noundef %34, ptr noundef %31)
              
                ret void
              }
              """;

      public static string RuntimeDeclarations = """
                                                 declare ptr @gc_alloc(i64)
                                                 declare void @gc_push_frame(ptr)
                                                 declare void @gc_pop_frame()
                                                 %GCFrame = type { ptr, ptr, i32 }
                                                 """;
}