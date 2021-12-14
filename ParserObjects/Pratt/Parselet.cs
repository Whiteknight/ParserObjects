using System;
using ParserObjects.Utility;

namespace ParserObjects.Pratt;

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
        Name = name ?? _match.Name ?? ((TokenTypeId > 0) ? TokenTypeId.ToString() : match.ToString()) ?? string.Empty;
    }

    public int TokenTypeId { get; }
    public int Lbp { get; }
    public int Rbp { get; }
    public string Name { get; }
    public IParser Parser => _match;

    public bool CanNud => _nud != null;

    public bool CanLed => _led != null;

    public (bool success, IToken<TInput, TOutput> token, int consumed) TryGetNext(IParseState<TInput> state)
    {
        var result = _match.Parse(state);
        if (!result.Success)
            return default;
        return (true, new ParseletToken<TInput, TValue, TOutput>(this, result.Value), result.Consumed);
    }

    public IOption<IToken<TOutput>> Nud(IParseContext<TInput, TOutput> context, IToken<TValue> sourceToken)
    {
        if (_nud == null)
            return FailureOption<IToken<TOutput>>.Instance;
        try
        {
            var resultValue = _nud(context, sourceToken);
            var token = new ValueToken<TInput, TOutput, TOutput>(TokenTypeId, resultValue, Lbp, Rbp, Name);
            return new SuccessOption<IToken<TOutput>>(token);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Rule)
        {
            return FailureOption<IToken<TOutput>>.Instance;
        }
    }

    public IOption<IToken<TOutput>> Led(IParseContext<TInput, TOutput> context, IToken left, IToken<TValue> sourceToken)
    {
        if (_led == null || left is not IToken<TOutput> leftTyped)
            return FailureOption<IToken<TOutput>>.Instance;

        try
        {
            var resultValue = _led(context, leftTyped, sourceToken);
            var resultToken = new ValueToken<TInput, TOutput, TOutput>(TokenTypeId, resultValue, Lbp, Rbp, Name);
            return new SuccessOption<IToken<TOutput>>(resultToken);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Rule)
        {
            return FailureOption<IToken<TOutput>>.Instance;
        }
    }

    public override string ToString() => Name;

    public INamed SetName(string name) => throw new InvalidOperationException("Cannot rename an internal parselet");
}
