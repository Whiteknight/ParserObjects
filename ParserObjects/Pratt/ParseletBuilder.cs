using System.Collections.Generic;
using ParserObjects.Internal.Pratt;
using static ParserObjects.Internal.Assert;

namespace ParserObjects.Pratt;

/// <summary>
/// Builder to create an IParselet from an IParser with specified configuration and binding values.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public struct ParseletBuilder<TInput, TValue, TOutput>
{
    /* Notice that we can have more than one NUD and LED per parselet.
     * The way the engine works, it gets a list of all available parselets for the current situation
     * and attempts them all. If the matcher matches the NUD or LED function is invoked to convert
     * the parse result into the TOutput type. If that function fails, the Engine will continue on
     * to find another parselet that Matches AND succeeds in the conversion. It's a kind of rare
     * use-case to have multiple NUD or LED funcs per matcher, but it is supported (and tested)
     */

    private readonly record struct GetParseletArguments(
        NudFunc<TInput, TValue, TOutput>? GetNud,
        LedFunc<TInput, TValue, TOutput>? GetLed,
        int Lbp,
        int Rbp
    );

    private readonly List<GetParseletArguments> _getParselets;

    private int _typeId;

    // We need at least one parameter here, because it's a struct
    public ParseletBuilder(string name)
    {
        _getParselets = [];
        _typeId = 0;
        Name = name;
    }

    public string Name { get; private set; }

    /// <summary>
    /// Build a parselet with the given matcher and the registered callbacks.
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public readonly IEnumerable<IParselet<TInput, TOutput>> Build(IParser<TInput, TValue> matcher)
    {
        var parselets = new IParselet<TInput, TOutput>[_getParselets.Count];
        for (int i = 0; i < _getParselets.Count; i++)
        {
            var args = _getParselets[i];
            parselets[i] = BuildParselet(args, matcher, _typeId, Name);
        }

        _getParselets.Clear();
        return parselets;
    }

    private static Parselet<TInput, TValue, TOutput> BuildParselet(GetParseletArguments args, IParser<TInput, TValue> parser, int typeId, string name)
        => new Parselet<TInput, TValue, TOutput>(typeId, parser, args.GetNud, args.GetLed, args.Lbp, args.Rbp, name);

    /// <summary>
    /// Set a Type ID value for this matcher. All tokens matched by this matcher will have the
    /// type ID specified here.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ParseletBuilder<TInput, TValue, TOutput> TypeId(int id)
    {
        _typeId = id;
        return this;
    }

    /// <summary>
    /// Register this matcher as a Null Denominator (NUD). It is preferred that you use
    /// ".Bind()" instead.
    /// </summary>
    /// <param name="rbp"></param>
    /// <param name="getNud"></param>
    /// <returns></returns>
    public readonly ParseletBuilder<TInput, TValue, TOutput> NullDenominator(int rbp, NudFunc<TInput, TValue, TOutput> getNud)
    {
        NotNull(getNud);
        _getParselets.Add(new GetParseletArguments(getNud, null, rbp, rbp));
        return this;
    }

    /// <summary>
    /// Register this matcher as a Left Denominator (LED). It is preferred that you use
    /// ".BindLeft()" instead.
    /// </summary>
    /// <param name="lbp"></param>
    /// <param name="rbp"></param>
    /// <param name="getLed"></param>
    /// <returns></returns>
    public readonly ParseletBuilder<TInput, TValue, TOutput> LeftDenominator(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed)
    {
        NotNull(getLed);
        _getParselets.Add(new GetParseletArguments(null, getLed, lbp, rbp));
        return this;
    }

    public ParseletBuilder<TInput, TValue, TOutput> Named(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    /// Synonym for LeftDenominator. Create a parse rule which binds to a left-hand value with a
    /// specified binding power, and may recurse into the Pratt engine to produce subsequent
    /// values.
    /// </summary>
    /// <param name="lbp"></param>
    /// <param name="rbp"></param>
    /// <param name="getLed"></param>
    /// <returns></returns>
    public readonly ParseletBuilder<TInput, TValue, TOutput> BindLeft(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed)
        => LeftDenominator(lbp, rbp, getLed);

    /// <summary>
    /// Synonym for LeftDenominator. Create a parse rule which binds to a left-hand value with a
    /// specified binding power, and may recurse into the Pratt engine to produce subsequent
    /// values.
    /// </summary>
    /// <param name="lbp"></param>
    /// <param name="getLed"></param>
    /// <returns></returns>
    public readonly ParseletBuilder<TInput, TValue, TOutput> BindLeft(int lbp, LedFunc<TInput, TValue, TOutput> getLed)
        => LeftDenominator(lbp, lbp + 1, getLed);

    /// <summary>
    /// Synonym for NullDenominator. Create a parse rule which does not bind to a value on the left
    /// side. This rule has specified binding power and may recurse into the Pratt engine to
    /// produce subsequent values.
    /// </summary>
    /// <param name="getNud"></param>
    /// <returns></returns>
    public readonly ParseletBuilder<TInput, TValue, TOutput> Bind(NudFunc<TInput, TValue, TOutput> getNud)
        => NullDenominator(0, getNud);

    /// <summary>
    /// Synonym for NullDenominator. Create a parse rule which does not bind to a value on the left
    /// side. This rule has specified binding power and may recurse into the Pratt engine to
    /// produce subsequent values.
    /// </summary>
    /// <param name="rbp"></param>
    /// <param name="getNud"></param>
    /// <returns></returns>
    public readonly ParseletBuilder<TInput, TValue, TOutput> Bind(int rbp, NudFunc<TInput, TValue, TOutput> getNud)
        => NullDenominator(rbp, getNud);
}
