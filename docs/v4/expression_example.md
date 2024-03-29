# Expression Parsing Example

We would like to create a calculator routine which parses a string like "1 + 2 * 3" into a result following standard order-of-operations rules. The first operators we will support are "+" and "*", where multiplication has higher precedence and both operators are left-associative. (Notice that I'm not recommending this as the "best" solution to this particular problem, you should probably consider something like [The Shunting Yard algorithm](https://en.wikipedia.org/wiki/Shunting-yard_algorithm) for best results).

Up-to-date working code for this example is part of the [ParserObjects unit test suite](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples/ExprCalculator). Where this documentation may be out of date or contain typos, the code in the unit test suite is guaranteed to build, run and produce the described results.

## Note

This example is mostly to showcase some techniques involved in parser writing, and uses the `LeftApply` parser to handle infix operators `+`, `-`, `*`, and `/`. There are other, arguably better, approaches to parsing mathematical expressions, which we will try to document elsewhere.

## The Lexical Grammar

We could use a scannerless design, but we want to be able to ignore whitespace between tokens and separate some of the low-level character-handling routines into a separate class. First, let's create a `Token` class (We should put in a `.ToString()` method to help with debugging later, but I'll leave that and the definition of the `TokenType` enum as an exercise for the reader):

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
using static ParserObjects.ParserMethods<char>;

public class LexicalGrammar
{
    public IParser<char, Token> CreateParser()
    {
        ...
    }
}
```

First thing we want to parse are operators. They are simple because each is only one character, and we want to transform these characters into appropriate tokens:

```csharp
var addition = Match('+')
    .Transform(c => new Token(TokenType.Addition, "+"));
var multiplication = Match('*')
    .Transform(c => new Token(TokenType.Multiplication, "*"));
```

Next we want to define a parser for reading numbers. Numbers consist of one or more decimal digits. We create a parser to match a digit character, then get a list of subsequent digits, and finally transform the list of digit characters into a Token with a string of digits as it's value:

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
using static ParserObjects.ParserMethods<ExpressionGrammar.Token>;

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
using static ParserObjects.Parsers.ParserMethods<Token>;

public static class TokenParserExtension
{
    public static IParser<Token, Token> Token(TokenType type)
        => Match(t => t.Type == type);
}
```

A number in the expression grammar is a Number token, transformed into an integer representation:

```csharp
var number = Token(TokenType.Number).Transform(t => int.Parse(t.Value));
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

With this we can parse a string like "1 + 2" or "1 * 2" but we can't parse something like "1 + 2 + 3" or "1 + 2 * 3" and we don't handle precedence at all.

## Precedence

Let's look at some pseudo-BNF for an expression grammar where multiplication has higher precedence than addition:

```
multiplicative := <number> ('*' <number>)*
additive := <multiplicative> ('+' <multiplicative>)*
```

This is a fine representation, we could use a separated list parser to parse them:

```csharp
var multiplicative = SeparatedList(number, Token(TokenType.Multiplication), atLeastOne: true)
    .Transform(values => MultiplyAllValuesTogether(values));
var additive = SeparatedList(multiplicative, Token(TokenType.Addition), atLeastOne: true)
    .Transform(values => values.Sum());
```

This works for our simple examples, but it's going to fail when our grammar gets any more complicated or when the order of operations matters. What we want is something that can recurse and execute operations in the correct order. We can use the `LeftApply` parser for this:

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

Each `LeftApply` parser represents a precedence level with left-associative operators. Later, if we want to extend our calculator to handle subtraction and division, we can add the necessary declarations to our lexical grammar and extend our expression grammar like this:

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
        var characterSequence = new StringCharacterSequence(equation);

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
var requiredEnd = If(
    End(),
    Produce(() => true)
);
var expression = Rule(
    additive,
    requiredEnd,  
    (add, end) => add
);
```

The `If` parser will attempt to find the end, and returns a dummy boolean value if it is found. Otherwise, it fails. 

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

What we're doing here is saying that if we see an operator, we must see something on the right side of that operator. We can either have the left value by itself, or the set of (left, operator, right). There's no other permissable combination. Not having the correct right-side-value will result in an exception.

## Continuing On

This is just a simple example and there are many gaps left to fill as an exercise for the reader. A real expression parser for a calculator or even a programming language like C# would be significantly more complicated with many more levels of precedence each with several additional operators. There's also the problem that our error-handling mechanism bails out with an exception at the very first error, and we might like it to try to continue the parse until the end and then report all possible errors at once. 

And then there are some of the other details, we might like to be able to handle negative numbers, or numbers with decimal points. And we definitely want to support parenthesis to enforce grouping if we want to order operations differently. 

This example, using many of the "classical" combinators like `Rule` and `First`, and the `LeftApply` parser, is mostly for showing the general method of parser construction. A better example of parsing mathematical expressions [uses the Pratt parser instead](prattexpr_example.md). 
