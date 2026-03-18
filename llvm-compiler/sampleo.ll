; ModuleID = 'sample.ll'
source_filename = "sample.ll"

%my_string_type = type { i32, ptr }

$sprintf = comdat any

@__to_string_constant = linkonce_odr dso_local unnamed_addr constant [3 x i8] c"%d\00"
@__my_print_constant = linkonce_odr dso_local unnamed_addr constant [5 x i8] c"%.*s\00"
@__my_string_concat_constant = linkonce_odr dso_local unnamed_addr constant [5 x i8] c"%.*s\00"
@strConst51 = internal constant [10 x i8] c": is even\0A"
@strConst42 = internal constant [9 x i8] c": is odd\0A"
@strConst31 = internal constant [1 x i8] c"\0A"
@strConst19 = internal constant [1 x i8] c"\0A"
@strConst10 = internal constant [3 x i8] c" - "
@strConst3 = internal constant [13 x i8] c"Factorial of "
@strConst7 = internal constant [1 x i8] c"\0A"

; Function Attrs: mustprogress nofree nounwind willreturn allockind("alloc,uninitialized") allocsize(0) memory(inaccessiblemem: readwrite)
declare noalias noundef ptr @malloc(i64 noundef) local_unnamed_addr #0

; Function Attrs: nofree nounwind
declare noundef i32 @printf(ptr nocapture noundef readonly, ...) local_unnamed_addr #1

define linkonce_odr dso_local i32 @sprintf(ptr noundef %0, ptr noundef %1, ...) local_unnamed_addr comdat {
  %3 = alloca ptr, align 8
  call void @llvm.va_start.p0(ptr nonnull %3)
  %4 = load ptr, ptr %3, align 8
  %5 = call i32 @__stdio_common_vsprintf(i64 noundef 1, ptr noundef %0, i64 noundef -1, ptr noundef %1, ptr noundef null, ptr noundef %4)
  %6 = call i32 @llvm.smax.i32(i32 %5, i32 -1)
  call void @llvm.va_end.p0(ptr nonnull %3)
  ret i32 %6
}

; Function Attrs: mustprogress nocallback nofree nosync nounwind willreturn
declare void @llvm.va_start.p0(ptr) #2

; Function Attrs: mustprogress nocallback nofree nosync nounwind willreturn
declare void @llvm.va_end.p0(ptr) #2

declare dso_local i32 @__stdio_common_vsprintf(i64 noundef, ptr noundef, i64 noundef, ptr noundef, ptr noundef, ptr noundef) local_unnamed_addr

define dso_local void @int_to_string(ptr dead_on_unwind noalias nocapture writable writeonly sret(%my_string_type) %0, i32 %1) local_unnamed_addr {
  %3 = icmp sgt i32 %1, 9
  br i1 %3, label %.lr.ph, label %._crit_edge

.lr.ph:                                           ; preds = %2, %.lr.ph
  %.011 = phi i32 [ %4, %.lr.ph ], [ %1, %2 ]
  %.0910 = phi i32 [ %5, %.lr.ph ], [ 0, %2 ]
  %4 = udiv i32 %.011, 10
  %5 = add nuw nsw i32 %.0910, 1
  %6 = icmp ugt i32 %.011, 99
  br i1 %6, label %.lr.ph, label %._crit_edge.loopexit

._crit_edge.loopexit:                             ; preds = %.lr.ph
  %7 = add nuw nsw i32 %.0910, 2
  br label %._crit_edge

._crit_edge:                                      ; preds = %._crit_edge.loopexit, %2
  %.09.lcssa = phi i32 [ 1, %2 ], [ %7, %._crit_edge.loopexit ]
  %8 = zext nneg i32 %.09.lcssa to i64
  %9 = tail call noalias ptr @malloc(i64 %8)
  %10 = tail call i32 (ptr, ptr, ...) @sprintf(ptr nonnull dereferenceable(1) %9, ptr nonnull dereferenceable(1) @__to_string_constant, i32 %1)
  store i32 %.09.lcssa, ptr %0, align 8
  %11 = getelementptr inbounds i8, ptr %0, i64 8
  store ptr %9, ptr %11, align 8
  ret void
}

; Function Attrs: nofree nounwind
define void @my_print(ptr nocapture readonly %0) local_unnamed_addr #1 {
  %char = getelementptr i8, ptr %0, i64 8
  %loadChar = load ptr, ptr %char, align 8
  %len_val = load i32, ptr %0, align 4
  %printres = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 %len_val, ptr %loadChar)
  ret void
}

