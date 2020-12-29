# Pratt Expression Calculator

This example shows the use of the Pratt parser for parsing mathematical expressions with associativity and precidence. This example is extremely similar to the ExprCalculator example except for the use of `Pratt` instead of `LeftApply`, the inclusion of a few additional operators (`^` for exponentiation and `(`, `)` for grouping), and handling of the `End` rule.

## End Handling

Instead of searching for `End()` in the `ExpressionGrammar`, this example instead searches for `End` in the `LexicalGrammar` and returns an End token. The `ExpressionGrammar` can then search for an `End` token.

## Pratt Parser

The Pratt algorithm was originally developed specifically to handle the case of mathematical expressions. Even though it's usable for other things, it really shines most brightly when parsing these kinds of things. There are a few pieces of code to pay attention to most:

```csharp
.Add(number)
```

This small piece of code is a shorthand for something like this:

```csharp
.Add(number, p => p
    .ProduceRight(0, (_, v) => v.Value))
```

It creates a non-operator value which does not itself interact with anything to the left or right of it. Most operators are defined like this:

```csharp
.Add(Token(TokenType.Addition), p => p
    .ProduceLeft(1, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return left.Value + right;
    }))
```

This is a shorthand for the nearly-identical

```csharp
.Add(Token(TokenType.Addition), p => p
    .ProduceLeft(1, 2, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return left.Value + right;
    }))
```

This is the implementation for a left-associative "infix" operator, where values are expected to appear on both the left and right sides. An expression of the form `"1+2+3"` is parsed as `(1+2)+3` instead of `1+(2+3)`. For addition the order of evaluation doesn't matter, but for subtraction and division it will. However, there's another operator for exponentiation which is a little different:

```csharp
.Add(Token(TokenType.Exponentiation), p => p
    .ProduceLeft(6, 5, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return (int)Math.Pow(left.Value, right);
    }))
```

For exponentiation there are two precidence values, with the left value being higher than the right value. This sets up exponentiation as being right-associative instead. So `"2^3^4"` is parsed as `2^(3^4)` instead of `(2^3)^4`. How does this work? If we think about the equation `"1+2+3"` with binding powers `(1, 2)`, addition binds more strongly on the right. So the first `+` operator "steals" the `2` and binds it immediately, because the second `+` binds on the left side more weakly than the first `+` binds on the right. So a rule `('+', 1, 2)` would parse as `"(1+2)+3"`. For exponentiation the rule `('^', 6, 5)` works in the opposite way, because for the expression `"2^3^4"` the second `^` binds to the `3` more strongly than the first `^` does, so it is parsed as `2^(3^4)`. 


