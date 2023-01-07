using System.Collections.Generic;
using ParserObjects.Internal.Pratt;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Pratt;

/// <summary>
/// Builder to create an IParselet from an IParser with specified configuration and binding values.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public struct ParseletBuilder<TInput, TValue, TOutput>
{
    private readonly record struct GetParseletArguments(
        NudFunc<TInput, TValue, TOutput>? GetNud,
        LedFunc<TInput, TValue, TOutput>? GetLed,
        int Lbp,
        int Rbp
    );

    // TODO: It is technically possible for a single ParseletBuilder to have multiple
    // .BindLeft() or .BindRight() calls, and for this list of _getParselets to have more than 2
    // entries in it. Do we want to allow this? Is there a real use-case for multiple BindRight
    // or BindLeft calls on a single matcher?
    private readonly List<GetParseletArguments> _getParselets;

    private int _typeId;

    // We need at least one parameter here, because it's a struct
    public ParseletBuilder(string name)
    {
        _getParselets = new List<GetParseletArguments>();
        _typeId = 0;
        Name = name;
    }

    public string Name { get; private set; }

    public IEnumerable<IParselet<TInput, TOutput>> Build(IParser<TInput, TValue> matcher)
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

    public ParseletBuilder<TInput, TValue, TOutput> TypeId(int id)
    {
        _typeId = id;
        return this;
    }

    public ParseletBuilder<TInput, TValue, TOutput> NullDenominator(int rbp, NudFunc<TInput, TValue, TOutput> getNud)
    {
        Assert.ArgumentNotNull(getNud, nameof(getNud));
        _getParselets.Add(new GetParseletArguments(getNud, null, rbp, rbp));
        return this;
    }

    public ParseletBuilder<TInput, TValue, TOutput> LeftDenominator(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed)
    {
        Assert.ArgumentNotNull(getLed, nameof(getLed));
        _getParselets.Add(new GetParseletArguments(null, getLed, lbp, rbp));
        return this;
    }

    private static Parselet<TInput, TValue, TOutput> BuildParselet(GetParseletArguments args, IParser<TInput, TValue> parser, int typeId, string name)
        => new Parselet<TInput, TValue, TOutput>(typeId, parser, args.GetNud, args.GetLed, args.Lbp, args.Rbp, name);

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
    public ParseletBuilder<TInput, TValue, TOutput> BindLeft(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed)
        => LeftDenominator(lbp, rbp, getLed);

    /// <summary>
    /// Synonym for LeftDenominator. Create a parse rule which binds to a left-hand value with a
    /// specified binding power, and may recurse into the Pratt engine to produce subsequent
    /// values.
    /// </summary>
    /// <param name="lbp"></param>
    /// <param name="getLed"></param>
    /// <returns></returns>
    public ParseletBuilder<TInput, TValue, TOutput> BindLeft(int lbp, LedFunc<TInput, TValue, TOutput> getLed)
        => LeftDenominator(lbp, lbp + 1, getLed);

    /// <summary>
    /// Synonym for NullDenominator. Create a parse rule which does not bind to a value on the left
    /// side. This rule has specified binding power and may recurse into the Pratt engine to
    /// produce subsequent values.
    /// </summary>
    /// <param name="getNud"></param>
    /// <returns></returns>
    public ParseletBuilder<TInput, TValue, TOutput> Bind(NudFunc<TInput, TValue, TOutput> getNud)
        => NullDenominator(0, getNud);

    /// <summary>
    /// Synonym for NullDenominator. Create a parse rule which does not bind to a value on the left
    /// side. This rule has specified binding power and may recurse into the Pratt engine to
    /// produce subsequent values.
    /// </summary>
    /// <param name="rbp"></param>
    /// <param name="getNud"></param>
    /// <returns></returns>
    public ParseletBuilder<TInput, TValue, TOutput> Bind(int rbp, NudFunc<TInput, TValue, TOutput> getNud)
        => NullDenominator(rbp, getNud);
}