define dso_local void @my_string_concat(ptr dead_on_unwind noalias nocapture writable writeonly sret(%my_string_type) %0, ptr nocapture noundef readonly %1, ptr nocapture noundef readonly %2) local_unnamed_addr {
  %4 = load i32, ptr %1, align 4
  %5 = load i32, ptr %2, align 4
  %6 = add nsw i32 %5, %4
  store i32 %6, ptr %0, align 8
  %7 = sext i32 %6 to i64
  %8 = tail call noalias ptr @malloc(i64 noundef %7)
  %9 = getelementptr inbounds i8, ptr %0, i64 8
  store ptr %8, ptr %9, align 8
  %10 = getelementptr inbounds i8, ptr %1, i64 8
  %11 = load ptr, ptr %10, align 8
  %12 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %8, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef %4, ptr noundef %11)
  %13 = getelementptr inbounds i8, ptr %2, i64 8
  %14 = load ptr, ptr %13, align 8
  %15 = load i32, ptr %2, align 4
  %16 = load i32, ptr %1, align 4
  %17 = sext i32 %16 to i64
  %18 = getelementptr inbounds i8, ptr %8, i64 %17
  %19 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %18, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef %15, ptr noundef %14)
  ret void
}

define noundef i32 @main() local_unnamed_addr {
entry:
  %main_result = tail call i32 @HelloWorldSample.Program.main()
  ret i32 0
}

define noundef i32 @HelloWorldSample.Program.main() local_unnamed_addr {
block_13:
  br label %loop_start_4

int_to_string.exit5:                              ; preds = %end_7
  %0 = tail call noalias dereferenceable_or_null(2) ptr @malloc(i64 2)
  %1 = tail call i32 (ptr, ptr, ...) @sprintf(ptr nonnull dereferenceable(1) %0, ptr nonnull dereferenceable(1) @__to_string_constant, i32 10), !noalias !0
  %2 = tail call noalias dereferenceable_or_null(15) ptr @malloc(i64 noundef 15)
  %3 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %2, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 13, ptr noundef nonnull @strConst3), !noalias !3
  %4 = getelementptr inbounds i8, ptr %2, i64 13
  %5 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %4, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 2, ptr noundef %0), !noalias !3
  %6 = tail call noalias dereferenceable_or_null(18) ptr @malloc(i64 noundef 18)
  %7 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %6, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 15, ptr noundef %2), !noalias !6
  %8 = getelementptr inbounds i8, ptr %6, i64 15
  %9 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %8, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 3, ptr noundef nonnull @strConst10), !noalias !6
  %10 = tail call noalias dereferenceable_or_null(7) ptr @malloc(i64 7)
  %11 = tail call i32 (ptr, ptr, ...) @sprintf(ptr nonnull dereferenceable(1) %10, ptr nonnull dereferenceable(1) @__to_string_constant, i32 3628800), !noalias !9
  %12 = tail call noalias dereferenceable_or_null(25) ptr @malloc(i64 noundef 25)
  %13 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %12, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 18, ptr noundef %6), !noalias !12
  %14 = getelementptr inbounds i8, ptr %12, i64 18
  %15 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %14, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 7, ptr noundef %10), !noalias !12
  %16 = tail call noalias dereferenceable_or_null(26) ptr @malloc(i64 noundef 26)
  %17 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %16, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 25, ptr noundef %12), !noalias !15
  %18 = getelementptr inbounds i8, ptr %16, i64 25
  %19 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %18, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 1, ptr noundef nonnull @strConst19), !noalias !15
  %printres.i = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 26, ptr %16)
  %20 = tail call noalias dereferenceable_or_null(2) ptr @malloc(i64 2)
  %21 = tail call i32 (ptr, ptr, ...) @sprintf(ptr nonnull dereferenceable(1) %20, ptr nonnull dereferenceable(1) @__to_string_constant, i32 53), !noalias !18
  %printres.i.i = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 2, ptr %20)
  %printres.i4.i = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 1, ptr nonnull @strConst7)
  br label %loop_start_8

break_2:                                          ; preds = %int_to_string.exit11
  ret i32 0

