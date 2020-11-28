using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Utility
{
    public record SingleReplaceResult (
        IReplaceableParserUntyped Replaceable,
        IParser Previous,
        IParser Current
    )
    {
        public bool Success => !ReferenceEquals(Previous, Current);

        public void Deconstruct(out bool success, out IParser previous, out IParser current)
        {
            success = Success;
            previous = Previous;
            current = Current;
        }

        public void Deconstruct(out bool success, out IParser previous, out IParser current, out IReplaceableParserUntyped replaceable)
        {
            success = Success;
            previous = Previous;
            current = Current;
            replaceable = Replaceable;
        }
    }

    public struct MultiReplaceResult
    {
        public MultiReplaceResult(IEnumerable<SingleReplaceResult> results)
        {
            Results = results.ToList();
        }

        public IReadOnlyList<SingleReplaceResult> Results { get; }

        public bool Success => Results != null && Results.Count > 0 && Results.All(r => r.Success);

        public static MultiReplaceResult Failure() => default;

        public void Deconstruct(out bool success, out IReadOnlyList<SingleReplaceResult> results)
        {
            success = Success;
            results = Results;
        }
    }
}
