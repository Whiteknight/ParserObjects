using ParserObjects.Pratt;

namespace ParserObjects.Internal.Pratt;

/// <summary>
/// An IToken variant which is used by the Engine to get a value of the correct type. This
/// type may have a hidden internal Value, which will be exposed to user callbacks but not to
/// the Engine. This interface is not intended for external use.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public interface IParserResultToken<TInput, TOutput> : IPrattToken
{
    /// <summary>
    /// Calculates the null denominator (prefix production) of this token, converting it into
    /// a token of the correct output type.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="engine"></param>
    /// <param name="canRecurse"></param>
    /// <param name="parseControl"></param>
    /// <returns></returns>
    Option<IValueToken<TOutput>> NullDenominator(IParseState<TInput> state, Engine<TInput, TOutput> engine, bool canRecurse, ParseControl parseControl);

    /// <summary>
    /// Calculates the left denominator (suffix production) of this token, converting it into
    /// a token of the correct output type.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="engine"></param>
    /// <param name="canRecurse"></param>
    /// <param name="parseControl"></param>
    /// <param name="left"></param>
    /// <returns></returns>
    Option<IValueToken<TOutput>> LeftDenominator(IParseState<TInput> state, Engine<TInput, TOutput> engine, bool canRecurse, ParseControl parseControl, IPrattToken left);
}
