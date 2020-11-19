using System.Collections.Generic;

namespace ParserObjects.Tests.Examples.SExpr
{
    public class AtomNode : INode
    {
        public ValueType Type { get; set; }
        public object Value { get; set; }
        public List<string> Diagnostics { get; set; }
        public Location Location { get; set; }
    }
}
