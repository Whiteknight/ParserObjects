# S-Expression Parser

This example shows a simple S-Expression parser. This parser parses a relatively modern-style dialect of S-expressions but does not meet any specification in particular. This parser does not specifically handle McCarthy-style list concatenation `(a . b)`, nor does it support Lisp-style polish notation `(+ 1 2)`. Both of these will parse, but there is no enforcement or special semantic meaning of either of these. Building on this simple parser to produce something more generally usable is left as an exercise to the reader.

The S-Expression parser shown here is a two-phase design with a lexer and a parser. The lexer converts an input sequence of characters into an input sequence of `Token`s and the parser converts the sequence of tokens into the `INode` tree which represents the result.

## Whitespace

Whitespace is dealt with in two steps. First, in the lexer, we create a Whitespace token containing the complete value of the whitespace:

```csharp
var ws = Whitespace().Transform(w => new Token { Type = ValueType.Whitespace, Value = w });
```

Then later in the `SExpressionParser` class we explicitly remove those whitespace tokens from the input sequence to the parser:

```csharp
var tokens = lexer
    .ToSequence(chars)
    .Select(r => r.Value)
    .Where(t => t.Type != ValueType.Whitespace);
```

This is different from some of the other examples, where whitespace is handled in the lexer.

## Location Tracking

The lexical grammar adds location information to the generated Tokens, so that we can include location information later in the parser and in the `INode` AST. In this case we add the location using the following pattern:

```csharp
var token = Rule(
    Produce(t => t.CurrentLocation),
    anyToken,
    (location, t) =>
    {
        t.Location = location;
        return t;
    }
);
```

The `Produce` parser is getting the `.CurrentLocation` from the input at the beginning of the rule, so we can see the location where the token value begins. Then the location is added to the token received from the `anyToken` rule. 

## Missing Items

This parser will handle missing `)` close parenthesis by creating a **synthetic Token** when a close paren is expected but not found. This synthetic token looks like a real token out of the lexer but includes an error message in the `.Diagnostics` list. We can set this up with this idiom:

```csharp
var requiredCloseParen = First(
    Token(ValueType.CloseParen),
    Produce(t => new Token
    {
        Location = t.CurrentLocation,
        Type = ValueType.CloseParen,
        Value = ")",
        Diagnostics = new List<string>
        {
            "Missing close parenthesis"
        }
    })
);
```

Here we use the `First` parser to either find the close paren we expect, or else call `Produce` to create the synthetic token.

It's important to note that many consumers of parsers, such as a text editor with syntax highlighting or an IDE which wants to highlight errors, will want this kind of behavior. The parser should not fail and bail out at the first sign of error. The parser should try to work past errors as best as possible, and try to list out as many errors as it can. Using synthetic elements in both the input sequences and output AST of the parser can help to provide these behaviors.

There is a unit test which shows how this feature works. When a string is parsed with unbalanced parenthesis, the parse "succeeds", but the result node includes an error message indicating that it was not a valid output:

```csharp
[Test]
public void Parse_MissingRightParen()
{
    var parser = new SExpressionParser();
    var node = parser.Parse("(");
    node.Diagnostics.Count.Should().Be(1);
    node.Diagnostics[0].Should().Be("Missing close parenthesis");
}
```