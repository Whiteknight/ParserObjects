using System.Collections.Generic;
using ParserObjects.Earley;

namespace ParserObjects.Internal.Earley;

public readonly record struct ParseResult<TOutput>(
    IReadOnlyList<Alternative<TOutput>> Alternatives,
    IParseStatistics Statistics
);
