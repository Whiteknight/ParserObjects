namespace ParserObjects.Pratt
{
    public interface IParseContext<TInput, TOutput>
    {
        TOutput Parse();

        TOutput Parse(int rbp);

        TValue Parse<TValue>(IParser<TInput, TValue> parser);

        /// <summary>
        /// Parse a value on the input, but do not return a value. If the value does not
        /// exist, the parse aborts, the input rewinds, and the parser returns to a previous
        /// level of recursion.
        /// </summary>
        /// <param name="parser"></param>
        void Expect(IParser<TInput> parser);

        (bool success, TOutput value) TryParse();

        (bool success, TOutput value) TryParse(int rbp);

        (bool success, TValue value) TryParse<TValue>(IParser<TInput, TValue> parser);

        void FailRule(string message = null);

        void FailLevel(string message = null);

        void FailAll(string message = null);
    }
}
