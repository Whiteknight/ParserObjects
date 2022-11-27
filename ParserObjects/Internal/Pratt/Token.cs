using ParserObjects.Pratt;

namespace ParserObjects.Internal.Pratt;

// Simple token type which wraps a value from the input stream and metadata necessary to
// work with it inside the Engine. This class is for (mostly) internal use only and shouldn't
// be used directly except through the provided abstractions.
public sealed class ParseletToken<TInput, TValue, TOutput> : IPrattToken<TValue>, IPrattToken<TInput, TOutput>
{
    private readonly Parselet<TInput, TValue, TOutput> _parselet;

    public ParseletToken(Parselet<TInput, TValue, TOutput> parselet, TValue value)
    {
        _parselet = parselet;
        Value = value;
        TokenTypeId = _parselet.TokenTypeId;
        LeftBindingPower = _parselet.Lbp;
        RightBindingPower = _parselet.Rbp;
        Name = _parselet.Name ?? string.Empty;
        IsValid = true;
    }

    public int TokenTypeId { get; }
    public TValue Value { get; }

    public int LeftBindingPower { get; }
    public int RightBindingPower { get; }
    public bool IsValid { get; }
    public string Name { get; }

    public Option<IPrattToken<TOutput>> NullDenominator(IPrattParseContext<TInput, TOutput> context)
        => _parselet.Nud(context, this);

    public Option<IPrattToken<TOutput>> LeftDenominator(IPrattParseContext<TInput, TOutput> context, IPrattToken left)
        => _parselet.Led(context, left, this);

    public override string ToString() => Value?.ToString() ?? string.Empty;
}

public sealed class ValueToken<TInput, TValue, TOutput> : IPrattToken<TValue>, IPrattToken<TInput, TOutput>
{
    public ValueToken(int typeId, TValue value, int lbp, int rbp, string name)
    {
        TokenTypeId = typeId;
        Value = value;
        LeftBindingPower = lbp;
        RightBindingPower = rbp;
        Name = name;
        IsValid = true;
    }

    public int TokenTypeId { get; }
    public TValue Value { get; }

    public int LeftBindingPower { get; }
    public int RightBindingPower { get; }
    public bool IsValid { get; }
    public string Name { get; }

    public Option<IPrattToken<TOutput>> NullDenominator(IPrattParseContext<TInput, TOutput> context)
        => default;

    public Option<IPrattToken<TOutput>> LeftDenominator(IPrattParseContext<TInput, TOutput> context, IPrattToken left)
        => default;

    public override string ToString() => Value?.ToString() ?? string.Empty;
}
