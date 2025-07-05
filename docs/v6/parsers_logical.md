# Logical Parsers

These parsers combine results together in logical ways and may or may not consume values or return results.

## If Parser

The `If` parser checks some kind of predicate parser for success and executes one of two parsers depending on the result. The predicate will be allowed to consume data, and that data will not be available to either of the resulting parsers. If the `onFailure` option is omitted, the parser will return a `Fail` result when the predicate fails.

```csharp
var parser = If(predicate, onSuccess);
var parser = If(predicate, onSuccess, onFailure);
var parser = onSuccess.If(predicate);
var parser = predicate.Then(onSuccess);
```

The If parser can be useful to turn a parser which does not return output into one that does return a value:

```csharp
var parser = If(End(), Produce(() => true));
```

## Lookahead Parsers

Lookahead parsers attempt to match a pattern but consume no input. These are useful when you want to see what is coming next in the input sequence, and make decisions about what to parse based on that information. Because lookahead parsers explicitly consume no input, you can use them with other logical parsers to try and see what's coming ahead in the input sequence and decide which parsers to invoke based on that information.

### Negative Lookahead Parser

The `NegativeLookahead` Parser invokes a parser, but consumes no input. It returns success if the parser would not match, and returns failure if it would match. It is the opposite of the `PositiveLookahead` parser. The negative lookahead parser does not return a value, only success or failure (If the inner parser fails, there is no result to return, and if the inner parser succeeds the negative lookahead parser would fail and return no value). The NegativeLookahead parser has an extension method form called `.NotFollowedBy()`:

```csharp
var parser = NegativeLookahead(innerParser);
var parser = someParser.NotFollowedBy(innerParser);
```

NotFollowedBy is equivalent to:

```csharp
var parser = Rule(
    someParser,
    NegativeLookahead(innerParser),
    (r1, _) => r1
);
```

### Positive Lookahead Parser

The `PositiveLookahead` parser invokes a parser but consumes no input. It returns success and the parsed value if the parser would match, and returns failure if it would not. It is the opposite of the `NegativeLookahead` parser. The `.FollowedBy()` extension method uses a positive lookahead parser to match a pattern, and only return a successful result if the first pattern is followed by the second pattern (but no input is consumed by the second pattern).

```csharp
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

Positive and Negative lookahead parsers can be created from one another using the Not parser. These two couples are the same (though the longer versions are less efficient):

```csharp
var parser = PositiveLookahead(parser);
var parser = NegativeLookahead(Not(parser));
```

```csharp
var parser = NegativeLookahead(parser);
var parser = PositiveLookahead(Not(parser));
```

## Combinatorial Parsers

### And Parser

The `And` parser is similar to the `Rule` parser except it does not return a value, it only checks if the parsers match, in order. The And parser will consume input when all the parsers succeed, but will consume no input if any of them fail.

```csharp
var parser = And(p1, p2, p3,...);
var parser = p1.And(p2, p3,...);
```

The `And` parser is useful with the `Capture` parser to get the results of several parsers together and then capture the input items which were matched.

### Not Parser

The `Not` parser inverts the success value of the inner parser. When the inner parser succeeds, the Not parser fails. When the inner parser fails, the Not parser succeeds. In either case, the Not parser will not consume any input and will not return a value.

```csharp
var parser = Not(predicate);
var parser = predicate.Not();
```

### Or Parser

The `Or` parser is similar to the `First` parser except it does not return a value, it only checks if any parsers match. This parser does consume input data on success, consumes nothing on failure, and will return an untyped value.

```csharp
var parser = Or(p1, p2, p3,...);
var parser = p1.Or(p2, p3,...);
```

Because the `Or` parser returns an untyped value, it can act as a more flexible, type-unsafe version of the `First` parser. You can mix and match parsers that would return multiple different data types, and then figure out which value was returned from among those when the parser succeeds.
