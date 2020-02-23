using System.Collections.Generic;

namespace ParserObjects.Parsers.Visitors
{
    public abstract class RewritingParserVisitorBase : VisitorDispatcherBase, IParserVisitor
    {
        public virtual IParser Visit(IParser parser)
        {
            var seen = new Dictionary<IParser, IParser>();
            return VisitInternal(parser, seen);
        }

        private IParser VisitInternal(IParser parser, Dictionary<IParser, IParser> seen)
        {
            if (parser == null)
                return null;
            if (seen.ContainsKey(parser))
                return seen[parser];
            var newParser = parser.Accept(this);
            newParser.Name = parser.Name;
            seen.Add(parser, newParser);
            foreach (var child in newParser.GetChildren())
            {
                var newChild = VisitInternal(child, seen);
                if (newChild == child)
                    continue;
                
                newParser = newParser.ReplaceChild(child, newChild);
                newParser.Name = parser.Name;
            }

            return newParser;
        }
    }
}