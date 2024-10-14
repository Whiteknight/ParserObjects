# Expression Parsing Example

We would like to create a calculator routine which parses a string like `"1 + 2 * 3"` into a result following standard order-of-operations rules. The first operators we will support are "+" and "*", where multiplication has higher precedence and both operators are left-associative. 

Up-to-date working code for this example is part of the [ParserObjects unit test suite](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples/ExprCalculator). Where this documentation may be out of date or contain typos, the code in the unit test suite is guaranteed to build, run and produce the described results.

## Note

This example is mostly to showcase some techniques involved in parser writing, and uses the `LeftApply` parser to handle infix operators `+`, `-`, `*`, and `/`. There are other, arguably better, approaches to parsing mathematical expressions, which we will explore in subsequent pages.

## The Lexical Grammar

A "parser" here can be composed of one phase ("scannerless") or can be broken down into two phases: Lexical Analysis (breaking the input into tokens) and Parsing (arranging the tokens according to a grammar).

A two-phase design lets us separate complexity and deal with some issues (whitespace, low-level character matching, etc) separately from the more important grammar issues. That's what we're going to do here, but it is by no means a requirement or even a suggestion for you to follow in your own parsers. We have to pick one approach for the purposes of this example, so we've chosen the one that helps us explain and demonstrates a variety of concepts. This means we're going to create two `IParser` objects: One to read a sequence of characters and output a special `Token` object, and another parser to read in a sequence of `Token` objects and output our final numerical result.

First, let's create the `Token` class (We should put in a `.ToString()` method to help with debugging later, but I'll leave that and the definition of the `TokenType` enum as an exercise for the reader):

```csharp
public class Token
{
    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public TokenType Type { get; }
    public string Value { get; }
}
```

Now we can stub out a lexical grammar as a `IParser<char, Token>` that takes characters as input and returns `Token`.  

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

First thing we want to parse are operators. We can use the `Match()` parser to match specific characters, and the `.Transform()` extension method to convert the value into a `Token`:

```csharp
var addition = Match('+')
    .Transform(c => new Token(TokenType.Addition, "+"));
var multiplication = Match('*')
    .Transform(c => new Token(TokenType.Multiplication, "*"));
```

Next we want to define a parser for reading numbers. Numbers consist of one or more decimal digits. We create a `Match()` parser to match a digit character, then use the `.List()` extension method to get a list of several digit characters in a row, and finally `.Transform()` the list of digit characters into a Token with a string of digits as it's value:

```csharp
var number = Match(c => char.IsDigit(c))
    .List(atLeastOne: true)
    .Transform(c => new Token(TokenType.Number, new string(c.ToArray())));
```

We also need a parser to gather up whitespace, but we don't need to transform it because we don't care about the output value. We only want to consume all whitespace between other tokens:

```csharp
var whitespace = Match(c => char.IsWhitespace(c)).List();
```

Our first rule will attempt to match all the known token types, the second rule will match optional whitespace followed by a token, but only return the token:

```csharp
var anyToken = First(
    addition,
    multiplication,
    number
);

var whitespaceAndToken = Rule(
    whitespace,
    anyToken,
    (ws, t) => t
)
```

Unit tests at this point can show that given an input string of numbers and operators, this tokenizer returns all the correct tokens in order. This is left as an exercise for the reader.

## The Expression Grammar

We do not need a parser which produces a stream of Tokens, we need a Parser which takes Tokens as input and returns a numerical value. First, let's create our Expression grammar class:

```csharp
using static ParserObjects.Parsers<ExpressionGrammar.Token>;

public class ExpressionGrammar
{
    public IParser<Token, int> CreateParser()
    {
        ...
    }
}
```

Let's first create a helpful static method for creating a parser to match tokens by type:

```csharp
using static ParserObjects.Parsers.Parsers<Token>;

public static class TokenParserExtension
{
    public static IParser<Token, Token> Token(TokenType type)
        => Match(t => t.Type == type);
}
```

A number in the expression grammar is a Number token, transformed into an integer representation:

```csharp
var number = Token(TokenType.Number)
    .Transform(t => int.Parse(t.Value));
```

A basic rule for addition would have a number, followed by a "+", followed by another number. Similar with multiplication:

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

With this we can parse a string like "1 + 2" or "1 * 2" but we can't parse something like "1 + 2 + 3" or "1 + 2 * 3" and we don't handle precedence or recursion at all.

