using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers.Visitors
{
    public class FindParserVisitor
    {
        private readonly Func<IParser, bool> _predicate;
        private readonly bool _justOne;
        private readonly IList<IParser> _found;
        private readonly ICollection<IParser> _seen;

        private bool _canStop;

        public FindParserVisitor(Func<IParser, bool> predicate, bool justOne)
        {
            _predicate = predicate;
            _justOne = justOne;
            _canStop = false;
            _found = new List<IParser>();
            _seen = new HashSet<IParser>();
        }

        public static IParser Named(string name, IParser root)
        {
            var visitor = new FindParserVisitor(p => p.Name == name, true);
            visitor.Visit(root);
            return visitor._found.FirstOrDefault();
        }

        public static IReadOnlyList<TParser> OfType<TParser>(IParser root)
            where TParser : IParser
        {
            var visitor = new FindParserVisitor(p => p is TParser, false);
            visitor.Visit(root);
            return visitor._found.Cast<TParser>().ToList();
        }

        public static bool Replace(IParser root, Func<IParser, bool> predicate, IParser replacement)
        {
            if (root == null || predicate == null || replacement == null)
                return false;
            var visitor = new FindParserVisitor(p => p is IReplaceableParserUntyped && predicate(p), true);
            visitor.Visit(root);
            foreach (var found in visitor._found.Cast<IReplaceableParserUntyped>())
                found.SetParser(replacement);
            return true;
        }

        public static bool Replace(IParser root, string name, IParser replacement) 
            => Replace(root, p => p.Name == name, replacement);

        public IParser Visit(IParser parser)
        {
            if (parser == null)
                return null;
            return VisitInternal(parser);
        }

        private IParser VisitInternal(IParser parser)
        {
            if (_canStop || _seen.Contains(parser))
                return parser;

            _seen.Add(parser);
            if (_predicate(parser))
            {
                _found.Add(parser);
                if (_justOne)
                {
                    _canStop = true;
                    return parser;
                }
            }

            foreach (var child in parser.GetChildren())
            {
                VisitInternal(child);
                if (_canStop)
                    break;
            }

            return parser;
        }
    }
}
