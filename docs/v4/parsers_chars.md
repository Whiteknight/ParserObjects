# Character Parsers

In addition to the [Core Parsers](parsers_core.md), ParserObjects provides a few pre-built parsers for common character and string parsing and matching tasks. Several of these methods will cache the created instances so you don't recreate them on every call. Exceptions will be noted below. All of the specialty parsers assume `char` input, because all of these tasks are related to characters and strings.

All these specialty parsers can be accessed with this declaration:

```csharp
using static ParserObjects.ParserMethods;
```

## Matching Parsers

### Character Matcher

The `MatchChar` parser matches a given character. It operates the same as `Match(char)` except it caches instances for characters that have previously been requested.

```csharp
var parser = MatchChar('x');
```

This is functionally equivalent to the following, but caches the instance to reduce memory consumption if parsers are being re-requested multiple times:

```csharp
var parser = Match('x');
```

### Character In Collection

If you want to match a character which is one of a finite set of options, you can use the `MatchAny` parser to check if the character is in a collection. 

```csharp
var possibilities = new HashSet<char>() { 'x', 'y', 'z' };
var parser = MatchAny(possibilities);
```

`HashSet<char>` or another collection type optimized for fast `.Contains()` is preferred, but you can use any collection. 

There is also a `NotMatchAny` which returns success if the character is *not* in the collection:

```csharp
var forbidden = new HashSet<char>() { 'a', 'b', c' };
var parser = NotMatchAny(forbidden);
```

### Character String Parser

The `CharacterString` parser matches a literal string of characters against a `char` input and returns the string on success.

```csharp
var parser = CharacterString("abc");
```

This is functionally equivalent to a combination of the `MatchSequence` and `Transform` parsers:

```csharp
var parser = Match("abc").Transform(x => new string(x.ToArray()));
```

## Character Class Parsers

The `Letter` parser matches any uppercase or lowercase letter character. `Word` matches a sequence of one or more letters and returns them as a string. `UpperCase` matches any one uppercase character, and `LowerCase` matches any one lowercase character. The `Symbol` parser matches any non-letter, non-number symbol or punctuation character. You can convert any of those to a string like `Word` does by using the `.ListCharToString()` extension method:

```csharp
var allUpperCase = UpperCase().ListCharToString();
var allLowerCase = LowerCase().ListCharToString();
```

## Digit Parsers

ParserObjects provides parsers for parsing digits ('0'-'9') and sequences of digits.

If you want to parse formatted numbers with possible negative values, decimal values and syntactic rules (no leading 0, etc), consider using one of the [Programming Parser Methods](parsers_programming.md) instead.

### A Single Digit

The `Digit` parser returns a single character in the range (`'0'-'9'`). The `NonZeroDigit` parser returns a single character in the range (`'1'-'9'`), and the `HexadecimalDigit` parser returns any valid hex character (`'a'-'f'`, `'A'-'F'`, `'0'-'9'`).

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

The `Line` method parses the remainder of the line until the next newline (`'\n'`) character. It does not return the newline character. The `PrefixedLine` method parses the line if it starts with the given prefix. If the prefix is null or empty, it is the same as `Line`.

```csharp
var parser = Line();
var parser = PrefixedLine("abc");
```

The `PrefixedLine` parser is not cached.

## Whitespace Parsers

The `WhitespaceCharacter` parser matches any single whitespace character and returns it. The `Whitespace` parser returns a string of one or more whitespace characters and returns the string. The `OptionalWhitespace` parser returns a string of zero or more whitespace characters and is equivalent to `.Whitespace().Optional()`.

```csharp
var parser = WhitespaceCharacter();
var parser = Whitespace();
var parser = OptionalWhitespace();
```

## Quoted String Parsers

```csharp
using static ParserObjects.QuotedParserMethods;
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

You can create your own quoted string parser method by specifying the start, stop and escape characters:

```csharp
var parser = DelimitedStringWithEscapedDelimiters('"', '"', '\\');
var parser = StrippedDelimitedStringWithEscapedDelimiters('"', '"', '\\');
```

The parsers created by these methods are not cached.

## Regexes

You can use basic regular expressions to create a parser:

```csharp
var parser = Regex("(a|b)?c*");
```

For more details on what syntax is supported by the `Regex` parser, see the [Regexes Page](regexes.md). Be warned that certain types of patterns may create pathological backtracking behavior which will hurt the performance of your parser. 

## Identifier Parsers

### Camel Case

You can parse **CamelCase** identifiers using the `CamelCase` and `UpperCamelCase` parsers:

```csharp
var parts = CamelCase().Parse("camelCaseIdentifier123ABC");
// returns ["camel", "Case", "Identifier", "123", "ABC"]
```

`UpperCamelCase` expects the first character of the first word to be capitalized. the `CamelCase` parser allows the first character to be upper or lower case. This parser treats number strings as a word, and also consecutive upper-case characters as a single word acronym.

### Spinal Case

Spinal Case (also known by some people as "kebab case") consists of words separated by a dash, and can be parsed with the `SpinalCase` parser:

```csharp
var parts = SpinalCase().Parse("spinal-case-identifier");
// returns ["spinal", "case", "identifier"]
```

Capitalization does not matter, and numbers may also be used. If you want only capitalized identifiers, you can use the `ScreamingSpinalCase` parser, which only recognizes upper-case letters and numbers:

```csharp
var parts = ScreamingSpinalCase().Parse('SCREAMING-SPINAL-CASE");
// returns ["SCREAMING", "SPINAL", "CASE"]
```

### Snake Case

Snake case consists of workds separated by an underscore and can be parsed with the `SnakeCase` parser, and an all-uppercase version can be parsed with the `ScreamingSnakeCase` parser:

```csharp
var parts = SpinalCase().Parse("snake_case_identifier");
// returns ["snake", "case", "identifier"]

var parts = ScreamingSnakeCase().Parse("SNAKE_CASE_IDENTIFIER");
// returns ["SNAKE", "CASE", "IDENTIFIER"]
```
