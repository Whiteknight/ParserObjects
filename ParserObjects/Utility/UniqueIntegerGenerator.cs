using System.Threading;

namespace ParserObjects.Utility;

public static class UniqueIntegerGenerator
{
    private static int _value;

    public static int GetNext() => Interlocked.Increment(ref _value);
}
