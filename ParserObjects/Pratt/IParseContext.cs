namespace ParserObjects.Pratt
{
    /// <summary>
    /// Parse context provides a limited view of the Pratt parser engine, so that user callbacks
    /// can interact with the engine to recursively obtain necessary values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IParseContext<TInput, TOutput> : IParser<TInput, TOutput>
    {
        /// <summary>
        /// Gets the contextual data store for the current parse operation.
        /// </summary>
        IDataStore Data { get; }

        /// <summary>
        /// Invoke the Pratt engine recursively to obtain the next output value, with the current
        /// right binding power. Aborts the current user callback if the parse fails.
        /// </summary>
        /// <returns></returns>
        TOutput Parse();

        /// <summary>
        /// Invoke the Pratt engine recursively to obtain the next output value, using the given
        /// right binding power. Aborts the current user callback if the parse fails.
        /// </summary>
        /// <param name="rbp"></param>
        /// <returns></returns>
        TOutput Parse(int rbp);

        /// <summary>
        /// Invoke the give parser to obtain a value, or aborts the current user callback if the
        /// parse fails.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        TValue Parse<TValue>(IParser<TInput, TValue> parser);

        /// <summary>
        /// Parse a value on the input, but do not return a value. If the value does not
        /// exist, the parse aborts, the input rewinds, and the parser returns to a previous
        /// level of recursion.
        /// </summary>
        /// <param name="parser"></param>
        void Expect(IParser<TInput> parser);

        /// <summary>
        /// Attempt to invoke the Pratt engine recursively to obtain the next output value, with
        /// the current right binding power. Does not abort on failure.
        /// </summary>
        /// <returns></returns>
        IOption<TOutput> TryParse();

        /// <summary>
        /// Attempt to invoke the Pratt engine recursively to obtain the next output value, with
        /// the given right binding power. Does not abort on failure.
        /// </summary>
        /// <param name="rbp"></param>
        /// <returns></returns>
        IOption<TOutput> TryParse(int rbp);

        /// <summary>
        /// Attempts to invoke the given parser to obtain a value. Does not abort on failure.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        IOption<TValue> TryParse<TValue>(IParser<TInput, TValue> parser);

        /// <summary>
        /// Unconditionally fail the current user callback and return to the current recursion
        /// level of the engine to try again.
        /// </summary>
        /// <param name="message"></param>
        void FailRule(string message = "");

        /// <summary>
        /// Unconditionally fail the current recursion level of the engine, and return to the
        /// previous recursion level to try again.
        /// </summary>
        /// <param name="message"></param>
        void FailLevel(string message = "");

        /// <summary>
        /// Unconditionally fail the entire Pratt parser.
        /// </summary>
        /// <param name="message"></param>
        void FailAll(string message = "");
    }
}
