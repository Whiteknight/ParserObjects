# Character Parsers

In addition to the [Core Parsers](parsers_core.md), ParserObjects provides a few pre-built parsers for common character and string parsing and matching tasks. Several of these methods will cache the created instances so you don't recreate them on every call. Exceptions will be noted below. All of the specialty parsers assume `char` input, because all of these tasks are related to characters and strings.

All these specialty parsers can be accessed with this declaration:

```csharp
using ParserObjects;
using static ParserObjects.Parsers;
```

## Matching Parsers

### Character Matcher

The `MatchChar` parser matches a given character. It is similar to the `Match` parser but optimized for character matching workflows. The library will do additional caching of parser instances where possible, and the `MatchChar` parser will never match the *end sentinel* of the input sequence.

```csharp
var parser = MatchChar('x');
var parser = MatchChar(c => char.IsSymbol(c));
```

This is functionally similar to, but slower than, `Match()`:

```csharp
var parser = Match('x');
var parser = Match(c => char.IsSymbol(c));
```

You can also do case insensitive match by setting the `caseInsensitive` flag:

```csharp
var parser = MatchChar('x', caseInsensitive: true);
```

**Note:** `MatchChar` will not read the end sentinel, where the `Match` parser will. So these two parsers will behave differently at the end of input:

```csharp
var parser1 = Match(c => !char.IsLetter(c));
var parser2 = MatchChar(c => !char.IsLetter(c));
```

The `Match` parser will see that the default end sentinel `'\0'` satisfies the predicate and return it, while the `MatchChar` parser will not read the end sentinel and will return failure.  Also, the `MatchChar` parser will always return failure if you ask it to match the end sentinel, because it will never attempt to read past the end of input:

```csharp
var parser = MatchChar('\0');
var result = parser.Parse("");
result.Success.Should().BeFalse();
```

If your input string contains nested null characters, those will match the above parser until the sequence reaches end of input.


### Character In Collection

If you want to match a character which is one of a finite set of options, you can use the `MatchAny` parser to check if the character is in a collection. 

```csharp
var possibilities = new HashSet<char>() { 'x', 'y', 'z' };
var parser = MatchAny(possibilities);
```

`HashSet<char>` or another collection type optimized for fast `.Contains()` is preferred, but you can use any collection.

There is also a `NotMatchAny` which returns success if the character is *not* in the collection:

```csharp
var forbidden = new HashSet<char>() { 'a', 'b', 'c' };
var parser = NotMatchAny(forbidden);
```

Notice that these methods do not have a `caseInsensitive` flag. If you want case-insensitive matching you need to set up your collection with a case-insensitive `IEqualityComparer<T>`.

### Character String Parser

The `MatchChars` parser matches a literal string of characters against a `char` input and returns the string on success. There is also an alias `CharacterString()` which does the same thing:

```csharp
var parser = MatchChars("abc");
var parser = CharacterString("abc");
```

This is functionally equivalent to (though faster than) a combination of the `MatchSequence` and `Transform` parsers:

```csharp
var parser = Match("abc").Transform(x => new string(x.ToArray()));
```

There is also a `MatchAny` parser, similar to the `Trie` parser, which takes several string patterns and will return a string if any of the patterns match. It also has a `caseInsensitive` mode:

```csharp
var parser = MatchAny(new[] { "pattern1", "pattern2", ...}, caseInsensitive: true);
```

## Character Class Parsers

The `Letter` parser matches any uppercase or lowercase letter character. `Word` matches a sequence of one or more letters and returns them as a string. `UpperCase` matches any one uppercase character, and `LowerCase` matches any one lowercase character. The `Symbol` parser matches any non-letter, non-number symbol or punctuation character. You can convert any of those to a string like `Word` does by using the `.ListCharToString()` extension method:

```csharp
var letter = Letter();
var word = Word();
var alsoWord = Letter().ListCharToString();
var allUpperCase = UpperCase();
var allLowerCase = LowerCase();
var symbols = Symbol();
```

## Digit Parsers

ParserObjects provides parsers for parsing digits ('0'-'9') and sequences of digits.

