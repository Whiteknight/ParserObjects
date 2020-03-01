# Core Parsers

## Declaration Styles

There are three basic styles of declaring parsers. The first is to use normal object constructors and methods:

```csharp
var parser = new ListParser<char, char>(
    new AnyParser<char>()
);
```

The second is to use some static methods to simplify:

```csharp
using static ParserObjects.Parsers.ParserMethods;

var parser = List<char, char>(
    Any<char>()
);
```

The third is to start with one of the above two styles and use monadic extension methods to combine them:

```csharp
using static ParserObjects.Parsers.ParserMethods;

var parser = Any<char>()
    .List();
```

## Fundamental Parser Types

These parsers represent the theoretical core of the parsing library.

### Any Parser

The `Any` parser matches any input value and returns it directly.

```csharp
var anyParser = new AnyParser<char>();
var anyParser = Any<char>();
```

It is functionally equivalent to the match predicate parser (described below)

```csharp
var anyParser = Match<char>(() => true);
```

### Empty Parser

The `Empty` parser consumes no input and always returns success with a value of `null`.

```csharp
var parser = new EmptyParser<char>();
var parser = Empty<char>();
```

### End Parser

The `End` parser returns success if the stream is at the end, failure otherwise.

```csharp
var parser = new EndParser<char>();
var parser = End<char>();
```

### Fail Parser

The `Fail` parser returns failure unconditionally.

```csharp
var parser = new FailParser<char, char>();
var parser = Fail<char, char>();
```

### First Parser

The `First` parser takes a list of parsers. Each parser it attempted in order, and the result is returned when the first parser succeeds. If none of the parsers succeed, the `First` parser fails.

```csharp
var parser = new FirstParser<char, object>(
    parser1,
    parser2,
    parser3
);
var parser = First(
    parser1, 
    parser2,
    parser3
)
```

### List Parser

The `List` parser attempts to parse the inner parser repeatedly until it fails, and returns an enumeration of the results.

```csharp
var parser = new ListParser<char, char>(innerParser);
var parser = List(innerParser);
var parser = innerParser.List();
```

### Match Predicate Parser

The `MatchPredicateParser` examines the next input item and returns it if it matches a predicate.

```csharp
var parser = new MatchPredicateParser<char, char>(c => ...);
var parser = Match<char>(c => ...);
```

### Produce Parser

The `Produce` parser produces a value but consumes no input. 

```csharp
var parser = new ProduceParser<char, string>(() => "abcd");
var parser = Produce<char, string>(() => "abcd");
```

This is functionally equivalent to a combination of the `Empty` and `Transform` parsers:

```csharp
var parser = Empty<char>().Transform(x => "abcd");
```

### Rule Parser

The `Rule` parser attempts to execute a list of parsers, and then return a combined result. If any parser in the list fails, the input is rewinded and the whole parser fails.

```csharp
var parser = new RuleParser<char, string>(
    parser1,
    parser2,
    parser3,
    (r1, r2, r3) => ...
)
```

## Matching Parsers

These parsers help to simplify matching of literal patterns.

### Match Sequence Parser

The `MatchSequenceParser` takes a literal list of values, and attempts to match these against the input sequence. If all input items match, in order, the values will be returned as a list.

```csharp
var parser = new MatchSequenceParser<char>("abcd");
var parser = Match<char>("abcd");
```

This is functionally equivalent (though faster and more succinct) to a combination of the `Rule` and `MatchPredicate` parsers:

```csharp
var parser = Rule(
    Match(c => c == 'a'),
    Match(c => c == 'b'),
    Match(c => c == 'c'),
    Match(c => c == 'd')
    (a, b, c, d) => new [] {a, b, c, d}
);
```

### Trie Parser

The `Trie` parser uses a trie to find the longest match in a list of possible literal sequences. This is useful for keyword and operator literals, where individual patterns may have overlapping prefixes. The ParserObjects library provides an `ITrie<TKey, TResult>` abstraction for this purpose.

```csharp
var parser = new TrieParser<char, string>(trie);
var parser = Trie(trie);
var parser = trie.ToParser();
```

