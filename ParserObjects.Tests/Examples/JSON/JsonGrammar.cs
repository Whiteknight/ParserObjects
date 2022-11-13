using static ParserObjects.Parsers<ParserObjects.Tests.Examples.JSON.JsonToken>;
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

            var openSquareBracket = MatchType(JsonTokenType.OpenSquareBracket);
            var closeSquareBracket = MatchType(JsonTokenType.CloseSquareBracket);

            var end = MatchType(JsonTokenType.End);

            // Setup the deferral to fix the circular reference
            IParser<JsonToken, IJsonValue> valueInner = null;
            var value = Deferred(() => valueInner);
            var valueList = value.ListSeparatedBy(comma);

            var objectProperty = (str, colon, value)
                .Rule((name, _, val) => (name.Value, val));
            var objectPropertyList = objectProperty.ListSeparatedBy(comma);

            var jsonObject = (openCurlyBracket, objectPropertyList, closeCurlyBracket)
                .Rule((_, v, _) => new JsonObject(v))
                .Named("JSON Object");

            var jsonArray = (openSquareBracket, valueList, closeSquareBracket)
                .Rule((_, items, _) => new JsonArray(items))
                .Named("JSON Array");

            valueInner = Predict<IJsonValue>(c => c
                .When(t => t.Type == JsonTokenType.OpenCurlyBracket, jsonObject)
                .When(t => t.Type == JsonTokenType.OpenSquareBracket, jsonArray)
                .When(t => t.Type == JsonTokenType.String, str)
                .When(t => t.Type == JsonTokenType.Number, number)
                .When(_ => true, Fail<IJsonValue>("Unexpected token type"))
            );

            return value.Named("JSON Value");
        }
    }
}
