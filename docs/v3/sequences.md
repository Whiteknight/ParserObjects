# Sequences

`ISequence<T>` is the fundamental input type for the ParserObjects library. It is conceptually similar to `IEnumerable<T>`/`IEnumerator<T>` or the **Iterator** pattern, but it has a few extra abilities specially designed for help with parsing. Unlike `IEnumerable<T>` there are a few invariants and special behaviors enforced by sequences which are important for parsing operations.

## Sequence Invariants and Design Requirements

Sequences provided by ParserObjects all satisfy the following invariants and design requirements, some of which will be described further throughout this page.

1. Sequences should properly report whether they are at the end of input at all times, without needing to read a value first.
2. Sequences should properly report the number of items consumed from the sequence at the current point in time, and also report the correct location in input at the current point in time
3. Sequences should be friendly to the underlying data source
   1. Sequences should only read values from the underlying source once, even on rewind
   2. Sequences should dispose, cleanup or free resources as soon as possible, even if the sequence is still in use
4. Sequences should be able to rewind to a previous position with `O(1)` time complexity
5. Sequences should be able to be read infinitely, and will return a special end sentinel value for all read attempts after the end of input.

## Methods and Usage

### Get Next Item

The `.GetNext()` method returns the next input item from the sequence. If the sequence is past the end of input, an "end sentinel" value will be returned instead. This is usually a default value like `null`, `'\0'` or `0` though the end sentinel value is configurable for some sources. If `null` or a default value is a valid input value, you may need to take extra care to differentiate a "real" input from the sequence from the "synthetic" end sentinel value.

All sequences are infinite, you can continue to read past the end of input for as long as you like, but all values returned after the end of input will be the end sentinel. This behavior allows us to create parsers without having to always check for end of input at every single step. Your parser can read an input, see that it doesn't match the current requirements, and return failure.

#### Newline Handling

To help be cross-platform compatible, all sequences in ParserObjects normalize all line endings to use `\n` only. Windows-style `\n\r` will be transformed to just `\n`. Old-MAC style `\r` will be transformed to `\n`. Several of the pre-built test parsers which rely on newline handling will not understand `\r` characters.

It is important to keep this rule in mind if you need to convert your results back to string: you may need to convert back to the preferred line endings of your current platform.

### Check for End

The special `.IsAtEnd` property will return `true` when the sequence has returned it's last element, `false` otherwise. The `.IsAtEnd` property should be updated immediately when the last item from the sequence is read, you do not need to read the end sentinel in a separate operation to update the flag. This behavior means that, for some sequence types, the underlying source may be advanced ahead of the call to `.GetNext()` so that the end can be planned for. `.IsAtEnd` means that the position of the sequence is at the end of the underlying data source. If `.Checkpoint()`/`.Rewind()` is used to move back to a prior position, `.IsAtEnd` will be updated to `false` even though the underlying data source may be exhausted.

### Rewinding to a Previous Location

The `.Checkpoint()` method creates an `ISequenceCheckpoint`, which is an implementation of the **Memento** pattern. This checkpoint can be very similar conceptually to a **Continuation**. An `ISequenceCheckpoint` object provides a single `.Rewind()` method which can be used to return the sequence to a different position. This allows you to rewind back to a prior position if you attempt a long parse and it fails.

```csharp
var checkpoint = sequence.Checkpoint();
... attempt a parse, consuming zero or more input values ...
checkpoint.Rewind();
```

The `.Checkpoint()`/`.Rewind()` operations are designed to be extremely cheap and fast to use. They are designed to always `O(1)` in time complexity. Rewind is not free, however, so in some cases it may be slightly cheaper to avoid calling rewind if you can easily detect that no input has been consumed.

The `.Rewind()` method can also be used to move the sequence forward to a position, not just backwards. Several specialty parsers use this ability to bounce back and forth along the input sequence, making attempts, rewinding to a previous position, then continuing again from where it left off. 

### Peek at the Next Item

You can inspect the next item from the sequence without consuming it using the `.Peek()` method. Peek is currently limited to returning only 1 item of lookahead. Most sequences optimize `.Peek()` so that it is less expensive than `.Checkpoint()`/`.Rewind()` for a single input value. If you need to lookahead for several input values, you can just read them and use checkpoints to get back to the starting location of your attempt.

### Location

You can get location information about the sequence using the `.CurrentLocation` property. This property will return a `Location` object with information about filename (if available), column number and line number (if available). FileName will be made available for sequences which read from a file, or will be a default value otherwise. Line number is only applicable for character sequences when `\n` characters are detected.

Reported location is based on the current position of the sequence, not the high-water mark of the underlying data source, so reported location will operate correctly across calls to `.Checkpoint()`/`.Rewind()`. 

### Characters Consumed

The `.Consumed` property will tell how many items of input have been consumed from the sequence at the current point in time. For non-text sequences, `.Consumed` may be identical or closely correlated to `.CurrentLocation.Column`, though slightly cheaper to execute. You can use this information to determine if you need to perform a `.Rewind()` or detect if you are getting into an infinite loop.

