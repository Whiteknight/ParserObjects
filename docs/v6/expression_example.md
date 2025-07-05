# Expression Parsing Example

We would like to create a calculator routine which parses a string like `"1 + 2 * 3"` into a result following standard order-of-operations rules. The first operators we will initially support are "+" and "\*", where multiplication has *higher precedence* and both operators are *left-associative*. 

Up-to-date working code for this example is part of the [ParserObjects unit test suite](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples/ExprCalculator). Where this documentation may be out of date or contain typos, the code in the unit test suite is guaranteed to build, run and produce the described results.

## Note

This example is mostly to showcase some techniques involved in parser writing, and uses basic parser combinator techniques. Later we will see more powerful approaches to the same problem. 

## Parse Phases

There are typically two phases to a Parser:

1. **Lexical Analysis** a.k.a. "Lexing" or "Scanning" which breaks the input into meaningful tokens, and
2. **Parsing** which turns an ordered series of tokens into a produced output.

A parser can use both phases or it can be "Scannerless" and do everything all at once. A two-phase design lets us separate complexity and deal with some issues like whitespace and character matching separately from the higher-level grammar requirements. This example is going to show a two-phase design, but examples in other documentation pages will show scannerless designs as well.

For a two-phase design we need to create two `IParser` objects: One to read a sequence of characters and output a special `Token` object, and another parser to read in a sequence of `Token` objects and output our final numerical result.

First, let's create the `Token` class which is the interchange between the two phases:

```csharp
public record Token(TokenType Type, string Value);
```

## The Lexer

Now we can stub out a lexical grammar as a `IParser<char, Token>` that takes characters as input and returns `Token` as output.  

```csharp
using ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

public class LexicalGrammar
{
    public IParser<char, Token> CreateParser()
    {
        ...
    }
}
```

The first thing we want to parse are operators. We can use the `Match()` parser to match specific characters, and the `.Transform()` extension method to convert the value into a `Token`:

```csharp
var addition = Match('+')
    .Transform(c => new Token(TokenType.Addition, "+"));

var multiplication = Match('*')
    .Transform(c => new Token(TokenType.Multiplication, "*"));
```

**Exercises for the Reader**:
1. Create the `TokenType` enum
2. Tokenize subtraction `'-'` and division `'/'` operators

Next we want to define a parser for reading numbers. Numbers consist of one or more decimal digits. We create a `Match()` parser to match a digit character, then use the `.List()` extension method to get a list of several digit characters in a row, and finally `.Transform()` the list of digit characters into a Token with a string of digits as it's value:

```csharp
var digit =  Match(c => char.IsDigit(c));
var number = digit
    .List(atLeastOne: true)
    .Transform(c => new Token(TokenType.Number, new string(c.ToArray())));
```

**Exercises for the Reader**:
1. Add minimum and maximum number of digits to avoid overflowing a 32-bit int

We also need a parser to gather up whitespace. We want to gather and ignore all whitespace between tokens, so that our parser doesn't need to worry about.

```csharp
var whitespace = Match(c => char.IsWhitespace(c)).List();
```

Now let's start combining what we have above. First, let's create a parser that returns any of the defined token types:

```csharp
var anyToken = First(
    addition,
    multiplication,
    number
);
```

When we ask the lexer to get the next token, we want to ignore all whitespace at the current location and then return the first `Token` after that. Here's our implementation:

```csharp
var whitespaceAndToken = Rule(
    whitespace,
    anyToken,
    (ws, t) => t
);
```

All together:

```csharp
using ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

public class LexicalGrammar
{
    public IParser<char, Token> CreateParser()
    {
        var addition = Match('+')
            .Transform(c => new Token(TokenType.Addition, "+"));

        var multiplication = Match('*')
            .Transform(c => new Token(TokenType.Multiplication, "*"));

        var whitespace = Match(c => char.IsWhitespace(c)).List();

        var anyToken = First(
            addition,
            multiplication,
            number
        );

        var whitespaceAndToken = Rule(
            whitespace,
            anyToken,
            (ws, t) => t
        );

        return whitespaceAndToken;
    }
}
```

