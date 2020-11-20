# Logical Parsers

Several parsers don't return a value, or the value they return doesn't matter. In these cases what's important is to see whether parsers succeed or fail, in different situations. These logical parsers are used to control the parse without returning values. 

## If Parser

The `If` parser checks some kind of predicate and executes one of two parsers depending on the result. The If parser is likely to be the entry point to most of the other parsers described in this file.

```csharp
var parser = new IfParser<TInput, TOutput>(predicate, onSuccess, onFailure);
var parser = If(predicate, onSuccess, onFailure);
var parser = onSuccess.If(predicate);
```

## Lookahead Parsers

Lookahead parsers attempt to match a pattern but consume no input. These are useful when you want to see what is coming next in the input sequence, and make decisions about what to parse based on that information. Because lookahead parsers explicitly consume no input, you can use them with other logical parsers to try and see what's coming ahead in the input sequence and decide which parsers to invoke based on that information.

### Negative Lookahead Parser

The `NegativeLookaheadParser` invokes a parser, but consumes no input. It returns success if the parser would not match, and returns failure if it would match. It is the opposite of the `PositiveLookaheadParser`. The negative lookahead parser does not return a value, only success or failure (If the inner parser fails, there is no result to return, and if the inner parser succeeds the negative lookahead parser would fail and return no value).

```csharp
var parser = new NegativeLookaheadParser<char>(innerParser);
var parser = NegativeLookahead(innerParser);
var parser = someParser.NotFollowedBy(innerParser);
```

### Positive Lookahead Parser

The `PositiveLookaheadParser` invokes a parser but consumes no input. It returns success and the parsed value if the parser would match, and returns failure if it would not. It is the opposite of the `NegativeLookaheadParser`. The `.FollowedBy()` extension method uses a positive lookahead parser to match a pattern, and only return a successful result if the first pattern is followed by the second pattern (but no input is consumed by the second pattern).

```csharp
var parser = new PositiveLookaheadParser<char>(innerParser);
var parser = PositiveLookahead(innerParser);
var parser = someParser.FollowedBy(innerParser);
```

The `PositiveLookahead` parser is functionally equivalent to a combination of `Choose` and `Empty`:

```csharp
var parser = innerParser.Choose(v => Empty<TValue>());
```

The `.FollowedBy()` method is functionally equivalent to a combination of `Rule` parser and `PositiveLookahead` parser:

```csharp
var parser = Rule(
    someParser,
    PositiveLookahead(innerParser),
    (first, followed) => first
);
```

Positive and Negative parsers can be created from one another using the Not parser. These two couples are the same (though the longer versions are less efficient):

```csharp
var parser = PositiveLookahead(predicate);
var parser = NegativeLookahead(Not(predicate));
```

```csharp
var parser = NegativeLookahead(predicate);
var parser = PositiveLookahead(Not(predicate));
```

## Combinatorial Parsers

### And Parser

The `And` parser is similar to the `Rule` parser except it does not return a value, it only checks if the parsers match, in order.

```csharp
var parser = new AndParser<TInput>(p1, p2, p3,...);
var parser = And(p1, p2, p3,...);
var parser = p1.And(p2, p3,...);
```

### Not Parser

The `Not` parser inverts the success value of the inner parser. When the inner parser succeeds, the Not parser fails. When the inner parser fails, the Not parser succeeds.

```csharp
var parser = new NotParser<TInput>(predicate);
var parser = Not(predicate);
var parser = predicate.Not();
```

### Or Parser

The `Or` parser is similar to the `First` parser except it does not return a value, it only checks if any parsers match.

```csharp
var parser = new OrParser<TInput>(p1, p2, p3,...)
var parser = Or(p1, p2, p3,...);
var parser = p1.Or(p2, p3,...);
```
