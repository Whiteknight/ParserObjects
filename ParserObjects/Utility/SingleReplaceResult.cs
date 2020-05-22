using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Utility
{
    public class SingleReplaceResult
    {
        public SingleReplaceResult(IReplaceableParserUntyped replaceable, IParser previous, IParser current)
        {
            Replaceable = replaceable;
            Previous = previous;
            Current = current;
        }

        public IReplaceableParserUntyped Replaceable { get; }
        public IParser Previous { get; }
        public IParser Current { get; }

        public bool IsSuccess => !ReferenceEquals(Previous, Current);
    }

    public class MultiReplaceResult
    {
        public IReadOnlyList<SingleReplaceResult> Results { get; }

        public MultiReplaceResult(IEnumerable<SingleReplaceResult> results)
        {
            Results = results.ToList();
        }

        public bool IsSuccess => Results.Count > 0 && Results.All(r => r.IsSuccess);

        public static MultiReplaceResult Failure() => new MultiReplaceResult(new SingleReplaceResult[0]);
    }
}
