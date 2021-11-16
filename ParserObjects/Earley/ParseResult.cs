using System.Collections.Generic;

namespace ParserObjects.Earley
{
    public record struct ParseResult<TOutput>(
        IReadOnlyList<IResultAlternative<TOutput>> Alternatives,
        IParseStatistics Statistics
    );
}
