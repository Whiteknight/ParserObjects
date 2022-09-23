using System.Collections.Generic;

namespace ParserObjects;

public record ErrorList(IReadOnlyList<IResult> ErrorResults);
