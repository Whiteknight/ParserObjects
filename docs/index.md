# Parser Objects

**ParserObjects** is a library for combinator-based parsers. Combinators are the same general algorithm as *Recursive Descent*, but using objects instead of functions to represent grammar production rules. Some benefits of this algorithm include:

1. **Dynamic**: You can easily construct or dynamically modify parser object graphs at runtime (normal Recursive Descent uses method references, which are generally set at compile time)
1. **Declarative**: Declaration of parsers can closely resemble a pseudo-BNF grammar for easy readability

The Wikipedia page on [Parsing](https://en.wikipedia.org/wiki/Parsing#Computer_languages) gives a good overview of parsing in general and the page on [Parser Combinators](https://en.wikipedia.org/wiki/Parser_combinator) cover a lot of the important topics and also point to other resources for continued reading.

## Key Concepts and Abstractions

ParserObjects defines a few important abstractions.

### `ISequence<T>`

`ISequence<T>` is very similar to `IEnumerable<T>'/`IEnumerator<T>`. It allows you to stream items from a source one by one, but it also provides a few additional features which `IEnumerable<T>'/`IEnumerator<T>` don't include:

1. `.GetNext()` gets the next item from the stream and advances the stream one position
1. `.Peek()` returns the next item from the stream but does not advance the stream
1. `.PutBack(T item)` pushes the item onto the head of the stream. They can be retrieved again with `.GetNext()` or `.Peek()`, first-in, first-out.
1. `.CurrentLocation` returns an approximate description of the current location in the source. If the source is a text file, for example, the location will include file name, line and column.
1. `.IsAtEnd` returns true if the sequence is at the end, false otherwise

A sequence is expected to always return a result even when it is at the end. A sequence should define some kind of sentinel value that can be returned when the sequence is at the end.

### `IParseResult<T>`

The `IParseResult<T>` abstraction returns the result of a parser. 

1. `.Success` a boolean flag which says whether the parse succeeded or failed. 
1. `.Value` the result value of the parse. If `.Success` is false, this value is invalid and should not be used.
1. `.Location` the approximate location of the parse result from the input stream. 
1. `.Transform<T2>()` transform the `IParseResult<T>` into `IParseResult<T2>`

### `IParser<TInput, TOutput>`

`IParser<TInput, TOutput>` is the core parser definition. The most important methods are:

1. `.Parse()` attempts to parse. Takes an input `ISequence<TInput>` and returns a `IParseResult<TOutput>`
1. `.ParseUntyped()` attempts to parse, returns an `IParseResult<object>`. This is frequently implemented in terms of `.Parse()` with the output cast to object.
1. `.GetChildren()` returns all the child parsers referenced by the current parser 
1. `.ReplaceChild()` returns a new parser, identical to the current parser, with one of it's child parsers replaced (non-recursive)

## Pages

* [Core Parsers Reference](parsers_core.md)
* [Expression Parsing Example](expression_example.md)