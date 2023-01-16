using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects;

public readonly struct ParserPredicateSelector<TInput, TMiddle, TOutput>
{
    private readonly List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)> _parsers;

    public ParserPredicateSelector(List<(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)> parsers)
    {
        _parsers = parsers;
    }

    public ParserPredicateSelector<TInput, TMiddle, TOutput> When(Func<TMiddle, bool> equals, IParser<TInput, TOutput> parser)
    {
        Assert.ArgumentNotNull(equals, nameof(equals));
        Assert.ArgumentNotNull(parser, nameof(parser));
        _parsers.Add((equals, parser));
        return this;
    }

    public IParser<TInput, TOutput> Pick(TMiddle next)
    {
        foreach (var (equals, parser) in _parsers)
        {
            if (equals(next))
                return parser;
        }

        return Parsers<TInput>.Fail<TOutput>($"No configured parsers handle {next}");
    }

    public IReadOnlyList<IParser> GetChildren()
    {
        var children = new IParser[_parsers.Count];
        for (int i = 0; i < _parsers.Count; i++)
            children[i] = _parsers[i].parser;
        return children;
    }
}

public readonly struct ParserPredicateSelector<TInput, TOutput>
{
    private readonly List<(Func<object, bool> equals, IParser<TInput, TOutput> parser)> _parsers;

    public ParserPredicateSelector(List<(Func<object, bool> equals, IParser<TInput, TOutput> parser)> parsers)
    {
        _parsers = parsers;
    }

    public ParserPredicateSelector<TInput, TOutput> When(Func<object, bool> equals, IParser<TInput, TOutput> parser)
    {
        Assert.ArgumentNotNull(equals, nameof(equals));
        Assert.ArgumentNotNull(parser, nameof(parser));
        _parsers.Add((equals, parser));
        return this;
    }

    public IParser<TInput, TOutput> Pick(object next)
    {
        foreach (var (equals, parser) in _parsers)
        {
            if (equals(next))
                return parser;
        }

        return Parsers<TInput>.Fail<TOutput>($"No configured parsers handle {next}");
    }

    public IReadOnlyList<IParser> GetChildren()
    {
        var children = new IParser[_parsers.Count];
        for (int i = 0; i < _parsers.Count; i++)
            children[i] = _parsers[i].parser;
        return children;
    }
}
