# Reverse Polish Notation Parser

The Reverse Polish Notation (RPN) parser demonstrates the use of the `FunctionParser` to parse a grammar which is not easily amenable to the "basic" set of parser combinators.

The grammar for RPN looks something like this:

```
<op>                ::= '+' | '-' | '*' | '/'
<operation>         ::= <numberOrOperation> <numberOrOperation> <op>
<numberOrOperation> ::= <operation> | <number>
```

ParserObjects provides parsers to help with left and right recursion with a particular focus in helping to parse infix expressions, but this grammar possibly recurses in both the left and middle positions, so those existing `LeftApply` and `RightApply` parsers don't really help.

By using the `FunctionParser` this example can instead convert to the more traditional stack-based algorithm for reading RPN, and wrap that algorithm up into a form that is compatible with other parser combinators.

For error-handling pay attention to this line:

```csharp
var tokenSequence = tokens
    .ToSequence(FromString(s))
    .Select(r => r.GetValueOrDefault(() => new RpnToken(r.ErrorMessage, RpnTokenType.Failure)));
```

In the `.Select` method, just calling `r => r.Value` would throw an exception if the token parser returned a fail result. However, by calling `r.GetValueOrDefault(...)` we can instead turn a failure result into a failure token, which could include additional information about where the lexical failure occured. Later in the `Sequence` parser, we have this line to detect it:

```csharp
if (token.Type == RpnTokenType.Failure)
    return fail("Tokenization error: " + token.Value);
````

Again, we could include more information about the failure within the failure token, such as the name of the parser where the failure occured or the location of the failure, but in this small example we only use the error message as the value.