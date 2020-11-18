# Sequences

`ISequence<T>` is the fundamental input type for the ParserObjects library. It is conceptually similar to `IEnumerable<T>`/`IEnumerator<T>` or the Iterator pattern, but it has a few extra abilities specially designed for help with parsing. Unlike `IEnumerable<T>` there are a few invariants and special behaviors enforced by sequences which are important for parsing operations.

## Methods and Usage

### Get Next Item

The `.GetNext()` method returns the next input item from the sequence. If the sequence is past the end of input, an "end sentinel" value will be returned instead. This is usually a default value like `null`, `'\0'` or `0` though the end sentinel value is configurable for most sources. If `null` or a default value is a valid input value, you may need to take extra care to differentiate a "real" input from the sequence from the "synthetic" end sentinel value.

All sequences are infinite, you can continue to read past the end of input for as long as you like, but all values returned after the end of input will be the end sentinel. This behavior allows us to create parsers without having to always check for end of input at every single step.

#### Newline Handling

To help be cross-platform compatible, all sequences in ParserObjects normalize all line endings to use `\n` only. Windows-style `\n\r` will be transformed to just `\n`. Old-MAC style `\r` will be transformed to `\n`.

`\r` characters will be ignored by character sequences in the `.PutBack()` method.

### Put Back an Item

If you read more characters than you need, you can use the `.PutBack()` method to return characters to the sequence. Characters put back in this way are available from the next call to `.GetNext()`. You can put back an item you read from the stream, or you can put back any other value you want. In this way the `ISequence<T>` can behave like a stack for some parsing tasks. You can also read ahead to try and match a long pattern, but then rewind the sequence to the beginning point if the pattern fails to match.

### Peek at the Next Item

You can inspect the next item from the sequence without consuming it using the `.Peek()` method. `.Peek()` is equivalent to the following combination of `.GetNext()` and `.PutBack()`:

```csharp
public static TInput Peek<TInput>(this ISequence<TInput> input)
{
    var next = input.GetNext();
    input.PutBack(next);
    return next;
}
```

Most implementations optimize the Peek operation, however, so it is not implemented as an extension method.

### Check for End

The special `.IsAtEnd` property will return `true` when the sequence has returned it's last element, `false` otherwise. The `.IsAtEnd` property should be updated immediately when the last item from the sequence is read, you do not need to read the end sentinel in a separate operation to update the flag. This behavior means that, for some sequence types, the underlying source may be advanced ahead of the call to `.GetNext()` so that the end can be planned for.

If the sequence is at end, but `.PutBack()` is called, `.IsAtEnd` will be updated to `false`.

### Rewinding to a Previous Location

`.PutBack()` is useful for small operations putting back only a few items onto the sequence. However, for larger rollbacks, it is prohibitive to use `.PutBack()` to put back all the read input values one at a time. Instead, sequences provide the `.Checkpoint()` method, which takes a snapshot of the state of the sequence at the current point in time.

```csharp
var checkpoint = sequence.Checkpoint();
... attempt a parse, consuming zero or more input values ...
checkpoint.Rewind();
```

### Location

You can get location information about the sequence using the `.CurrentLocation` property. This property will return a `Location` object with information about filename (if available), column number and line number (if available). FileName will be made available for sequences which read from a file, or will be a default value otherwise. Line number is only applicable for character sequences when `\n` characters are detected.

ParserObjects tries to keep track of the column number of the previous few end-of-line markers, so that the column number can be updated correctly after `.PutBack('\n')`. However, the sequences only maintain a small buffer of these values, and calling `.PutBack('\n')` too many times may overflow this buffer and cause incorrect values to be be returned from `.CurrentLocation`. This value should be corrected after the next time `\n` is read from `.GetNext()`. If you need to put back a large number of characters, you should use `.Checkpoint()`/`.Rewind()` instead, because this method will properly restore the location buffers and all other state. 

## Sequence Types

There are several types of sequences which can be used in different situations.

### Read Characters from a String

You can use the `StringCharacterSequence` sequence to return a sequence of `char` from a string. You can create the sequence directly or you can use the `.ToSequence()` extension method on an existing string.

```csharp
var sequence = new StringCharacterSequence("abcd");
var sequence = "abcd".ToSequence();
```

### Read Characters from a File or Stream

You can read characters from a file or from any other stream using the `StreamCharacterSequence` or the `.ToCharSequence()` extension method:

```csharp
var sequence = new StreamCharacterSequence("my-file-name.txt");
var sequence = new StreamCharacterSequence(stream);
var sequence = stream.ToCharSequence();
```

Once you instantiate the sequence, ParserObjects expects to have exclusive use of the Stream. Concurrent reads of the stream from other locations in your code may lead to unexpected results and missing data.

`StreamCharacterSequence` implements `IDisposable`, you should try to dispose it after use to avoid holding streams open for too long.

### Read Bytes from a File or Stream

Analogous to the `StreamCharacterSequence` is the `StreamByteSequence` which reads bytes from a file or stream.

```csharp
var sequence = new StreamByteSequence("my-file-name.txt");
var sequence = new StreamByteSequence(stream);
var sequence = stream.ToByteSequence();
```

`StreamByteSequence` is `IDisposable`, you should try to dispose it when you are done with your parsing operation to avoid leaks. Reading from the stream concurrently from other places in your code will cause data loss and unexpected behavior.

### Convert an Enumerable to a Sequence

You can convert `IEnumerable<T>` to `ISequence<T>` using an extension method:

```csharp
var sequence = enumerable.ToSequence();
```

Modifying the contents of the underlying `IEnumerable` while the parse is happening may lead to unexpected errors.

### Filtering Sequences

You can filter sequences to remove unwanted values using the `FilterSequence` decorator:

```csharp
var filtered = sequence.Where(i => !char.IsControl(i));
var filtered = new FilterSequence<char>(sequence, i => !char.IsControl(i));
```

Calling `.PutBack()` with a character which does not match the predicate will be ignored.

### Mapping Values

You can map values from a sequence to another type or format using the `MapSequence` decorator.

```csharp
var mapped = sequence.Select(i => char.ToUpper(i));
var mapped = new MapSequence<char, char>(sequence, i => char.ToUpper(i));
```

Items added to the sequence with `.PutBack()` are expected to already be mapped and will not be mapped a second time.

### Convert a Parser To a Sequence

You can convert the outputs of an `IParser` into a sequence. This is useful for scanner-based parser designs, where characters are read into tokens, and then tokens are input into the parser. You must provide both the parser and the input to the parser.

```csharp
var newSequence = parser.ToSequence(innerSequence);
var newSequence = new ParseResultSequence<char, Token>(parser, innerSequence)
```

The [expression parsing example](expression_example.md) uses the `.ToSequence()` extension to convert a parser into a sequence.

## Parsing

To use a sequence for parsing, you can pass the sequence to the `IParser<TInput>.Parse()` method, or you can use a function to execute a parse operation using the sequence.

```csharp

var result = sequence.Parse((state, success, fail) => {
    if (state.Input.GetNext() == 'a')
        return success("ok");
    return fail();
});