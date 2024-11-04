using System;
using System.Collections.Generic;
using System.Diagnostics;
using ParserObjects.Internal;
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
    private LinkedList<(int Version, Dictionary<string, object> Values)>? _data;

    public ParseState(ISequence<TInput> input, Action<string> logCallback, IResultsCache? cache)
    {
        Input = input;
        _data = null;
        _logCallback = logCallback;
        Cache = cache ?? NullResultsCache.Instance;
    }

    public ParseState(ISequence<TInput> input, Action<string> logCallback)
        : this(input, logCallback, NullResultsCache.Instance)
    {
    }

    public ISequence<TInput> Input { get; }

    public DataStore Data => new DataStore(GetDataStore());

    public IResultsCache Cache { get; }

    public void Log(IParser parser, string message) => _logCallback?.Invoke($"{parser}: {message}");

    public TResult WithDataFrame<TArgs, TResult>(TArgs args, Func<IParseState<TInput>, TArgs, TResult> withContext, IReadOnlyDictionary<string, object>? data = null)
    {
        int frame = 0;
        try
        {
            frame = PushDataFrame(data);
            return withContext(this, args);
        }
        finally
        {
            PopDataFrame(frame);
        }
    }

    public int PushDataFrame(IReadOnlyDictionary<string, object>? data = null)
    {
        var store = GetDataStore();
        var frame = new Dictionary<string, object>();
        var version = store.Last!.Value.Version + 1;
        if (data != null)
        {
            foreach (var kvp in data)
                frame.Add(kvp.Key, kvp.Value);
        }

        store.AddLast((version, frame));
        return version;
    }

    public int PopDataFrame(int version = 0)
    {
        Assert.ArgumentGreaterThanOrEqualTo(version, 0);
        var store = GetDataStore();
        if (version == 0)
            version = store.Last!.Value.Version - 1;

        while (store.Last!.Value.Version > version)
            store.RemoveLast();

        return store.Last!.Value.Version;
    }

    public int GetCurrentDataFrame() => GetDataStore().Last!.Value.Version;

    private LinkedList<(int Version, Dictionary<string, object> Values)> GetDataStore()
    {
        if (_data == null)
        {
            _data = new LinkedList<(int Version, Dictionary<string, object> Values)>();
            _data.AddLast((1, new Dictionary<string, object>()));
        }

        return _data;
    }
}

/// <summary>
/// Access data from the current Parse State.
/// </summary>
public readonly struct DataStore
{
    private readonly LinkedList<(int Version, Dictionary<string, object> Values)> _store;

    public DataStore(LinkedList<(int Version, Dictionary<string, object> Values)> data)
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
    public readonly Option<T> Get<T>(string name)
    {
        var node = _store.Last;
        while (node != null)
        {
            if (node.Value.Values.TryGetValue(name, out var value))
            {
                return value switch
                {
                    T typed => new Option<T>(true, typed),
                    _ => default
                };
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
        var dict = _store!.Last!.Value!.Values;
        Debug.Assert(dict != null, "Shouldn't be null");

        if (dict.ContainsKey(name))
        {
            dict[name] = value!;
            return;
        }

        dict.Add(name, value!);
    }
}
