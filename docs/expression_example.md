# Expression Parsing Example

We would like to create a calculator routine which parsers a string like "1 + 2 * 3" into a result following standard order-of-operations rules. The first operators we will support are "+" and "*", where multiplication has higher precidence and both operators are left-associative. (Notice that I'm not recommending this as the "best" solution to this particular problem, you should probably consider something like [The Shunting Yard algorithm](https://en.wikipedia.org/wiki/Shunting-yard_algorithm) for best results).

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

Now we can put together a lexical grammar as a `IParser<char, Token>` that takes characters as input and returns `Token`s.

```csharp
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
var addition = Match<char>('+')
    .Transform(c => new Token(TokenType.Addition, "+"));
var multiplication = Match<char>('*')
    .Transform(c => new Token(TokenType.Multiplication, "*"));
```

Now we want to define a parser for reading numbers. Numbers can be multiple digits, at least one, and are in decimal. We create a parser to match a digit character, then get a list of subsequent digits, and then transform the list of digit characters into a Token with a string of digits as it's value:

```csharp
var number = Match<char>(c => char.IsDigit(c))
    .List(atLeastOne: true)
    .Transform(c => new Token(TokenType.Number, new string(c.ToArray())));
```

We also want to gather up whitespace, but we don't need to transform it because we don't care about the output value:

```csharp
var whitespace = Match<char>(c => char.IsWhitespace(c)).ToList();
```

Now we want to start creating our output rules. Our first rule will attempt to match all the known token types, the second rule will match optional whitespace followed by a token, but only return the token:

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

## The Expression Grammar

Now we want to write a Parser which takes Tokens as input and returns a numerical value. First, let's create our Expression grammar class:

```csharp
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
public static class TokenParserExtension
{
    public static IParser<Token, Token> Token(TokenType type)
        => Match<Token>(t => t.Type == type);
}
```

A number in the expression grammar is a number token, transformed into an integer representation:

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

This is all well and good, we can parse a string like "1 + 2" or "1 * 2" but we can't parse something like "1 + 2 + 3" or "1 + 2 * 3" and we don't handle precidence at all.

## Precidence

Let's look at some pseudo-BNF for an expression grammar where multiplication has higher precidence than addition:

```
multiplicative := <additive> ('*' <additive>)*
additive := <number> ('+' <number>)*
```

This is a fine representation, we could use a separated list parser to parse them:

```csharp
var multiplicative = SeparatedList(number, Token(TokenType.Multiplicative), atLeastOne: true)
    .Transform(values => MultiplyAllValuesTogether(values));
var additive = SeparatedList(multiplicative, Token(TokenType.Additive), atLeastOne: true)
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

Each `LeftApply` parser represents a precidence level. Later, if we want to extend our calculator to handle subtraction and division, we can add the necessary declarations to our lexical grammar and extend our expression grammar like this:

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
    public int Calculate(string syntax)
    {
        var characterSequence = new StringCharacterSequence(syntax);
        var lexicalParser = LexicalGrammar.CreateParser();
        var tokenSequence = lexicalParser.ToSequence(characterSequence);
        var expressionParser = ExpressionGrammar.CreateParser();
        var result = expressionParser.Parse(tokenSequence);
        return result.Value;
    }
}
```

Obviously there are some steps missing from the above, we don't have any error handling or any indication for when something isn't right, or when the input isn't entirely consumed. For example, a string like "1 + 2 GARBAGE" would parse just fine and would consume the "1 + 2" part and return 3, but the input sequence would still have data in it. Similarly a string like "1 + 2 *" with the dangling asterisk at the end would parse fine and return the value 3 but there would still be an asterisk in the input sequence unparsed.

## Catch Some Problems

We want to make sure we parse the whole input, and we want to alert the user that they are not following the rules of the grammar. First, we'll change the return value of our Expression Grammar to make sure we see end-of-input:

```csharp
var expression = Rule(
    additive,
    End<char>(),
    (add, end) => add
);
```

Now if we don't see end-of-input the parse will fail. We could also do something like this:
```csharp
var requiredEnd = First(
    End<Token>(),
    Produce<Token, object>(t => throw new Exception($"Expected end of input but found {t.Peek()} at {t.CurrentLocation}"))
);
var expression = Rule(
    additive,
    requiredEnd,  
    (add, end) => add
);
```

The `First` parser will attempt to find the end, and returns immediately if it does. If it doesn't see the end, however, it will invoke the `Produce` parser which will throw that exception.

We can use a similar technique in our expression parser. We can see a pattern "<number>" or a pattern "<number> <operator> <number>" but not just "<number> <operator>".

```csharp
var requiredNumber = First(
    number,
    Produce<Token, object>(t => throw new Exception($"Expected number but found {t.Peek()} at {t.CurrentLocation}"))
)
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
    number,
    Produce<Token, object>(t => throw new Exception($"Expected multiplicative but found {t.Peek()} at {t.CurrentLocation}"))
)
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

## Continuing On

This is just a simple example and there are many gaps left as an exercise for the reader. A real expression parser for a calculator or even a programming language like C# would be significantly more complicated with many more levels of precidence each with several additional operators. There's also the problem that our error-handling mechanism bails out with an exception at the very first error, and we might like it to try to continue the parse until the end and then report all possible errors at once. 

And then there are some of the other details, we might like to be able to handle negative numbers, or numbers with decimal points. And we definitely want to support parenthesis to enforce grouping if we want to order operations differently. We will look at all these examples in later pages.