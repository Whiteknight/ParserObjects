using static ParserObjects.Parsers<ParserObjects.Tests.Examples.JSON.JsonToken>;

namespace ParserObjects.Tests.Examples.JSON
{
    public static class JsonParsers
    {
        public static IParser<JsonToken, JsonToken> MatchType(JsonTokenType type)
            => Match(t => t.Type == type);
    }
}
