﻿using static ParserObjects.JavaScriptStyleParserMethods;
using static ParserObjects.ParserMethods;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Tests.Examples.JSON
{
    public static class JsonLexer
    {
        public static IParser<char, JsonToken> CreateParser()
        {
            var whitespace = OptionalWhitespace();

            var str = StrippedString()
                .Transform(s => new JsonToken(s, JsonTokenType.String));

            var number = NumberString()
                .Transform(n => new JsonToken(n, JsonTokenType.Number));

            var comma = Match(',')
                .Transform(_ => new JsonToken(",", JsonTokenType.Comma));

            var openSquareBracket = Match('[')
                .Transform(_ => new JsonToken("[", JsonTokenType.OpenSquareBracket));
            var closeSquareBracket = Match(']')
                .Transform(_ => new JsonToken("]", JsonTokenType.CloseSquareBracket));

            var openCurlyBracket = Match('{')
                .Transform(_ => new JsonToken("{", JsonTokenType.OpenCurlyBracket));
            var closeCurlyBracket = Match('}')
                .Transform(_ => new JsonToken("}", JsonTokenType.CloseCurlyBracket));

            var colon = Match(':')
                .Transform(_ => new JsonToken(":", JsonTokenType.Colon));

            var end = If(End(), Produce(() => new JsonToken("", JsonTokenType.End)));

            var token = (
                str,
                number,
                comma,
                openSquareBracket,
                closeSquareBracket,
                openCurlyBracket,
                closeCurlyBracket,
                colon,
                end
            ).First();

            return (whitespace, token)
                .Produce((_, t) => t)
                .Named("token");
        }
    }
}
