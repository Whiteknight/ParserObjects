using System;
using Microsoft.Extensions.Caching.Memory;
using ParserObjects.Caching;
using ParserObjects.Utility;

namespace ParserObjects;

/// <summary>
/// Contains state data and metadata about the progress of the parse. Provides references to
/// useful objects which may affect the parse.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class ParseState<TInput> : IParseState<TInput>
{
    private readonly Action<string> _logCallback;
    private readonly CascadingKeyValueStore _store;

    public ParseState(ISequence<TInput> input, Action<string> logCallback, IResultsCache? cache)
    {
        Input = input;
        _store = new CascadingKeyValueStore();
        _logCallback = logCallback;
        Cache = cache ?? new NullResultsCache();
    }

    public ParseState(ISequence<TInput> input, Action<string> logCallback, IMemoryCache cache)
    {
        Input = input;
        _store = new CascadingKeyValueStore();
        _logCallback = logCallback;
        Cache = new MemoryCacheResultsCache(cache);
    }

    public ParseState(ISequence<TInput> input, Action<string> logCallback)
        : this(input, logCallback, new NullResultsCache())
    {
    }

    /// <summary>
    /// Gets the input sequence being used by the parser.
    /// </summary>
    public ISequence<TInput> Input { get; }

    /// <summary>
    /// Gets the current contextual state data used by the parser.
    /// </summary>
    public IDataStore Data => _store;

    /// <summary>
    /// Gets the current result cache.
    /// </summary>
    public IResultsCache Cache { get; }

    /// <summary>
    /// If a log callback is provided, pass a log message to the callback.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="message"></param>
    public void Log(IParser parser, string message) => _logCallback?.Invoke($"{parser}: {message}");
}
