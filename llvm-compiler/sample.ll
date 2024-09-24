@.str = private unnamed_addr constant [3 x i8] c"%d\00", align 1
declare i32 @printf(i8* noundef, ...) #1

; declare extern_weak void @__printint__(i32)

define i32 @main() {
entry:
  %x = alloca i32, align 4
  %n = alloca i32, align 4
  store i32 1, i32* %n, align 4
  %n1 = load i32, i32* %n, align 4
  %add = add i32 %n1, 1
  store i32 %add, i32* %x, align 4
  %x2 = load i32, i32* %x, align 4
  ; call void @__printint__(i32 %x2)
  call i32 (i8*, ...) @printf(i8* noundef getelementptr inbounds ([3 x i8], [3 x i8]* @.str, i64 0, i64 0), i32 noundef %x2)
  ret i32 20
}