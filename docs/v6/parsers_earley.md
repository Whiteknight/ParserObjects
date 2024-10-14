# Earley Parser

The [Earley Parsing Algorithm](https://en.wikipedia.org/wiki/Earley_parser) is a powerful parser which can handle ambiguous grammars, left recursion and right recursion. It does this by maintaining a list of all possibilities in progress as the parse proceeds, and then returns a parse forest of all results which match the start symbol. 

To create an Earley parser you must be able to specify your grammar in terms of terminals, non-terminals and productions.
* A `Terminal` in this case is an `IParser<TInput, TOutput>` object, which can parse some input but does not recurse into the Earley engine itself
* A `Production` is a rule that takes a list of terminals and non-terminals and produces a value, similar to the `Rule()` parser
* A `Non-Terminal` is a symbol which contains multiple possible productions.

When you create the `Earley()` parser, you are given a `SymbolFactory` object to create symbols with. Symbols are going to need a unique name, and if you don't supply one yourself the system will generate one for you (though the generated names won't be very helpful to you). Symbols also require the type of value they produce. Here is how to create a symbol named `"S"` with data type `string`:

```csharp
var sym = symbols.New<string>("S");
```

Once you have a symbol you can add productions to it. A production is like a `Rule()` parser, where a series of other symbols or parsers are evaluated in sequence and, if they all match, a production rule is executed to produce a value of the symbol's type (`<string>`, in the example above):

```csharp
sym.Rule(
    parser1, 
    parser2, 
    symbol3, 
    (value1, value2, value3) => ...
);
```

At the end of the setup callback, you should return the *Start Symbol* that the parser will attempt to match. The result value of the Early parser will be all the possible matches of the Start Symbol.

The Earley parser operates at each location by taking three steps:

1. **Prediction**: The parser looks at all the current non-terminal symbols and predicts which rules might match next
2. **Scanning**: The parser attempts to `.Parse()` with all the current terminal parsers to see which ones match at the current position
3. **Completion**: The parser takes any non-terminals which have matched completely, and uses those to advance any other non-terminals which depend on them.

This is an abbreviated description of the algorithm, of course, but it gives a sense for how the algorithm works. Every time the Start Symbol completes, the Earley parser adds that value to the list of results. The parser stops when the input reaches the end, when no more rules match at the current position, or when input stops advancing.

The power and flexibility of being able to specify a grammar without regard for ambiguity or recursion are high, though these are matched by the complexity of handling an `IMultiResult<TOutput>` which may contain multiple successful results of varying lengths. It's worth noting that even final results which are identical might still be returned as duplicates because the result was obtained in multiple separate ways. For example the expression `"4+5*6"` without precedence rules might parse as `"(4+5)*6" = 54` or `"4+(5*6)" = 34`. Extra care needs to be taken around precedence and associativity rules to make sure you get the correct results in the face of multiple possible parses.