**Exercises for the Reader**:
1. Create unit tests for our `LexicalGrammar` which shows that passing various input strings to the lexer returns the `Token` types we expect.
2. Show that the parser `LexicalGrammar.CreateParser().ToList()` returns a list of all tokens in the input string.

## The Expression Grammar

We need to turn a series of `Token` into a final numerical result. Now we need to create a parser to parse an input sequence of `Token` into that result. Let's create our Expression grammar class:

```csharp
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<ExpressionGrammar.Token>;

public class ExpressionGrammar
{
    public IParser<Token, int> CreateParser()
    {
        ...
    }
}
```

Let's create a helpful static factory method to create a parser that matches a `Token` by type:

```csharp
public static IParser<Token, Token> Token(TokenType type)
    => Match(t => t.Type == type);
```

A number in the expression grammar is a Number token, transformed into an integer representation. We can use our `Token()` parser above to match a `Number` `Token` into an integer:

```csharp
var number = Token(TokenType.Number)
    .Transform(t => int.Parse(t.Value));
```

### Basic Operations

A basic rule for addition would have a number, followed by a `"+"`, followed by another number. Similar with multiplication. In BNF notation we can describe those rules like this:

```
addition := <number> '+' <number>
multiplication := <number> '*' <number>
```

We can turn those rules into parsers very simply using the `Rule()` parser:

```csharp
var addition = Rule(
    number,
    Token(TokenType.Addition),
    number,
    (left, op, right) = left + right
);

var multiplication = Rule(
    number,
    Token(TokenType.Multiplication),
    number,
    (left, op, right) = left * right
);
```

With this we can parse a string like "1 + 2" or "1 * 2" but we cannot parse something like "1 + 2 + 3" or "1 + 2 * 3". This is a good start, but we need to do better. 

### Precedence

Let's make our BNF a little bit more correct. A `multiplicative` is a number followed by one or more multiplications (a `'*'` and another number). Likewise an `additive` is a `multiplicative` followed by one or more additions (a `'+'` and another `multiplicative`). In BNF:

```
multiplicative := <number> ('*' <number>)*
additive := <multiplicative> ('+' <multiplicative>)*
```

These updated rules show clear *precedence*: we parse `*` with higher priority than `+` so the multiplications happen first and additions happen second.

In a first attempt we can try to model this with our `List` parser. `multiplicative` is a list of numbers separated by `*`, and an `additive` is a list of `multiplicative` separated by `+`:

```csharp
var multiplicative = List(
        number, 
        Token(TokenType.Multiplication), 
        atLeastOne: true
    )
    .Transform(values => MultiplyAllValuesTogether(values));

var additive = List(
        multiplicative, 
        Token(TokenType.Addition), 
        atLeastOne: true
    )
    .Transform(values => values.Sum());
```

This works for our simple examples, but there's a problem: What happens when we add in division (which has the same precedence as multiplication) or subtraction (which has the same precedence as addition)? There's no clear way to do that, because the `List()` parser is returning the numbers in arrays but does not include which operators were between them.

What we want instead is something that can recurse and execute operations in the correct order. The `LeftApply()` parser is designed to do exactly this: parse left-associative operators.

```csharp
var multiplicative = LeftApply(
    number,
    left => Rule(
        left,
        Token(TokenType.Multiplication),
        number,
        (l, op, r) => l * r
    )
);

var additive = LeftApply(
    multiplicative,
    left => Rule(
        left,
        Token(TokenType.Addition),
        multiplicative,
        (l, op, r) => l + r
    )
);
```

`LeftApply()` is a little tricky so let's step through it. First we parse a "start value". In this case, we're parsing `number`. We will call that the "left" value. Then, in a loop, we are going to take the current "left" value, use that in a `Rule()` with an operator and another `number`, and then treat the result of that as the next "left" value. 

Since our concern was that we couldn't easily add division and subtraction operations previously, let's show how easy it is to add them now:

