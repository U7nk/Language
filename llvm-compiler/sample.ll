declare noalias i8* @malloc(i64)
declare ptr @gc_alloc(i64)
declare void @gc_push_frame(ptr)
declare void @gc_pop_frame()
%GCFrame = type { ptr, ptr, i32 }
declare i32 @printf(ptr, ...)

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
%my_string_type = type { i32, ptr }
@__to_string_constant = linkonce_odr dso_local unnamed_addr constant [3 x i8] c"%d\00"
define dso_local void @int_to_string(ptr dead_on_unwind noalias writable sret(%my_string_type) %0, i32 %1) #0 {
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
  %27 = getelementptr %my_string_type, ptr %0, i32 0, i32 0
  store i32 %26, ptr %27
  %28 = load ptr, ptr %7
  %29 = getelementptr %my_string_type, ptr %0, i32 0, i32 1
  store ptr %28, ptr %29
  ret void
}
@__my_print_constant = linkonce_odr dso_local unnamed_addr constant [5 x i8] c"%.*s\00"
define void @my_print(%my_string_type* %0) 
{
  %char = getelementptr %my_string_type, %my_string_type* %0, i32 0, i32 1
  %loadChar = load ptr, ptr %char
  %len = getelementptr %my_string_type, %my_string_type* %0, i32 0, i32 0
  %len_val = load i32, i32* %len
  %printres = call i32 (i8*, ...) @printf(ptr @__my_print_constant, i32 %len_val, ptr %loadChar)
  ret void
}
@__my_string_concat_constant = linkonce_odr dso_local unnamed_addr constant [5 x i8] c"%.*s\00"

