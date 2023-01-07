using System;
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
public sealed class ParseletBuilder<TInput, TValue, TOutput>
{
    // TODO: It is technically possible for a single ParseletBuilder to have multiple
    // .BindLeft() or .BindRight() calls, and for this list of _getParselets to have more than 2
    // entries in it. Do we want to allow this? Is there a real use-case for multiple BindRight
    // or BindLeft calls on a single matcher?
    private readonly List<Func<IParser<TInput, TValue>, int, string, IParselet<TInput, TOutput>>> _getParselets;

    private int _typeId;

    public ParseletBuilder()
    {
        _getParselets = new List<Func<IParser<TInput, TValue>, int, string, IParselet<TInput, TOutput>>>();
        Name = string.Empty;
    }

    public string Name { get; private set; }

    public IEnumerable<IParselet<TInput, TOutput>> Build(IParser<TInput, TValue> matcher)
    {
        var parselets = new IParselet<TInput, TOutput>[_getParselets.Count];
        for (int i = 0; i < _getParselets.Count; i++)
            parselets[i] = _getParselets[i](matcher, _typeId, Name);
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
        _getParselets.Add((m, tid, n) => new Parselet<TInput, TValue, TOutput>(
            tid,
            m,
            getNud,
            null,
            rbp,
            rbp,
            n
        ));
        return this;
    }

    public ParseletBuilder<TInput, TValue, TOutput> LeftDenominator(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed)
    {
        Assert.ArgumentNotNull(getLed, nameof(getLed));
        _getParselets.Add((m, tid, n) => new Parselet<TInput, TValue, TOutput>(
            tid,
            m,
            null,
            getLed,
            lbp,
            rbp,
            n
        ));
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
