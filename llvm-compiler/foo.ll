; ModuleID = 'foo.cpp'
source_filename = "foo.cpp"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-i128:128-f80:128-n8:16:32:64-S128"
target triple = "x86_64-pc-windows-msvc19.41.34120"

%"struct.Foo::Bar" = type { i8, i8, i8 }
%"struct.Foo::Babayka" = type { i8, i8, i8 }

$"?add_two_number@Bar@Foo@@QEAAXXZ" = comdat any

$"?add_three_number@Babayka@Foo@@QEAAX_N@Z" = comdat any

; Function Attrs: mustprogress noinline norecurse optnone uwtable
define dso_local noundef i32 @main() #0 {
  %1 = alloca i32, align 4
  %2 = alloca %"struct.Foo::Bar", align 1
  %3 = alloca %"struct.Foo::Babayka", align 1
  store i32 0, ptr %1, align 4
  call void @llvm.memset.p0.i64(ptr align 1 %2, i8 0, i64 3, i1 false)
  call void @llvm.memset.p0.i64(ptr align 1 %3, i8 0, i64 3, i1 false)
  %4 = getelementptr inbounds %"struct.Foo::Bar", ptr %2, i32 0, i32 0
  store i8 1, ptr %4, align 1
  call void @"?add_two_number@Bar@Foo@@QEAAXXZ"(ptr noundef nonnull align 1 dereferenceable(3) %2)
  call void @"?add_three_number@Babayka@Foo@@QEAAX_N@Z"(ptr noundef nonnull align 1 dereferenceable(3) %3, i1 noundef zeroext false)
  ret i32 0
}

; Function Attrs: nocallback nofree nounwind willreturn memory(argmem: write)
declare void @llvm.memset.p0.i64(ptr nocapture writeonly, i8, i64, i1 immarg) #1

; Function Attrs: mustprogress noinline nounwind optnone uwtable
define linkonce_odr dso_local void @"?add_two_number@Bar@Foo@@QEAAXXZ"(ptr noundef nonnull align 1 dereferenceable(3) %0) #2 comdat align 2 {
  %2 = alloca ptr, align 8
  store ptr %0, ptr %2, align 8
  %3 = load ptr, ptr %2, align 8
  %4 = getelementptr inbounds %"struct.Foo::Bar", ptr %3, i32 0, i32 0
  store i8 1, ptr %4, align 1
  %5 = getelementptr inbounds %"struct.Foo::Bar", ptr %3, i32 0, i32 2
  store i8 0, ptr %5, align 1
  ret void
}

; Function Attrs: mustprogress noinline nounwind optnone uwtable
define linkonce_odr dso_local void @"?add_three_number@Babayka@Foo@@QEAAX_N@Z"(ptr noundef nonnull align 1 dereferenceable(3) %0, i1 noundef zeroext %1) #2 comdat align 2 {
  %3 = alloca i8, align 1
  %4 = alloca ptr, align 8
  %5 = alloca i8, align 1
  %6 = zext i1 %1 to i8
  store i8 %6, ptr %3, align 1
  store ptr %0, ptr %4, align 8
  %7 = load ptr, ptr %4, align 8
  %8 = load i8, ptr %3, align 1
  %9 = trunc i8 %8 to i1
  %10 = getelementptr inbounds %"struct.Foo::Babayka", ptr %7, i32 0, i32 0
  %11 = zext i1 %9 to i8
  store i8 %11, ptr %10, align 1
  %12 = getelementptr inbounds %"struct.Foo::Babayka", ptr %7, i32 0, i32 2
  store i8 0, ptr %12, align 1
  store i8 1, ptr %5, align 1
  %13 = load i8, ptr %3, align 1
  %14 = trunc i8 %13 to i1
  %15 = zext i1 %14 to i32
  %16 = icmp eq i32 %15, 1
  br i1 %16, label %17, label %29

17:                                               ; preds = %2
  %18 = getelementptr inbounds %"struct.Foo::Babayka", ptr %7, i32 0, i32 0
  %19 = load i8, ptr %18, align 1
  %20 = trunc i8 %19 to i1
  %21 = zext i1 %20 to i32
  %22 = load i8, ptr %3, align 1
  %23 = trunc i8 %22 to i1
  %24 = zext i1 %23 to i32
  %25 = or i32 %21, %24
  %26 = icmp ne i32 %25, 0
  %27 = getelementptr inbounds %"struct.Foo::Babayka", ptr %7, i32 0, i32 2
  %28 = zext i1 %26 to i8
  store i8 %28, ptr %27, align 1
  store i8 0, ptr %5, align 1
  br label %30

29:                                               ; preds = %2
  store i8 1, ptr %5, align 1
  br label %30

30:                                               ; preds = %29, %17
  %31 = load i8, ptr %5, align 1
  %32 = trunc i8 %31 to i1
  %33 = getelementptr inbounds %"struct.Foo::Babayka", ptr %7, i32 0, i32 2
  %34 = zext i1 %32 to i8
  store i8 %34, ptr %33, align 1
  ret void
}

attributes #0 = { mustprogress noinline norecurse optnone uwtable "min-legal-vector-width"="0" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cmov,+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #1 = { nocallback nofree nounwind willreturn memory(argmem: write) }
attributes #2 = { mustprogress noinline nounwind optnone uwtable "min-legal-vector-width"="0" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cmov,+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }

!llvm.module.flags = !{!0, !1, !2, !3}
!llvm.ident = !{!4}

!0 = !{i32 1, !"wchar_size", i32 2}
!1 = !{i32 8, !"PIC Level", i32 2}
!2 = !{i32 7, !"uwtable", i32 2}
!3 = !{i32 1, !"MaxTLSAlign", i32 65536}
!4 = !{!"clang version 18.1.8 (https://github.com/llvm/llvm-project.git 3b5b5c1ec4a3095ab096dd780e84d7ab81f3d7ff)"}
