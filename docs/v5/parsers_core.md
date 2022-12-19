# Core Single Parsers

The core single parsers are all generic on the type of input they accept. For a list of additional parsers which are related strictly to parsing strings with character input sequences, see the [String/Character Parsers](parsers_chars.md) page. In addition, there are several specialty parsers related to parsing common patterns from programming languages, which also take character input sequences. You can find these in the [Programming Parsers](parsers_programming.md) page.

Throughout the descriptions of these parsers, examples will be shown where one parser is functionally or logically equivalent to a combination of other parsers. This is done to give multiple possible ways to understand some of the trickier concepts.

The best way to access these core parsers is through the static factory methods. Add this to the top of your C# file:

```csharp
using ParserObjects;
using static ParserObjects.Parsers<char>;
```

(Replace `<char>` with whatever your input type is.)

## Declaration Styles

There are two basic styles of declaring parsers. The first is to use the static factory methods to create parsers:

```csharp
using static ParserObjects.Parsers<char>;

var parser = List(
    Any()
);
```

The second is to use monadic extension methods to combine them:

```csharp
using ParserObjects;
using static ParserObjects.Parsers<char>;

var parser = Any().List();
```

In a few cases there are tuple syntaxes available as well. These will be noted in the appropriate sections. 

Do not use parser class names directly from the `ParserObjects.Internal.Parsers` namespace. These class names are not designed for easy discovery or use, and they may change between releases to better describe what they are and how they operate. The Function and Method names described above will stay the same between releases, even if the ways they are implemented may change.

## Fundamental Parser Types

These parsers represent the theoretical core of the parsing library. To use these, import the methods you're using (replace `<char>` with whatever input type you are using):

```csharp
using ParserObjects;
using static ParserObjects.Parsers<char>;
```

### Any Parser

The `Any` parser matches any single input value and returns it directly. It consumes one item of input, and only fails when the sequence is at the end.

```csharp
var anyParser = Any();
```

It is functionally equivalent to the match predicate parser, though simpler and faster (described below):

```csharp
var anyParser = Match(_ => true);
```

### Bool Parser

The `Bool` parser invokes a parser and returns `true` if the inner parser succeeds, `false` otherwise. It is useful if you want to know whether something matches, but don't care what the result value is, or if you want to convert `IParser<TInput>` to `IParser<TInput, bool>`.

```csharp
var parser = Bool(innerParser);
```

### Chain Parser

The `Chain` parser invokes an initial parser to obtain a prefix value, then uses that prefix value to select the next parser to invoke.

```csharp
var parser = Chain(initial, result => {
    if (!result.Success)
        return new HandleFailureParser();
    if (result.Value == 'a')
        return new AParser();
    if (result.Value == 'b')
        return new BParser();
});
var parser = initial.Chain(result => {
    if (!result.Success)
        return new HandleFailureParser();
    if (result.Value == 'a')
        return new AParser();
    if (result.Value == 'b')
        return new BParser();
});
```

The Chain parser will throw an `InvalidOperationException` if the callback method returns a `null` value.

### Chain With Parser

The `ChainWith` parser is related to the `Chain` parser but uses a different fluent syntax for selecting a value.

```csharp
var parser = ChainWith(initial, config => config
    .When(x => x == 'a', new AParser())
    .When(x => x == 'b', new BParser())
);
```

### Choose Parser

The `Choose` parser invokes an initial parser to parse a prefix value without consuming any input, then uses that prefix value to select the next parser to invoke.

```csharp
var parser = Choose(initial, result => {
    if (!result.Success)
        return new HandleFailureParser();
    if (result.Value == 'a')
        return new AParser();
    if (result.Value == 'b')
        return new BParser();
});
var parser = initial.Choose(result => {
    if (!result.Success)
        return new HandleFailureParser();
    if (result.Value == 'a')
        return new AParser();
    if (result.Value == 'b')
        return new BParser();
});
```

The `Choose` parser is implemented using the `Chain` parser internally and is equivalent to a combination of the `Chain` and `None` parsers:

```csharp
var parser = initial
    .None()
    .Chain(result => ...);
```

### Combine Parser

The `Combine` parser takes a list of parsers, parses each in sequence, and returns a list of `object` results. You can transform or filter these results as appropriate for your application.

```csharp
var parser = Combine(p1, p2, p3, ...);
```

### Empty Parser

The `Empty` parser consumes no input and always returns success with a default value, even when the input sequence is at end. It consumes no input and returns no value.

```csharp
var parser = Empty();
```

### End Parser

The `End` parser returns success if the stream is at the end, failure otherwise. It consumes no input and returns no value.

```csharp
var parser = End();
```

### Examine Parser

