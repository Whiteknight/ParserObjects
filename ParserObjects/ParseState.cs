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
        Cache = cache ?? new SimpleDictionaryCache();
    }

    public ParseState(ISequence<TInput> input, Action<string> logCallback)
        : this(input, logCallback, NullResultsCache.Instance)
    {
    }

    public ISequence<TInput> Input { get; }

    public DataStore Data => new DataStore(GetDataStore());

    public IResultsCache Cache { get; }

    public void Log(IParser parser, string message) => _logCallback?.Invoke($"{parser}: {message}");

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
        Assert.ArgumentNotNull(data);
        _store = data;
    }

    public int PushDataFrame(IReadOnlyDictionary<string, object>? data = null)
    {
        var frame = new Dictionary<string, object>();
        var version = _store.Last!.Value.Version + 1;
        data?.AddTo(frame);
        _store.AddLast((version, frame));
        return version;
    }

    public int PopDataFrame(int version = 0)
    {
        Assert.ArgumentGreaterThanOrEqualTo(version, 0);
        if (version == 0)
            version = _store.Last!.Value.Version - 1;

        while (_store.Last!.Value.Version > version)
            _store.RemoveLast();

        return _store.Last!.Value.Version;
    }

    public int GetCurrentDataFrame() => _store.Last!.Value.Version;

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
            if (!node.Value.Values.TryGetValue(name, out var value))
            {
                node = node.Previous;
                continue;
            }

            return value switch
            {
                T typed => new Option<T>(true, typed),
                _ => default
            };
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
