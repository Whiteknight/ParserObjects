namespace ParserObjects
{
    public interface IParserVisitor
    {
        IParser Visit(IParser parser);
    }
}