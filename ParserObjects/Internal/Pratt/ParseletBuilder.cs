using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects;
using ParserObjects.Internal.Utility;
using ParserObjects.Pratt;

namespace ParserObjects.Internal.Pratt;

/// <summary>
/// Builder to create an IParselet from an IParser with specified configuration and binding values.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class ParseletBuilder<TInput, TValue, TOutput> : IPrattParseletBuilder<TInput, TValue, TOutput>
{
    private readonly List<Func<IParser<TInput, TValue>, int, string, IParselet<TInput, TOutput>>> _getParselets;
    private readonly IParser<TInput, TValue> _matcher;

    private int _typeId;

    public ParseletBuilder(IParser<TInput, TValue> matcher)
    {
        Assert.ArgumentNotNull(matcher, nameof(matcher));
        _matcher = matcher;
        _getParselets = new List<Func<IParser<TInput, TValue>, int, string, IParselet<TInput, TOutput>>>();
        Name = string.Empty;
    }

    public string Name { get; private set; }

    public IEnumerable<IParselet<TInput, TOutput>> Build()
    {
        var parselets = _getParselets.Select(f => f(_matcher, _typeId, Name)).ToList();
        _getParselets.Clear();
        return parselets;
    }

    public IPrattParseletBuilder<TInput, TValue, TOutput> TypeId(int id)
    {
        _typeId = id;
        return this;
    }

    public IPrattParseletBuilder<TInput, TValue, TOutput> NullDenominator(int rbp, NudFunc<TInput, TValue, TOutput> getNud)
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

    public IPrattParseletBuilder<TInput, TValue, TOutput> LeftDenominator(int lbp, int rbp, LedFunc<TInput, TValue, TOutput> getLed)
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

    public INamed SetName(string name)
    {
        Name = name;
        return this;
    }
}
