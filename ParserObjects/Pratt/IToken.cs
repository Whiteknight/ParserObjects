namespace ParserObjects.Pratt
{
    /// <summary>
    /// A value in the Pratt parser with precidence/associativity and ID information.
    /// </summary>
    public interface IToken
    {
        int TokenTypeId { get; }
        int LeftBindingPower { get; }
        int RightBindingPower { get; }
        bool IsValid { get; }
    }

    /// <summary>
    /// An IToken variant with a value. Used in user-callback code to get values already parsed
    /// by the engine.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IToken<out TValue> : IToken
    {
        TValue Value { get; }
    }

    /// <summary>
    /// An IToken variant which is used by the Engine to get a value of the correct type. This
    /// type may have a hidden internal Value, which will be exposed to user callbacks but not to
    /// the Engine.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IToken<TInput, TOutput> : IToken
    {
        (bool success, IToken<TOutput> value) NullDenominator(IParseContext<TInput, TOutput> context);

        (bool success, IToken<TOutput> value) LeftDenominator(IParseContext<TInput, TOutput> context, IToken left);
    }
}
