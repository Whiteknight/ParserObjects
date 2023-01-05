# Parser Objects

**ParserObjects** is a library for parsers and parser combinators. Combinators are the same general algorithm as *Recursive Descent*, but using objects instead of functions to represent grammar production rules. Some benefits of this approach include:

1. **Dynamic**: You can easily construct or dynamically modify parser object graphs at runtime (normal Recursive Descent uses method references, which are generally set at compile time)
1. **Declarative**: Declaration of parsers can closely resemble a pseudo-BNF grammar for easy readability

The Wikipedia page on [Parsing](https://en.wikipedia.org/wiki/Parsing#Computer_languages) gives a good overview of parsing in general and the page on [Parser Combinators](https://en.wikipedia.org/wiki/Parser_combinator) cover a lot of the important topics and also point to other resources for continued reading. See the [Test Suite Examples](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples) for examples of use and implementation patterns.

## Project Goals

The ParserObjects project intends to be an easy, usable and flexible parsing solution for the .NET ecosystem. The goal of this project is to be an on-ramp for developers who need to parse data but aren't necessarily experts in parsing theory. You don't need to know BNF or CFG or the details of any parsing algorithm in order to get started with ParserObjects. Parsers should be easy to build, the code should be easy to read, and the examples should be easy to understand.

This library also intends to showcase the Combinators approach to parsing, and the underlying ethos of being able to compose solutions together from reusable components. 

The developers of this library believe very strongly in the compositional approach. Large solutions are made by composing together smaller solutions. We also believe that all abstractions leak. For this reason a design decision has been made which runs counter to how many other large library projects organize things: Almost every class in this library is explicitly marked `public`, even the ones that we recommend users do not access directly.

The `ParserObjects.Internal` namespace contains many classes and methods which are available to you, but are **explicitly not supported in any way**. Significant "breaking" changes may be made to the contents of this namespace without increasing the major version number of the library, and without offering any warning,  explanation or assistance in migrating to the new implementations. We provide this access so that you have the power to compose your own solutions using the same building blocks that we use. If you find yourself using and relying upon a bit of functionality from the `ParserObjects.Internal` namespace, and would like to have it added to the supported interface of the library, please contact the development team and we will be happy to help.

## Key Concepts and Abstractions

ParserObjects defines a few important abstractions.

### `ISequence<T>`

`ISequence<T>` is very similar to `IEnumerable<T>`/`IEnumerator<T>`. It allows you to stream items from a source one by one, but it also provides a few additional features which `IEnumerable<T>` and `IEnumerator<T>` don't include. See the page on [Sequence reference and usage](sequences.md) for detailed information. Sequences are the basic input type for parsers.

You can get access to available sequence types in your code by statically including:

```csharp
using static ParserObjects.Sequences;
```

### `IResult<T>` and `IMultiResult<T>`

The `IResult<T>` and `IMultiResult<T>` abstractions represent the result of a parser execution. Results communicate success or failure. On success it includes the derived value and on failure it includes an error message. The result also includes metadata about how many inputs were consumed and the location in the input stream where the match occurred, among other things.

### `IParser<TInput, TOutput>` and `IMultiParser<TInput, TOutput>`

`IParser<TInput, TOutput>` and `IMultiParser<TInput, TOutput>` and related interfaces represent a parser object. These are the core component of the ParserObjects library. Parsers take an `ISequence<TInput>` of input values and return an `IResult<TOutput>` or `IMultiResult<TOutput>` indicating success or failure.

See the page on [Parser Usage](parser_usage.md) for information about using parsers. 

You can get access to most parser types in your code by statically including:

```csharp
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;
```

* [Core Parsers Reference](parsers_core.md)
* [Logical Parsers Reference](parsers_logical.md)
* [String/Character Parser Reference](parsers_chars.md)
* [Programming Parser Reference](parsers_programming.md)
* [Pratt Parser Reference](parsers_pratt.md)
* [Multi Parsers Reference](parsers_multi.md)
* [Earley Parser Reference](parsers_earley.md)

## Examples

See the following pages for examples:

* [Expression Parsing Example](expression_example.md)
* [Pratt Expression Parsing Example](prattexpr_example.md)
* [Earley Expression Parsing Example](earleyexpr_example.md)

There are also several small examples of usable in [the test suite](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples).