```csharp
var startConsumed = sequence.Consumed;
... Do some work ...
var endConsumed = sequence.Consumed;
var totalConsumed = endConsumed - startConsumed;
if (totalConsumed == 0)
    ...
```

Built-in parsers which loop, such as `List` will automatically break if they detect that their child parsers are consuming zero input. This is to prevent something like `List(Empty())` from creating an infinite loop in your code. Likewise, several parsers can tell when zero inputs have been consumed, and avoid calls to `.Checkpoint()`/`.Rewind()` (even though such calls are cheap, they are not free).

## Sequence Types

There are several types of sequences which can be used in different situations.

### Read Characters from a String

You can use the `StringCharacterSequence` sequence to return a sequence of `char` from a string. You can create the sequence directly or you can use the `.ToSequence()` extension method on an existing string.

```csharp
var sequence = new StringCharacterSequence("abcd");
var sequence = "abcd".ToSequence();
```

This sequence converts line endings to `\n`, it ignores `\r` characters, and uses `\0` as the end sentinel.

### Read Characters from a File or Stream

You can read characters from a file or from any other stream using the `StreamCharacterSequence` or the `.ToCharSequence()` extension method:

```csharp
var sequence = new StreamCharacterSequence("my-file-name.txt");
var sequence = new StreamCharacterSequence(stream);
var sequence = stream.ToCharSequence();
```

Once you instantiate the sequence, ParserObjects expects to have exclusive use of the Stream. Concurrent reads of the stream from other locations in your code may lead to unexpected results and missing data.

This sequence converts line endings to `\n`, it ignores `\r` characters, and uses `\0` as the end sentinel.

`StreamCharacterSequence` implements `IDisposable`, you should try to dispose it after use to avoid holding streams open for too long. It will automatically dispose the underlying stream when it reaches the end of the stream, even though the sequence may continue to be used. 

### Read Bytes from a File or Stream

Analogous to the `StreamCharacterSequence` is the `StreamByteSequence` which reads bytes from a file or stream.

```csharp
var sequence = new StreamByteSequence("my-file-name.txt");
var sequence = new StreamByteSequence(stream);
var sequence = stream.ToByteSequence();
```

`StreamByteSequence` is `IDisposable`, you should try to dispose it when you are done with your parsing operation to avoid leaks. Reading from the stream concurrently from other places in your code will cause data loss and unexpected behavior. 

This sequence uses `0` as the end sentinel value.

### Convert an Enumerable to a Sequence

You can convert `IEnumerable<T>` to `ISequence<T>` using an extension method:

```csharp
var sequence = enumerable.ToSequence();
```

Modifying the contents of the underlying `IEnumerable` while the parse is happening may lead to unexpected errors.

This sequence uses a default value (`null` in most cases where objects are used) as the end sentinel, though it also provides a mechanism for you to supply your own end sentinel. Your user-supplied end sentinel value should properly implement `.Equals` so that end values can be detected.

### Filtering Sequences

You can filter sequences to remove unwanted values using the `FilterSequence` decorator:

```csharp
var filtered = sequence.Where(i => !char.IsControl(i));
var filtered = new FilterSequence<char>(sequence, i => !char.IsControl(i));
```

The filter sequence decorator uses the same end sentinel value as the underlying sequence. The end sentinel is not subject to filtering and will be returned whether it matches the predicate or not.

### Mapping Values

You can map values from a sequence to another type or format using the `MapSequence` decorator.

```csharp
var mapped = sequence.Select(i => char.ToUpper(i));
var mapped = new MapSequence<char, char>(sequence, i => char.ToUpper(i));
```

End sentinel values will be mapped and may be cached, so care should be taken to properly handle that case.

### Convert a Parser To a Sequence

You can convert the outputs of an `IParser` into a sequence. This is useful for scanner-based parser designs, where characters are read into tokens, and then tokens are input into the parser. You must provide both the parser and the input to the parser to create the new sequence.

```csharp
var newSequence = parser.ToSequence(innerSequence);
var newSequence = new ParseResultSequence<char, Token>(parser, innerSequence)
```

The [expression parsing example](expression_example.md) uses the `.ToSequence()` extension to convert a parser into a sequence. The end sentinel for this sequence is whatever value the underlying parser produces at end-of-input. This end sentinel value, when detected, will be cached so that the parser will not continually have to recreate the value. If the underlying parser does not correct handle end-of-input, it may return a failure result, which should be handled correctly later in the parse.

## Parsing

To use a sequence for parsing, you can pass the sequence to the `IParser<TInput>.Parse()` method

```csharp
var result = parser.Parse(sequence);
```

In this operation, the parser will read as many input values from the sequence as necessary to make the match, and any additional data will be left in the sequence. Subsequent calls to parse with the same sequence will continue from the point where the previous parse left off. See the ["Parser Invariants" section of the Parser Usage page](parser_usage.md) for more information about how a parser interacts with a sequence.
