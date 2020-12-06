# Parser Objects

**ParserObjects** is a library for combinator-based parsers. Combinators are the same general algorithm as *Recursive Descent*, but using objects instead of functions to represent grammar production rules. Some benefits of this algorithm include:

1. **Dynamic**: You can easily construct or dynamically modify parser object graphs at runtime (normal Recursive Descent uses method references, which are generally set at compile time)
1. **Declarative**: Declaration of parsers can closely resemble a pseudo-BNF grammar for easy readability

The Wikipedia page on [Parsing](https://en.wikipedia.org/wiki/Parsing#Computer_languages) gives a good overview of parsing in general and the page on [Parser Combinators](https://en.wikipedia.org/wiki/Parser_combinator) cover a lot of the important topics and also point to other resources for continued reading.

## Key Concepts and Abstractions

ParserObjects defines a few important abstractions.

### `ISequence<T>`

`ISequence<T>` is very similar to `IEnumerable<T>`/`IEnumerator<T>`. It allows you to stream items from a source one by one, but it also provides a few additional features which `IEnumerable<T>`/`IEnumerator<T>` don't include. See the page on [Sequence reference and usage](sequences.md) for detailed information.

### `IResult<T>`

The `IResult<T>` abstraction represents the result of a parser execution. It communicates success or failure, and on success it may include the parsed value. The result may also include metadata about the result (location, a custom error message, etc).

### `IParser<TInput, TOutput>`

`IParser<TInput, TOutput>` represents a parser object, and is the core component of the ParserObjects library. 

See the page on [Parser Usage](parser_usage.md) for information about using parsers. 

* [Generic Parsers Reference](parsers_core.md)
* [String/Character Parser Reference](parsers_chars.md)
* [Programming Parser Reference](parsers_programming.md)

## Examples

See the following pages for examples

* [Expression Parsing Example](expression_example.md)