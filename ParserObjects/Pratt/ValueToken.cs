namespace ParserObjects.Pratt;

public readonly struct ValueToken<TValue>
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
