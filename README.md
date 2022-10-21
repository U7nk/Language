[![Build Status](http://84.38.184.25:8080/buildStatus/icon?subject=Build_and_tests&job=language-ci%2Fmaster)](http://84.38.184.25:8080/job/language-ci/job/master/)
# Language
Statically typed, object oriented Language with garbage collection.

Made for learning and fun.

This project contains of:
  * Language interpreter written in C#
  * Language to bytecode compiler in C#
  * Language runtime written in C++

## runtime
Stack based virtual machine.

## syntax
Complex Hello World program:

![image](https://user-images.githubusercontent.com/69924108/197231823-2921e525-ce81-477c-a3d5-6b58c5e85acd.png)

Simple Hello World program on top level statements:

![image](https://user-images.githubusercontent.com/69924108/197232400-5300537a-6138-45a6-95cb-64b14b00cdea.png)

## source code to execution pipeline
Green - compilation

Red - runtime

![image](https://user-images.githubusercontent.com/69924108/197239235-e47e8994-dfce-4e36-942d-9e6e7523b7dd.png)
* Bound tree - Syntax tree with types
* Lowered tree - Bound tree without loops and ifs(labels and goto instead)
