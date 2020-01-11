using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ParserObjects.Parsers.Visitors
{
    public class ReplaceParserVisitor : IParserVisitor
    {
        private readonly Func<IParser, bool> _predicate;
        private readonly IParser _replacement;
        private readonly Dictionary<IParser, IParser> _seen;

        public ReplaceParserVisitor(Func<IParser, bool> predicate, IParser replacement)
        {
            _predicate = predicate;
            _replacement = replacement;
            _seen = new Dictionary<IParser, IParser>();
        }

        public IParser Visit(IParser parser)
        {
            if (_seen.ContainsKey(parser))
                return _seen[parser];

            if (_predicate(parser))
            {
                _seen.Add(parser, _replacement);
                return _replacement;
            }

            var newParser = parser;
            _seen.Add(parser, newParser);

            foreach (var child in parser.GetChildren())
            {
                var newChild = Visit(child);
                if (newChild == child)
                    continue;
                Debug.WriteLine("Replacing " + child);
                newParser = newParser.ReplaceChild(child, newChild);
                newParser.Name = (parser.Name ?? "") + ".Replaced";
            }

            _seen[parser] = newParser;
            return newParser;
        }
    }
}