## Transforming parsers

These parsers exist to transform results from one form to another.

### Flatten Parser

The `Flatten` parser transforms a parser which returns `IEnumerable<TOutput>` into a parser which returns `TOutput`. It does this by caching values and needs to be invoked multiple times to get all results.

```csharp
var parser = new FlattenParser<char, string, char>(sourceParser);
var parser = Flatten<char, string, char>(sourceParser);
var parser = sourceParser.Flatten<char, string, char>();
```

### Transform Parser

The `Transform` parser transforms the output of an inner parser. If the inner parser fails the `Transform` parser fails. If the inner parser succeeds, the `Transform` parser will return a transformed result.

```csharp
var parser = new TransformParser<char, string>(innerParser, r => ...);
var parser = Transform(innerParser, r => ...);
var parser = innerParser.Transform(r => ...);
```

## Lookahead Parsers

Lookahead parsers attempt to match a pattern but consume no input.

### Negative Lookahead Parser

The `NegativeLookaheadParser` invokes an inner parser, but consumes no input. It returns success if the inner parser would not match, and returns failure if it would match. It is the opposite of the `PositiveLookaheadParser`.

```csharp
var parser = new NegativeLookaheadParser<char>(innerParser);
var parser = NegativeLookahead(innerParser);
var parser = someParser.NotFollowedBy(innerParser);
```

### Positive Lookahead Parser

The `PositiveLookaheadParser` invokes an inner parser but consumes no input. It returns success if the inner parser would match, and returns failure if it would not. It is the opposite of the `NegativeLookaheadParser`.

```csharp
var parser = new PositiveLookaheadParser<char>(innerParser);
var parser = PositiveLookahead<char>(innerParser);
var parser = someParser.FollowedBy(innerParser);
```

## Recursive Parsers

These parsers exist to help simplify recursion scenarios.

### LeftApplyZeroOrMore Parser

The `LeftApplyZeroOrMore` parser is a complicated parser for left-associative parsing. The left value is parsed first and the value of it is applied to the right side production rule. The value of the right parser will then be used as the new left value and it will attempt to continue until a right parser does not match.

```csharp
var parser = new LeftApplyZeroOrMore(
    itemParser, 
    left => ...
);
var parser = LeftApply(
    itemParser, 
    left
);
```

### Right Apply Zero or More Parser

The `RightApplyZeroOrMore` parser is for right-associative recursion. It is conceptually similar to the `LeftApplyZeroOrMore` parser, but with right-recursion. It parses an item and then attempts to parse a separator followed by a recursion to itself. The pseudo-BNF for it is:

```
self := <item> (<middle> <self>)?
```

```csharp
var parser = new RightApplyZeroOrMore<char, char, string>(item, middle, (l, m, r) => ...);
var parser = RightApply(item, middle, (l, m, r) => ...);
```

This same recursive functionality can be reproduced by a combination of `Deferred`, `First`, and `Rule`:

```csharp
IParser<char, string> parserCore = null;
var parser = new Deferred(() => parserCore);
parserCore = First(
    Rule(
        item, 
        middle,
        parser,
        (l, m, r) => ...
    ),
    item
);
```

## Referencing Parsers

These parsers exist to help with referencing issues.

### Deferred Parser

The `Deferred` parser references another parser and resolves the reference at parse time instead of at declaration time. This allows your parser to continue recursion and circular references.

```csharp
var parser = new DeferredParser(() => targetParser);
var parser = Deferred(() => targetParser);
```

### Replaceable Parser

The `Replaceable` parser references an inner parser and invokes it transparently. However, the replaceable parser allows the inner parser to be replaced in-place without cloning. This is useful in cases where you want to make modifications to the parser tree.

```csharp
var parser = new ReplaceableParser<char, char>(innerParser);
var parser = Replaceable(innerParser);
var parser = innerParser.Replaceable();
```

## Derived Parsers

The following look like individual parsers, but they're actually implemented as combinations of the above parsers

### Separated List

The `SeparatedList` parser is similar to a `List` parser except the items have separators between them

```csharp
var parser = SeparatedList(item, separator);
```