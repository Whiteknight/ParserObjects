# JSON Parser

The JSON parser example showcases the use of the `Predict` parser for selecting among a number of options without having to attempt to match every alternative using a `First` parser. In this particular case the predictions are simple and there is not a huge performance difference between the two approaches.

This example uses a two-phase parser with `JsonLexer` breaking the input into tokens, and the `JsonParser` using the input token stream. The `JsonLexer` ignores leading whitespace before each token. 

The heart of the `JsonGrammar` is this `Predict` parser:

```csharp
valueInner = Predict<IJsonValue>(c => c
    .When(t => t.Type == JsonTokenType.OpenCurlyBracket, jsonObject)
    .When(t => t.Type == JsonTokenType.OpenSquareBracket, jsonArray)
    .When(t => t.Type == JsonTokenType.String, str)
    .When(t => t.Type == JsonTokenType.Number, number)
    .When(_ => true, Fail<IJsonValue>("Unexpected token type"))
);
```

This parser looks at the next token and then decides how to continue. In the case of JSON, if the lookahead item is a `{` we can invoke the object parser, and if it's a `[` we can invoke the array parser. 


