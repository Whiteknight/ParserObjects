using System;
using ParserObjects.Internal.Utility;
using ParserObjects.Pratt;

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
        Assert.ArgumentNotNull(match, nameof(match));
        TokenTypeId = tokenTypeId;
        _match = match;
        _nud = nud;
        _led = led;
        Lbp = lbp;
        Rbp = rbp;
        Name = name ?? _match.Name ?? (TokenTypeId > 0 ? TokenTypeId.ToString() : match.ToString()) ?? string.Empty;
    }

    /* At each iteration of the engine, it is going to invoke all available Parselets. The Parselets
     * invoke their Parsers to return an IResult<TValue>, which in turn is converted to an
     * IParserResultToken<TInput, TOutput> (notice that the token's TValue result is "hidden" here
     * by the interface). This is because the Token can "get" a TOutput by passing the TValue to
     * the user callback Nud or Led function. IParserResultToken<TInput, TOutput> returns a TOutput
     * by transforming it's hidden TValue value to TOutput.
     *
     * IParserResultToken<TInput, TOutput>.NullDenominator or .LeftDenominator call Parselet.Nud
     * or Parselet.Led, respectively to do the transformation, and return an IValueToken<TOutput>.
     * That IValueToken<TOutput> is then used in the next Engine iteration as the "left" value.
     */

    public int TokenTypeId { get; }
    public int Lbp { get; }
    public int Rbp { get; }
    public string Name { get; }
    public IParser Parser => _match;

    public bool CanNud => _nud != null;

    public bool CanLed => _led != null;

    public (bool success, IParserResultToken<TInput, TOutput> token, int consumed) TryGetNext(IParseState<TInput> state)
    {
        var result = _match.Parse(state);
        if (!result.Success)
            return default;
        return (true, new ParseletToken<TInput, TValue, TOutput>(this, result.Value), result.Consumed);
    }

    public Option<IValueToken<TOutput>> Nud(PrattParseContext<TInput, TOutput> context, IValueToken<TValue> sourceToken)
    {
        if (_nud == null)
            return default;
        try
        {
            var resultValue = _nud(context, sourceToken);
            var token = new ValueToken<TOutput>(TokenTypeId, resultValue, Lbp, Rbp, Name);
            return new Option<IValueToken<TOutput>>(true, token);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Rule)
        {
            return default;
        }
    }

    public Option<IValueToken<TOutput>> Led(PrattParseContext<TInput, TOutput> context, IPrattToken left, IValueToken<TValue> sourceToken)
    {
        if (_led == null || left is not IValueToken<TOutput> leftTyped)
            return default;

        try
        {
            var resultValue = _led(context, leftTyped, sourceToken);
            var resultToken = new ValueToken<TOutput>(TokenTypeId, resultValue, Lbp, Rbp, Name);
            return new Option<IValueToken<TOutput>>(true, resultToken);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Rule)
        {
            return default;
        }
    }

    public override string ToString() => Name;

    public INamed SetName(string name) => throw new InvalidOperationException("Cannot rename an internal parselet");
}
