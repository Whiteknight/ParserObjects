# ParserObjects

ParserObjects is a library for object-based parsers, combinators, parsing utilities and abstractions. See the [Documentation](https://whiteknight.github.io/ParserObjects/v3) for more details on usage.

    Install-Package ParserObjects

## Sequences and Parsers

An `ISequence` is similar to an `IEnumerable` or `IEnumerator` in concept, though with a focus on supporting parsing operations. Sequences allow you to `GetNext()` items one at a time, to `Peek()` at items without consuming them, and `PutBack()` an item to rewind the sequence. There are many ways to create a sequence:

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

The ParserObjects library provides many small parsers and parser combinators that can be built up into parsers for large and complex grammars. In addition, ParserObjects provides several pre-made parsers for specialty parsing needs.

## Current Status

ParserObjects library is currently in active development. v2.0.0 is out and is the recommended version to use.
