using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Cache = cache ?? NullResultsCache.Instance;
    }

    public ParseState(ISequence<TInput> input, Action<string> logCallback)
        : this(input, logCallback, NullResultsCache.Instance)
    {
    }

    public ISequence<TInput> Input { get; }

    public DataStore Data => new DataStore(_data);

    public IResultsCache Cache { get; }

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

/// <summary>
/// Access data from the current Parse State.
/// </summary>
public struct DataStore
{
    private readonly LinkedList<Dictionary<string, object>> _store;

    public DataStore(LinkedList<Dictionary<string, object>> data)
    {
        _store = data;
    }

    /// <summary>
    /// Get a value from the current data context with the given name. If no value exists with that
    /// name, or if the value is not the correct type, a failure result will be returned.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns>The value or failure.</returns>
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

    /// <summary>
    /// Sets a value to the current data context with the given name. If a value exists with the
    /// same name it will be overwritten.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void Set<T>(string name, T value)
    {
        var dict = _store!.Last!.Value!;
        Debug.Assert(dict != null, "Shouldn't be null");

        if (dict.ContainsKey(name))
        {
            dict[name] = value;
            return;
        }

        dict.Add(name, value);
    }
}
