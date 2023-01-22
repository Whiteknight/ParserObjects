# Parser Usage

A **Parser** is an object which takes an input sequence, and attempts to match some kind of rule or pattern starting from the current location. Parsers in the ParserObjects library are all denoted with the `IParser`, `IParser<TInput>`, `IParser<TInput, TOutput>`, `IMultiParser<TInput>` or `IMultiParser<TInput, TOutput>` interfaces, depending on use.

Generally speaking there are two types of parsers. A **Single Parser** and a **Multi Parser**. A Single Parser parses the input sequence and returns a single result with pass/fail status and a value or error message. A multi parser may return many results from the current position. It is up to the user to decide which results, if any, should be used to continue the parse.

## Creating Parsers

Parsers can be created from static factory methods in the `ParserObjects.Parsers` class, or from extension methods in the `ParserObjects` namespace. Once you have created the base parsers from these places, you can compose them together to form parsers for larger and more complicated grammars.

**Note**: You should only use the static factory methods and extension methods when creating new parsers. You should not be directly accessing the parser classes, or calling constructors yourself. The library technically allows this for advanced use cases, but it is *explicitly not supported*. Subsequent releases of the library may change class names or constructor parameters on a whim, while the factory methods and extension methods described will stay the same.

## Parser Interfaces

The `IParser` interface is the parent interface that all parsers must inherit. It offers very little functionality by itself.

`IParser<TInput>` is a parser which can call `.Parse()` on an input to return an `IResult`. Parsers which inherit this interface can match a pattern or rule, but may not necessarily produce meaningful output. When using this interface, the goal is typically to tell if the pattern matches or not, or to do a match without knowing the type of the output result.

`IParser<TInput, TOutput>` is a parser which can call `.Parse()` on an input and will return an `IResult<TOutput>`. This is more common than the `IParser<TInput>` type and should be preferred where possible because of the extra strong-typing.

The `IMultiParser<TInput>` is a parser which can call `.Parse()` on an input to return an `IMultiResult`. Parsers which inherit this interface can match a pattern or rule, but may not necessarily produce meaningful output. When using this interface, the goal is typically to tell if the pattern matches or not, or to do a match without knowing the type of the output result.

`IMultiParser<TInput, TOutput>` is a parser which can call `.Parse()` on an input and will return an `IMultiResult<TOutput>` of several possible parse results from the current position.

## Parser Invariants

`IParser` implementations in the ParserObjects library all follow these rules and invariants:

1. On failure, the parser consumes no data from the input sequence, and the input sequence is returned to the state it was in before the parse was attempted.
2. On success, the parser consumes only the data it needed to match a pattern and construct the result value. All other data including buffers and lookaheads, are returned to the input sequence. 
3. All parsers are built to be composed. They only read input data starting from the current position of the input sequence, and the parser does not necessarily expect to read until the end of input. 
4. Parsers will generally try to communicate failure through a result value and will try not to throw exceptions unless some major invariant has been violated. However, many parsers take user callback delegates, which may throw exceptions. Exceptions thrown from user code will be allowed to bubble up to a user handler, and may leave the input sequence in an invalid or incomplete state.
5. Parsers should not return `null` but should always return a valid `IResult` or `IResult<TOutput>` with a valid non-null result value.

`IMultiParser<>` types have two caveats to these rules, because the multiple results may have consumed different amounts of input:
1. The MultiParser should always rewind the input to the position it was in before the parse
2. Each successful alternative result will include a continuation which can return the input to the state it was in at the end of the parse for that value

We will discuss multi parsers and working with multiple results in more detail later on.

All the core parser types try to be completely generic in terms of input type. Parsers can generally operate on sequences of `char` just as easily as on sequences of `byte`, or custom token objects. All internal parser types are, or are considered to be, immutable. A parser will not allow it's data to be changed and attempts to change data may result in a new parser instance being created. There are a few counter-examples, but these should be considered the rare exception and not treated as permission to have parsers with mutable data.

It is possible to implement your own `IParser` classes in your own project. When doing so, it is strongly recommended that your parser types are designed to follow these invariants as well, and keep your parsers immutable.

## Parser Results

Parser `.Parse()` calls generally do not throw or propagate exceptions, except in some rare cases. For the most part results will be communicated using an `IResult<TOutput>` or `IMultiResult<TOutput>` object. Parse results should usually include the `Location` where the success or failure happened, so that information can be communicated to the user.

Result values may be transformed from one type to another without losing metadata using the `.Transform(value => ... )` method.

## Matching and Parsing

Matching and Parsing require a `ParseState<T>` object, which can be built from an `ISequence<T>`. Sequences, likewise can come from streams, strings or enumerables or other sources. See the page on [Sequences for more information](sequences.md).

### Matching

