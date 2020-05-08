# Programming Parsers

ParserObjects contains parsers for some common constructs from modern programming languages. Notice that several languages "borrow" or "inherit" similar rules from related languages. In these cases, parsers will not typically be reproduced for each language. A C++-style identifier is identical to a C-style identifier, for example, so the methods will not be duplicated. These parsers are provided for ease and convenience only, and do not guarantee completeness or correctness across all possible forms or versions.

## C Parsers

```csharp
using static ParserObjects.Parsers.Specialty.CStyleParserMethods;
```

### Comments

A C-style comment starts with `/*` and ends with `*/` and can go across multiple lines. This is also known as a "multi-line comment in C, C++, C# and other similar languages.

```csharp
var parser = Comment();
```

### Numbers

A C-style hexadecimal string starts with the prefix `0x` and is followed by one more more hexadecimal digits. The `HexadecimalString` parser returns the matched string, while the `HexadecimalInteger` parser parses the string and returns an `int`.

```csharp
var parser = HexadecimalString();
var parser = HexadecimalInteger();
```

A C-style integer may be the literal number `0`, or it may have an optional `-` for negative followed by a non-zero digit, followed by a string of zero or more digits. The `IntegerString` parser returns the literal string, while the `Integer` parser returns the parsed `int`.

```csharp
var parser = IntegerString();
var parser = Integer();
```

A C-style floating point number has a whole and fractional part separated by a `.`. The `DoubleString` parser returns the literal string, while the `Double` parser returns the parsed `double`. (These parsers do not currently recognize the `f` suffix to denote a `Float` literal).

```csharp
var parser = DoubleString();
var parser = Double();
```

### Identifiers

A C-style identifier may start with an underscore (`_`) or a letter, and may be followed by zero or more underscores, letters or digits.

```csharp
var parser = Identifier();
```

## C++ Parsers

```csharp
using static ParserObjects.Parsers.Specialty.CPlusPlusStyleParsersMethods;
```

### Comments

A C++-style comment, also known as a "single line comment" starts with the prefix "`//`" and continues to the end of the current line.

```csharp
var parser = Comment();
```

## JavaScript Parsers

### Numbers

A JavaScript number has a complicated set of rules and may be an integer, a floating point value or use scientific notation. The `NumberString` parser returns the literal parsed string while the `Number` parser returns the parsed `double` value.

```csharp
var parser = NumberString();
var parser = Number();
```

## SQL Parsers

```csharp
using static ParserObjects.Parsers.Specialty.SqlStyleParserMethods;
```

### Comments

An SQL comment starts with the prefix "`--`" and continues to the end of the line.

```csharp
var parser = Comment();
```