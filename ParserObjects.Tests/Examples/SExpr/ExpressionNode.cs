using System.Collections.Generic;

namespace ParserObjects.Tests.Examples.SExpr
{
    public class ExpressionNode : INode
    {
        public List<INode> Children { get; set; }
        public List<string> Diagnostics { get; set; }
        public Location Location { get; set; }
    }
}
