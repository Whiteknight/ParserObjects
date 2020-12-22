using static ParserObjects.JavaScriptStyleParserMethods;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Examples.JSON
{
    public static class JsonLexer
    {
        public static IParser<char, JsonToken> CreateParser()
        {
            var token = First(
                StrippedString().Transform(s => new JsonToken(s, JsonTokenType.String)),
                NumberString().Transform(n => new JsonToken(n, JsonTokenType.Number)),
                Match(',').Transform(_ => new JsonToken(",", JsonTokenType.Comma)),
                Match('[').Transform(_ => new JsonToken("[", JsonTokenType.OpenSquareBracket)),
                Match(']').Transform(_ => new JsonToken("]", JsonTokenType.CloseSquareBracket)),
                Match('{').Transform(_ => new JsonToken("{", JsonTokenType.OpenCurlyBracket)),
                Match('}').Transform(_ => new JsonToken("}", JsonTokenType.CloseCurlyBracket)),
                Match(':').Transform(_ => new JsonToken(":", JsonTokenType.Colon)),
                If(End(), Produce(() => new JsonToken("", JsonTokenType.End)))
            );

            return Rule(
                OptionalWhitespace(),
                token,
                (_, t) => t
            ).Named("token");
        }
    }
}
