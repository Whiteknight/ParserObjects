namespace ParserObjects;

/// <summary>
/// State information about parse, including the input sequence, logging and contextual data.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public interface IParseState<TInput>
{
    /// <summary>
    /// Gets a contextual data store which parsers may access during the parse.
    /// </summary>
    DataStore Data { get; }

    /// <summary>
    /// Gets the current input sequence.
    /// </summary>
    ISequence<TInput> Input { get; }

    /// <summary>
    /// Gets the current result cache.
    /// </summary>
    IResultsCache Cache { get; }

    /// <summary>
    /// Logs a message for the user to help with debugging.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="message"></param>
    void Log(IParser parser, string message);
}
