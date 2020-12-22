using static ParserObjects.ParserMethods<ParserObjects.Tests.Examples.JSON.JsonToken>;
using static ParserObjects.Tests.Examples.JSON.JsonParsers;

namespace ParserObjects.Tests.Examples.JSON
{
    public static class JsonGrammar
    {
        public static IParser<JsonToken, IJsonValue> CreateParser()
        {
            var number = MatchType(JsonTokenType.Number).Transform(n => new JsonNumber(n.Value));
            var str = MatchType(JsonTokenType.String).Transform(s => new JsonString(s.Value));

            var comma = MatchType(JsonTokenType.Comma);
            var openCurlyBracket = MatchType(JsonTokenType.OpenCurlyBracket);
            var closeCurlyBracket = MatchType(JsonTokenType.CloseCurlyBracket);
            var colon = MatchType(JsonTokenType.Colon);
            var end = MatchType(JsonTokenType.End);

            IParser<JsonToken, IJsonValue> valueInner = null;
            var value = Deferred(() => valueInner);

            var objectProperty = (str, colon, value).Produce((name, _, val) => (name.Value, val));

            var jsonObject = Rule(
                openCurlyBracket,
                objectProperty.ListSeparatedBy(comma),
                closeCurlyBracket,
                (_, v, _) => new JsonObject(v)
            );

            var jsonArray = Rule(
                MatchType(JsonTokenType.OpenSquareBracket),
                value.ListSeparatedBy(comma),
                MatchType(JsonTokenType.CloseSquareBracket),
                (_, items, _) => new JsonArray(items)
            );

            valueInner = Predict<IJsonValue>(c => c
                .When(t => t.Type == JsonTokenType.OpenCurlyBracket, jsonObject)
                .When(t => t.Type == JsonTokenType.OpenSquareBracket, jsonArray)
                .When(t => t.Type == JsonTokenType.String, str)
                .When(t => t.Type == JsonTokenType.Number, number)
                .When(_ => true, Fail<IJsonValue>("Unexpected token type"))
            );

            return value.Named("JSON");
        }
    }
}
