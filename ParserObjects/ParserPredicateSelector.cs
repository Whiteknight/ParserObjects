using System;
using System.Collections.Generic;
using ParserObjects.Internal;

namespace ParserObjects;

/// <summary>
/// Associated each parser with a predicate. Returns the first parser where the predicate returns
/// true.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public readonly struct ParserPredicateSelector<TInput, TMiddle, TOutput>
{
    private readonly List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)> _parsers;

    public ParserPredicateSelector(List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)> parsers)
    {
        _parsers = parsers;
    }

    /// <summary>
    /// Add a new parser to be returned when the predicate condition is satisfied.
    /// </summary>
    /// <param name="equals"></param>
    /// <param name="parser"></param>
    /// <returns></returns>
    public ParserPredicateSelector<TInput, TMiddle, TOutput> When(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)
    {
        Assert.ArgumentNotNull(equals);
        Assert.ArgumentNotNull(parser);
        _parsers.Add((equals, parser));
        return this;
    }

    /// <summary>
    /// Return the first parser whose predicate matches the given next item.
    /// </summary>
    /// <param name="next"></param>
    /// <returns></returns>
    public IParser<TInput, TOutput> Pick(TMiddle next)
    {
        foreach (var (equals, parser) in _parsers)
        {
            if (equals(next))
                return parser;
        }

        return Parsers<TInput>.Fail<TOutput>($"No configured parsers handle {next}");
    }

    /// <summary>
    /// Return a list of all parsers held in this object.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<IParser> GetChildren()
    {
        var children = new IParser[_parsers.Count];
        for (int i = 0; i < _parsers.Count; i++)
            children[i] = _parsers[i].parser;
        return children;
    }
}