All `IParser<TInput>` types define a `.Match()` method which takes a `ParseState<TInput>` and returns a boolean. If `true`, the match has succeeded and the input sequence has been advanced but no result value has been constructed or returned. If `false`, the match was not successful and the input sequence was returned to it's original position.

```csharp
var ok = parser.Match(parseState);
var ok = parser.Match(sequence);
var ok = parser.Match("abcdef");
```

`.Match()`, when it can be used, is a performance optimization over `.Parse()`. Matching does fewer memory allocations overall, and frequently does less computation. If you need a result value or a descriptive error message from the parser, the `.Parse()` method must be used instead.

**Note**: The `.Match()` method is designed to consume input and return true or false. It cannot account for situations where a user callback function may have thrown an exception in a `.Parse()` method call. For this reason, when a parser uses a user callback function delegate, the `.Match()` call is not strictly equivalent to `.Parse().Success`.

### Parsing

Parsing is where input tokens are consumed from the input sequence and result value is generated. A parse result contains a lot of information including a `.Success` flag, a value or an error message, a `Location` where the result (or error) came from, and possibly some custom data as well.

```csharp
var result = parser.Parse(parseState);
var result = parser.Parse(sequence);
var result = parser.Parse("abcdef");
```

A parse consumes input from the sequence, and matches the pattern from the current location in the sequence. If the parser does not consume all input from the sequence there may be more data available and the parser may be invoked on it again to get another result. Alternatively, a different parser may be invoked on the sequence to get a new result with whatever remaining data there is:

```csharp
var result1 = parser1.Parse(sequence);
var result2 = parser2.Parse(sequence);
...
```

### Untyped Parsing

`IParser<TInput>` and `IMultiParser<TInput>` define parsers which can parse an input, but do not return a typed output. In addition, `IParser<TInput, TOutput>` and `IMultiParser<TInput, TOutput>` implement `IParser<TInput>` and `IMultiParser<TInput>` respectively, and so can parse without specifying output type information. This parse method variant will return an object which may contain the parsed value, but as an `object` instead of as a specific type.

```csharp
var result = unTypedParser.Parse(state);
var result = ((IParser<TInput>)typedParser).Parse(state);
```

Some parsers such as `End()` do not return a meaningful value, only success or failure. Notice that, because an `object` result is returned, untyped parse is not equivalent to `.Match()`. If you truely do not need the result value, it is an optimization to use `.Match()` instead.

## Composing

The strength of the Combinators approach to parsing is that you can compose large parsers from many small parsers. For example, if you have a parser to match a single letter, you can combine that with the `List` parser to get a list of all letters in a row. Then you can combine that with a parser to transform a list of characters to a string.

```csharp
var parser = Letter()
    .List()
    .Transform(l => new string(l.ToArray()));
```

To match something more complicated, like a parameterless C# method declaration for example, we can combine many parsers together:

```csharp
var methodDeclareParser = Rule(
    // Visibility modifier
    First(
        CharacterString("public"),
        CharacterString("private")
    ),
    Whitespace(),

    // Return type
    Word(),
    Whitespace(),

    // Method name
    Word(),
    OptionalWhitespace(),

    Rule(
        Match('('),
        Match(')'),
        (open, close) => new ParameterList()
    ),
    OptionalWhitespace(),
    Rule(
        Match('{'),
        Match('}'),
        (open, close) => new MethodBody();
    ),

    (visibility, _, returnType, _, name, _, parameters, _, body) 
        => new MethodDeclaration(visibility, returnType, name, parameters, body)
)
```

You can nest all your declarations together in a single variable like the above example, or you can break separate parsers out into separate variables. Assigning a parser to a named variable helps with readability and makes parsers more reusable:

```csharp
var ws = Whitespace();
var ows = OptionalWhitespace();
var word = Word();

var visibility = First(
    CharacterString("public"),
    CharacterString("private")
);

var parameterList = Rule(
    Match('('),
    Match(')'),
    (open, close) => new ParameterList()
);

var methodBody = Rule(
    Match('{'),
    Match('}'),
    (open, close) => new MethodBody();
);

var methodDeclareParser = Rule(
    visibility,
    ws,
    word,
    ws,
    word,
    ows,
    parameterList,
    ows,
    methodBody,
    (visibility, _, returnType, _, name, _, parameters, _, body) 
        => new MethodDeclaration(visibility, returnType, name, parameters, body)
)
```

## Naming, Finding and Replacing

All `IParser` instances will have a unique `Id` value. These Id values should be unique across the entire application. If you are writing your own custom `IParser` implementation, care should be taken to make sure you set this unique Id value for every instance. 

```csharp
public int Id { get; } = ParserObjects.Utility.UniqueIntegerGenerator.GetNext();
```

If you know the Id of a parser, you can find it:

```csharp
var needleParser = haystackParser.ById(25);
```

