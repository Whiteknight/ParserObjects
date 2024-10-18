using System.Buffers;

namespace ParserObjects.Internal.Earley;

// Represents an array which has been borrowed from the ArrayPool<T>.Shared
public readonly struct RentedArray<T>
{
    public RentedArray(T[] values, int count)
    {
        Values = values;
        Count = count;
    }

    public readonly T this[int i] => Values[i];

    public readonly int Count { get; }

    public readonly int Length => Count;

    public readonly T[] Values { get; }

    public void Return()
    {
        if (Values != null)
            ArrayPool<T>.Shared.Return(Values);
    }
}
