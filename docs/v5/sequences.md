# Sequences

`ISequence<T>` is the fundamental input type for the ParserObjects library. It is conceptually similar to `IEnumerable<T>`/`IEnumerator<T>` or the **Iterator** pattern, but it has a few extra abilities specially designed for help with parsing. Unlike `IEnumerable<T>` there are a few invariants and special behaviors enforced by sequences which are important for parsing operations.

You can get access to factory methods for all sequence types by adding this to the top of your file:

```csharp
using static ParserObjects.Sequences;
```

In addition, several sequence types can be created via extension methods:

```csharp
using ParserObjects;
```

**Note**: Sequences should be created from the factory methods in the `ParserObjects.Sequences` class, or via extension methods in the `ParserObjects` namespace *only*. Trying to create sequence classes directly by calling their constructors is *explicitly not supported*, even if it is technically possible. If you find yourself needing to access functionality of a sequence that is not available from one of the factory methods, please contact the maintainers and ask for that functionality to be added to the supported interface.

## Sequence Invariants and Design Requirements

Sequences provided by ParserObjects all satisfy the following invariants and design requirements, some of which will be described further throughout this page.

1. Sequences should properly report whether they are at the end of input at all times, without needing to read a value first.
2. Sequences should properly report the number of items consumed from the sequence at the current point in time, and also report the correct location in input at the current point in time
3. Sequences should be friendly to the underlying data source
   1. Sequences should read from the underlying data source as infrequently as possible.
   2. Sequences should dispose, cleanup or free resources as soon as reasonably possible.
4. Sequences should be able to rewind to a previous position with `O(1)` time complexity (with the caveat that refilling a buffer may be `O(n)`)
5. Sequences should be able to be read infinitely, and will return a special end sentinel value for all read attempts after the end of input.

## Methods and Usage

### Get Next Item

The `.GetNext()` method returns the next input item from the sequence. If the sequence is past the end of input, an "end sentinel" value will be returned instead. This is usually a default value like `null`, `'\0'` or `0` though the end sentinel value is configurable for most sources. If `null` or a default value is a valid input value, you may need to take extra care to differentiate a "real" input from the sequence from the "synthetic" end sentinel value.

All sequences are infinite, you can continue to read past the end of input for as long as you like, but all values returned after the end of input will be the end sentinel. This behavior allows us to create parsers without having to always check for end of input at every single step. Your parser can read an input, see that it doesn't match the current requirements, and return failure. For example, we can change this:

```csharp
If(End(), Fail(), Match('m'))
```

...to this:

```csharp
Match('m')
```

That is, we don't need to explicitly check for `End()` before every read to avoid exceptions being thrown. In other words, reading from a sequence *should never throw an exception*, so you can focus more energy on being *expressive* and less energy on being *defensive*.

The end sentinel value is usually the C# `default` value for that type, but you can change it in the `SequenceOptions`:

```csharp
var sequence = FromString("...", new SequenceOptions<char> 
{ 
    EndSentinel = 'X' 
});
```

All factory methods for sequences (listed below) will allow passing a `SequenceOptions<T>`.

#### Newline Handling

To help be cross-platform compatible, all sequences in ParserObjects normalize all line endings to use `\n` only by default. Windows-style `\n\r` will be transformed to just `\n`. Old-MAC style `\r` will be transformed to `\n`. 

It is important to keep this rule in mind if you need to convert your results back to string: you may need to convert back to the preferred line endings of your current platform.

You can change this behavior in the `SequenceOptions`:

```csharp
var sequence = FromString("...", new SequenceOptions<char> 
{
    MaintainLineEndings = true 
});
```

### Check for End

The special `.IsAtEnd` property will return `true` when the sequence has returned it's last element, `false` otherwise. The `.IsAtEnd` property should be updated immediately when the last item from the sequence is read, you do not need to read the end sentinel in a separate operation to update the flag. This behavior means that, for some sequence types, the underlying data source may be advanced ahead of the call to `.GetNext()` so that the end can be planned for. `.IsAtEnd` means that the position of the sequence is at the end of the underlying data source. If `.Checkpoint()`/`.Rewind()` is used to move back to a prior position, `.IsAtEnd` will be updated to `false` even though the underlying data source may be exhausted.

