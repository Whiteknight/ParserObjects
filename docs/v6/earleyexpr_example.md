# Earley Expression Parsing Example

In the first [expression parser example](expression_example.md) we showed an expression parser with basic combinator types, and then we showed the same basic example using the [Pratt parser](pratexpr_example.md). Now we are going to show the same thing with the Earley parser. Using the Lexical Grammar, Token type and helper methods from the previous example, we can create an expression-parser using the Earley parser to see how it works.

First we start by stubbing out our parser:

```csharp
var parser = Earley<int>(symbols => {
    ...
});
```

The Earley parse algorithm is a context-free recursive parsing algorithm which is extremely powerful. All that power comes with some drawbacks, however. We will see it in this example.

Let's start by defining a few parsers to help with getting the tokens we need:

```csharp
var plus = Token(TokenType.Addition);
var star = Token(TokenType.Multiplication);

var number = Token(TokenType.Number)
    .Transform(t => int.Parse(t.Value));
```

The BNF for our grammar is going to look like this:

```
Expr ::= <number>
Expr ::= <Expr> '+' <Expr>
Expr ::= <Expr> '*' <Expr>
```

Notice how unlike in the combinators example and the Pratt example, the BNF rules here are *recursive*. That is, the symbol `Expr` appears in it's own definition. Right-recursion (where the symbol appears at the end of it's own definition) is possible with many parsers, but Left-recursion (where the symbol appears at *the beginning* of it's own definition) is not. With the Earley parser, both recursion scenarios are possible and *easy*.

Create an Expression symbol in the Earley parser which combines these things together:

```csharp
var parser = Earley<int>(symbols => {
    var expr = symbols.New("Expression");

    // Expr ::= <number>
    expr.Rule(number, n => n);

    // Expr ::= <Expr> '+' <Expr>
    expr.Rule(expr, plus, expr, (l, _, r) => l + r);

    // Expr ::= <Expr> '*' <Expr>
    expr.Rule(expr, star, expr, (l, _, r) => l * r);

    return expr;
});
```

The `return expr;` line tells the Earley parser which symbol is considered the "Start Symbol", or the top-level symbol for the grammar. The parser returns a successful result any time the Start Symbol is matched. Now we create a Calculator harness to hold this. The calculator is modified to return a list of results since Earley is a multi parser:

```csharp
public class Calculator
{
    public IReadOnlyList<int> Calculate(string equation)
    {
        // Turn the input string into a sequence of characters
        var characterSequence = new StringCharacterSequence(equation);

        // Get the lexical grammar, and use it to create a sequence of tokens
        var lexicalParser = LexicalGrammar.CreateParser();
        var tokenSequence = lexicalParser.ToSequence(characterSequence);

        // Get the expression grammar and parse the token sequence into a result
        var expressionParser = ExpressionGrammar.CreateParser();
        var multiResult = expressionParser.Parse(tokenSequence);

        // Get the successful values of the result
        return result.Results
            .Where(r => r.Success)
            .Select(r => r.Value)
            .ToList();
    }
}
```

When we run the code `Calculator.Calculate("4*5+6")` we get the following results:

* `4` (4)
* `20` (4*5)
* `26` ((4*5)+6)
* `44` (4*(5+6))

This is an interesting result because it shows us all the ways the formula could be interpreted. We can stop the parse early and only return partial results ("4" and "4*5") or we can parse the whole thing and end up with an ambiguous result. At this point we have to decide how we want to proceed.

First we probably want to get rid of the two partial solutions. We want the parser to parse the entire input. We can do that by detecting `End` and creating a new symbol "S" which is the customary name for the start symbol:

```csharp
var eof = IsEnd();
var s = symbols.New("S");
s.Rule(expr, eof, (value, _) => value);
return s;
```

This allows us to weed out the two partial results, but it doesn't help to disambiguate the remaining two. For that we need to insert precedence rules. Let's try to add some precedence to our BNF rules:

```
Term ::= <number>
Term ::= <Term> '*' <Term>
Expr ::= <Term>
Expr ::= <Expr> '+' <Expr>
```

He we can see we want to parse `Term` first and then an `Expr` is a sum of `Term`. This is similar to our `multiplicative` and `additive` rules from the first example. Writing this in the Earley parser gives us:

```csharp
var term = symbols.New<int>("Term");
term.Rule(number, n => n);
term.Rule(term, star, term, (l, _, r) => l * r);

var expr = symbols.New<int>("Expr");
expr.Rule(term, t => t);
expr.Rule(expr, plus, expr, (l, _, r) => l + r);
```

This works the way we expect. An input `"4*5+6"` now correctly parses as `"(4*5)+6" = 26`. This is a good success. Now we can start to add more operators:

```
Term ::= <number>
Term ::= <Term> '*' <Term>
Term ::= <Term> '/' <Term>
Expr ::= <Term>
Expr ::= <Expr> '+' <Expr>
Expr ::= <Expr> '-' <Expr>
```

```csharp
var term = symbols.New<int>("Term");
term.Rule(number, n => n);
term.Rule(term, star, term, (l, _, r) => l * r);
term.Rule(term, divide, term, (l, _, r) => l / r);

var expr = symbols.New<int>("Expr");
expr.Rule(term, t => t);
expr.Rule(expr, plus, expr, (l, _, r) => l + r);
expr.Rule(expr, minus, expr, (l, _, r) => l - r);
```

Now the input `4*5+6/3-2` should be parsed as `"(4*5)+(6/3)-2" = 20` but when we run the code we get two values again. The number `20` is in the list twice! It turns out that the Earley algorithm arrived at the same result in two different ways, `<Expr> + <Expr>` and `<Expr> - <Expr>`. The addition and subtraction have the same precedence level and can be evaluated in either order depending whether we recurse `<Expr>` on the left or on the right. So, let's remove the option and only recurse on the left. These operators are typically left-associative, after all:

```
Primary ::= <number>
Term ::= <Primary>
Term ::= <Term> '*' <Primary>
Term ::= <Term> '/' <Primary>
Expr ::= <Term>
Expr ::= <Expr> '+' <Term>
Expr ::= <Expr> '-' <Term>
```

```csharp
var primary = symbols.New("Primary")
    .Rule(number, n => n);

var term = symbols.New<int>("Term");
term.Rule(primary, n => n);
term.Rule(term, star, primary, (l, _, r) => l * r);
term.Rule(term, divide, primary, (l, _, r) => l / r);

var expr = symbols.New<int>("Expr");
expr.Rule(term, t => t);
expr.Rule(expr, plus, term, (l, _, r) => l + r);
expr.Rule(expr, minus, term, (l, _, r) => l - r);
```

Now when we run the calculator example we get the one answer we expect, `20`. 

Notice that this grammar we have created is still *left-recursive*, which is very difficult to express and implement in other parser types. With the Earley algorithm we are able to create a left-recursive grammar in only a few lines of code, and are able to follow a very natural development path without worrying about how recursion was being implemented. As an exercise for the reader, try to re-implement the above grammar using right recursion instead.

The Earley parser is very powerful, but sometimes all that extra power gives you some unexpected problems. You may find that if your rules are not precise enough you will end up with more results than you expect, and you will have to do some work to narrow the grammar down to give you just the values that you want.