loop_start_8:                                     ; preds = %int_to_string.exit5, %int_to_string.exit11
  %phiRes2741 = phi i32 [ 2, %int_to_string.exit5 ], [ %binaryRes37, %int_to_string.exit11 ]
  %22 = icmp sgt i32 %phiRes2741, 9
  br i1 %22, label %.lr.ph.i7, label %int_to_string.exit11

.lr.ph.i7:                                        ; preds = %loop_start_8, %.lr.ph.i7
  %.011.i8 = phi i32 [ %23, %.lr.ph.i7 ], [ %phiRes2741, %loop_start_8 ]
  %.0910.i9 = phi i32 [ %24, %.lr.ph.i7 ], [ 0, %loop_start_8 ]
  %23 = udiv i32 %.011.i8, 10
  %24 = add nuw nsw i32 %.0910.i9, 1
  %25 = icmp ugt i32 %.011.i8, 99
  br i1 %25, label %.lr.ph.i7, label %._crit_edge.loopexit.i10

._crit_edge.loopexit.i10:                         ; preds = %.lr.ph.i7
  %26 = add nuw nsw i32 %.0910.i9, 2
  br label %int_to_string.exit11

int_to_string.exit11:                             ; preds = %loop_start_8, %._crit_edge.loopexit.i10
  %.09.lcssa.i6 = phi i32 [ 1, %loop_start_8 ], [ %26, %._crit_edge.loopexit.i10 ]
  %27 = zext nneg i32 %.09.lcssa.i6 to i64
  %28 = tail call noalias ptr @malloc(i64 %27)
  %29 = tail call i32 (ptr, ptr, ...) @sprintf(ptr nonnull dereferenceable(1) %28, ptr nonnull dereferenceable(1) @__to_string_constant, i32 %phiRes2741), !noalias !21
  %printres.i15 = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 %.09.lcssa.i6, ptr %28)
  %printres.i19 = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 1, ptr nonnull @strConst31)
  %binaryRes37 = add i32 %phiRes2741, 1
  %binaryRes28.not = icmp eq i32 %phiRes2741, 0
  br i1 %binaryRes28.not, label %break_2, label %loop_start_8

loop_start_4:                                     ; preds = %block_13, %end_7
  %phiRes140 = phi i32 [ 0, %block_13 ], [ %binaryRes49, %end_7 ]
  %30 = and i32 %phiRes140, 1
  %binaryRes40 = icmp eq i32 %30, 0
  %31 = icmp ugt i32 %phiRes140, 9
  br i1 %binaryRes40, label %then_5, label %else_6

else_6:                                           ; preds = %loop_start_4
  br i1 %31, label %.lr.ph.i21, label %int_to_string.exit25

.lr.ph.i21:                                       ; preds = %else_6, %.lr.ph.i21
  %.011.i22 = phi i32 [ %32, %.lr.ph.i21 ], [ %phiRes140, %else_6 ]
  %.0910.i23 = phi i32 [ %33, %.lr.ph.i21 ], [ 0, %else_6 ]
  %32 = udiv i32 %.011.i22, 10
  %33 = add nuw nsw i32 %.0910.i23, 1
  %34 = icmp ugt i32 %.011.i22, 99
  br i1 %34, label %.lr.ph.i21, label %._crit_edge.loopexit.i24

._crit_edge.loopexit.i24:                         ; preds = %.lr.ph.i21
  %35 = add nuw nsw i32 %.0910.i23, 2
  br label %int_to_string.exit25

int_to_string.exit25:                             ; preds = %else_6, %._crit_edge.loopexit.i24
  %.09.lcssa.i20 = phi i32 [ 1, %else_6 ], [ %35, %._crit_edge.loopexit.i24 ]
  %36 = zext nneg i32 %.09.lcssa.i20 to i64
  %37 = tail call noalias ptr @malloc(i64 %36)
  %38 = tail call i32 (ptr, ptr, ...) @sprintf(ptr nonnull dereferenceable(1) %37, ptr nonnull dereferenceable(1) @__to_string_constant, i32 %phiRes140), !noalias !24
  %39 = add nsw i32 %.09.lcssa.i20, 9
  %40 = sext i32 %39 to i64
  %41 = tail call noalias ptr @malloc(i64 noundef %40)
  %42 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %41, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef %.09.lcssa.i20, ptr noundef %37), !noalias !27
  %43 = sext i32 %.09.lcssa.i20 to i64
  %44 = getelementptr inbounds i8, ptr %41, i64 %43
  %45 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %44, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 9, ptr noundef nonnull @strConst42), !noalias !27
  %printres.i29 = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 %39, ptr %41)
  br label %end_7

