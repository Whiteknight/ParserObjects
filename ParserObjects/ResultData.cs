using System.Collections.Generic;
using System.Linq;

namespace ParserObjects;

/// <summary>
/// A wrapper struct for a list of objects. Can be used to get an object of a given type from
/// the list, if it exists.
/// </summary>
public readonly struct ResultData
{
    private readonly bool _hasData;

    public ResultData(object data)
    {
        _hasData = data is not null;
        Data = data;
    }

    public object? Data { get; }

    public Option<T> OfType<T>()
        => (_hasData, Data) switch
        {
            (true, T typed) => new Option<T>(true, typed),
            (true, IEnumerable<object> list) => list.OfType<T>().Select(item => new Option<T>(true, item)).FirstOrDefault(),
            _ => default
        };
}
