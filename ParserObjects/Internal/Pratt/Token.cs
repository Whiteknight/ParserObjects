using ParserObjects.Pratt;

namespace ParserObjects.Internal.Pratt;

// Simple token type which wraps a value from the input stream and metadata necessary to
// work with it inside the Engine. This class is for (mostly) internal use only and shouldn't
// be used directly except through the provided abstractions.
public sealed class ParseletToken<TInput, TValue, TOutput> : IValueToken<TValue>, IParserResultToken<TInput, TOutput>
{
    private readonly Parselet<TInput, TValue, TOutput> _parselet;

    public ParseletToken(Parselet<TInput, TValue, TOutput> parselet, TValue value)
    {
        _parselet = parselet;
        Value = value;
        TokenTypeId = _parselet.TokenTypeId;
        LeftBindingPower = _parselet.Lbp;
        RightBindingPower = _parselet.Rbp;
    }

    public int TokenTypeId { get; }
    public TValue Value { get; }

    public int LeftBindingPower { get; }
    public int RightBindingPower { get; }
    public string Name => _parselet.Name;

    public Option<IValueToken<TOutput>> NullDenominator(IParseState<TInput> state, Engine<TInput, TOutput> engine, bool canRecurse, ParseControl parseControl)
    {
        var context = new PrattParseContext<TInput, TOutput>(state, engine, _parselet.Rbp, canRecurse, Name, parseControl);
        return _parselet.Nud(context, this);
    }

    public Option<IValueToken<TOutput>> LeftDenominator(IParseState<TInput> state, Engine<TInput, TOutput> engine, bool canRecurse, ParseControl parseControl, IPrattToken left)
    {
        var context = new PrattParseContext<TInput, TOutput>(state, engine, _parselet.Rbp, canRecurse, Name, parseControl);
        return _parselet.Led(context, left, this);
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
}

public sealed class ValueToken<TValue> : IValueToken<TValue>
{
    public ValueToken(int typeId, TValue value, int lbp, int rbp, string name)
    {
        TokenTypeId = typeId;
        Value = value;
        LeftBindingPower = lbp;
        RightBindingPower = rbp;
        Name = name;
    }

    public int TokenTypeId { get; }
    public TValue Value { get; }

    public int LeftBindingPower { get; }
    public int RightBindingPower { get; }
    public string Name { get; }

    public override string ToString() => Value?.ToString() ?? string.Empty;
}
