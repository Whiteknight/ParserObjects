# Programming Parsers

ParserObjects contains parsers for some common constructs from modern programming languages. Notice that several languages "borrow" or "inherit" similar rules from related languages. In these cases, parsers will not typically be reproduced for each language. A C++-style identifier is identical to a C-style identifier, for example, so the methods will not be duplicated. These parsers are provided for ease and convenience only, and do not guarantee completeness or correctness across all possible forms or versions.

## C Parsers

The C-Style parsers are used to parse some common constructs from C and programming languages derived from it.

```csharp
using static ParserObjects.Parsers.C;
```

### Comments

A C-style comment starts with `/*` and ends with `*/` and can go across multiple lines. This is also known as a "multi-line comment" in C, C++, C# and other similar languages.

```csharp
var parser = Comment();
```

### Hexadecimal Numbers

A C-style hexadecimal string has an optional negative sign `-`, starts with the prefix `0x` and is followed by 1 to 8 hexadecimal digits. The `HexadecimalString` parser returns the matched string, while the `HexadecimalInteger` parser parses the string and returns an `int`.

```csharp
var parser = HexadecimalString();
var parser = HexadecimalInteger();
```

Values can be in the range `-0x80000000` to `0x7FFFFFFF` inclusive. Without the negative sign, the range is `0x80000000` to `0x7FFFFFFF`. The hex values are parsed as an unsigned integer, then cast to an `int`, then the negative sign is applied, if any.

### Integer Literals

A C-style integer literal may take several different forms: a hexadecimal literal, an octal literal or a decimal literal. These values may be in the range `[int.MinValue, int.MaxValue]`. If there are additional digits beyond these limits, the parser will return successfully and the remaining digits will be left in the input sequence to be consumed by the next subsequent parse operation. 

```csharp
// Between int.MinValue and int.MaxValue, inclusive
var parser = IntegerString();
var parser = Integer();

// Between long.MinValue and long.MaxValue, inclusive
var parser = LongIntegerString();
var parser = LongInteger();
```

The `Integer` and `LongInteger` parsers recognize the following formats:

* Hexadecimal literals, like the `HexadecimalInteger()` parser described above. Values must start with `0x`, contain up to 8 or 16 digits, and may be in the range `-0x80000000` to `0x7FFFFFFF` or `-0x8000000000000000` to `0x7FFFFFFFFFFFFFFF`.
* Decimal literals. Values may be in the range `-2147483648` to `2147483647` or `-9223372036854775808` to `9223372036854775807`.
* Octal literals starting with `0`

### Floating-Point Number Literals

A C-style floating point number has a whole and fractional part separated by a `.`. The `DoubleString` parser returns the literal string, while the `Double` parser returns the parsed `double`. 

```csharp
var parser = DoubleString();
var parser = Double();
```

### Identifiers

A C-style identifier may start with an underscore (`_`) or a letter, and may be followed by zero or more underscores, letters or digits.

```csharp
var parser = Identifier();
```

### String

A C-style string uses double-quotes and backslash-escapes with a few predefined escape sequences, hex codes, octal codes, and unicode code points. The `String` parser parses the literal string and returns the whole thing as-written, including quotes and escapes. The `StrippedString` parser removes the quotes and replaces the escape sequences with the characters they represent.

```csharp
var parser = String();
var parser = StrippedString();
```

## C++ Parsers

```csharp
using static ParserObjects.Parsers.Cpp;
```

### Comments

A C++-style comment, also known as a "single line comment" starts with the prefix "`//`" and continues to the end of the current line.

```csharp
var parser = Comment();
```

## JavaScript Parsers

```csharp
using static ParserObjects.Parsers.JS;
```

### Numbers

A JavaScript number has a complicated set of rules and may be an integer, a floating point value or use scientific notation. The `NumberString` parser returns the literal parsed string while the `Number` parser returns the parsed `double` value.

```csharp
var parser = NumberString();
var parser = Number();
```

### Strings

JavaScript-style strings may be single- or double-quoted, they use backslash-escapes including hex escapes and unicode code points. The `String` parser returns the whole literal string, including quotes and escapes. The `StrippedString` parser returns the value of the string, without the quotes and with the backslash escapes converted into their actual byte forms.

```csharp
var parser = String();
var parser = StrippedString();
```

## SQL Parsers

```csharp
using static ParserObjects.Parsers.Sql;
```

