# Pratt Parsers

Pratt parsers are a special type of [Operator Precidence Parser](https://en.wikipedia.org/wiki/Operator-precedence_parser#Pratt_parsing) which can dramatically simplify some parsing scenarios, especially mathematical expression parsing. Setting up a Pratt parser is a little different from other parsers because you have to know the type of operator you are parsing, and explicitly set precidence strength values for both left and right association for each rule.

## Basic Operation

```csharp
using static ParserObjects.ParserMethods<char>;
```

```csharp
var target = Pratt<MyValue>(config => {
    ...
});
```

The Pratt parser works by adding rules along with the explicit precidence and association values. Each rule must first provide a parser, which gathers the next value to be wrapped as a token. After this, the rule may specify one or both production callbacks for left and right productions.

```csharp
.Add(Match('-'), ...)
```

A right production rule is used for prefixes. These are things which appear first, and modify or operate upon something to the right of it. In this case, the unary `'-'` operator is used to turn another value negative. In this example, we use the `ctx` parameter to recurse back into the parser to get the right-hand operand:

```csharp
.Add(Match('-'), p => p
    .ProduceRight(7, (ctx, neg) => -ctx.Parse())
)
```

A left production rule is used for suffixes, or things which appear after a value and modify or operate on the value directly to the left. In this case, the unary `'!'` factorial symbol is a suffix which returns an integer that is the product of all integers between 0 and the left value:

```csharp
.Add(Match('!'), p => p
    .ProduceLeft(5, (ctx, left, fact) => {
        var result = 1;
        for (int i = 1; i <= left.Value; i++)
            result = result * i;
        return result;
    })
)
```

An infix operator is an operator which binds on the left, but also recurses to get a value on the right:

```csharp
.Add(Match('-'), p => p
    .ProduceLeft(1, (ctx, left, op) => {
        var right = ctx.Parse();
        return left - right;
    })
)
```

Notice that a single operator may bind on both left or right, depending on the situation. The `'-'` operator is an example, it can be both a unary prefix and an infix operator:

```csharp
.Add(Match('-'), p => p
    .ProduceRight(7, (ctx, neg) => -ctx.Parse())
    .ProduceLeft(1, (ctx, left, op) => {
        var right = ctx.Parse();
        return left - right;
    })
)
```

The parser will invoke the correct production callback depending on the situation where the token is found.

## Precidence and Association

In the examples above the `.ProduceLeft()` and `.ProduceRight()`, methods all took an unexplained integer parameter. These values are called **binding power**. Pratt parsers implement precidence and association using these binding power values. Every rule has potentially two binding power values: one for the left and one for the right. Rules with higher binding power values have higher precidence. By default the Left Binding Power is 0 unless otherwise set, and the Right Binding Power defaults to be the same as the Left Binding Power unless otherwise set. In this example the `'-'` has a binding power of 1 (very low) when used as an infix operator on the left, and a binding power of 7 (much higher) when used as a unary operator on the right:

```csharp
.Add(p => p
    .Parse(Match('-'))

    .ProduceLeft(1, (ctx, left, op) => {
        var right = ctx.Parse();
        return left - right;
    })

    .ProduceRight(7, (ctx, neg) => -ctx.Parse())
)
```

Infix operators can be changed from left-associative to right-associative by changing the binding powers. An infix operator is left-associative if the Left Binding Power is lower than or equal to the Right Binding Power. It is Right-Associative if the Right Binding Power is lower. For example, in C and C-like languages, the `'+'` operator binds left-associative, but the `'='` operator is right-associative with lower precidence. We would indicate that with this example code:

```csharp
var parser = Pratt<string>(c => c
    .Add(DigitString())
    .Add(Identifier())
    .Add(Match('+'), p => p
        .ProduceLeft(3, (ctx, left, op) => {
            var right = ctx.Parse();
            return $"({left}+{right})";
        })
    )
    .Add(Match('='), p => p
        .ProduceLeft(2, 1, (ctx, left, op) => {
            var right = ctx.Parse();
            return $"({left}={right})";
        })
    )
);
```

With this parser, an input string like `"a=b=4+5+6"` would be correctly parenthesized as `"(a=(b=((4+5)+6)))".

