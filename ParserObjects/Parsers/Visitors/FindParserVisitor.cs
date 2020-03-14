using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers.Visitors
{
    /// <summary>
    /// Parser-visitor type to traverse the parser tree and find matching parser nodes.
    /// </summary>
    public class FindParserVisitor
    {
        private class State
        {
            public Func<IParser, bool> Predicate { get; }
            public bool JustOne { get; }
            public IList<IParser> Found { get; }
            public ICollection<IParser> Seen { get; }

            public bool CanStop { get; set;  }

            public State(Func<IParser, bool> predicate, bool justOne)
            {
                Predicate = predicate;
                JustOne = justOne;
                CanStop = false;
                Found = new List<IParser>();
                Seen = new HashSet<IParser>();
            }
        }

        /// <summary>
        /// Search for a parser with the given Name. Returns only the first result in case of duplicates
        /// </summary>
        /// <param name="name"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IParser Named(string name, IParser root)
        {
            Assert.ArgumentNotNullOrEmpty(name, nameof(name));
            Assert.ArgumentNotNull(root, nameof(root));
            var visitor = new FindParserVisitor();
            var state = new State(p => p.Name == name, true);
            visitor.Visit(root, state);
            return state.Found.FirstOrDefault();
        }

        /// <summary>
        /// Search for all parsers of the given type. Returns all results.
        /// </summary>
        /// <typeparam name="TParser"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IReadOnlyList<TParser> OfType<TParser>(IParser root)
            where TParser : IParser
        {
            Assert.ArgumentNotNull(root, nameof(root));
            var visitor = new FindParserVisitor();
            var state = new State(p => p is TParser, false);
            visitor.Visit(root, state);
            return state.Found.Cast<TParser>().ToList();
        }

        /// <summary>
        /// Search for ReplaceableParsers matching a predicate and attempt to replace their contents with the
        /// replacement parser if it is found. The replacement parser must be non-null and of the correct
        /// type. Replaces all matching instances.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="predicate"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static bool Replace(IParser root, Func<IReplaceableParserUntyped, bool> predicate, IParser replacement)
        {
            if (root == null || predicate == null || replacement == null)
                return false;
            var visitor = new FindParserVisitor();
            var state = new State(p => p is IReplaceableParserUntyped replaceable && predicate(replaceable), true);
            visitor.Visit(root, state);
            foreach (var found in state.Found.Cast<IReplaceableParserUntyped>())
                found.SetParser(replacement);
            return true;
        }

        /// <summary>
        /// Search for ReplaceableParsers with the given name and attempt to replace their contents with the
        /// replacement parser. The replacement parser must be non-null and of the correct type. Replaces
        /// all matching instances.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="name"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static bool Replace(IParser root, string name, IParser replacement) 
            => Replace(root, p => p.Name == name, replacement);

        private void Visit(IParser parser, State state)
        {
            if (parser == null)
                return;
            VisitInternal(parser, state);
        }

        private void VisitInternal(IParser parser, State state)
        {
            if (state.CanStop || state.Seen.Contains(parser))
                return;
            state.Seen.Add(parser);

            if (state.Predicate(parser))
            {
                state.Found.Add(parser);
                if (state.JustOne)
                {
                    state.CanStop = true;
                    return;
                }
            }

            foreach (var child in parser.GetChildren())
            {
                VisitInternal(child, state);
                if (state.CanStop)
                    break;
            }
        }
    }
}
