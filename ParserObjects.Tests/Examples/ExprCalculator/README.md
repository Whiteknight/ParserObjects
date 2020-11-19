# Expression Calculator Example

This example shows the use of parser types to build an Infix expression parser for simple operations `+` and  `*`. Other operations are left as an exercise for the user.

This example uses a two-phase design, with a `LexicalGrammar` converting an input string into a stream of `Token`s and an `ExpressionGrammar` parsing a stream of `Token`s into a final result.

In the lexical grammar, pay particular attention to this line:

```csharp
var whitespaceAndToken = Rule(
    whitespace,
    anyToken,
    (ws, t) => t
);
```

Here we are looking for optional whitespace, followed by any matching token token rule, but we only return the token. Whitespace is ignored. In this way, the expression grammar can completely ignore whitespace and make simpler rules to only work with meaningful tokens. We could also create Whitespace Tokens and return those from the lexical grammar, and use the `.Where(t => t.Type != TokenType.Whitespace)` method on our sequence. The two results would be identical.


In the expression grammar, there are a few things to pay attention to. The first is this idiom, which is repeated several times:

```csharp
var requiredNumber = First(
    number,
    ThrowError("Expected number")
);
```

In this code example, the `First` parser lets us try to match several alternatives, one after the other. In this case, we either want to match `number` or else we want to throw an error. In this case we are actually throwing an `Exception` from that method. We see that idiom used at the end of the grammar, where we expect to parse the entire input by either matching `End` or throwing an exception:

```csharp
var requiredEnd = First(
    End(),
    Produce<bool>(t => throw new Exception($"Expected end of input but found {t.Peek()} at {t.CurrentLocation}"))
);

var expression = Rule(
    additive,
    requiredEnd,
    (add, end) => add
);
```

Parsing expressions means keeping track of both precidence of different operators, and the associativity of each precidence level. In our case both the addition and multiplication operations are left associative. ParserObjects provides the `LeftApply` parser to handle associativity:

```csharp
var multiplicative = LeftApply(
    number,
    left => First(
        Rule(
            left,
            Token(TokenType.Multiplication),
            requiredNumber,
            (l, op, r) => l * r
        ),
        Rule(
            left,
            Token(TokenType.Division),
            requiredNumber,
            (l, op, r) => l / r
        )
    )
);
```

In the LeftApply parser, it parses the left token first (`number`) and then it tries to combine that with additional tokens. When one of the rules succeeds, that value is used as the new left-hand size and it tries to match the rules again. In the expression `1 * 2 * 3 * 4 * 5`, the parser tries to match a number (`left`), an operator and another number. When it finds that, it calculates the result and uses that as the new left-hand side of the equation `2 * 3 * 4 * 5`. It matches the rule again to reduce to `6 * 4 * 5`, followed by `24 * 5` and then the final answer is received after matching the rule for the final time `120`.

