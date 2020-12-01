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

The difference between the `Chain` parser and the `Choose` parser is that the chain parser invokes the initial and consumes input, while the choose parser invokes the initial but does not consume input.

### Combine Parser

The `Combine` parser takes a list of parsers, parses each in sequence, and returns a list of `object` results. You can transform or filter these results as appropriate for your application.

```csharp
var parser = Combine(p1, p2, p3,...);
```

The Combine parser is implemented using the `RuleParser`.

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

### Examine Parser

The `Examine` parser allows inserting callbacks before or after any other parser, and is primarily used for debugging. You can also use the Examine parser to make adjustments to the input stream or state data before the parse, and augment the result after a parse.

```csharp
var parser = new Examine<TInput, TOutput>.Parser(inner, c => { ... }, c => { ... });
var parser = inner.Examine(c => { ... }, c => { ... });
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

The `Function` parser takes a callback function to perform the parse. The callback takes `success` and `fail` arguments, which are functions to generate the correct result object with filled-in metadata.

```csharp
var parser = new FuncParser<char, string>((t, success, fail) => {
    // for success
    return success("Ok");

    // for failure
    return fail("parse failed");
});
var parser = Function((t, success, fail) => {
    // for success
    return success("ok");

    // for failure
    return fail("parse failed");
});
```

The Function parser is very similar to the `Sequential` parser. Both use a callback to execute the parse. The Function parser is almost completely free from structure and does not assume that the parse internally is performed using `IParser`s. The Sequential parser, on the other hand provides a state object which should be used to perform parses, and expects that the parsing internally will be done using `IParser` instances.

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
    new IParser[] {
        parser1,
        parser2,
        parser3
    },
    r => ...
);
var parser = Rule(
    parser1, 
    parser2, 
    parser3, 
    (r1, r2, r3) => ...
);
var parser = (parser1, parser2, parser3).Produce((r1, r2, r3) => ...);
```

The `RuleParser` class is not designed to be used directly. It is instead the mechanism by which the `Rule()` and `Combine()` methods are implemented, and these should be preferred instead. The `Rule()` method gives strong-typing for all parameters (up to 9) and the `Combine()` parser returns an `IReadOnlyList<object>` of all results (any number);

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

### Match Pattern Parser

The `MatchPatternParser` takes a literal list of values, and attempts to match these against the input sequence. If all input items match, in order, the values will be returned as a list. (some of the below examples take advantage of the fact that a `string` is an `IEnumerable<char>` to help simplify)

```csharp
var parser = new MatchPatternParser<char>(new char[] { 'a', 'b', 'c', 'd' });
var parser = new MatchPatternParser<char>("abcd");
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

The `Trie` parser uses a trie to find the longest match in a list of possible literal sequences. This is a useful optimization for keyword and operator literals, where individual patterns may have overlapping prefixes. The ParserObjects library provides `IReadOnlyTrie<TKey, TResult>` and `IInsertableTrie<TKey, TResult>` abstractions for this purpose.

```csharp
var parser = new TrieParser<char, string>(trie);
var parser = Trie(trie);
var parser = trie.ToParser();
var parser = Trie(trie => trie.Add(...));
```

## Transforming parsers

These parsers exist to transform results from one form to another.

### Transform Parser

The `Transform` parser transforms the output of an inner parser. If the inner parser fails the `Transform` parser fails. If the inner parser succeeds, the `Transform` parser will return a transformed result.

```csharp
var parser = new TransformParser<char, string>(innerParser, r => ...);
var parser = Transform(innerParser, r => ...);
var parser = innerParser.Transform(r => ...);
```

### Transform Result Parser

The `TransformResult` parser has an opportunity to transform the entire result, including all metadata, and it operates even when the result is a failure result. 

```csharp
var parser = new TransformResultParser<TInput, TOriginal, TOutput>(inner, (state, result) => { ... });
```

### Transform Error Parser

The `TransformError` parser is implemented by the `TransformResult` parser, but the callback only executes when the result is a failure. This is used to transform the result to, for example, provide a better error message.

```csharp
var parser = TransformError(parser, (state, errorResult) => { ... });
```

## Recursive Parsers

These parsers exist to help simplify certain recursion scenarios, especially in parsing equations and mathematical expressions. They are not helpful in all recursion scenarios.

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

### Pratt Parser

The `Pratt` parser is an implementation of the Pratt parsing algorithm, which may be particularly helpful with parsing mathematical expressions.

```csharp
var parser = Pratt(config => { ... });
```

For detailed information about configuring and using the `Pratt` parser, see the [Pratt Parser page](parsers_pratt.md)

## Referencing Parsers

These parsers exist to help with referencing issues, to help resolve circular dependencies or decide on which parser to use at parse-time.

### Create Parser

The `Create` parser creates a parser at parse time using information available in the current parse state. Create parser looks similar to the `Deferred` parser, though has a few important semantic differences: The create callback takes the `ParseState`, and it cannot be used with find/replace operations. The Create parser is expected to create new parser instances at different times, so it is not considered to have "children". 

```csharp
var parser = new CreateParser<TInput, TOutput>(state => { ... });
var parser = Create(state => { ... });
```

### Deferred Parser

The `Deferred` parser references another parser and resolves the reference at parse time instead of at declaration time. This allows your parser to handle recursion and circular references. The parser returned from the Deferred parser is expected by the system to be the same throughout the entire parse and may be cached. Deferred parser can be used with find/replace operations, unlike the `Create` parser.

```csharp
var parser = new DeferredParser<TInput, TOutput>(() => targetParser);
var parser = Deferred(() => targetParser);
```

### Replaceable Parser

The `Replaceable` parser references an inner parser and invokes it transparently. However, the replaceable parser allows the inner parser to be replaced in-place without cloning.  This is useful in cases where you want to make modifications to the parser tree without creating a whole new tree.

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
