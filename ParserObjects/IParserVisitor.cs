namespace ParserObjects
{
    public interface IParserVisitor
    {
        IParser Visit(IParser parser);
    }

    public interface ICustomVisitorDispatcher
    {
        IParser VisitCustom(IParser p);
    }
}