### Rewinding to a Previous Location

The `.Checkpoint()` method creates a `SequenceCheckpoint` structure, which is an implementation of the **Memento** pattern. This checkpoint can be very similar conceptually to a **Continuation**. A `SequenceCheckpoint` object provides a single `.Rewind()` method which can be used to return the sequence to a different position. This allows you to rewind back to a prior position if you attempt a long parse and it fails.

```csharp
var checkpoint = sequence.Checkpoint();
// ... attempt a parse, consuming zero or more input values ...
checkpoint.Rewind();
```

You can also call `sequence.Rewind(checkpoint)` on the sequence, for the same effect.

The `.Checkpoint()`/`.Rewind()` operations are designed to be extremely cheap and fast to use. Rewind is not free, however, so in some cases it may be slightly cheaper to avoid calling rewind if you can easily detect that no input has been consumed. If you only need to check ahead one value, the `.Peek()` operation (described next) is often significantly cheaper:

```csharp
var checkpoint = sequence.Checkpoint();
var lookahead = sequence.GetNext();
checkpoint.Rewind();

// this is the same, but faster:
var lookahead = sequence.Peek();
```

The `.Rewind()` method can also be used to move the sequence forward to a position, not just backwards. Several specialty parsers use this ability to bounce back and forth along the input sequence, making attempts, rewinding to a previous position, then continuing again from where it left off. 

### Peek at the Next Item

You can inspect the next item from the sequence without consuming it using the `.Peek()` method. Peek is currently limited to returning only 1 item of lookahead. Most sequences optimize `.Peek()` so that it is less expensive than `.Checkpoint()`/`.Rewind()` for a single input value. If you need to lookahead for several input values, you can just read them and use checkpoints to get back to the starting location of your attempt.

### Location

You can get location information about the sequence using the `.CurrentLocation` property. This property will return a `Location` object with information about filename (if available), column number and line number (if available). `.FileName` will be made available for sequences which read from a file, or will be a default value otherwise. `.Line` number is only applicable for character sequences, and is updated when `\n` characters are detected.

Reported location is based on the current position of the sequence, not the high-water mark of the underlying data source, so reported location will operate correctly across calls to `.Checkpoint()`/`.Rewind()`. 

### Items Consumed

The `.Consumed` property will tell how many items of input have been consumed from the sequence at the current point in time. For non-text sequences, `.Consumed` may be identical or closely correlated to `.CurrentLocation.Column`, though slightly cheaper to execute. You can use this information to determine if you need to perform a `.Rewind()` or detect if you are getting into an infinite loop.

```csharp
var startConsumed = sequence.Consumed;
// ... Do some work ...
var endConsumed = sequence.Consumed;
var totalConsumed = endConsumed - startConsumed;
if (totalConsumed == 0)
    ...
```

Built-in parsers which loop, such as `List` will automatically break if they detect that their child parsers are consuming zero input. This is to prevent something like `List(Empty())` from creating an infinite loop in your code. Likewise, several parsers can tell when zero inputs have been consumed, and avoid calls to `.Checkpoint()`/`.Rewind()` (even though such calls are cheap, they are not free).

Notice also that the `SequenceCheckpoint` structure has a `.Consumed` property which will tell you the number of values consumed at that point. The above code example could easily be rewritten as:

```csharp
var startCheckpoint = sequence.Checkpoint();
var startConsumed = startCheckpoint.Consumed;
// ... Do some work ...
var endConsumed = sequence.Consumed;
var totalConsumed = endConsumed - startConsumed;
if (totalConsumed == 0)
    ...
```

### Cleanup

Some sequences might have disposable resources. Cleaning them up manually will help you control usage of memory and resources. This is especially true for sequences which operate on `Stream`s. It is good practice, if you are creating a sequence manually, to clean it up afterwards:

```csharp
(sequence as IDisposable)?.Dispose();
```

### Checkpoint Ownership

A checkpoint "belongs" to a single sequence, and you cannot use a checkpoint created from one sequence to change the position in another sequence. You can tell if a checkpoint belongs to a given sequence with the `.Owns()` method:

