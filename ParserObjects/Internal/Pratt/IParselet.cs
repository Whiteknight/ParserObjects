using ParserObjects.Pratt;

namespace ParserObjects.Internal.Pratt;

/// <summary>
/// A parselet is an adaptor over IParser, with additional metadata used internally by the
/// Pratt parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public interface IParselet<TInput, TOutput> : INamed
{
    /// <summary>
    /// Gets a user-supplied token ID value, which is attached to all tokens generated by this
    /// parselet. The token ID value is not used internally by the engine and is only for use
    /// by the user.
    /// </summary>
    int TokenTypeId { get; }

    /// <summary>
    /// Gets the left binding power of tokens produced by this parselet.
    /// </summary>
    int Lbp { get; }

    /// <summary>
    /// Gets the right binding power of tokens produced by this parselet.
    /// </summary>
    int Rbp { get; }

    /// <summary>
    /// Gets the parser which is used by this parselet to match values.
    /// </summary>
    IParser Parser { get; }

    /// <summary>
    /// Attempts to get the next NUD token from the input sequence.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="engine"></param>
    /// <param name="parseControl"></param>
    /// <returns></returns>
    (bool success, ValueToken<TOutput> token, int consumed) TryGetNextNud(IParseState<TInput> state, Engine<TInput, TOutput> engine, ParseControl parseControl);

    /// <summary>
    /// Attepts to get the next LED token from the input sequence.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="engine"></param>
    /// <param name="parseControl"></param>
    /// <param name="left"></param>
    /// <returns></returns>
    (bool success, ValueToken<TOutput> token, int consumed) TryGetNextLed(IParseState<TInput> state, Engine<TInput, TOutput> engine, ParseControl parseControl, ValueToken<TOutput> left);

    /// <summary>
    /// Gets a value indicating whether this parselet can be used as a null denominator.
    /// </summary>
    public bool CanNud { get; }

    /// <summary>
    /// Gets a value indicating whether this parselet can be used as a left denominator.
    /// </summary>
    public bool CanLed { get; }

    /* Note: We need this interface as an adaptor because Parselet<TInput, TValue, TOutput> has a
     * "hidden" TValue type parameter, which is different for each parselet and cannot be handled
     * by the Engine. Engine works in terms of TInput and TOutput only.
     */
}
