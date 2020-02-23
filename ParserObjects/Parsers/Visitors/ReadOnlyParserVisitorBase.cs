using System.Collections.Generic;

namespace ParserObjects.Parsers.Visitors
{
    public abstract class ReadOnlyParserVisitorBase<TState> : VisitorDispatcherBase, IParserVisitor
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
            parser.Accept(this);
            seen.Add(parser, parser);
            foreach (var child in parser.GetChildren())
                VisitInternal(child, seen);
            return parser;
        }
    }
}
