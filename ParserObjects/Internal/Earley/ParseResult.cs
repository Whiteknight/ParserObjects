using System.Collections.Generic;
using ParserObjects.Earley;

namespace ParserObjects.Internal.Earley;

public record struct ParseResult<TOutput>(
    IReadOnlyList<IResultAlternative<TOutput>> Alternatives,
    IParseStatistics Statistics
);
