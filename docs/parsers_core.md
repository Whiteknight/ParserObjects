# Core Parsers

## Declaration Styles

There are three basic styles of declaring parsers. The first is to use normal object constructors and methods:

```csharp
var parser = new ListParser<char, char>(
    new AnyParser()
);
```

The second is to use some static methods to simplify:

```csharp
using static ParserObjects.Parsers.ParserMethods;

var parser = List(
    Any()
);
```

The third is to start with one of the above two styles and use monadic extension methods to combine them:

```csharp
using static ParserObjects.Parsers.ParserMethods;

var parser = Any()
    .List();
```

In a few cases there are tuple syntaxes available to further simplify. These will be noted in the appropriate sections.

## Fundamental Parser Types

These parsers represent the theoretical core of the parsing library. To use these, import the methods you're using:

```csharp
using ParserMethods.Parsers;
using static ParserMethods.Parsers.ParserMethods;
```

### Any Parser

The `Any` parser matches any single input value and returns it directly.

```csharp
var anyParser = new AnyParser();
var anyParser = Any();
```

It is functionally equivalent to the match predicate parser (described below)

```csharp
var anyParser = Match(_ => true);
```

### Empty Parser

The `Empty` parser consumes no input and always returns success with a default value.

```csharp
var parser = new EmptyParser();
var parser = Empty();
```

### End Parser

The `End` parser returns success if the stream is at the end, failure otherwise.

```csharp
var parser = new EndParser();
var parser = End();
```

### Fail Parser

The `Fail` parser returns failure unconditionally.

```csharp
var parser = new FailParser<char, char>();
var parser = Fail<char>();
```

### First Parser

The `First` parser takes a list of parsers. Each parser is attempted in order, and the result is returned as soon as any parser succeeds. If none of the parsers succeed, the `First` parser fails.

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
var parser = (parser1, parser2, parser3).First();
```

The tuple variant of this parser is limited to 9 child parsers. The other variants can take any number of child parsers.

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
var parser = new MatchPredicateParser<char>(c => ...);
var parser = Match(c => ...);
```

### Produce Parser

The `Produce` parser produces a value but consumes no input. 

```csharp
var parser = new ProduceParser<char, string>(() => "abcd");
var parser = Produce(() => "abcd");
```

This is functionally equivalent to a combination of the `Empty` and `Transform` parsers:

```csharp
var parser = Empty().Transform(x => "abcd");
```

### Rule Parser

The `Rule` parser attempts to execute a list of parsers, and then return a combined result. If any parser in the list fails, the input is rewinded and the whole parser fails.

```csharp
var parser = new RuleParser<char, string>(
    parser1,
    parser2,
    parser3,
    (r1, r2, r3) => ...
);
var parser = Rule(parser1, parser2, parser3, (r1, r2, r3) => ...);
var parser = (parser1, parser2, parser3).Produce((r1, r2, r3) => ...);
```

All variants of the Rule parser are limited to 9 child parsers. If you need more than 9 child parsers in a rule, consider merging together adjacent parsers to create intermediate values.

## Matching Parsers

These parsers help to simplify matching of literal patterns.

### Character String Parser

The `CharacterString` parser matches a literal string of characters against a `char` input and returns the string on success.

```csharp
using ParserObjects.Parsers.ParserMethods;
```

```csharp
var parser = CharacterString("abc");
```

This is functionally equivalent to a combination of the `MatchSequence` and `Transform` parsers (both described below):

```csharp
var parser = Match("abc").Transform(x => "abc");
```

### Match Sequence Parser

The `MatchSequenceParser` takes a literal list of values, and attempts to match these against the input sequence. If all input items match, in order, the values will be returned as a list. (some of the below examples take advantage of the fact that a `string` is an `IEnumerable<char>` to help simplify)

```csharp
var parser = new MatchSequenceParser<char>(new char[] { 'a', 'b', 'c', 'd' });
var parser = new MatchSequenceParser<char>("abcd");
var parser = Match(new char[] { 'a', 'b', 'c', 'd' });
var parser = Match("abcd");
```

This is functionally equivalent (though faster and more succinct) to a combination of the `Rule` and `MatchPredicate` parsers:

```csharp
var parser = Rule(
    Match(c => c == 'a'),
    Match(c => c == 'b'),
    Match(c => c == 'c'),
    Match(c => c == 'd')
    (a, b, c, d) => new [] { a, b, c, d }
);
```

### Trie Parser

The `Trie` parser uses a trie to find the longest match in a list of possible literal sequences. This is a useful optimization for keyword and operator literals, where individual patterns may have overlapping prefixes. The ParserObjects library provides an `IReadOnlyTrie<TKey, TResult>` abstraction for this purpose.

```csharp
var parser = new TrieParser<char, string>(trie);
var parser = Trie(trie);
var parser = trie.ToParser();
var parser = Trie(trie => trie.Add(...));
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

The `FlattenParser` is conceptually similar to the `.SelectMany()` LINQ method.

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

The `NegativeLookaheadParser` invokes a parser, but consumes no input. It returns success if the parser would not match, and returns failure if it would match. It is the opposite of the `PositiveLookaheadParser`.

```csharp
var parser = new NegativeLookaheadParser<char>(innerParser);
var parser = NegativeLookahead(innerParser);
var parser = someParser.NotFollowedBy(innerParser);
```

### Positive Lookahead Parser

The `PositiveLookaheadParser` invokes a parser but consumes no input. It returns success if the parser would match, and returns failure if it would not. It is the opposite of the `NegativeLookaheadParser`.

```csharp
var parser = new PositiveLookaheadParser<char>(innerParser);
var parser = PositiveLookahead(innerParser);
var parser = someParser.FollowedBy(innerParser);
```

## Recursive Parsers

These parsers exist to help simplify recursion scenarios.

### Left Apply Parser

The `LeftApplyParser` is a complicated parser for left-associative parsing. The left value is parsed first and the value of it is applied to the right side production rule. The value of the right parser will then be used as the new left value and it will attempt to continue until a right parser does not match. The pseudo-BNF for it is:

```
self := <self> <right> | <left>
```

```csharp
var parser = new LeftApplyParser<char, object>(
    itemParser, 
    left => ...
);
var parser = LeftApply(
    itemParser, 
    left
);
```

### Right Apply Zero or More Parser

The `RightApplyZeroOrMoreParser` is for right-associative recursion. It is conceptually similar to the `LeftApplyZeroOrMore` parser, but with right-recursion. It parses an item and then attempts to parse a separator followed by a recursion to itself. The pseudo-BNF for it is:

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

The `Deferred` parser references another parser and resolves the reference at parse time instead of at declaration time. This allows your parser to handle recursion and circular references.

```csharp
var parser = new DeferredParser(() => targetParser);
var parser = Deferred(() => targetParser);
```

### Replaceable Parser

The `Replaceable` parser references an inner parser and invokes it transparently. However, the replaceable parser allows the inner parser to be replaced in-place without cloning. This is the only parser type in the entire library which is not immutable. This is useful in cases where you want to make modifications to the parser tree without creating a whole new tree.

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
