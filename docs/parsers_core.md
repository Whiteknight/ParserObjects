# Core Parsers

The core parsers are all generic, and they can operate on any type of input sequence. For a list of additional parsers which are related strictly to parsing strings with character input sequences, see the [String/Character Parsers](parsers_chars.md) page. In addition, there are several specialty parsers related to parsing common patterns from programming languages, which also take character input sequences. You can find these in the [Programming Parsers](parsers_programming.md) page.

Throughout the descriptions of these parsers, examples will be shown where one parser is functionally or logically equivalent to a combination of other parsers. This is done to give multiple possible ways to understand some of the trickier concepts.

## Declaration Styles

There are three basic styles of declaring parsers. The first is to use normal object constructors and methods (change `<char>` to be whatever input type your parser methods are consuming):

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
using ParserObjects.Parsers;
using static ParserObjects.ParserMethods<char>;
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

### Chain Parser

The `Chain` parser invokes an initial parser to parse a prefix value, then uses that prefix value to select the next parser to invoke.

```csharp
var parser = new ChainParser<char, char>(initial, c => {
    if (c == 'a')
        return new AParser();
    if (c == 'b')
        return new BParser();
});
var parser = initial.Chain(c => {
    if (c == 'a')
        return new AParser();
    if (c == 'b')
        return new BParser();
});
```

### Choose Parser

The `Choose` parser invokes an initial parser to parse a prefix value without consuming any input, then uses that prefix value to select the next parser to invoke.

```csharp
var parser = new ChooseParser<char, char>(initial, c => {
    if (c == 'a')
        return new AParser();
    if (c == 'b')
        return new BParser();
});
var parser = initial.Choose(c => {
    if (c == 'a')
        return new AParser();
    if (c == 'b')
        return new BParser();
});
```

The difference between the `Chain` parser and the `Choose` parser is that the chain parser invokes the initial and consumes input, while the choose parser invokes the initial but does not consume input. The choose parser is functionally equivalent to a combination of `PositiveLookahead` and `Chain`:

```csharp
var parser = initial.Choose(value => ... );
var parser = PositiveLookahead(intial).Chain(value => ... );
```

### Combine Parser

The `Combine` parser takes a list of parsers, parses each in sequence, and returns a list of `object` results. You can transform or filter these results as appropriate for your application.

```csharp
var parser = Combine()

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

### Function Parser

The `Function` parser takes a callback function to perform the parse. 

```csharp
var parser = new FuncParser<char, string>(t => {
    // for success
    return new SuccessResult<string>("ok");

    // for failure
    return new FailResult<string>();
});
var parser = Function(t => {
    // for success
    return new SuccessResult<string>("ok");

    // for failure
    return new FailResult<string>();
});
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

The `Rule` parser attempts to execute a list of parsers, and then return a combined result. If any parser in the list fails, the input is rewinded and the whole parser fails. You can create rule parsers by using the `.Produce()` extension method on a `Tuple` or `ValueTuple` of parser objects, which may be cleaner to read and write in some situations

```csharp
var parser = new RuleParser<char, string>(
    parser1,
    parser2,
    parser3,
    (r1, r2, r3) => ...
);
var parser = Rule(
    parser1, 
    parser2, 
    parser3, 
    (r1, r2, r3) => ...
);
var parser = (parser1, parser2, parser3).Produce((r1, r2, r3) => ...);
```

All variants of the Rule parser are limited to 9 child parsers. If you need more than 9 child parsers in a rule, consider merging together adjacent parsers to create intermediate values.

### Sequential Parser

The `Sequential` parser allows turning a parser graph into a block of sequential code. This allows you to use procedural logic to aid in parsing and to set breakpoints between parsers to get maximum debuggability. Some grammars are best parsed using a stack or other The downside is that the Sequential Parser does not work with some features like BNF stringification or `.Replace()`/`.ReplaceChild()` operations.

```csharp
var parser = new SequentialParser(t => 
{
    var type = t.Parse(Word());
    if (type == 'decimal')
    {
        var colon = t.Parse(Match(':'));
        var value = t.Parse(Integer());
        return value;
    }
    if (type == 'hex')
    {
        var colon = t.Parse(Match(':'));
        var value = t.Parse(HexadecimalInteger());
        return value;
    }
    return 0;
});
```

The `t` object assists in performing the parse and it has ability to handle errors by causing the whole `Sequential` parser to fail if any of the child parsers fail. 

## Matching Parsers

These parsers help to simplify matching of literal patterns.

### Match Sequence Parser

The `MatchSequenceParser` takes a literal list of values, and attempts to match these against the input sequence. If all input items match, in order, the values will be returned as a list. (some of the below examples take advantage of the fact that a `string` is an `IEnumerable<char>` to help simplify)

```csharp
var parser = new MatchSequenceParser<char>(new char[] { 'a', 'b', 'c', 'd' });
var parser = new MatchSequenceParser<char>("abcd");
var parser = Match(new char[] { 'a', 'b', 'c', 'd' });
var parser = Match("abcd"); // a string is an IEnumerable<char>, so we can use a string to match chars
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

**Noticed** that the `Flatten` parser is not currently reentrant. If you recurse into the same instance of the `Flatten` parser, the results will be jumbled and appear out of order.

### Transform Parser

The `Transform` parser transforms the output of an inner parser. If the inner parser fails the `Transform` parser fails. If the inner parser succeeds, the `Transform` parser will return a transformed result.

```csharp
var parser = new TransformParser<char, string>(innerParser, r => ...);
var parser = Transform(innerParser, r => ...);
var parser = innerParser.Transform(r => ...);
```

## Lookahead Parsers

Lookahead parsers attempt to match a pattern but consume no input. These are useful when you want to see what is coming next in the input sequence, and make decisions about what to parse based on that information.

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