The `Examine` parser allows inserting callbacks before or after any other parser, and is primarily used for debugging. You can also use the Examine parser to make adjustments to the input stream or state data before the parse, and augment the result after a parse.

```csharp
var parser = inner.Examine(
    before: c => { ... }, 
    after: c => { ... }
);
```

### Fail Parser

The `Fail` parser returns failure unconditionally. It can be used to explicitly insert failure conditions into your parser graph, to provide error messages which are more helpful than the default error messages, or to serve as a placeholder for replacement operations. The Fail parser has an output type so it can be inserted into places in your parser graph that expect an output type to be specified.

```csharp
var parser = Fail<char>("helpful error message");
var parser = Fail("helpful error message");
```

If the output type is not specified, it returns the same as the input type.

### First Parser

The `First` parser takes a list of parsers. Each parser is attempted in order, and the result is returned as soon as any parser succeeds. If none of the parsers succeed, the `First` parser fails. The First parser can also be written as an extension method on a tuple of parsers.

```csharp
var parser = First(
    parser1, 
    parser2,
    parser3
);
var parser = (parser1, parser2, parser3).First();
```

The tuple variant of this parser is limited up to 9 child parsers. The other variants can take any number of child parsers.

### Function Parser

The `Function` parser takes a callback function to perform the parse. The callback takes `success` and `fail` arguments, which are functions to generate the correct result object with filled-in metadata. It will automatically rewind the input sequence on failure, so you do not need to cleanup manually. It will also automatically report the correct number of consumed input tokens so you do not need to track it yourself.

```csharp
var parser = Function((t, success, fail) => {
    // for success
    return success("ok");

    // for failure
    return fail("parse failed");
});
```

The Function parser is very similar to the `Sequential` parser. Both use a callback to execute the parse. The Function parser is almost completely free from structure and does not assume that the parse internally is performed using `IParser` instances. The Sequential parser, on the other hand provides a state object which should be used to perform parses, and expects that the parsing internally will be done using `IParser` instances. The `Function` parser is used internally to implement several of the other parser types in this list.

### List Parser

The `List` parser attempts to parse the **item** parser repeatedly until it fails, and returns an enumeration of the results. Optionally the items may have a **separator** between them. The List parser takes optional `minimum` and `maximum` values, to control the number of items matched. If you specify a minimum, the list will fail unless at least that number of items has been matched. If you do not specify a minimum, the list may return success if no items are matched, and return an empty list as a result. If a maximum number is specified, the list will continue matching only until that maximum number is reached then it will stop even if more matches are possible.

```csharp
var parser = List(innerParser);
var parser = List(innerParser, 3, 5);
var parser = List(innerParser, separatorParser);
var parser = List(innerParser, separatorParser, 3, 5);

// same as List(innerParser, minimum: 1);
var parser = List(innerParser, true);
var parser = List(innerParser, separatorParser, true);

var parser = innerParser.List();
var parser = innerParser.List(3, 5);
var parser = innerParser.List(separatorParser);
var parser = innerParser.List(separatorParser, 3, 5);

// Same as innerParser.List(minimum: 1);
var parser = innerParser.List(true);
```

If the inner parser returns success but consumes zero input, the List parser will break the loop and return only a single item. If a minimum number is set, the List parser will loop only until the minimum value and then break, returning success with a list with the correct number of items. This is a precaution to prevent the list parser from getting into an infinite loop when no input is being consumed.

### Match Predicate Parser

The `MatchPredicateParser` examines the next input item and returns it if it matches a predicate.

```csharp
var parser = Match(c => ...);
```

### None Parser

The `None` parser evaluates an inner parser and the rewinds the input sequence to ensure no data has been consumed. 

```csharp
var parser = None(Any());
var parser = Any().None();
```

### NonGreedyList Parser

The `NonGreedyList` parser is similar to `List()` except it attempts to match the fewest number of items possible. It takes a continuation parser which will be invoked to continue the parse:

```csharp
var parser = NonGreedyList(
    itemParser,
    values => new Rule(
        values,
        finalParser
        (v, f) => { ... }
    )
);
```

Like the `List()` parser, `NonGreedyList()` parser also takes optional **separator**, **minimum** and **maximum** parameters.

### Optional Parser

The `Optional` parser attempts to invoke the inner parser, but returns success no matter the result. The Optional parser takes a callback argument to return a default value if the parse fails. If the default value callback is not provided, the Optional parser will return an `IOption` object which will report on success or failure of the inner parser.

```csharp
var parser = Optional(innerParser);
var parser = Optional(innerParser, () => defaultValue);

var parser = innerParser.Optional();
var parser = innerParser.Optional(() => defaultValue);
```

The Optional parser is functionally equivalent to a combination of `First` and `Produce` parsers:

```csharp
var parser = First(
    innerParser,
    Produce(() => defaultValue)
);
```

