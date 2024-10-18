using System.Collections.Generic;

namespace ParserObjects.Tests.Examples.SExpr;

public interface INode
{
    List<string> Diagnostics { get; set; }
    Location Location { get; set; }
}
