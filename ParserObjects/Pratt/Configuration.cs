using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Pratt;

public struct Configuration<TInput, TOutput>
{
    private readonly List<IParser> _references;

    public Configuration(List<IParselet<TInput, TOutput>> parselets, List<IParser> references)
    {
        Parselets = parselets;
        _references = references;
    }

    public List<IParselet<TInput, TOutput>> Parselets { get; }

    public Configuration<TInput, TOutput> Add<TValue>(IParser<TInput, TValue> matcher, Func<ParseletBuilder<TInput, TValue, TOutput>, ParseletBuilder<TInput, TValue, TOutput>> setup)
    {
        Assert.ArgumentNotNull(matcher, nameof(matcher));
        Assert.ArgumentNotNull(setup, nameof(setup));

        var parseletConfig = new ParseletBuilder<TInput, TValue, TOutput>(matcher.Name ?? string.Empty);
        parseletConfig = setup(parseletConfig);
        var parselets = parseletConfig.Build(matcher);
        Parselets.AddRange(parselets);
        return this;
    }

    public Configuration<TInput, TOutput> Add(IParser<TInput, TOutput> matcher)
        => Add(matcher, static p => p.Bind(0, static (_, v) => v.Value));

    public Configuration<TInput, TOutput> Reference(IParser parser)
    {
        _references.Add(parser);
        return this;
    }
}
