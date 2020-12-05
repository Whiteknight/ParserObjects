# C#-like Class Definition Parser

This parser parses a very small subset of a C#-like language, where classes, structs and interfaces can be declared. Members (constructors, methods, fields, properties) are not parsed. This parser demonstrates the use of the Pratt parser for handling non-expression grammars and using recursion and syntax-directed translation to build an abstract syntax tree.