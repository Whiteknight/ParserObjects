# Specialty Parsers

In addition to the [Core Parsers](parsers_core.md), ParserObjects provides a few pre-built parsers for common and familiar programming tasks. Several of these methods will cache the created instances so you don't recreate them on every call. Exceptions will be noted below. All of the specialty parsers assume `char` input, because all of these tasks are related to characters and strings.

## Digit Parsers

ParserObjects provides parsers for parsing digits ('0'-'9') and sequences of digits.

```csharp
using static ParserObjects.Parsers.Specialty.DigitParserMethods;
```

### A Single Digit

The `Digit` parser returns a single character in the range (`'0'-'9'`). The `NonZeroDigit` parser returns a single character in the range (`'1'-'9'`), and the `HexadecimalDigit` parser returns any valid hex character (`'a'-'z'`, `'A'-'Z'`, `'0'-'9'`).

```csharp
var parser = Digit();
var parser = NonZeroDigit();
var parser = HexadecimalDigit();
```

### A String of Digits

The `DigitString` parser returns a string of consecutive digits. The `HexadecimalString` parser returns a string of hexadecimal digits.

```csharp
var parser = DigitString();
var parser = HexadecimalString();
```

## Line Parsers

```csharp
using static ParserObjects.Parsers.Specialty.LineParserMethods
```

The `Line` method parses the remainder of the line until the next newline (`'\n'`) character. It does not return the newline character. The `PrefixedLine` method parses the line if it starts with the given prefix. If the prefix is null or empty, it is the same as `Line`.

```csharp
var parser = Line();
var parser = PrefixedLine("abc");
```

The `PrefixedLine` parser is not cached.

## Whitespace Parsers

```csharp
using static ParserObjects.Parsers.Specialty.WhitespaceParserMethods;
```

The `WhitespaceCharacter` parser matches any single whitespace character and returns it. The `Whitespace` parser returns a string of one or more whitespace characters and returns the string. The `OptionalWhitespace` parser returns a string of zero or more whitespace characters.

```csharp
var parser = WhitespaceCharacter();
var parser = Whitespace();
var parser = OptionalWhitespace();
```

## Quoted String Parsers

```csharp
using static ParserObjects.Parsers.Specialty.QuotedParserMethods;
```

ParserObjects provides several methods for parsing quoted strings with escape characters. By default, the backslash (`'\'`) is used as an escape character, which is a common idiom in modern programming and data serialization languages. These methods all have two variants, one to return the literal match including quotes and escape sequences, and a "stripped" version to return the string contents without quotes or escapes.

### Double Quoted Strings

```csharp
var parser = DoubleQuotedString();
var parser = StrippedDoubleQuotedString();
```

### Single Quoted Strings

```csharp
var parser = SingleQuotedString();
var parser = StrippedSingleQuotedString();
```

### Custom Strings

You can create your own string parser method by specifying the start, stop and escape characters:

```csharp
var parser = DelimitedStringWithEscapedDelimiters('"', '"', '\\');
var parser = StrippedDelimitedStringWithEscapedDelimiters('"', '"', '\\');
```

The parsers created by these methods are not cached.

## Regexes

You can use basic regular expressions to create a parser:

```csharp
using static ParserObjects.Parsers.Specialty.RegexParserMethods;
```

```csharp
var parser = Regex("(a|b)?c*");
```

For more details on what syntax is supported by the `Regex` parser, see the [Regexes Page](regexes.md). Be warned that certain types of patterns may create pathological backtracking behavior which will hurt the performance of your parser. 
