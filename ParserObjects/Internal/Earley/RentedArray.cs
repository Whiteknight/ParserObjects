using System.Buffers;

namespace ParserObjects.Internal.Earley;

// Represents an array which has been borrowed from the ArrayPool<T>.Shared
public struct RentedArray<T>
{
    private readonly T[] _values;

    public RentedArray(T[] values, int count)
    {
        _values = values;
        Count = count;
    }

    public T this[int i]
    {
        get { return _values[i]; }
    }

    public int Count { get; }
    public int Length => Count;

    public T[] Values => _values;

    public void Return()
    {
        if (_values != null)
            ArrayPool<T>.Shared.Return(_values);
    }
}