Parsers may optionally have a name to help with debugging, stringification, find/replace operations and other situations. The `.Named("...")` extension method will create a new parser, with all data copied from the original parser, with the specified name. If the parser already has the desired name, the same instance will be returned. It is recommended that the names of parsers should be unique, though this is not required. Some operations, such as find/replace will assume names are unique and will return only the first instance with the given name. 

```csharp
var newParser = parser.Named("myParser");
```

Names are useful for debugging purposes, so you can see which parser is executing. You can also search for parsers by name if you have a composed parser:

```csharp
var needleParser = haystackParser.FindNamed("myParser").Value;
```

The real power of this mechanism is the ability to replace a parser at runtime. This is done by using the special `ReplaceableParser` and the `.Replace()` method:

```csharp
// By name
var result = parser.Replace("myParser", replacement);

// By predicate
var result = parser.Replace(p => IsWhatWeAreLookingFor(p), replacement);

// By reference
var result = parser.Replace(oldParser, newParser);
```

You can also create a replacement in a callback:

```csharp
var result = parser.Replace("myParser", oldParser => CreateNewParser(oldParser));
```

The replacement parser must be assignable to the same `IParser<TInput, TOutput>` as the original, because all parsers are type safe. If the replacement is not the correct type, the replace operation will fail.

Parsers **must** have a unique Id value but they may not have a name and those names might not be unique if they are set.

## Stringification and BNF

You can generate a string of pseudo-BNF ([Backus-Naur Form](https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form)) with the `.ToBnf()` extension method:

```csharp
var bnf = parser.ToBnf();
```

This string is useful primarily as an auditing or debugging tool, to make sure that parsers implement the grammar you expect. Some parsers cannot be completely reduced to BNF form (`Sequential` and `Function` parsers, for example). Other parsers will be converted to something which is not standards-compliant BNF, but which may still be useful in debugging.

For BNF stringification to work properly, parsers must have a name. Only parsers which have a name will get a line of BNF generated in the output. It is a very good idea to use `.Named("...")` in general, and it is nearly mandatory if you want to have usable BNF stringification.

**Note**: The output of `.ToBnf()` is described as "pseudo-BNF" on purpose. The output string is not guaranteed to be standards-compliant, machine-readable BNF, and in fact should be assumed not to be. It is a mixture of BNF, RegularExpression pattern syntax and human-readable notes, and is generally intended for reading by humans as a tool for verification and debugging only. 

## General Visiting

Because a single parser may be reused multiple times during composition, parsers form a *directed graph* instead of a tree. The use of the special `Deferred` parser means that graphs may be `cyclic`. If you are trying to create a visitor to traverse a parser graph, you must account for duplicate visits and cycles. The `FindParserVisitor` (used for the `.FindNamed()` and `.Replace()` methods above) and the `BnfStringifyVisitor` (used for the `.ToBnf()` method above) are good examples of visiting the graph and doing analysis or modification.

## Handling End-Of-Input

The input sequence will return **end sentinel** values when it reaches the end of input, and it will also set the `.IsAtEnd` property to `true`. Either of these cases can be used to fail a match. If you are writing your own `IParser<TInput, TOutput>` implementation or you are using something like the `Function` parser, you can detect either case:

```csharp
var parser = Function(sequence => {

    // Option 1: Explicitly check for end
    if (sequence.IsAtEnd)
        return Result.Fail<string>();
    
    // Option 2: GetNext, the end sentinel won't match so the branch won't be 
    // taken
    if (sequence.GetNext() == 'a')
        return Result.Success<string>("ok");
    return Result.Fail<string>();

    // Option 3: Look explicitly for the end sentinel value
    if (sequence.Peek() == '\0')
        return Result.Fail<string>();
    return Result.Success<string>("ok");
})
```

Another concern is a parser which expects to parse the entirety of the input string. In these cases, leaving some input unparsed should result in an error. There are two ways to fulfill this requirement. First you can include the `End` parser in your parser graph to make sure that your parse hits the end of input in order to make a successful result. Second, you can check the input sequence after the parse has concluded to make sure it is empty:

```csharp
// Option 1: End must match or the whole rule fails
var parser = Rule(
    innerParser,
    If(End(), Produce(() => true)),
    (value, end) => value
);
```

```csharp
// Option 2: run the parser and check for end of input yourself
var result = parser.Parse(sequence);
if (!sequence.IsAtEnd)
    throw new MyParseIncompleteException("Parser did not reach the end!");
```

Some sequences, such as `FromString()` include a `.GetRemainder()` method to get remaining unparsed characters:

```csharp
var sequence = FromString("a+b-c*d");
var result = parser.Parse(sequence);
if (!sequence.IsAtEnd)
    throw new MyParseIncompleteException("Parser did not parse the end of string: " + sequence.GetRemainder());
```
