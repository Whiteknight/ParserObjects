# Parser Usage

## Parser Results

Parser `.Parse()` calls generally do not throw or propagate exceptions, except in some rare cases. For the most part results will be communicated using an `IResult<T>` object. You can create a result object using the `Result.Fail<T>()` or `Result.Success<T>(value)` methods. You can also call `.Fail<T>()` or `.Success<T>(value)` methods on the `ParseState<T>` object, to create values, which may help to fill in some additional metadata. Parse results should usually include the `Location` where the failure happened, so that information can be communicated to the user.

Results may be transformed from one type to another without losing metadata using the `.Transform(value => ... )` method.

## Matching and Parsing

Parsing requires a `ParseState<T>` object, which can be built from an `ISequence<T>`. Sequences, likewise can come from streams, strings or enumerables or other sources. See the page on [Sequences for more information](sequences.md).

```csharp
var result = parser.Parse(parseState);
var result = parser.Parse(sequence);
var result = parser.Parse("abcdef");
```

A parse consumes input from the sequence, and matches the pattern from the current location in the sequence. If the parser does not consume all input from the sequence, there may be more data available, and the parser may be invoked on it again to get another result. Alternatively, a different parser may be invoked on the sequence to get a new result with whatever remaining data there is:

```csharp
var result1 = parser1.Parse(sequence);
var result2 = parser2.Parse(sequence);
...
```

### Untyped Parsing

Sometimes you want to parse without having to specify the output type. You can use the `.ParseUntyped()` method to return an object value:

```csharp
var result = parser.ParseUntyped(state);
```

## Composing

The strength of the Combinators approach to parsing is that you can compose large parsers from many small parsers. For example, if you have a parser to match a single letter, you can combine that with the list parser to get a list of all letters in a row. Then you can combine that with a parser to transform a list of characters to a string.

```csharp
var parser = Letter().List().Transform(l => new string(l.ToArray()));
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

    (visibility, _, returnType, _, name, _, parameters, _, body) => new MethodDeclaration(visibility, returnType, name, parameters, body)
)
```

You can nest all your declarations together in a single variable like the above example, or you can break separate parsers out into separate variables. Assigning a parser to a named variable helps with readability and makes parsers reusable:

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
    (visibility, _, returnType, _, name, _, parameters, _, body) => new MethodDeclaration(visibility, returnType, name, parameters, body)
)
```

## Naming, Finding and Replacing

You can give a parser a name using the `.Named("name")` extension method. You can also set the `.Name` property on the parser.

```csharp
parser = parser.Named("myParser");
var parser = new MyParser().Named("myParser");
var parser = new MyParser() { Name = "myParser" };
```

Names are useful for debugging purposes, so you can see which parser is executing. You can also search for parsers by name if you have a composed parser:

```csharp
var needleParser = haystackParser.FindNamed("myParser");
```

The real power of this mechanism is the ability to replace a parser at runtime.

```csharp
var result = parser.Replace("myParser", newParser);
```

The replacement parser must be assignable to the same `IParser<TInput, TOutput>` as the original, because all parsers are type safe.

## Stringification and BNF

You can generate a string of pseudo-BNF ([Backus-Naur Form](https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form)) with the `.ToBnf()` extension method:

```csharp
var bnf = parser.ToBnf();
```

This string is useful primarily as an auditing or debugging tool, to make sure that parsers implement the grammar you expect. Some parsers cannot be completely reduced to BNF form (`Sequential` and `Function` parsers, for example). 

For BNF stringification to work, parsers must have a name. Only parsers which have a name will get a line of BNF generated in the output. It is a very good idea to use `.Named("")` in general, and it is nearly mandatory if you want to have usable BNF stringification.

**Note**: The output of `.ToBnf()` is described as "pseudo-BNF" on purpose. The output string is not guaranteed to be standards-compliant, machine-readable BNF, and in fact should be assumed not to be. It is a mixture of BNF, RegularExpression pattern syntax and human-readable notes, and is generally intended for reading by humans as a tool for verification and debugging only. 

## General Visiting

Because a single parser may be reused multiple times during composition, parsers form a *directed graph* instead of a tree. The use of the `Deferred` parser means that graphs may be `cyclic`. If you are trying to create a visitor to traverse a parser graph, you must account for cycles. The `FindParserVisitor` (used for the `.FindNamed()` and `.Replace()` methods above) and the `BnfStringifyVisitor` (used for the `.ToBnf()` method above) are good examples of visiting the graph and doing analysis or modification.

## Handling End-Of-Input

The input sequence will return end sentinel values when it reaches the end of input, and it will also set the `.IsAtEnd` property to true. Either of these cases can be used to fail a match. If you are writing your own `IParser<TInput, TOutput>` implementation or you are using something like the `Function` parser, you can detect either case:

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
})
```

Another concern is a parser which expects to parse the entirety of the input string. Leaving some input unparsed will result in an error. There are two ways to handle this case. First you can include the `End` parser in your parser graph to make sure that your parse hits the end of input in order to make a successful result, or you can check the input sequence after the parse has concluded to make sure it is empty:

```csharp
// Option 1: End must match or the whole rule fails
var parser = Rule(
    innerParser,
    End(),
    (value, end) => value
);
```

```csharp
// Option 2: run the parser and check for end of input yourself
var result = parser.Parse(sequence);
if (!sequence.IsAtEnd)
    throw new MyParseIncompleteException("Parser did not reach the end!");
```

Some sequences, such as the `StringCharacterSequence` include non-standard methods to get remaining unparsed characters:

```csharp
var sequence = new StringCharacterSequence("a+b-c*d");
var result = parser.Parse(sequence);
if (!sequence.IsAtEnd)
    throw new MyParseIncompleteException("Parser did not parse the end of string: " + sequence.GetRemainder());
```

Not all parsers support this method. It may be prohibitively expensive to `.GetRemainder()` on a file stream of arbitrary length, for example. In some small cases or in debugging situations you can use a `StringCharacterSequence` for testing and use `.GetRemainder()` to help figure out why your parser is not reaching end of input as you expect.
