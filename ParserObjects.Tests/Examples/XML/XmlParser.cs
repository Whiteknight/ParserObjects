namespace ParserObjects.Tests.Examples.XML;

public static class XmlParser
{
    public static XmlNode Parse(string xml)
    {
        var parser = XmlGrammar.CreateParser();
        var result = parser.Parse(xml);
        return result.Success ? result.Value : null;
    }
}
