namespace ParserObjects.Tests.Examples.XML
{
    public static class XmlParser
    {
        public static XmlNode Parse(string xml)
        {
            var parser = XmlGrammar.CreateParser();
            var (success, value) = parser.Parse(xml);
            return success ? value : null;
        }
    }
}
