using System;
using System.Collections.Generic;
using ParserObjects.Internal.Pratt;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Pratt;

/// <summary>
/// Configuration for the Pratt engine. Use this type to add matchers and precidence rules to your
/// parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public struct Configuration<TInput, TOutput>
{
    private readonly List<IParser> _references;

    public Configuration(List<IParselet<TInput, TOutput>> parselets, List<IParser> references)
    {
        Parselets = parselets;
        _references = references;
    }

    /// <summary>
    /// Gets the current list of Parselets. You shouldn't modify this unless you know exactly what you
    /// are doing.
    /// </summary>
    public List<IParselet<TInput, TOutput>> Parselets { get; }

    /// <summary>
    /// Add a new parselet to the list of parselets with configured settings and callbacks.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="matcher"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Add a new parselet for the given parser and default settings.
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public Configuration<TInput, TOutput> Add(IParser<TInput, TOutput> matcher)
        => Add(
            matcher,
            static p => p.Bind(
                0,
                static (_, v) => v.Value
            )
        );

    /// <summary>
    /// Add an explicit parse reference which will be returned from the Pratt's GetChildren()
    /// method, but isn't otherwise included as a matcher in this configuration.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public Configuration<TInput, TOutput> Reference(IParser parser)
    {
        _references.Add(parser);
        return this;
    }
}