### Comments

An SQL comment starts with the prefix "`--`" and continues to the end of the line.

```csharp
var parser = Comment();
```

### Identifiers

There are many different syntaxes for SQL identifiers depending on vendor and version. However the ParserObjects SQL identifier parser attempts to parse several common variations.

```csharp
var parser = Identifier();
```

This will parse:

1. Undelimited identifiers which may start with a letter, `'_'`, `'@'` or `'#'` and be followed by any number of these characters, digits, or `'$'`.
2. T-SQL style `[]` delimited identifiers which may contain most non-bracket characters including whitespace,
3. Oracle style `''` delimited identifiers which may contain most non-quote characters including whitespace, and
4. Oracle style `""` delimited identifiers which may contain most non-quite characters including whitespace, and will use `""` for embedded quotes

This parser may not be standards-compliant with any particular database vendor, but it should serve as a good approximation for many common use-cases.

## Guid Parsers

```csharp
using static ParserObjects.Parsers.Guids;
```

Guids come in four formats, based on the C# `Guid.ToString(format)` output:

### Guid N

'N' Guids are 32 hexadecimal digits: `12345678ABCD1234ABCD1234567890AB`

```csharp
var p = GuidN();
```

### Guid D

'D' Guids are 32 hexadecimal digits separated by dashes: `12345678-ABCD-1234-ABCD-1234567890AB`

```csharp
var p = GuidD();
```

### Guid B and P

'B' Guids are the same as 'D' Guids with curly brackets: `{12345678-ABCD-1234-ABCD-1234567890AB}`.

'P' Guids are the same as 'D' Guids with parenthesis: `(12345678-ABCD-1234-ABCD-1234567890AB})`.

```csharp
var pb = GuidB();

var pp = GuidP();
```

## Formatted Date/Time

You can parse formatted Date and Time strings by specifying a format using mostly the same formatting rules as [Microsoft's Custom Date/Time Format Strings](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings). These parsers take a format string and return a Parser.

The `DateAndTime` parser takes a format string and returns a `DateTimeOffset` which may include both date and time information. 

The `Date` parser takes a format string and returns a `DateTime` value without Time information.

The `Time` parser takes a format string and returns a `TimeSpan` value which represents time of day but does not have date information.

```csharp
var dateAndTime = DateAndTime("YYYY-MM-dd HH:mm:ss.fff");
var date = Date("MM/dd/YY");
var time = Time("HH:mm:ss.fff");
```

Format specifiers can be in any number or order, can have any values you want or omit any you don't want. The following specifiers are available:

* `YYYY` the 4-digit year
* `MM` the 2-digit month number (with leading zero, if the value is less than 10)
* `MMM` The 3-character month abbreviation for the current locale. `"Jan"`, `"Feb"`, `"Mar"`, etc. These values are not case-sensitive.
* `MMMM` the full month name for the current locale. `"January"`, `"February"`, `"March"`, etc. These values are not case-sensitive.
* `dd` the 2-digit day of month
* `HH` and `hh` both are equivalent, the 2-digit hour with leading zero.
* `H` and `h` both are equivalent, the 2-digit hour without leading zero.
* `mm` the 2-digit minute with leading zero
* `m` the 2-digit minute without leading zero
* `ss` the 2-digit second with leading zero
* `s` the 2-digit second without leading zero
* `f` a digit of fractions of a second. `"f"` is 10ths of a second, `"ff"` is hundredths of a second, and `"fff"` is milliseconds. The precision only goes down to milliseconds, additional digits are ignored.
* Any other character is taken as a literal and that character is skipped during parsing.

Notice that ambiguities can arise when we parse values without separators and without leading zeros. For example:

```csharp
var parser = Time("Hms");
var result = parser.Parse("11111");
```

This result is ambiguous because the source might intend the value to be `11:11:01` or `01:11:11` or `11:01:11`. Because of the greedy nature of the `.List()` parser used in the implementation of the `Time` parser, it will be parsed as `11:11:01`. Likewise the input value `1111` will throw an error here because `H` will greedily match `"11"`, `m` will greedily match `"11"` and there will be no input left to match `s`. 

**Note:** The 2-digit specifiers with leading zeros (`HH`, `mm`, etc) are more efficient to parse than the specifiers which may omit leading zeros (`H`, `m`, etc). Where possible, prefer the variants with leading zeros.
