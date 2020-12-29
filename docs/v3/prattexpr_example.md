# Pratt Expression Parsing Example

In the [previous example](expression_example.md) we showed the use of basic combinators to parse mathematical expressions. These classical combinators can parse mathematical expressions like this for the most part, though the better tool for the job is the **Pratt Parser**. Pratt parser is a *precedence-climbing* parser or *operator precedence parser* which is particularly well-suited for parsing expressions.

From the previous example we are going to keep the `Calculator` class, the `LexicalGrammar` class, and remaining components not related to the parser itself (`Token`, extension methods, etc).  First thing we need to do is stub out the Pratt parser:

```csharp
var expression = Pratt<int>(config => config
    ...
);
```

## The Pratt Algorithm Overview

The Pratt algorithm considers two types of productions: Items which bind to an item immediately to the left, and items which do not bind to an item immediately to the left. For example, the number `'4'` is just a number, and can exist by itself without being preceded by anything immediately to the left of it. However the infix operator `'+'` requires a value on the left side (and the right side also, but that's not important right now). In the literature, the first type of production would be called a "**Null Denominator**" or "NUD", and the second type of production is called a "**Left Denominator**" or "LED". ParserObjects uses this terminology internally, but the methods exposed to the user are named more simply as `.ProduceRight` and `.ProduceLeft` for NUD and LED, respectively.

The Pratt algorithm also has a concept of "binding power". The Binding Power of a production rule tells how strongly one production binds to another production on the left and right sides. This binding power takes the place of both precedence and associativity concepts in other parsers. Items such as numbers, which stand alone and don't bind to other items can have a precedence of `0`. All production rules have a left- and right-binding power value, though in most cases you only need to specify one and the other can be inferred.

The Pratt algorithm works by first looking for a NUD value at the start, because there is nothing to the left of it to bind to, and then loop to find all matching LED values which can attach to that initial NUD value. Any rule may recurse back into the parser if it wants to hunt for a new value. For example, in the mathematical expression `"1 + 2"` The `1` is the NUD. Then the Parser matches the `+` operator as the LED. The callback for the `+` operator then recurses into the parser again to find a new value, which finds `2` as a NUD, no LEDs, and then returns. We will illustrate this concept with some code. First, let's add numbers:

```csharp
var number = Token(TokenType.Number).Transform(t => int.Parse(t.Value));

var expression = Pratt<int>(config => config
    .Add(number, p => p.ProduceRight(0, (ctx, value) => value.Value))
);
```

What this statement means is that we want the Pratt parser to recognize numbers, to treat them with the lowest possible precedence, and just return the value as-is. A shorthand for this kind of production is this:

```csharp
.Add(number)
```

Next, we want to add an operator. We will start with '+':

```csharp
.Add(Token(TokenType.Addition), p => p
    .ProduceLeft(1, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return left.Value + right;
    }))
```

Here we're looking for Addition tokens, and binding those to the item on the left with a binding power of 1 (very low, because '+' is the lowest-precedence operator in our grammar). Notice the call to `ctx.Parse()` which recurses into the Pratt engine to find the right-hand side value. With this code now, our parser can parse expressions like `"4"`, `"4 + 5"`, `"4 + 5 + 6"`. So we want to add another operator to make things more interesting. Let's add Multiplication and Division:

```csharp
.Add(Token(TokenType.Multiplication), p => p
    .ProduceLeft(3, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return left.Value * right;
    }))
.Add(Token(TokenType.Division), p => p
    .ProduceLeft(3, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return left.Value / right;
    }))
```

Notice that this is almost exactly the same code as for the addition example, but with a binding power of `3` instead of `1`. That means multiplication and division have higher precedence than addition, but the same as each other. An expression of the form `"4 + 5 * 6"` will parse correctly as `"4 + (5 * 6)"` and output a result of `34`.

Now we will do something a little more complicated. The `-` operator has two roles: it can be a unary prefix which turns a number negative, or it can be an infix operator for subtraction. In other words, the `-` can be both a NUD and a LED, so we need to specify two production rules:

```csharp
.Add(Token(TokenType.Subtraction), p => p
    .ProduceRight(9, (ctx, _) =>
    {
        var right = ctx.Parse();
        return -right;
    })
    .ProduceLeft(1, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return left.Value - right;
    }))
```

As a prefix, unary '-' binds to the value with a binding power of 9, which is very high. As an infix operator for subtraction, '-' binds with a value of `1`, which is the same as addition. So an expression `"4 + -5 - 6"` will parse as `"(4 + (-5)) - 6"` and give the correct answer `-7`. Both the right and left production rules call `ctx.Parse()` to get the value for the right-hand side.

Now let's look at another interesting case: exponentiation. We want exponentiation to the *right associative*, which means the expression `"2 ^ 3 ^ 4"` should parse as `"2 ^ (3 ^ 4)"`. First, we add the `^` operator to the lexer:

```csharp
var exponent = Match('^')
    .Transform(_ => new Token(TokenType.Exponentiation, "^"));
```

And then we can add it to the parser:

```csharp
.Add(Token(TokenType.Exponentiation), p => p
    .ProduceLeft(6, 5, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return (int)Math.Pow(left.Value, right);
    }))
```

Notice that we specify two binding power values: `6` and `5`. The first number is how strongly the operator binds to the value on the left, the second number is how strongly it binds on the right. A longer way of specifying the addition rule using both values would look like this:

```csharp
.Add(Token(TokenType.Addition), p => p
    .ProduceLeft(1, 2, (ctx, left, _) =>
    {
        var right = ctx.Parse();
        return left.Value + right;
    }))
```

An operator is left associative if the right binding power is greater than the left binding power. An operator is right associative if the left binding power is higher than the right. Look at an example expression like `"4 + 5 + 6"`. In this expression, the first `+` binds more strongly to the `5` than the second `+` does, because `+` binds more strongly on the right. This means that the parser evaluates `"4 + 5"` first, and then the `"+ 6"` next. For exponentiation in an expression like `"2 ^ 3 ^ 4"` the values are opposite. The second `^` binds to the `3` more strongly than the first one does, so the parser parses the expression as `2 ^ (3 ^ 4)`.

Now we're going to look at another element that the original example did not cover: parenthesis. Parenthesis treat a sub-expression together as a group as if it was a single number, and the contents of a parenthesis is evaluated first regardless of precedence. Since parenthesis do not interact with a value on the left, we can define them with a `.ProduceRight` rule, and use a `0` binding power the same as for numbers:

```csharp
.Add(Token(TokenType.OpenParen), p => p
    .ProduceRight(0, (ctx, _) =>
    {
        var contents = ctx.Parse();
        ctx.Expect(Token(TokenType.CloseParen));
        return contents;
    }))
```

The production rule calls `ctx.Parse()` to get the contents of the sub-expression, and then it calls `ctx.Expect(Token(TokenType.CloseParen))` to read the close parenthesis from the input stream. If the expected close parenthesis does not appear, the `.Expect()` method will cause the rule to fail. Finally it returns the parsed content value as the value of the parenthesis rule. The complete code of the Pratt expression parser is this:

```csharp
var expression = Pratt<int>(config => config
    .Add(number)
    .Add(Token(TokenType.Addition), p => p
        .ProduceLeft(1, (ctx, left, _) =>
        {
            var right = ctx.Parse();
            return left.Value + right;
        }))
    .Add(Token(TokenType.Subtraction), p => p
        .ProduceRight(9, (ctx, _) =>
        {
            var right = ctx.Parse();
            return -right;
        })
        .ProduceLeft(1, (ctx, left, _) =>
        {
            var right = ctx.Parse();
            return left.Value - right;
        }))
    .Add(Token(TokenType.Multiplication), p => p
        .ProduceLeft(3, (ctx, left, _) =>
        {
            var right = ctx.Parse();
            return left.Value * right;
        }))
    .Add(Token(TokenType.Division), p => p
        .ProduceLeft(3, (ctx, left, _) =>
        {
            var right = ctx.Parse();
            return left.Value / right;
        }))
    .Add(Token(TokenType.Exponentiation), p => p
        .ProduceLeft(6, 5, (ctx, left, _) =>
        {
            var right = ctx.Parse();
            return (int)Math.Pow(left.Value, right);
        }))
    .Add(Token(TokenType.OpenParen), p => p
        .ProduceRight(0, (ctx, _) =>
        {
            var contents = ctx.Parse(0);
            ctx.Expect(Token(TokenType.CloseParen));
            return contents;
        }))
);
```

See the [Pratt Calculator Example](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples/PrattCalculator) code in the test suite for the complete source code for this example. As an exercise for the reader, try implementing additional operators: The prefix `+` which indicates the value is positive, the suffix `!` to compute the factorial, or the `|4|` notation for computing the absolute value.