end_7:                                            ; preds = %int_to_string.exit35, %int_to_string.exit25
  %binaryRes49 = add nuw nsw i32 %phiRes140, 1
  %binaryRes2 = icmp ult i32 %phiRes140, 100
  br i1 %binaryRes2, label %loop_start_4, label %int_to_string.exit5

then_5:                                           ; preds = %loop_start_4
  br i1 %31, label %.lr.ph.i31, label %int_to_string.exit35

.lr.ph.i31:                                       ; preds = %then_5, %.lr.ph.i31
  %.011.i32 = phi i32 [ %46, %.lr.ph.i31 ], [ %phiRes140, %then_5 ]
  %.0910.i33 = phi i32 [ %47, %.lr.ph.i31 ], [ 0, %then_5 ]
  %46 = udiv i32 %.011.i32, 10
  %47 = add nuw nsw i32 %.0910.i33, 1
  %48 = icmp ugt i32 %.011.i32, 99
  br i1 %48, label %.lr.ph.i31, label %._crit_edge.loopexit.i34

._crit_edge.loopexit.i34:                         ; preds = %.lr.ph.i31
  %49 = add nuw nsw i32 %.0910.i33, 2
  br label %int_to_string.exit35

int_to_string.exit35:                             ; preds = %then_5, %._crit_edge.loopexit.i34
  %.09.lcssa.i30 = phi i32 [ 1, %then_5 ], [ %49, %._crit_edge.loopexit.i34 ]
  %50 = zext nneg i32 %.09.lcssa.i30 to i64
  %51 = tail call noalias ptr @malloc(i64 %50)
  %52 = tail call i32 (ptr, ptr, ...) @sprintf(ptr nonnull dereferenceable(1) %51, ptr nonnull dereferenceable(1) @__to_string_constant, i32 %phiRes140), !noalias !30
  %53 = add nsw i32 %.09.lcssa.i30, 10
  %54 = sext i32 %53 to i64
  %55 = tail call noalias ptr @malloc(i64 noundef %54)
  %56 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %55, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef %.09.lcssa.i30, ptr noundef %51), !noalias !33
  %57 = sext i32 %.09.lcssa.i30 to i64
  %58 = getelementptr inbounds i8, ptr %55, i64 %57
  %59 = tail call i32 (ptr, ptr, ...) @sprintf(ptr noundef nonnull dereferenceable(1) %58, ptr noundef nonnull dereferenceable(1) @__my_string_concat_constant, i32 noundef 10, ptr noundef nonnull @strConst51), !noalias !33
  %printres.i39 = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 %53, ptr %55)
  br label %end_7
}

define void @HelloWorldSample.Program.foo(i32 %x1) local_unnamed_addr {
block_14:
  %0 = icmp sgt i32 %x1, 9
  br i1 %0, label %.lr.ph.i, label %int_to_string.exit

.lr.ph.i:                                         ; preds = %block_14, %.lr.ph.i
  %.011.i = phi i32 [ %1, %.lr.ph.i ], [ %x1, %block_14 ]
  %.0910.i = phi i32 [ %2, %.lr.ph.i ], [ 0, %block_14 ]
  %1 = udiv i32 %.011.i, 10
  %2 = add nuw nsw i32 %.0910.i, 1
  %3 = icmp ugt i32 %.011.i, 99
  br i1 %3, label %.lr.ph.i, label %._crit_edge.loopexit.i

._crit_edge.loopexit.i:                           ; preds = %.lr.ph.i
  %4 = add nuw nsw i32 %.0910.i, 2
  br label %int_to_string.exit

int_to_string.exit:                               ; preds = %block_14, %._crit_edge.loopexit.i
  %.09.lcssa.i = phi i32 [ 1, %block_14 ], [ %4, %._crit_edge.loopexit.i ]
  %5 = zext nneg i32 %.09.lcssa.i to i64
  %6 = tail call noalias ptr @malloc(i64 %5)
  %7 = tail call i32 (ptr, ptr, ...) @sprintf(ptr nonnull dereferenceable(1) %6, ptr nonnull dereferenceable(1) @__to_string_constant, i32 %x1), !noalias !36
  %printres.i = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 %.09.lcssa.i, ptr %6)
  %printres.i4 = tail call i32 (ptr, ...) @printf(ptr nonnull dereferenceable(1) @__my_print_constant, i32 1, ptr nonnull @strConst7)
  ret void
}

