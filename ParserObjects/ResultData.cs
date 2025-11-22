using System;
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

    private ResultData(ImmutableList<object> data)
    {
        _storageType = ResultDataType.List;
        Data = data;
    }

    public object? Data { get; }

    public Option<T> OfType<T>()
        => (_storageType, Data) switch
        {
            (ResultDataType.One, T typed) => new Option<T>(true, typed),
            (ResultDataType.List, ImmutableList<object> list) => list.OfType<T>().Select(item => new Option<T>(true, item)).FirstOrDefault(),
            _ => default
        };

    public ResultData And(object data)
        => (_storageType, Data) switch
        {
            (ResultDataType.None, _) => new ResultData(data),
            (ResultDataType.One, not null) => new ResultData(ImmutableList<object>.Empty.Add(Data).Add(data)),
            (ResultDataType.List, ImmutableList<object> list) => new ResultData(list.Add(data)),
            _ => throw new InvalidOperationException("Unexpected situation")
        };

    private enum ResultDataType
    {
        None = 0,
        One = 1,
        List = 2
    }
}
