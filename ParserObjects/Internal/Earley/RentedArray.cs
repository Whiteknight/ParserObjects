using System.Buffers;

namespace ParserObjects.Internal.Earley;

public struct RentedArray<T>
{
    private readonly T[] _values;

    public static RentedArray<T> Rent(int i)
    {
        var array = ArrayPool<T>.Shared.Rent(i);
        return new RentedArray<T>(array, i);
    }

    // TODO: Should we take the reference to the ArrayPool<T> here, or can we always assume
    // it is ArrayPool<T>.Shared?
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
