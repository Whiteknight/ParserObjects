using static ParserObjects.Sequences;

namespace ParserObjects.Tests.Examples.JSON;

public class JsonDeserializer
{
    public IJsonValue Deserialize(string json)
    {
        var input = FromString(json);
        var lexer = JsonLexer.CreateParser();
        var tokens = lexer.ToSequence(input).Select(r => r.Value);
        var parser = JsonGrammar.CreateParser();
        var result = parser.Parse(tokens);
        return result.Success ? result.Value : null;
    }
}
