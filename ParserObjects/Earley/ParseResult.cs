using System.Collections.Generic;

namespace ParserObjects.Earley
{
    public struct ParseResult<TOutput>
    {
        public ParseResult(IReadOnlyList<IResultAlternative<TOutput>> alternatives, IParseStatistics statistics)
        {
            Alternatives = alternatives;
            Statistics = statistics;
        }

        public IReadOnlyList<IResultAlternative<TOutput>> Alternatives { get; set; }
        public IParseStatistics Statistics { get; }
    }
}
