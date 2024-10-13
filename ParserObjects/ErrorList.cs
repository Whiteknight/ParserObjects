using System.Collections.Generic;

namespace ParserObjects;

public record ErrorList(IReadOnlyList<Result<object>> ErrorResults);