```csharp
if (sequence.Owns(checkpoint))
    ...
```

### Getting spans of items

If you create two checkpoints, you can get an array containing all the values between them with the `.GetBetween()` method:

```csharp
var values = sequence.GetBetween(startCheckpoint, endCheckpoint);
```

Notice that this is an `O(N)` operation, and may require re-reading data from the source, such as re-reading data from a `Stream` if the start or end checkpoints are outside the current buffered data. 

If the `start` checkpoint and the `end` checkpoint are for the same location, or if `start` is after `end`, an empty array will be returned.

### Resetting the Sequence

You can reset the sequence back to the beginning with `.Reset()`.  This is a quick operation, similar to `.Rewind()`, and it may involve a buffer refill for sequences which use buffers.

### Character-based Sequences

`char`-based sequence types implement the `ICharSequence` interface, which is `ISequence<char>` with a few other helpful methods to interact with the data as `string` in some cases. Notice that generic `ISequence<T>` types which may operate on `char` sometimes (such as `FromList<T>()`) will not have these methods.

With an `ICharSequence` you can get the remaining data from the sequence, from the current position until the end of input, as a string:

```csharp
var remainder = sequence.GetRemainder();
```

This can be useful in some cases where you want to tell the user about data which was not successfully parsed, or in testing/debugging scenarios when you want to see where a parse failed.

The sequences which implement this are `FromString()`, `FromCharacterFile()` and `FromCharacterStream()`. Other char-based sequence types added in the future will also have this method. 

## Sequence Types

There are several types of sequences which can be used in different situations.

### Read Characters from a String

One of the most common sources of data to parse is an in-memory string.

```csharp
var sequence = FromString("...");
var sequence = "...".ToCharSequence();
```

By default this sequence converts line endings to `\n` and uses `\0` as the end sentinel. You can change those options with the `SequenceOptions<char>` parameter.

### Read Characters from a File or Stream

You can read characters from a file or from any other stream using the `StreamCharacterSequence` or the `.ToCharSequence()` extension method:

```csharp
var sequence = FromCharacterStream(stream);
var sequence = FromCharacterFile(stream);
var sequence = stream.ToCharSequence();
```

Once you instantiate the sequence, ParserObjects expects to have exclusive use of the Stream. Concurrent reads of the stream from other locations in your code may lead to unexpected results and missing data.

This sequence converts line endings to `\n`, it ignores `\r` characters, and uses `\0` as the end sentinel. These values can be changed with the `SequenceOptions<char>` parameter.

**Notice**: The exact implementation returned from each of these methods may be different depending on the length of the stream and the encoding used. Simpler cases (where the stream length is shorter than the size of the internal buffer, or when a single-byte encoding such as ASCII is used) may be able to return optimized sequence types. *Code to the `ISequence<char>` abstraction and do not depend on any specific concrete implementation*.

**Notice**: Multi-byte encodings read from a long stream have a limitation that *a checkpoint cannot be created between the low and high characters of a surrogate pair*. This is due to a problem calculating the exact position of the underlying stream when we are between parts of a single codepoint which uses a variable-length encoding. For example, if we have a UTF8 encoded input stream with a 4-byte astral plane codepoint, which C# converts to two `char` values encoded in UTF16, it's not meaningful to ask what the position is of the stream in the middle of that code point. The stream reader cannot be reset into the middle of that codepoint, because it cannot understand the last few bytes without having the first byte available. If you are dealing with variable-length encodings such as `UTF8` or `UTF16` and may be parsing characters with surrogate pairs, you cannot call `.Checkpoint()`, `.GetBetween()`, or `.GetRemainder()` in the middle of the surrogate pair. `.Peek()` and `.GetNext()` will work in that situation, however. The underlying sequence will throw an exception for malformed input if there is an unmatched high or low surrogate, so if your code reads a high surrogate you should be able to read the low surrogate with confidence that you will receive it, so do you not need to set a checkpoint between them.

### Read Bytes from a File or Stream

You can read bytes from a stream or file.

```csharp
var sequence = FromByteFile("my-file-name.txt");
var sequence = FromStream(stream);
var sequence = stream.ToByteSequence();
```

