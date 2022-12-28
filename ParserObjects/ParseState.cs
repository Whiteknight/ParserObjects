using System;
using System.Collections.Generic;
using ParserObjects.Internal.Caching;

namespace ParserObjects;

/// <summary>
/// Contains state data and metadata about the progress of the parse. Provides references to
/// useful objects which may affect the parse.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed class ParseState<TInput> : IParseState<TInput>
{
    private readonly Action<string> _logCallback;
    private readonly LinkedList<Dictionary<string, object>> _data;

    public ParseState(ISequence<TInput> input, Action<string> logCallback, IResultsCache? cache)
    {
        Input = input;
        _data = new LinkedList<Dictionary<string, object>>();
        _data.AddLast(new Dictionary<string, object>());
        _logCallback = logCallback;
        Cache = cache ?? new NullResultsCache();
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
    public DataStore Data => new DataStore(_data);

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

    public TResult WithDataFrame<TArgs, TResult>(TArgs args, Func<IParseState<TInput>, TArgs, TResult> withContext, IReadOnlyDictionary<string, object>? data = null)
    {
        try
        {
            _data.AddLast(new Dictionary<string, object>());
            if (data != null)
            {
                foreach (var kvp in data)
                    _data.Last!.Value!.Add(kvp.Key, kvp.Value);
            }

            return withContext(this, args);
        }
        finally
        {
            if (_data.Count > 1)
                _data.RemoveLast();
        }
    }
}

public struct DataStore
{
    private readonly LinkedList<Dictionary<string, object>> _store;

    public DataStore(LinkedList<Dictionary<string, object>> data)
    {
        _store = data;
    }

    public Option<T> Get<T>(string name)
    {
        var node = _store.Last;
        while (node != null)
        {
            if (node.Value.ContainsKey(name))
            {
                var value = node.Value[name];
                if (value is T typed)
                    return new Option<T>(true, typed);
                return default;
            }

            node = node.Previous;
        }

        return default;
    }

    public void Set<T>(string name, T value)
    {
        var dict = _store!.Last!.Value!;
        if (value == null)
            return;

        if (dict.ContainsKey(name))
        {
            dict[name] = value;
            return;
        }

        dict.Add(name, value);
    }
}
