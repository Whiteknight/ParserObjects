namespace ParserObjects
{
    public interface IParseResult<out TOutput>
    {
        bool Success { get; }
        TOutput Value { get; }
        Location Location { get; }

        IParseResult<object> Untype();
    }
}