## Precedence

Let's look at some pseudo-BNF for an expression grammar where multiplication has higher precedence than addition:

```
multiplicative := <number> ('*' <number>)*
additive := <multiplicative> ('+' <multiplicative>)*
```

This is a fine representation, we could use a `List()` parser with a separator to parse them:

```csharp
var multiplicative = List(number, Token(TokenType.Multiplication), atLeastOne: true)
    .Transform(values => MultiplyAllValuesTogether(values));
var additive = List(multiplicative, Token(TokenType.Addition), atLeastOne: true)
    .Transform(values => values.Sum());
```

This works for our simple examples, but it's going to fail when our grammar gets any more complicated or when the order of operations matters. What we want is something that can recurse and execute operations in the correct order. We can use the `LeftApply()` parser for this:

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

See how the declaration of this parser starts to look a little bit like the pseudo-BNF we described above: A `multiplicative` starts with a number followed by a series of operators and numbers, etc. Each `LeftApply` parser represents a precedence level with left-associative operators. Later, if we want to extend our calculator to handle subtraction and division, we can add the necessary declarations to our lexical grammar and extend our expression grammar like this:

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

## The Calculator

Our calculator class needs to take an input, turn it into a sequence of tokens, and then use those tokens to calculate the final result. 

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

Obviously there are some steps missing from the above, we don't have any error handling or any indication for when something isn't right, or when the input isn't entirely consumed. For example, a string like "1 + 2 GARBAGE" would parse just fine and would consume the "1 + 2" part and return 3, but the input sequence would still have data in it. Similarly a string like "1 + 2 *" with the dangling asterisk at the end would parse fine and return the value 3 but there would still be an asterisk in the input sequence unparsed.

## Catch Some Problems

We want to make sure we parse the whole input, and we want to alert the user that they are not following the rules of the grammar. First, we'll change the return value of our Expression Grammar to make sure we see end-of-input:

```csharp
var requiredEnd = IsEnd();
var expression = Rule(
    additive,
    requiredEnd,  
    (add, end) => add
);
```

The `IsEnd` parser will attempt to find the end, and returns a boolean true value if it is found. Otherwise, it returns false. 

We can use a similar technique in our expression parser. We can see a pattern "`<number>`" or a pattern "`<number> <operator> <number>`" but not just "`<number> <operator>`". First, let's start by defining a new method for a parser which throws an error:

```csharp
public static IParser<Token, int> ThrowError(string message)
    => Produce<Token, int>(t => throw new Exception($"{message} at {t.CurrentLocation}"));
```

The `Produce` parser is expected to return a result value, but it does that by executing an arbitrary lambda function. We can use that to throw an exception at any point in the parser we want. Now we can add some error handling to our grammar:

```csharp
var requiredNumber = First(
    number,
    ThrowError("Expected number")
);
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
var requiredMultiplicative = First(
    multiplicative,
    ThrowError("Expected multiplicative")
);
var additive = LeftApply(
    multiplicative,
    left => First(
        Rule(
            left,
            Token(TokenType.Addition),
            requiredMultiplicative,
            (l, op, r) => l + r
        ),
        Rule(
            left,
            Token(TokenType.Subtraction),
            requiredMultiplicative,
            (l, op, r) => l - r
        )
);
```

What we're doing here is saying that if we see an operator, we must see something on the right side of that operator. We can either have the left value by itself, or the set of (left, operator, right). There's no other permissable combination. Not having the correct right-side-value will result in an exception being thrown with a helpful error message.

## Continuing On

This is just a simple example and there are many gaps left to fill as an exercise for the reader. A real expression parser for a calculator or even a programming language like C# would be significantly more complicated with many more levels of precedence each with several additional operators. There's also the problem that our error-handling mechanism bails out with an exception at the very first error, and we might like it to try to continue the parse until the end and then report all possible errors at once. 

And then there are some of the other details, we might like to be able to handle negative numbers, or numbers with decimal points. And we definitely want to support parenthesis to enforce grouping if we want to order operations differently. 

This example, using many of the "classical" combinators like `Rule` and `First`, and the `LeftApply` parser, is mostly for showing the general method of parser construction. A better example of parsing mathematical expressions [uses the Pratt parser instead](prattexpr_example.md). 