```csharp
var multiplicative = LeftApply(
    number,
    left => First(
        Rule(
            left,
            Token(TokenType.Multiplication),
            number,
            (l, op, r) => l * r
        ),
        Rule(
            left,
            Token(TokenType.Division),
            number,
            (l, op, r) => l / r
        )
    )
);
var additive = LeftApply(
    multiplicative,
    left => First(
        Rule(
            left,
            Token(TokenType.Addition),
            multiplicative,
            (l, op, r) => l + r
        ),
        Rule(
            left,
            Token(TokenType.Subtraction),
            multiplicative,
            (l, op, r) => l - r
        )
);
```

The `First()` parser inside each `LeftApply()` parser will take the first match from the possibilities: multiplication and division in the first level, addition and subtraction in the second level.

Together our expression grammar looks like this:

```csharp
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<ExpressionGrammar.Token>;

public static class ExpressionGrammar
{
    public static IParser<Token, Token> Token(TokenType type)
        => Match(t => t.Type == type);

    public IParser<Token, int> CreateParser()
    {
        var number = Token(TokenType.Number)
            .Transform(t => int.Parse(t.Value));

        var multiplicative = LeftApply(
            number,
            left => First(
                Rule(
                    left,
                    Token(TokenType.Multiplication),
                    number,
                    (l, op, r) => l * r
                ),
                Rule(
                    left,
                    Token(TokenType.Division),
                    number,
                    (l, op, r) => l / r
                )
            )
        );
        var additive = LeftApply(
            multiplicative,
            left => First(
                Rule(
                    left,
                    Token(TokenType.Addition),
                    multiplicative,
                    (l, op, r) => l + r
                ),
                Rule(
                    left,
                    Token(TokenType.Subtraction),
                    multiplicative,
                    (l, op, r) => l - r
                )
        );

        var expression = additive;
        return expression;
    }
}
```

## The Calculator

Our calculator class needs to take an input string, turn it into a sequence of tokens, and then use those tokens to calculate the final result. 

First, we'll create our lexer and use the lexer to define a sequence of tokens:

```csharp
// Our input, a sequence of characters representing an equation
var characterSequence = FromString(equation);

// Get the lexical grammar, and use it to create a sequence of tokens
var lexicalParser = LexicalGrammar.CreateParser();
var tokenSequence = lexicalParser.ToSequence(characterSequence);
```

Second, we can create our expression parser and use that to produce a result:

```csharp
var expressionParser = ExpressionGrammar.CreateParser();
var result = expressionParser.Parse(tokenSequence);
```

All together:

```csharp
public class Calculator
{
    public int Calculate(string equation)
    {
        // Turn the input string into a sequence of characters
        var characterSequence = FromString(equation);

        // Get the lexical grammar, and use it to create a sequence of tokens
        var lexicalParser = LexicalGrammar.CreateParser();
        var tokenSequence = lexicalParser.ToSequence(characterSequence);

        // Get the expression grammar and parse the token sequence into a result
        var expressionParser = ExpressionGrammar.CreateParser();
        var result = expressionParser.Parse(tokenSequence);

        // Get the value of the result
        return result.Value;
    }
}
```

This is a very good start, but there is no error handling. 
1. An input string like `"1 GARBAGE 2"` will return a value of `1`, because the parser does not recognize `GARBAGE` and so it stops.
2. An input string like `"1 + "` will also return a value of `1` because the `LeftApply` rule to match `+` followed by `number` fails so only the first number is returned.
 
**Exercises for the Reader:**
1. Unit tests to show that basic expressions with the operations '+', '-', '*' and '/' work as expected
2. Unit tests to show that the above error conditions **fail** for the current parser design.

## Catch Some Problems

New requirements:

1. We want to parse the entire input. It is an error if there are unparsed characters remaining in the input sequence.
2. We want to alert the user when the input expression does not follow grammar rules.

### The End

To make sure we parse the entire input string, let's change the rules of our grammar to require an explicit End of Input using the `IsEnd()` parser:

```csharp
var expression = Rule(
    additive,
    IsEnd(),  
    (add, end) => add
);
```