define dso_local void @my_string_concat(ptr dead_on_unwind noalias writable sret(%my_string_type) %0, ptr noundef %1, ptr noundef %2) {
  ; Get lengths of input strings
  %8 = getelementptr inbounds %my_string_type, ptr %1, i32 0, i32 0
  %9 = load i32, ptr %8
  %11 = getelementptr inbounds %my_string_type, ptr %2, i32 0, i32 0
  %12 = load i32, ptr %11

  ; Compute and store total length
  %13 = add nsw i32 %9, %12
  %14 = getelementptr inbounds %my_string_type, ptr %0, i32 0, i32 0
  store i32 %13, ptr %14

  ; Allocate memory (with +1 for null terminator)
  %16 = load i32, ptr %14
  %17 = sext i32 %16 to i64
  %18 = add i64 %17, 1
  %19 = call noalias ptr @malloc(i64 noundef %18)
  %20 = getelementptr inbounds %my_string_type, ptr %0, i32 0, i32 1
  store ptr %19, ptr %20

  ; Copy first string
  %21 = getelementptr inbounds %my_string_type, ptr %1, i32 0, i32 1
  %22 = load ptr, ptr %21
  %25 = load i32, ptr %8
  %27 = load ptr, ptr %20
  %28 = call i32 (ptr, ptr, ...) @sprintf(ptr noundef %27, ptr noundef @__my_string_concat_constant, i32 noundef %25, ptr noundef %22)

  ; Copy second string
  %30 = getelementptr inbounds %my_string_type, ptr %2, i32 0, i32 1
  %31 = load ptr, ptr %30
  %34 = load i32, ptr %11
  %39 = sext i32 %25 to i64
  %41 = getelementptr inbounds i8, ptr %27, i64 %39
  call i32 (ptr, ptr, ...) @sprintf(ptr noundef %41, ptr noundef @__my_string_concat_constant, i32 noundef %34, ptr noundef %31)

  ret void
}
%"HelloWorldSample.Program" = type { i1 }
%"HelloWorldSample.Barabas" = type { i1, i32, ptr }
define i32 @main() {
entry:
%main_result = call i32 @"HelloWorldSample.Program.main"()
ret i32 %main_result
}
@strConst3 = internal constant [13 x i8] c"Factorial of "
@strConst10 = internal constant [3 x i8] c" - "
@strConst19 = internal constant [1 x i8] c"
"
@strConst42 = internal constant [9 x i8] c": is odd
"
@strConst51 = internal constant [10 x i8] c": is even
"
define i32 @"HelloWorldSample.Program.main" (  )
{
block_13:
    %string_concat56 = alloca %my_string_type
    %tmp52 = alloca %my_string_type
    %int_to_string_res50 = alloca %my_string_type
    %i_2 = alloca i32
    %string_concat47 = alloca %my_string_type
    %tmp43 = alloca %my_string_type
    %int_to_string_res41 = alloca %my_string_type
    %r_2 = alloca i32
    %int_to_string_res33 = alloca %my_string_type
    %phi_1 = alloca i32
    %r = alloca i32
    %string_concat24 = alloca %my_string_type
    %tmp20 = alloca %my_string_type
    %string_concat18 = alloca %my_string_type
    %int_to_string_res16 = alloca %my_string_type
    %string_concat15 = alloca %my_string_type
    %tmp11 = alloca %my_string_type
    %string_concat9 = alloca %my_string_type
    %int_to_string_res8 = alloca %my_string_type
    %tmp4 = alloca %my_string_type
    %f = alloca i32
    %phi_0 = alloca i32
    %i = alloca i32
    store i32 0, i32* %i
    br label %loop_start_1
loop_start_1:
%phiRes1 = phi i32 [%binaryRes49, %end_7], [0, %block_13]
    store i32 %phiRes1, i32* %phi_0
    %binaryRes2 = icmp slt i32 %phiRes1, 101
    br i1 %binaryRes2, label %loop_body_4, label %loop_break_0
loop_break_0:
    store i32 10, i32* %f
    %charPtr5 = getelementptr %my_string_type, ptr %tmp4, i32 0, i32 1
    %constPtr6 = getelementptr [14 x i8], ptr @strConst3, i32 0, i64 0
    store ptr %constPtr6, ptr %charPtr5
    %leno7 = getelementptr %my_string_type, ptr %tmp4, i32 0, i32 0
    store i32 13, ptr %leno7
    call void @int_to_string(ptr %int_to_string_res8, i32 10)
    call void @my_string_concat(ptr %string_concat9, ptr %tmp4, ptr %int_to_string_res8)
    %charPtr12 = getelementptr %my_string_type, ptr %tmp11, i32 0, i32 1
    %constPtr13 = getelementptr [4 x i8], ptr @strConst10, i32 0, i64 0
    store ptr %constPtr13, ptr %charPtr12
    %leno14 = getelementptr %my_string_type, ptr %tmp11, i32 0, i32 0
    store i32 3, ptr %leno14
    call void @my_string_concat(ptr %string_concat15, ptr %string_concat9, ptr %tmp11)
    %callRes17 = call i32 @"HelloWorldSample.Program.factorial"(i32 10)
    call void @int_to_string(ptr %int_to_string_res16, i32 %callRes17)
    call void @my_string_concat(ptr %string_concat18, ptr %string_concat15, ptr %int_to_string_res16)
    %charPtr21 = getelementptr %my_string_type, ptr %tmp20, i32 0, i32 1
    %constPtr22 = getelementptr [2 x i8], ptr @strConst19, i32 0, i64 0
    store ptr %constPtr22, ptr %charPtr21
    %leno23 = getelementptr %my_string_type, ptr %tmp20, i32 0, i32 0
    store i32 1, ptr %leno23
    call void @my_string_concat(ptr %string_concat24, ptr %string_concat18, ptr %tmp20)
    call void @my_print(ptr %string_concat24)

    call void @"HelloWorldSample.Program.foo"(i32 53)

    store i32 2, i32* %r
    br label %loop_start_3
loop_start_3:
%phiRes27 = phi i32 [%binaryRes37, %loop_body_8], [2, %loop_break_0]
    store i32 %phiRes27, i32* %phi_1
    %binaryRes28 = icmp ne i32 %phiRes27, 1
    br i1 %binaryRes28, label %loop_body_8, label %loop_break_2
loop_break_2:
    ret i32 0
loop_body_8:
    %size_ptr_Barabas_29 = getelementptr %"HelloWorldSample.Barabas", ptr null, i32 1
    %int_size_ptr_Barabas_30 = ptrtoint ptr %size_ptr_Barabas_29 to i64
    %malloc_Barabas_31 = call ptr @malloc(i64 %int_size_ptr_Barabas_30) 
    %field_ptr32 = getelementptr %"HelloWorldSample.Barabas", ptr %malloc_Barabas_31, i32 0, i32 1
    store i32 1, ptr %field_ptr32

    %field_access_Papados_34 = getelementptr %"HelloWorldSample.Barabas", ptr %malloc_Barabas_31, i32 0, i32 1
    %load_of_Papados_35 = load i32, ptr %field_access_Papados_34
    call void @int_to_string(ptr %int_to_string_res33, i32 %load_of_Papados_35)
    call void @my_print(ptr %int_to_string_res33)

    %binaryRes37 = add i32 %phiRes27, 1
    store i32 %binaryRes37, i32* %r_2
    br label %loop_start_3
loop_body_4:
    %binaryRes38 = sdiv i32 %phiRes1, 2
    %binaryRes39 = mul i32 %binaryRes38, 2
    %binaryRes40 = icmp eq i32 %binaryRes39, %phiRes1
    br i1 %binaryRes40, label %then_5, label %else_6
else_6:
    call void @int_to_string(ptr %int_to_string_res41, i32 %phiRes1)
    %charPtr44 = getelementptr %my_string_type, ptr %tmp43, i32 0, i32 1
    %constPtr45 = getelementptr [10 x i8], ptr @strConst42, i32 0, i64 0
    store ptr %constPtr45, ptr %charPtr44
    %leno46 = getelementptr %my_string_type, ptr %tmp43, i32 0, i32 0
    store i32 9, ptr %leno46
    call void @my_string_concat(ptr %string_concat47, ptr %int_to_string_res41, ptr %tmp43)
    call void @my_print(ptr %string_concat47)

    br label %end_7
end_7:
    %binaryRes49 = add i32 %phiRes1, 1
    store i32 %binaryRes49, i32* %i_2
    br label %loop_start_1
then_5:
    call void @int_to_string(ptr %int_to_string_res50, i32 %phiRes1)
    %charPtr53 = getelementptr %my_string_type, ptr %tmp52, i32 0, i32 1
    %constPtr54 = getelementptr [11 x i8], ptr @strConst51, i32 0, i64 0
    store ptr %constPtr54, ptr %charPtr53
    %leno55 = getelementptr %my_string_type, ptr %tmp52, i32 0, i32 0
    store i32 10, ptr %leno55
    call void @my_string_concat(ptr %string_concat56, ptr %int_to_string_res50, ptr %tmp52)
    call void @my_print(ptr %string_concat56)

    br label %end_7
}
@strConst7 = internal constant [1 x i8] c"
"
define void @"HelloWorldSample.Program.foo" ( i32 %x1 )
{
block_14:
    %b_2 = alloca i32
    %tmp8 = alloca %my_string_type
    %int_to_string_res5 = alloca %my_string_type
    %x_0 = alloca i32
    %phi_0 = alloca i32
    %c = alloca i32
    %b = alloca i32
    store i32 %x1, i32* %b
    store i32 %x1, i32* %c
    %binaryRes2 = sdiv i32 %x1, 2
    %binaryRes3 = icmp eq i32 %binaryRes2, 1
    br i1 %binaryRes3, label %then_10, label %end_9
end_9:
%phiRes4 = phi i32 [%x1, %then_10], [%x1, %block_14]
    store i32 %phiRes4, i32* %phi_0
    store i32 %phiRes4, i32* %x_0
    call void @int_to_string(ptr %int_to_string_res5, i32 %phiRes4)
    call void @my_print(ptr %int_to_string_res5)

    %charPtr9 = getelementptr %my_string_type, ptr %tmp8, i32 0, i32 1
    %constPtr10 = getelementptr [2 x i8], ptr @strConst7, i32 0, i64 0
    store ptr %constPtr10, ptr %charPtr9
    %leno11 = getelementptr %my_string_type, ptr %tmp8, i32 0, i32 0
    store i32 1, ptr %leno11
    call void @my_print(ptr %tmp8)

    ret void
then_10:
    store i32 %x1, i32* %b_2
    br label %end_9
}
define i32 @"HelloWorldSample.Program.factorial" ( i32 %n1 )
{
block_15:
    %binaryRes2 = icmp eq i32 %n1, 0
    br i1 %binaryRes2, label %then_12, label %end_11
end_11:
    %binaryRes3 = sub i32 %n1, 1
    %callRes4 = call i32 @"HelloWorldSample.Program.factorial"(i32 %binaryRes3)
    %callRes5 = call i32 @"HelloWorldSample.Program.add"(i32 %n1, i32 %callRes4)
    ret i32 %callRes5
then_12:
    ret i32 1
}
define i32 @"HelloWorldSample.Program.add" ( i32 %x1, i32 %y2 )
{
block_16:
    %binaryRes3 = mul i32 %x1, %y2
    ret i32 %binaryRes3
}