### Peek Parser

The `Peek` parser peeks at the next value of input, but does not consume it. It returns failure when the input sequence is at end, success otherwise.

```csharp
var parser = new PeekParser<char>();
var parser = Peek();
```

This parser is functionally equivalent to the `Any` and `None` parsers:

```csharp
var parser = Any().None();
```

### Predict Parser

The `Predict` parser peeks at a lookahead value in the input stream, and uses that value to determine what parser to invoke next.

```csharp
var parser = Predict(config => config
    .When(c => c == 'a', new AParser())
    .When(c => c == 'b', new BParser())
);
```

If no matching value is found, the Predict parser returns failure. The `Predict` parser is implemented internally using the `Chain` parser and the `Peek` parser. It is logically equivalent, though nicer syntax, to:

```csharp
var parser = Peek().Chain(r => ...);
```

### Produce Parser

The `Produce` parser produces a value but consumes no input. It always returns success.

```csharp
var parser = Produce(() => "abcd");
var parser = Produce((input, data) => "abcd");
```

The produce parser may be used to construct synthetic values at parse time. It can return a constant value or create a new value on every call, the value will not be cached. It may look at and consume input from the input sequence. It may use values from the contextual state data.

The simple case of the Produce parser is functionally equivalent to a combination of the `Empty` and `Transform` parsers:

```csharp
var parser = Empty().Transform(_ => "abcd");
```

### Rule Parser

The `Rule` parser attempts to execute a list of parsers, and then return a combined result. If any parser in the list fails, the input is rewound and the whole parser fails. You can create rule parsers by using the `.Rule()` extension method on a `Tuple` or `ValueTuple` of parser objects, which may be cleaner to read and write in some situations

```csharp
var parser = Rule(
    parser1, 
    parser2, 
    parser3, 
    (r1, r2, r3) => ...
);
var parser = (parser1, parser2, parser3).Rule((r1, r2, r3) => ...);
```

The `Rule()` method and tuple variants are both limited to 9 parsers at most. If you need to combine the results of more than 9 parsers, use the `Combine` parser instead. 

### Sequential Parser

The `Sequential` parser allows turning a parser graph into a block of sequential code. This allows you to use procedural logic to aid in parsing and to set breakpoints between parsers to get maximum debuggability. Some grammars are best parsed using a stack or other mechanism, instead of the recursive descent algorithm used by ParserObjects, so the Sequential parser allows you to use those algorithms instead. The downside is that the Sequential Parser does not work with some features like BNF stringification or `.Replace()`/`.ReplaceChild()` operations.

```csharp
var parser = Sequential(t => 
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

### Synchronize Parser

The `Synchronize` parser allows entering **panic mode** when a parse fails. In panic mode, the parser will discard tokens to get back to a known "good" state, before attempting the parse again. This is useful for cases where you want to report all syntax errors to the user, not just the first error.

```csharp
var parser = Synchronize(inner, x => x == ';');
var parser = inner.Synchronize(x => x == ';');
```

Once you define your parser, you can check to see if there are any errors. If the parser eventually succeeds, the successful result will also be available:

```csharp
var result = parser.Parse(...);
var allErrors = result.TryGetData<ErrorList>();
var successResult = result.TryGetData<IResult>();
```

You can use the list of errors to report problems back to the user.

### Try Parser

The `Try` parser catches user-thrown exceptions from within the parse and handles them. When an exception is caught, the input sequence is rewound to the location where the `Try` parser began.

```csharp
var parser = Try(innerParser, ex => {...}, bubble: true);
```

The second parameter is a callback to allow examining the exception when it is received. This can be a useful place to set a breakpoint during debugging. The third parameter `bubble` tells whether to rethrow the exception (`true`) or to handle the exception and return a failure result (`false`).

You can get information about the exception thrown from the result, if you set `bubble: false`:

```csharp
var result = parser.Parse(...);
var exception = result.TryGetData<Exception>();
```

## Matching Parsers

These parsers help to simplify matching of literal patterns.

### Match Pattern Parser

The `MatchPatternParser` takes a literal list of values, and attempts to match these against the input sequence. If all input items match, in order, the values will be returned as a list. (some of the below examples take advantage of the fact that a `string` is an `IEnumerable<char>` to help simplify)

```csharp
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

The `Trie` parser uses a trie to find the longest match in a list of possible literal sequences. This is a useful optimization for keyword and operator literals, where individual patterns may have overlapping prefixes. The ParserObjects library provides `IReadOnlyTrie<TKey, TResult>` and `IInsertableTrie<TKey, TResult>` abstractions for this purpose.

```csharp
var parser = Trie(trie);
var parser = trie.ToParser();
var parser = Trie(trie => trie.Add(...));
var parser = MatchAny("value", "value2", "value3");
```

