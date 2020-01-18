namespace ParserObjects
{
    /// <summary>
    /// Base type for parser tree visitors.
    /// </summary>
    public interface IParserVisitor
    {
        /// <summary>
        /// Visit the parser tree. Returns a new parser tree if modifications are made. Returns the same
        /// parser tree otherwise. 
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        IParser Visit(IParser parser);
    }
}