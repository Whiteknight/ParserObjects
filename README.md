# ParserObjects

ParserObjects is a library for object-based parsers, combinators, parsing utilities and abstractions. See the [Documentation](https://whiteknight.github.io/ParserObjects/v5) for more details on usage.

    Install-Package ParserObjects

See the [Test Suite Examples](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples) for some examples of usage.

## Project Goals

This project has several goals:

1. To enable composing of parsers from smaller building blocks
2. To enable crafting of parsers using a variety of techniques and algorithms, whichever is the best fit for your project
3. To use *streaming input* from a variety of sources, instead of reading data into fixed memory buffers
4. To be *generic* as much as possible, supporting common string-parsing algorithms but also being extensible to any structured input type
5. Using good interface design to encourage proper use and good code style
6. To provide an easy on-ramp for people who need to parse data, but are not experts in parsing theory

## Sequences and Parsers

An `ISequence` is similar to an `IEnumerable` or `IEnumerator` in concept, though with a focus on supporting parsing operations. Sequences allow you to `GetNext()` items one at a time, to `Peek()` at items without consuming them, and rewind the sequence to a previous location. There are many ways to create a sequence:

```csharp
using static ParserObjects.Sequences;

// Turn an existing IReadOnlyList<T> into an ISequence<T>
var sequence = FromList(myList);

// Get a character sequence from a string
var sequence = FromString("...");

// Get a character sequence from a file or stream
var sequence = FromCharacterFile("fileName.txt");
var sequence = FromCharacterStream(myStream);
```

An `IParser` is an object that attempts to match a pattern in an input sequence. If the pattern is matched, the items will be consumed by the parser and a result will be returned. If the parser cannot match at the beginning of the input sequence, it returns a failure and no items are consumed. There are several dozen different types of parsers in the library, which can be found by including this in your file:

```csharp
using static ParserObjects.Parsers<char>;
```

Once you have a parser, you can pass it an input sequence to obtain a parse result:

```csharp
var result = parser.Parse(sequence);
```

The ParserObjects library provides many small parsers and parser combinators that can be built up into parsers for large and complex grammars. In addition, ParserObjects provides several pre-made parsers for specialty parsing needs. Please see the [Documentation](https://whiteknight.github.io/ParserObjects/v4) for more information, and the [Test Suite Examples](https://github.com/Whiteknight/ParserObjects/tree/master/ParserObjects.Tests/Examples) for real-world usage patterns.

## Contributing

We welcome contributions including code, feedback, bug reports and suggestions.

## Current Status

ParserObjects library is currently in active development. **v5.0.0** is available on Nuget and is preferred over all previous versions. 