The `MatchAny` parser is implemented using the `Trie` mechanism internally, and works only on `char` input, `string` output scenarios.

## Transforming parsers

These parsers exist to transform results from one form to another.

### Transform Parser

The `Transform` parser transforms the output of an inner parser. If the inner parser fails the `Transform` parser fails. If the inner parser succeeds, the `Transform` parser will return a transformed result.

```csharp
var parser = Transform(innerParser, r => ...);
var parser = innerParser.Transform(r => ...);
```

### Transform Result Parser

The `TransformResult` parser has an opportunity to transform the entire result, including all metadata, and it operates even when the result is a failure result. 

```csharp
var parser = TransformResult(inner, (state, result) => { ... });
```

**Note**: The callback for `TransformResult` can perform any arbitrary operation on the result or on the current parse state. For this reason, `.Match()` cannot be optimized in any meaningful way, and other analysis and meta operations may not provide meaningful results. It is best to avoid the use of `TransformResult` unless absolutely necessary.

### Transform Error Parser

The `TransformError` parser is implemented by the `TransformResult` parser, but the callback only executes when the result is a failure. This is used to transform the result to, for example, provide a better error message.

```csharp
var parser = TransformError(parser, (state, errorResult) => { ... });
```

**Note**: The callback for `TransformError` can perform any arbitrary operation on the result or on the current parse state. For this reason, `.Match()` cannot be optimized in any meaningful way, and other analysis and meta operations may not provide meaningful results. It is best to avoid the use of `TransformError` unless absolutely necessary.

## Recursive Parsers

These parsers exist to help simplify certain recursion scenarios, especially in parsing equations and mathematical expressions. They are not helpful in all recursion scenarios.

### Left Apply Parser

The `LeftApplyParser` is a parser for left-associative parsing. The left value is parsed first and the value of it is applied to the right side production rule. The value of the right parser will then be used as the new left value and it will attempt to continue until a right parser does not match. The pseudo-BNF for it is:

```
self := <self> <right> | <item>
```

```csharp
var parser = LeftApply(
    itemParser, 
    left => Rule(
        left,
        ...
    )
);
```

### Right Apply Parser

The `RightApply` is for right-associative recursion. It is conceptually similar to the `LeftApply` parser, but with right-recursion instead. It parses an item and then attempts to parse a separator followed by a recursion to itself. The pseudo-BNF for it is:

```
self := <item> (<middle> <self>)?
```

```csharp
var parser = RightApply(item, middle, (l, m, r) => ...);
```

This same recursive functionality can be reproduced by a combination of `Deferred`, `First`, and `Rule`:

```csharp
IParser<char, string> parserCore = null;
var parser = Deferred(() => parserCore);
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

For detailed information about configuring and using the `Pratt` parser, see the [Pratt Parser page](parsers_pratt.md). It may be simpler to use in many situations than the `LeftApply` and `RightApply` parsers are.

## Referencing Parsers

These parsers exist to help with referencing issues, to help resolve circular dependencies or decide on which parser to use at parse-time.

### Create Parser

The `Create` parser creates a parser at parse time using information available in the current parse state. Create parser looks similar to the `Deferred` parser, though has a few important semantic differences: The create callback takes the `ParseState`, and it cannot be used with find/replace operations. The Create parser is expected to create new parser instances at different times, so it is not considered to have "children". This means that the parser returns by the Create parser will not be visible to Visitors.

```csharp
var parser = new CreateParser<TInput, TOutput>(state => { ... });
var parser = Create(state => { ... });
```

### Deferred Parser

The `Deferred` parser references another parser and resolves the reference at parse time instead of at declaration time. This allows your parser to handle recursion and circular references. The parser returned from the Deferred parser is expected by the system to be the same throughout the entire parse and may be cached after first access. Because the parser returned by Deferred is expected to be static and available at any time after the parser graph is created, the parser can be used with find/replace operations and should correctly work with BNF stringification.

```csharp
var parser = new DeferredParser<TInput, TOutput>(() => targetParser);
var parser = Deferred(() => targetParser);
```

### Replaceable Parser

The `Replaceable` parser references an inner parser and invokes it transparently. However, the replaceable parser allows the inner parser to be replaced in-place without cloning.  This is useful in cases where you want to make modifications to the parser tree without creating a whole new tree.

```csharp
var parser = Replaceable(innerParser);
var parser = innerParser.Replaceable();
```

If an inner parser is not explicitly specified, the inner parser will be a `Fail` parser. These two lines are equivalent:

```csharp
var parser = Replaceable<TOutput>();
var parser = Replaceable(Fail<TOutput>());
```

It is extremely helpful to name your replaceable parsers so you can quickly find and replace values by name.

