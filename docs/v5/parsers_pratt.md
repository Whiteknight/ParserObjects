# Pratt Parsers

Pratt parsers are a special type of [Operator Precedence Parser](https://en.wikipedia.org/wiki/Operator-precedence_parser#Pratt_parsing) which can dramatically simplify some parsing scenarios, especially mathematical expression parsing. Setting up a Pratt parser is a little different from other parsers because you have to know the type of operator you are parsing, and explicitly set precedence strength values for both left and right association for each rule.

## Basic Operation

```csharp
using static ParserObjects.Parsers<char>;
```

```csharp
var target = Pratt<MyValue>(config => {
    ...
});
```

The Pratt parser works by adding rules along with the explicit precedence and association values. Each rule must first provide a parser, which gathers the next value to be wrapped as a token. After this, the rule may specify one or both production callbacks for left and right productions.

```csharp
.Add(Match('-'), ...)
```

A **right production rule** is used for prefixes and is specified with the `.Bind()` method. These are things which appear first, and modify or operate upon something to the right of it. In this case, the unary `'-'` operator is used to turn another value negative. In this example, we use the `ctx` parameter to recurse back into the parser to get the right-hand operand:

```csharp
.Add(Match('-'), p => p
    .Bind(7, (ctx, neg) => -ctx.Parse())
)
```

A **left production rule** is used for suffixes, or things which appear after a value and modify or operate on the value directly to the left. It is specified with the `BindLeft()` method. In this case, the unary `'!'` factorial symbol is a suffix which returns an integer that is the product of all integers between 0 and the left value:

```csharp
.Add(Match('!'), p => p
    .BindLeft(5, (ctx, left, fact) => {
        var result = 1;
        for (int i = 1; i <= left.Value; i++)
            result = result * i;
        return result;
    })
)
```

An infix operator is an operator which binds on the left, but also recurses to get a value on the right. In this case the infix `-` operator takes a left value and subtracts from it the right value:

```csharp
.Add(Match('-'), p => p
    .BindLeft(1, (ctx, left, op) => {
        var right = ctx.Parse();
        return left - right;
    })
)
```

Notice that a single operator may bind on both left or right, depending on the situation. The `'-'` operator is an example, it can be both a unary prefix and an infix operator:

```csharp
.Add(Match('-'), p => p
    // Unary `-` for negative
    .Bind(7, (ctx, neg) => -ctx.Parse())

    // Infix operator for subtraction
    .BindLeft(1, (ctx, left, op) => {
        var right = ctx.Parse();
        return left - right;
    })
)
```

The parser will invoke the correct production callback depending on the situation where the token is found. Where multiple rules might match, the binding rules are attempted in declared order, and the first success is used.

## Precedence and Associativity

In the examples above the `.ProduceLeft()` and `.ProduceRight()`, methods all took an unexplained integer parameter. These values are called **binding power**. Pratt parsers implement precedence and association using these binding power values. Every rule has potentially two binding power values: one for the left and one for the right. Rules with higher binding power values have higher precedence. By default the Left Binding Power is 0 unless otherwise set, and the Right Binding Power defaults to be the same as the Left Binding Power unless otherwise set. In this example the `'-'` has a binding power of 1 (very low) when used as an infix operator on the left, and a binding power of 7 (much higher) when used as a unary operator on the right:

```csharp
.Add(p => p
    .Parse(Match('-'))

    .BindLeft(1, (ctx, left, op) => {
        var right = ctx.Parse();
        return left - right;
    })

    .Bind(7, (ctx, neg) => -ctx.Parse())
)
```

Infix operators can be changed from left-associative to right-associative by changing the binding powers. An infix operator is left-associative if the Left Binding Power is lower than or equal to the Right Binding Power. It is Right-Associative if the Right Binding Power is lower. This is tricky because it sounds backwards.

If we have the expression `5+4-3`, and if we want `+` and `-` to be left-associative, we set the left binding power to be lower than the right binding power. That means that the '+' will bind to the 4 first, and then to the 5, creating the first term `(5+4)`, and then the '-' will bind to the 3 first and then to the first term creating `(5+4)-3`. Likewise to be right-associative operators are set to bind to the left more strongly, so `+` binds to the 5 first, `-` binds to the 4 first and then to the 3, creating a term `(4-3)`, and then the `+` binds to the right side to create `5+(4-3)`.

For example, in C and C-like languages, the `'+'` operator binds left-associative, but the `'='` operator is right-associative with lower precedence. We would indicate that with this example code:

```csharp
var parser = Pratt<string>(c => c
    .Add(DigitString())
    .Add(Identifier())

    // '+' has binding power 3, which is higher than '='
    // Without specifying a second binding power value, it will be left-associative
    .Add(Match('+'), p => p
        .BindLeft(3, (ctx, left, op) => {
            var right = ctx.Parse();
            return $"({left}+{right})";
        })
    )

    // `=` has a lower binding power and has a lower value on the right, which means it will
    // be right-associative
    .Add(Match('='), p => p
        .BindLeft(2, 1, (ctx, left, op) => {
            var right = ctx.Parse();
            return $"({left}={right})";
        })
    )
);
```

With this parser, an input string like `"a=b=4+5+6"` would be correctly parenthesized as `"(a=(b=((4+5)+6)))".

