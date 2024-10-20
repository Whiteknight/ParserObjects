using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace ParserObjects;

/// <summary>
/// A wrapper struct for a list of objects. Can be used to get an object of a given type from
/// the list, if it exists.
/// </summary>
public readonly struct ResultData
{
    private readonly ResultDataType _storageType;

    public ResultData(object data)
    {
        _storageType = data is null ? ResultDataType.None : ResultDataType.One;
        Data = data;
    }

    private ResultData(ResultDataType type, object? data)
    {
        (_storageType, Data) = (type, data) switch
        {
            (ResultDataType.One, not null) => (ResultDataType.One, data),
            (ResultDataType.List, ImmutableList<object>) => (ResultDataType.List, data),
            _ => (ResultDataType.None, null)
        };
    }

    public object? Data { get; }

    public Option<T> OfType<T>()
        => (_storageType, Data) switch
        {
            (ResultDataType.One, T typed) => new Option<T>(true, typed),
            (ResultDataType.List, IEnumerable<object> list) => list.OfType<T>().Select(item => new Option<T>(true, item)).FirstOrDefault(),
            _ => default
        };

    public ResultData And(object data)
        => (_storageType, Data) switch
        {
            (ResultDataType.One, not null) => new ResultData(ResultDataType.List, ImmutableList<object>.Empty.Add(Data).Add(data)),
            (ResultDataType.List, ImmutableList<object> list) => new ResultData(ResultDataType.List, list.Add(data)),
            (ResultDataType.List, _) => throw new InvalidOperationException("Cannot be here"),
            _ => new ResultData(data is null ? ResultDataType.None : ResultDataType.One, data)
        };

    private enum ResultDataType
    {
        None = 0,
        One = 1,
        List = 2
    }
}
