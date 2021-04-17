# ParserObjects

ParserObjects is a library for object-based parsers, combinators, parsing utilities and abstractions. See the [Documentation](https://whiteknight.github.io/ParserObjects/v3) for more details on usage.

    Install-Package ParserObjects

See the [Test Suite Examples](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples) for some examples of usage.

## Project Goals

The goal of the ParserObjects library is to create an implementation of parser combinators that is easy to use, easy to read, and easy to understand. Notice that parser combinators are not the solution for all parsing problems. As an implementation of the Recursive Descent algorithm, and all LL(k) parsers in general, this algorithm has problems with ambiguous grammars (the first match wins) and left-recursion. For many parsing tasks for which these are not concerns, ParserObjects intends to be a fast and easy way to get started with parsing.

This project has several goals:

1. To use *streaming input* from a variety of sources, instead of reading data into fixed memory buffers
2. To be *generic* as much as possible, supporting common string-parsing algorithms but also being extensible to any structured input type
3. Using good interface design to encourage proper use and good code style
4. To provide an easy on-ramp for people who need to parse data, but are not experts in parsing theory

## Sequences and Parsers

An `ISequence` is similar to an `IEnumerable` or `IEnumerator` in concept, though with a focus on supporting parsing operations. Sequences allow you to `GetNext()` items one at a time, to `Peek()` at items without consuming them, and rewind the sequence to a previous location. There are many ways to create a sequence:

```csharp
using ParserObjects.Sequences;

// Turn an existing IEnumerable<T> into an ISequence<T>
var sequence = myEnumerable.AsSequence();

// Get a character sequence from a string
var sequence = new StringCharacterSequence("...");

// Get a character sequence from a file or stream
var sequence = new StreamCharacterSequence("fileName.txt");
var sequence = new StreamCharacterSequence(myStream);
```

An `IParser` is an object that attempts to match a pattern in an input sequence. If the pattern is matched, the items will be consumed by the parser and a result will be returned. If the parser cannot match at the beginning of the input sequence, it returns a failure and no items are consumed.

```csharp
var result = parser.Parse(sequence);
```

The ParserObjects library provides many small parsers and parser combinators that can be built up into parsers for large and complex grammars. In addition, ParserObjects provides several pre-made parsers for specialty parsing needs. Please see the [Documentation](https://whiteknight.github.io/ParserObjects/v3)  for more information, and the [Test Suite Examples](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples) for real-world usage patterns.

## Current Status

ParserObjects library is currently in active development. **v3.0.0** is available on Nuget and is preferred over all previous versions. v3.0.0 is not backwards-compatible with v2.X or v1.X versions.
