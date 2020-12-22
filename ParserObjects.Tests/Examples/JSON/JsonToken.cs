namespace ParserObjects.Tests.Examples.JSON
{
    public class JsonToken
    {
        public JsonToken(string value, JsonTokenType type)
        {
            Value = value;
            Type = type;
        }

        public string Value { get; set; }
        public JsonTokenType Type { get; set; }
    }
}
