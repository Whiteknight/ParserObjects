# Multi Parsers

Multi Parsers, using the `IMultiParser<TInput>` and `IMultiParser<TInput, TOutput>` interfaces, handle ambiguous grammars where multiple valid parses might exist starting from the current input location. Often times this is undesirable because we want our grammars to be unambiguous. However there are cases where we might want to consider multiple possible parses and be able to select from them. Either we can show the options to the user with suggestions to select one, or we can provide a selection criteria ourselves.

In the literature, the collection of multiple return values from such a parser is called a "Parse Forest".

## Multi Results

An `MultiResult` or `MultiResult<TOutput>` contains a list of all possible results from the parser. This list may be empty or it may contain many values. Each `Alternative<TOutput>` may represent a success or a failure. The `MultiResult` is considered a success if any alternative in the list is successful. Otherwise, if the list is empty or if all alternatives are failures, the `MultiResult` is a failure. 

## Fundamental Parser Types 

Several of the [Fundamental Parser Types](parsers_core.md) have variants which work with multi parsers. Several of these work identically to the normal single-parser variants already described. Sometimes these have `Multi` suffix to disambiguate them when the parameter list is not enough to help the compiler decide on which to use.

* `Cache` caches the value of the multi-parser so that subsequent calls to `.Parse()` at the same location return a cached result
* `CreateMulti` creates a multi-parser using contextual information
* `Deferred` returns a multi-parser reference
* `Examine` examines parse state before and after the multi parser
* `FailMulti` returns a multi-result which is a failure
* `ProduceMulti` takes a list of literal values and returns an `MultiResult<TOutput>` with each value as an alternative
* `Replaceable` allows a multi-parser instance to be replaced
* `Transform` transforms the values in each alternative
* `TransformResult` transforms the entire multi result object
* `TrieMulti` uses a trie to find all matches with the given prefix sequence
* `Try` executes the parser and captures any exceptions thrown

## Multi Parser Types

### Each

The `Each` parser takes a list of `IParser<TInput, TOutput>` and attempts each from the current position. The results from each parser are returned as alternatives of a single multi-result. This is useful in cases where we may want to explore multiple possibilities and select the "best" one once each path has been attempted. 

```csharp
var parser = Each(p1, p2, p3...);
```

The `Each()` parser is similar in concept and implementation to the `First()` parser, though `First()` returns only the first successful result while `Each()` returns all possible results and allows selecting of a single continuation based on user-defined criteria. We can approximate the single-result `First` parser like this:

```csharp
var parser = Each(p1, p2, p3)
    .First(r => true);
```

### Earley

The `Earley` parser implements the [Earley Algorithm](https://en.wikipedia.org/wiki/Earley_parser) which can handle ambiguous grammars and both left- and right-recursion in rules. To create an Earley parser:

```csharp
var parser = Earley<TOutput>(symbols => { ... });
```

For more information see the page on [Earley Parsers](parsers_earley.md) and see the example where we use the Earley parser to parse [mathematical expressions](earleyexpr_example.md).

## Continuing Parsers

Once we have executed a multi parser and received a multi-result, we can continue the parse with all alternatives simultaneously. This is helpful in cases where we have parsed a sub-expression, but want to see if some of the alternatives will allow the remainder of the parse to complete successfully.

### Continue With

The `ContinueWith` parser continues the parse by treating each result alternative as the starting point and passing each one individually to the next parser in line. The parser which continues may be either a single- or multi-parser. If a continuation fails, that alternative is removed. If a continuation succeeds, that result (or results) is added to the new `MultiResult<TOutput>`.

```csharp
var parser = multiParser.ContinueWith(result => ...create a parser...);
```

### Continue With Each

The `ContinueWithEach` parser is a combination of `ContinueWith` and `Each`. The multiparser is executed, and then the result value is used to construct a list of individual parsers to execute for each result.

```csharp
var parser = multiParser.ContinueWithEach(result => {
    return new[] {
        p1,
        p2,
        p3,
        ...
    }
})
```

### Transform

The `Transform` parser transforms all of the successful values.

```csharp
var parser = multiParser.Transform(r => ...);
```

## Result Selecting Parser Types

Result-selecting parsers convert an `IMultiParser<TInput>` into an `IParser<TInput>` or an `IMultiParser<TInput, TOutput>` into an `IParser<TInput, TOutput>` by selecting a single "best" result from the list of all result alternatives. These selection parsers return success if the selected alternative is a success, or they report failure if the selected alternative is a failure or if no result is selected.

### First

The `First` parser selects the first matching result from the list of possibilities, or returns failure.

```csharp
var parser = multiParser.First(r => ...);
```

**Notice**: that the order of results from a multi-parser may not necessarily be deterministic. The output results of an `Each` parser will be in the same order as the declaration order of the parsers passed to `Each()`, but for some parsers like `Earley` the order of results may not be able to be determined ahead of time. Using `First` then may not return a result you expect.

### Longest

The `Longest` parser selects the result which consumed the most input items from the input sequence. 

```csharp
var parser = multiParser.Longest();
```

If there is a tie, the first option is selected. As per the note above, "first" is problematic because it is not deterministic.

### Select

The `Select` parser allows the user to select the best result based on any criteria. `Select` is used to implement the other selection parsers described in this section. The select callback takes three parameters, the multiresult, a factory to create a success result and a factory to create a failure result.

```csharp
var parser = multiParser.Select((result, success, fail) => {
    // return fail() if no good option is found:
    return fail();

    // return success() if a good option is found:
    return success(result.Results.First());
});
```

### Single

The `Single` parser expects there to be only a single successful result. This is a way to indicate that the multi-parser is not expected to represent an ambiguous grammar. This parser returns a single result (success or failure) if there is only one, or returns failure if there are 0 alternatives or if there is more than one alternative.

```csharp
var parser = multiParser.Single();
```