The `IsEnd` parser will attempt to find the end of input, and will fail if it does not see it. Now we know that if the `additive` rule is not able to consume all input, the `IsEnd()` will fail and the entire expression will fail.

Inside our calculator, we can detect a parse failure and alert the user with both a message of what the failure was and where the failure happened:

```csharp
var result = expressionParser.Parse(tokenSequence);
if (!result.Success)
    throw new ExpressionParseFailureException(result.ErrorMessage, result.Location);
```

### Dangling Operators

The above end-of-input parsing will tell us when there's an error, but the error message will be something generic like "expected end of input" and won't tell us about other errors which may be more specific. For example an input string like `"1 + "` has an operator but no number following it, so the `+` operator is "dangling". When we see an operator *we must see a number after it*. Otherwise we want a specific error message to tell us that we're expecting a number.

In BNF:

```
multiplicative := <number> ('*' (<number> | <throwError>))*
additive := <multiplicative> ('+' (<multiplicative> | <throwError>))*
```

First, let's start by defining a new factory method for a parser which throws an error with a helpful message:

```csharp
public static IParser<Token, int> ThrowError(string message)
    => Produce<Token, int>(
        t => throw new ExpressionParseFailureException(message, t.CurrentLocation)
    );
```

The `Produce` parser is expected to return a result value, but it does that by executing an arbitrary lambda function. We can use `Produce` to throw an exception at any point in the parser we want. In this case we are throwing an exception with our error message and the current location in the input sequence where the error occurred. With that, we can create new rules to require a specific rule to succeed or throw an error:

```csharp
var requiredNumber = First(
    number,
    ThrowError("Expected number")
);

var requiredMultiplicative = First(
    multiplicative,
    ThrowError("Expected multiplicative")
);
```

And with that, we can update our existing parsers:

```csharp
var multiplicative = LeftApply(
    number,
    left => First(
         Rule(
            left,
            Token(TokenType.Multiplication),
            requiredNumber,     // Number or throw
            (l, op, r) => l * r
        ),
        Rule(
            left,
            Token(TokenType.Division),
            requiredNumber,     // Number or throw
            (l, op, r) => l / r
        )
    )
);

var additive = LeftApply(
    multiplicative,
    left => First(
        Rule(
            left,
            Token(TokenType.Addition),
            requiredMultiplicative,     // multiplicative or throw
            (l, op, r) => l + r
        ),
        Rule(
            left,
            Token(TokenType.Subtraction),
            requiredMultiplicative,     // multiplicative or throw
            (l, op, r) => l - r
        )
);
```

What we're doing here is saying that if we see an operator, we *must see something on the right side of that operator*. We can either have the left value by itself, or the set of (left, operator, right). There's no other permissable combination. If we don't see what we need, throw an error and tell the user what went wrong and where.

## Continuing On

This is just a simple example and there are many gaps left to fill as an exercise for the reader. A real expression parser for a calculator or even a programming language like C# would be significantly more complicated with many more levels of precedence each with several additional operators. There's also the problem that our error-handling mechanism bails out with an exception at the very first error, and we might like it to try to continue the parse until the end and then report all possible errors at once. 

And then there are some of the other details, we might like to be able to handle negative numbers, or numbers with decimal points. And we definitely want to support parenthesis to enforce grouping if we want to order operations differently. 

**Exercises for the Reader:**
1. Add the exponentiation operator `^` which calls `Math.Pow()` to raise one number to the power of another number. This has a higher precedence than `multiplicative`.
2. Add negative numbers. Hint: that we cannot do this in the lexer, because there's no way to differentiate there whether `"- number"` is a subtraction or a negation, so we must handle it in our expression parser.
3. Upgrade to use decimal numbers. A `number` in the lexer will have the BNF `number := <digits> ('.' <digits>)?` Hint: the `First()` parser can help here.

## Summary

This example shows usage of many of the "classical" combinators like `Rule`, `First`, and the `LeftApply` parser. It is mostly for showing the general method of parser construction. A better example of parsing mathematical expressions [uses the Pratt parser instead](prattexpr_example.md). 
