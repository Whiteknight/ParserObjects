namespace ParserObjects.Pratt;

/// <summary>
/// Calculate the Null Denominator of the given token.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="context"></param>
/// <param name="value"></param>
/// <returns></returns>
public delegate TOutput NudFunc<TInput, TValue, TOutput>(IPrattParseContext<TInput, TOutput> context, IValueToken<TValue> value);

/// <summary>
/// Calculate the Left Denominator for the given token.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="context"></param>
/// <param name="left"></param>
/// <param name="value"></param>
/// <returns></returns>
public delegate TOutput LedFunc<TInput, TValue, TOutput>(IPrattParseContext<TInput, TOutput> context, IValueToken<TOutput> left, IValueToken<TValue> value);

/// <summary>
/// Used to configure a Parselet for the Pratt parser. A parselet is an adaptor over IParser
/// with metadata necessary for the Pratt algorithm.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public interface IPrattParseletBuilder<TInput, TValue, TOutput> : INamed
{
    /// <summary>
    /// Sets a type ID value for this token. The type ID is a user-provided value which is not
    /// used internally by the Pratt algorithm.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IPrattParseletBuilder<TInput, TValue, TOutput> TypeId(int id);

    /// <summary>
    /// Specify a parse rule which interacts with the previously parsed (left-hand) value. This
    /// parser may be an infix operator or a continuation of some sequence. This rule may call
    /// other parsers or recurse into the Pratt engine to obtain subsequent values.
    /// </summary>
    /// <param name="lbp"></param>
    /// <param name="rbp"></param>
    /// <param name="getLed"></param>
    /// <returns></returns>
    IPrattParseletBuilder<TInput, TValue, TOutput> LeftDenominator(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed);

    /// <summary>
    /// Specify a parse rule which does not interact with a value on the left hand side. This may
    /// be a value, a prefix operator, or the beginning of a larger production rule. This rule may
    /// call other parsers or recurse into the Pratt engine to obtain subsequent values.
    /// </summary>
    /// <param name="rbp"></param>
    /// <param name="getNud"></param>
    /// <returns></returns>
    IPrattParseletBuilder<TInput, TValue, TOutput> NullDenominator(int rbp, NudFunc<TInput, TValue, TOutput> getNud);
}

public static class ParseletBuilderExtensions
{
    /// <summary>
    /// Synonym for LeftDenominator. Create a parse rule which binds to a left-hand value with a
    /// specified binding power, and may recurse into the Pratt engine to produce subsequent
    /// values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lbp"></param>
    /// <param name="rbp"></param>
    /// <param name="getLed"></param>
    /// <returns></returns>
    public static IPrattParseletBuilder<TInput, TValue, TOutput> BindLeft<TInput, TValue, TOutput>(this IPrattParseletBuilder<TInput, TValue, TOutput> builder, int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed)
        => builder.LeftDenominator(lbp, rbp, getLed);

    /// <summary>
    /// Synonym for LeftDenominator. Create a parse rule which binds to a left-hand value with a
    /// specified binding power, and may recurse into the Pratt engine to produce subsequent
    /// values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="builder"></param>
    /// <param name="lbp"></param>
    /// <param name="getLed"></param>
    /// <returns></returns>
    public static IPrattParseletBuilder<TInput, TValue, TOutput> BindLeft<TInput, TValue, TOutput>(this IPrattParseletBuilder<TInput, TValue, TOutput> builder, int lbp, LedFunc<TInput, TValue, TOutput> getLed)
        => builder.LeftDenominator(lbp, lbp + 1, getLed);

    /// <summary>
    /// Synonym for NullDenominator. Create a parse rule which does not bind to a value on the left
    /// side. This rule has specified binding power and may recurse into the Pratt engine to
    /// produce subsequent values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="builder"></param>
    /// <param name="getNud"></param>
    /// <returns></returns>
    public static IPrattParseletBuilder<TInput, TValue, TOutput> Bind<TInput, TValue, TOutput>(this IPrattParseletBuilder<TInput, TValue, TOutput> builder, NudFunc<TInput, TValue, TOutput> getNud)
        => builder.NullDenominator(0, getNud);

    /// <summary>
    /// Synonym for NullDenominator. Create a parse rule which does not bind to a value on the left
    /// side. This rule has specified binding power and may recurse into the Pratt engine to
    /// produce subsequent values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="builder"></param>
    /// <param name="rbp"></param>
    /// <param name="getNud"></param>
    /// <returns></returns>
    public static IPrattParseletBuilder<TInput, TValue, TOutput> Bind<TInput, TValue, TOutput>(this IPrattParseletBuilder<TInput, TValue, TOutput> builder, int rbp, NudFunc<TInput, TValue, TOutput> getNud)
        => builder.NullDenominator(rbp, getNud);
}
