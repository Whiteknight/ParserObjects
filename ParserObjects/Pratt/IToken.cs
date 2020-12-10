namespace ParserObjects.Pratt
{
    /// <summary>
    /// A value in the Pratt parser with precidence/associativity and ID information.
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// Gets a user-provided ID value which can be used for matching purposes.
        /// </summary>
        int TokenTypeId { get; }

        /// <summary>
        /// Gets the strength with which this token binds to the production on the left.
        /// </summary>
        int LeftBindingPower { get; }

        /// <summary>
        /// Gets the strength with which this token binds to the production on the right.
        /// </summary>
        int RightBindingPower { get; }

        /// <summary>
        /// Gets a value indicating whether this token was obtained successfully and contains
        /// all necessary data.
        /// </summary>
        bool IsValid { get; }
    }

    /// <summary>
    /// An IToken variant with a value. Used in user-callback code to get values already parsed
    /// by the engine.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IToken<out TValue> : IToken
    {
        /// <summary>
        /// Gets the parse result value of the token.
        /// </summary>
        TValue Value { get; }
    }

    /// <summary>
    /// An IToken variant which is used by the Engine to get a value of the correct type. This
    /// type may have a hidden internal Value, which will be exposed to user callbacks but not to
    /// the Engine. This interface is not intended for external use.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IToken<TInput, TOutput> : IToken
    {
        /// <summary>
        /// Calculates the null denominator (prefix production) of this token, converting it into
        /// a token of the correct output type.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        (bool success, IToken<TOutput> value) NullDenominator(IParseContext<TInput, TOutput> context);

        /// <summary>
        /// Calculates the left denominator (suffix production) of this token, converting it into
        /// a token of the correct output type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="left"></param>
        /// <returns></returns>
        (bool success, IToken<TOutput> value) LeftDenominator(IParseContext<TInput, TOutput> context, IToken left);
    }
}
