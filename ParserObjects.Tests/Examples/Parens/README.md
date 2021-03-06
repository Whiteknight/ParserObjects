﻿# Expression Parenthesizer

This example uses the `Pratt` parser to parse an expression with multiple levels of precidence, and produce a parenthesized output which removes ambiguity in the order of operations. It is worth noting that this example is significantly smaller than some other expression-parsing examples for similar functionality. This example can be modified to produce a more complicated and feature-ful parser, which is left as an exercise for the reader.

One interesting feature which this parser handles and other expression parsers in this suite do not, is the "implied multiplication" of the form `"x(y)"`. This notation is common in hand-written mathematics but rarely used in programming languages. This construct translates to `"x * (y)"` though it has a slightly higher precidence than normal multiplication and division. The close contact between the two terms signifies that they are tightly linked, and should not be separated due to normal order-of-operations rules. The expression `"4*3(2)"` is parenthesized as `"(4*(3*(2)))"` instead of `"((4*3)*(2))"` which might be the case if all multiplications were strictly evaluated from left-to-right at a single level of precidence. It's worth noting that the reverse notation is not implemented: `"(4)2"` does not parse (again, this can be left as an exercise for the reader).

There are a few interesting things in this parser. First, we handle whitespace by making whitespace tokens bind extremely tightly on both left and right, and using both left and right production rules to discard the whitespace in favor of whatever the next token is instead:

```csharp
.Add(c => c
    .Parse(Whitespace())
    .RightBindingPower(100)
    .ProduceRight((ctx, value) =>
    {
        return ctx.Parse();
    })
    .LeftBindingPower(100)
    .ProduceLeft((ctx, left, value) =>
    {
        return left;
    })
    .Named("whitespace")
)
```

Notice also the rule for `'-'`, which can be both a unary prefix (to create a negative value) or an infix operator (for subtraction). The infix case is handled by the `.ProduceLeft()` callback, which Parses operators using the default left binding power (`1`). The prefix case is handled by the `.ProduceRight()` callback, which uses a higher binding power value (`7`) because the negative sign should bind very closely with the value to the right of it and should not get mixed up with other addition and subtraction operations:

```csharp
.Add(c => c
    .Parse(Match('-').Transform(x => x.ToString()))
    .LeftBindingPower(1)
    .ProduceRight((ctx, op) =>
    {
        var r = ctx.Parse(7);
        return $"({op}{r})";
    })
    .ProduceLeft((ctx, l, op) =>
    {
        var r = ctx.Parse(1);
        return $"({l}{op}{r})";
    })
    .Named("Subtract/Negate")
)
```