```csharp
using static ParserObjects.Parsers;
```

If you want to parse formatted numbers with possible negative values, decimal values and syntactic rules (no leading 0, etc), consider using one of the [Programming Parser Methods](parsers_programming.md) instead.

### A Single Digit

The `Digit` parser returns a single character in the range (`'0'-'9'`). The `NonZeroDigit` parser returns a single character in the range (`'1'-'9'`), and the `HexadecimalDigit` parser returns any valid hex character (`'a'-'f'`, `'A'-'F'`, `'0'-'9'`).

```csharp
var parser = Digit();
var parser = NonZeroDigit();
var parser = HexadecimalDigit();
```

### A String of Digits

The `DigitString` parser returns a string of consecutive digits.

```csharp
var parser = DigitString();
```

### A String of Digits as an Integer

The `DigitsAsInteger` parser reads a string of consecutive digits and parses them as an `int`:

```csharp
var parser = DigitsAsInteger();
```

This is the same as:

```csharp
var parser = DigitString().Transform(int.Parse);
```

**Notice:** This parser doesn't do any special behavior with respect to leading zeros, doesn't handle decimal points, fractions, or scientific notation, doesn't parse leading `-` for negatives, etc. For a more structured number parser following existing programming language rules see [The C Parsers](parsers_programming.md) or the [JS Parsers](parsers_programming.md).

## Line Parsers

The `Line` method parses the remainder of the line until the next newline character. It does not return the newline character. The `PrefixedLine` method parses the line if it starts with the given prefix. If the prefix is null or empty, it is the same as `Line`.

```csharp
var parser = Line();
var parser = PrefixedLine("abc");
```

The `PrefixedLine` parser instance is not cached by the library.

## Whitespace Parsers

The `WhitespaceCharacter` parser matches any single whitespace character and returns it. The `Whitespace` parser returns a string of one or more whitespace characters as a string. The `OptionalWhitespace` parser returns a string of zero or more whitespace characters and is equivalent to `.Whitespace().Optional()`.

```csharp
var parser = WhitespaceCharacter();
var parser = Whitespace();
var parser = OptionalWhitespace();
```

## Quoted String Parsers

```csharp
using static ParserObjects.Parsers;
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

## Regular Expressions

You can use regular expressions to create a parser:

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

`UpperCamelCase` expects the first character of the first word to be capitalized. The `CamelCase` parser allows the first character to be upper or lower case. This parser treats number strings as a word, and also consecutive upper-case characters as a single word acronym.

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

Snake case consists of words separated by an underscore and can be parsed with the `SnakeCase` parser, and an all-uppercase version can be parsed with the `ScreamingSnakeCase` parser:

```csharp
var parts = SpinalCase().Parse("snake_case_identifier");
// returns ["snake", "case", "identifier"]

var parts = ScreamingSnakeCase().Parse("SNAKE_CASE_IDENTIFIER");
// returns ["SNAKE", "CASE", "IDENTIFIER"]
```

## Stringify Parsers

The `Stringify` parser takes a parser which returns a `IReadOnlyList<char>` result and transforms it to return a `string` result instead. This may be useful in places where you are using a `List` or a `Match(pattern)` parser and need to get a string result without having to concatenate all the substrings together yourself. 

```csharp
var getWord = Stringify(
    List(
        Match(c => char.IsLetter(c))
    )
);
var getWord = Match(c => char.IsLetter(c)).List().Stringify();
```

## Capturing Parsers

### CaptureString Parser

The `CaptureString` parser takes a list of several parsers, matches each of them in series, and returns the complete match as a `string`. This parser can only be used with an `ISequence<char>` input, is optimized for `ICharSequence` inputs, and represents a significant optimization over alternative parsers for the same behavior:

```csharp
var parser = CaptureString(p1, p2, p3, ...);
```

This has equivalent behavior to, but much lower performance than, a `Combine` or `Rule` parser with a `Transform`:

```csharp
var parser = Combine(
        p1,
        p2,
        p3,
    )
    .Transform(l => string.Join("", l));
```