; Function Attrs: nofree norecurse nosync nounwind memory(none)
define i32 @HelloWorldSample.Program.factorial(i32 %n1) local_unnamed_addr #3 {
block_15:
  %binaryRes21 = icmp eq i32 %n1, 0
  br i1 %binaryRes21, label %common.ret, label %end_11

common.ret:                                       ; preds = %end_11, %block_15
  %accumulator.tr.lcssa = phi i32 [ 1, %block_15 ], [ %binaryRes3.i, %end_11 ]
  ret i32 %accumulator.tr.lcssa

end_11:                                           ; preds = %block_15, %end_11
  %n1.tr3 = phi i32 [ %binaryRes3, %end_11 ], [ %n1, %block_15 ]
  %accumulator.tr2 = phi i32 [ %binaryRes3.i, %end_11 ], [ 1, %block_15 ]
  %binaryRes3 = add i32 %n1.tr3, -1
  %binaryRes3.i = mul i32 %n1.tr3, %accumulator.tr2
  %binaryRes2 = icmp eq i32 %binaryRes3, 0
  br i1 %binaryRes2, label %common.ret, label %end_11
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @HelloWorldSample.Program.add(i32 %x1, i32 %y2) local_unnamed_addr #4 {
block_16:
  %binaryRes3 = mul i32 %y2, %x1
  ret i32 %binaryRes3
}

; Function Attrs: nocallback nofree nosync nounwind speculatable willreturn memory(none)
declare i32 @llvm.smax.i32(i32, i32) #5

attributes #0 = { mustprogress nofree nounwind willreturn allockind("alloc,uninitialized") allocsize(0) memory(inaccessiblemem: readwrite) "alloc-family"="malloc" }
attributes #1 = { nofree nounwind }
attributes #2 = { mustprogress nocallback nofree nosync nounwind willreturn }
attributes #3 = { nofree norecurse nosync nounwind memory(none) }
attributes #4 = { mustprogress nofree norecurse nosync nounwind willreturn memory(none) }
attributes #5 = { nocallback nofree nosync nounwind speculatable willreturn memory(none) }

!0 = !{!1}
!1 = distinct !{!1, !2, !"int_to_string: argument 0"}
!2 = distinct !{!2, !"int_to_string"}
!3 = !{!4}
!4 = distinct !{!4, !5, !"my_string_concat: argument 0"}
!5 = distinct !{!5, !"my_string_concat"}
!6 = !{!7}
!7 = distinct !{!7, !8, !"my_string_concat: argument 0"}
!8 = distinct !{!8, !"my_string_concat"}
!9 = !{!10}
!10 = distinct !{!10, !11, !"int_to_string: argument 0"}
!11 = distinct !{!11, !"int_to_string"}
!12 = !{!13}
!13 = distinct !{!13, !14, !"my_string_concat: argument 0"}
!14 = distinct !{!14, !"my_string_concat"}
!15 = !{!16}
!16 = distinct !{!16, !17, !"my_string_concat: argument 0"}
!17 = distinct !{!17, !"my_string_concat"}
!18 = !{!19}
!19 = distinct !{!19, !20, !"int_to_string: argument 0"}
!20 = distinct !{!20, !"int_to_string"}
!21 = !{!22}
!22 = distinct !{!22, !23, !"int_to_string: argument 0"}
!23 = distinct !{!23, !"int_to_string"}
!24 = !{!25}
!25 = distinct !{!25, !26, !"int_to_string: argument 0"}
!26 = distinct !{!26, !"int_to_string"}
!27 = !{!28}
!28 = distinct !{!28, !29, !"my_string_concat: argument 0"}
!29 = distinct !{!29, !"my_string_concat"}
!30 = !{!31}
!31 = distinct !{!31, !32, !"int_to_string: argument 0"}
!32 = distinct !{!32, !"int_to_string"}
!33 = !{!34}
!34 = distinct !{!34, !35, !"my_string_concat: argument 0"}
!35 = distinct !{!35, !"my_string_concat"}
!36 = !{!37}
!37 = distinct !{!37, !38, !"int_to_string: argument 0"}
!38 = distinct !{!38, !"int_to_string"}
