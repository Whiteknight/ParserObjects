namespace ParserObjects.Pratt;

/// <summary>
/// A value in the Pratt parser with precidence/associativity and ID information.
/// </summary>
public interface IPrattToken
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
public interface IValueToken<out TValue> : IPrattToken
{
    /// <summary>
    /// Gets the parse result value of the token.
    /// </summary>
    TValue Value { get; }
}