This sequence uses `0` as the default end sentinel value, but may be configured to use something else with the `SequenceOptions<byte>` parameter.

### Convert an `IReadOnlyList<T>` to a Sequence

You can convert `IReadOnlyList<T>` to `ISequence<T>` using an extension method:

```csharp
var sequence = FromList(list);
var sequence = list.ToSequence();
```

You can also convert an `IEnumerable<T>` to a sequence, but ParserObjects will read the entire enumerable into an `IReadOnlyList<T>` first because there is no other way to provide `.Rewind()` mechanics on an `IEnumerable<T>`:

```csharp
var sequence = FromEnumerable(enumerable);
var sequence = enumerable.ToSequence();
```

This sequence uses a default value (`null` in most cases where objects are used) as the end sentinel, though you can change this with the `SequenceOptions<T>` parameter. Your user-supplied end sentinel value should properly implement `.Equals` so that end values can be detected. End sentinel values will be cached and not recomputed on each call to `.GetNext()`. 

**Notice**: If you use `FromEnumerable()` with a `string` argument, you will not get any of the special character-handling of the character sequences described previously. It will not normalize newlines, keep track of `.Line` and `.Column` correctly, or implement the `.GetRemainder()` method. If your data is character-based, you should try to use one of the char-based sequence types instead.

### Filtering Sequences

You can filter sequences to remove unwanted values:

```csharp
var filtered = sequence.Where(i => !char.IsControl(i));
```

The filter sequence decorator uses the same end sentinel value as the underlying sequence. The end sentinel is not subject to filtering and will be returned whether it matches the predicate or not.

**Notice**: Metadata such as `.Consumed` or `.CurrentLocation` will be based on the values from the underlying sequence only. Therefore the `.Consumed` or `.CurrentLocation` values may update more than one at a time if values are filtered out from the underlying sequence. 

### Mapping Values

You can map values from a sequence to another type or format:

```csharp
var mapped = sequence.Select(i => char.ToUpper(i));
```

End sentinel values will be mapped and may be cached, so care should be taken to properly handle that case.

**Notice**: State is maintained by the underlying sequence, so calling `checkpoint.Rewind()` to a previous position will cause data to be re-read and re-mapped. For this reason, your mapping operation should not be expensive, or should try to return cached values for items which are equivalent regardless of position or parse state.

### Convert a Parser To a Sequence

You can convert the outputs of an `IParser` into a sequence. This is useful for scanner-based parser designs, where characters are read into tokens, and then tokens are input into the parser. You must provide both the parser and the input to the parser to create the new sequence.

```csharp
var newSequence = FromParseResult(innerSequence, parser);
var newSequence = parser.ToSequence(innerSequence);
```

The [expression parsing example](expression_example.md) uses the `.ToSequence()` extension to convert a parser into a sequence. The end sentinel for this sequence is whatever value the underlying parser produces at end-of-input. This end sentinel value, when detected, will be cached so that the parser will not continually have to recreate the value. If the underlying parser does not correctly handle end-of-input, it may return a failure result, which should be handled correctly later in the parse.

**Notice**: Metadata such as `.Consumed` or `.CurrentLocation` will be based on the values from the underlying sequence only. Therefore the `.Consumed` or `.CurrentLocation` values may update more than one at a time if values are filtered out from the underlying sequence. 

## Parsing

To use a sequence for parsing, you can pass the sequence to the `IParser<TInput>.Parse()` method

```csharp
var result = parser.Parse(sequence);
```

In this operation, the parser will read as many input values from the sequence as necessary to make the match, and any additional data will be left in the sequence. Subsequent calls to parse with the same sequence will continue from the point where the previous parse left off. See the ["Parser Invariants" section of the Parser Usage page](parser_usage.md) for more information about how a parser interacts with a sequence.

If your parser has a `char` input and your data comes from a string, you can call this method variant, which invokes `FromString()` internally:

```csharp
var result = parser.Parse("...");
```

## Statistics

All sequences allow accessing statistics, which are useful in some cases to understand performance. 

```csharp
var stats = sequence.GetStatistics();
```

The statistics returned are a read-only snapshot. You must call `.GetStatistics()` again to get updated values.
