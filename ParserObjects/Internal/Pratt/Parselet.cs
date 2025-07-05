using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ParserObjects.Pratt;
using static ParserObjects.Internal.Assert;

namespace ParserObjects.Internal.Pratt;

/// <summary>
/// User-configured rule, which acts as an adaptor from IParser to IParselet.
/// Is mostly used as a collection of configured internal values and should not be accessed
/// directly from user code.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class Parselet<TInput, TValue, TOutput> : IParselet<TInput, TOutput>
{
    private readonly IParser<TInput, TValue> _match;
    private readonly NudFunc<TInput, TValue, TOutput>? _nud;
    private readonly LedFunc<TInput, TValue, TOutput>? _led;

    public Parselet(int tokenTypeId, IParser<TInput, TValue> match, NudFunc<TInput, TValue, TOutput>? nud, LedFunc<TInput, TValue, TOutput>? led, int lbp, int rbp, string name)
    {
        TokenTypeId = tokenTypeId;
        _match = NotNull(match);
        _nud = nud;
        _led = led;
        Lbp = lbp;
        Rbp = rbp;
        Name = name ?? _match.Name ?? (TokenTypeId > 0 ? TokenTypeId.ToString() : match.ToString()) ?? string.Empty;
    }

    /* At each iteration of the engine, it is going to attempt to invoke all available Parselets,
     * in order by how they were defined by the user. The Parselets invoke their Parsers to return
     * an Result<TValue>, which is fed into the NED or LED delegate callbacks to produce a
     * ValueToken<TOutput>.
     *
     * TValue is the return value of the parser, but the Engine only works in terms of TInput and
     * TOutput. So we do not return raw TValue values from here.
     */

    public int TokenTypeId { get; }

    public int Lbp { get; }

    public int Rbp { get; }

    public string Name { get; }

    public IParser Parser => _match;

    public bool CanNud => _nud != null;

    public bool CanLed => _led != null;

    /*
     * It is possible for _nud and _led callbacks to throw exceptions which are not ParseException
     * in those cases we do not catch it, but instead allow the exceptions to bubble up to a Try()
     * parser or to another place for the user to catch and examine it.
     */

    public (bool Success, ValueToken<TOutput> Token, int Consumed) TryGetNextNud(IParseState<TInput> state, Engine<TInput, TOutput> engine, ParseControl parseControl)
    {
        Debug.Assert(CanNud);
        Debug.Assert(_nud != null);

        var startCp = state.Input.Checkpoint();
        var result = _match.Parse(state);
        if (!result.Success)
            return default;

        var context = new PrattParseContext<TInput, TOutput>(state, engine, Rbp, result.Consumed > 0, Name, parseControl);
        try
        {
            var resultValue = _nud!(context, new ValueToken<TValue>(TokenTypeId, result.Value, Lbp, Rbp));
            var token = new ValueToken<TOutput>(TokenTypeId, resultValue, Lbp, Rbp);
            return (true, token, state.Input.Consumed - startCp.Consumed);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Rule)
        {
            startCp.Rewind();
            return default;
        }
    }

    public (bool Success, ValueToken<TOutput> Token, int Consumed) TryGetNextLed(IParseState<TInput> state, Engine<TInput, TOutput> engine, ParseControl parseControl, ValueToken<TOutput> left)
    {
        Debug.Assert(CanLed);
        Debug.Assert(_led != null);

        var startCp = state.Input.Checkpoint();
        var result = _match.Parse(state);
        if (!result.Success)
            return default;

        var context = new PrattParseContext<TInput, TOutput>(state, engine, Rbp, true, Name, parseControl);
        try
        {
            var resultValue = _led!(context, left, new ValueToken<TValue>(TokenTypeId, result.Value, Lbp, Rbp));
            var resultToken = new ValueToken<TOutput>(TokenTypeId, resultValue, Lbp, Rbp);
            return (true, resultToken, state.Input.Consumed - startCp.Consumed);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Rule)
        {
            startCp.Rewind();
            return default;
        }
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => Name;

    [ExcludeFromCodeCoverage]
    public INamed SetName(string name)
        => throw new InvalidOperationException("Cannot rename an internal parselet");
}
