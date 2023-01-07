using System;
using System.Collections.Generic;
using System.Linq;
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

    public IEnumerable<IParser> GetChildren()
        => _parsers.Select(v => v.parser);
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

    public IEnumerable<IParser> GetChildren()
        => _parsers.Select(v => v.parser);
}
