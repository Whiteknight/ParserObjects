namespace ParserObjects.Pratt;

// A token which includes a parsed value and some metadata about it's context.
public readonly record struct ValueToken<TValue>(
    int TokenTypeId,
    TValue Value,
    int LeftBindingPower,
    int RightBindingPower,
    string Name
)
{
    public override string ToString() => Value?.ToString() ?? string.Empty;
}
