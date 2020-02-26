using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParserObjects.Parsers.Logical;

namespace ParserObjects.Parsers.Visitors
{
    public class BnfStringifyVisitor
    {
        private readonly StringBuilder _builder;
        private readonly Stack<StringBuilder> _history;
        private readonly HashSet<IParser> _seen;
        protected StringBuilder _current;

        public BnfStringifyVisitor(StringBuilder sb)
        {
            _builder = sb;
            _history = new Stack<StringBuilder>();
            _seen = new HashSet<IParser>();
        }

        public static string ToBnf(IParser parser)
        {
            var sb = new StringBuilder();
            var visitor = new BnfStringifyVisitor(sb);
            visitor.Visit(parser);
            return sb.ToString();
        }

        public void Visit(IParser parser)
        {
            // Top-level sb, we'll be throwing this away
            _current = new StringBuilder();
            VisitChild(parser);
        }

        protected virtual void VisitChild(IParser parser)
        {
            if (parser == null)
                return;
            if (_seen.Contains(parser))
            {
                if (!string.IsNullOrEmpty(parser.Name))
                {
                    _current.Append("<");
                    _current.Append(parser.Name);
                    _current.Append(">");
                    return;
                }

                _current.Append("<ALREADY SEEN UNNAMED PARSER>");
                return;
            }

            _seen.Add(parser);

            if (string.IsNullOrEmpty(parser.Name))
            {
                ((dynamic)this).VisitTyped((dynamic)parser);
                return;
            }

            // 1. Append a tag to the current builder
            _current.Append("<");
            _current.Append(parser.Name);
            _current.Append(">");

            // 2. Start a new builder to write out the child rule
            _history.Push(_current);
            _current = new StringBuilder();
            ((dynamic)this).VisitTyped((dynamic)parser);

            // 3. Write the child rule to the overall builder, then pop it
            var rule = _current.ToString();
            if (!string.IsNullOrEmpty(rule))
            {
                _builder.Append(parser.Name);
                _builder.Append(" = ");
                _builder.AppendLine(_current.ToString());
            }

            _current = _history.Pop();
        }

        // fallback method if a better match can't be found
        protected virtual void VisitTyped(IParser p)
        {
            Console.WriteLine($"No override match found for {p.GetType().Name}");
            _current.Append("UNSUPPORTED_TYPE");
        }

        protected virtual void VisitTyped<TInput>(AndParser<TInput> p)
        {
            var children = p.GetChildren().ToArray();
            VisitChild(children[0]);
            _current.Append(" && ");
            VisitChild(children[1]);
        }

        protected virtual void VisitTyped<TInput>(AnyParser<TInput> p)
        {
            _current.Append("ANY");
        }

        protected virtual void VisitTyped<TInput, TOutput>(DeferredParser<TInput, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
        }

        protected virtual void VisitTyped<TInput>(EmptyParser<TInput> p)
        {
            // TODO: Would like to output lower-case epsilon, which is the usual symbol
            _current.Append("EMPTY");
        }

        protected virtual void VisitTyped<TInput>(EndParser<TInput> p)
        {
            _current.Append("END");
        }

        protected virtual void VisitTyped<TInput, TOutput>(FailParser<TInput, TOutput> p)
        {
            _current.Append("FAIL");
        }

        protected virtual void VisitTyped<TInput, TOutput>(FirstParser<TInput, TOutput> p)
        {
            var children = p.GetChildren();
            _current.Append("(");
            VisitChild(children.First());

            foreach (var child in children.Skip(1))
            {
                _current.Append(" | ");
                VisitChild(child);
            }

            _current.Append(")");
        }

        protected virtual void VisitTyped<TInput, TCollection, TOutput>(FlattenParser<TInput, TCollection, TOutput> p) 
            where TCollection : IEnumerable<TOutput>
        {
            VisitChild(p.GetChildren().First());
        }

        protected virtual void VisitTyped<TInput, TOutput>(LeftApplyZeroOrMoreParser<TInput, TOutput> p)
        {
            // TODO: Find a better way to represent this in the generated BNF
            var children = p.GetChildren().ToArray();
            VisitChild(children[0]);
            _current.Append(" => ");
            VisitChild(children[1]);
        }

        protected virtual void VisitTyped<TInput, TOutput>(ListParser<TInput, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
            _current.Append(p.AtLeastOne ? "+" : "*");
        }

        protected virtual void VisitTyped<TInput>(MatchPredicateParser<TInput> p)
        {
            // TODO: find a way to do something with this
            //_current.Append("PREDICATE")
        }

        protected virtual void VisitTyped<TInput>(MatchSequenceParser<TInput> p)
        {
            var pattern = string.Join(" ", p.Pattern.Select(i => $"'{i.ToString()}'"));
            _current.Append(pattern);
        }

        protected virtual void VisitTyped<TInput>(NegativeLookaheadParser<TInput> p)
        {
            // TODO: What's the right symbol to use for this?
            _current.Append("!");
            VisitChild(p.GetChildren().First());
        }

        protected virtual void VisitTyped<TInput>(OrParser<TInput> p)
        {
            var children = p.GetChildren().ToArray();
            VisitChild(children[0]);
            _current.Append(" || ");
            VisitChild(children[1]);
        }

        protected virtual void VisitTyped<TInput>(PositiveLookaheadParser<TInput> p)
        {
            // TODO: What's the right symbol to use for this?
            _current.Append("+");
            VisitChild(p.GetChildren().First());
        }

        protected virtual void VisitTyped<TInput, TOutput>(ProduceParser<TInput, TOutput> p)
        {
            _current.Append("EMPTY");
        }

        protected virtual void VisitTyped<TInput>(NotParser<TInput> p)
        {
            var child = p.GetChildren().First();
            _current.Append("!");
            VisitChild(child);
        }

        protected virtual void VisitTyped<TInput, TOutput>(ReplaceableParser<TInput, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
        }

        protected virtual void VisitTyped<TInput, TMiddle, TOutput>(RightApplyZeroOrMoreParser<TInput, TMiddle, TOutput> p)
        {
            // TODO: What's the correct notation for this?
            var children = p.GetChildren().ToArray();
            VisitChild(children[0]);
            _current.Append(" (");
            VisitChild(children[1]);
            _current.Append($" <{p.Name ?? "SELF"}>!)");
        }

        protected virtual void VisitTyped<TInput, TOutput>(RuleParser<TInput, TOutput> p)
        {
            _current.Append("(");
            var children = p.GetChildren().ToArray();
            VisitChild(children[0]);
            foreach (var child in children.Skip(1))
            {
                _current.Append(" ");
                VisitChild(child);
            }

            _current.Append(")");
        }

        protected virtual void VisitTyped<TInput, TMiddle, TOutput>(TransformParser<TInput, TMiddle, TOutput> p)
        {
            VisitChild(p.GetChildren().First());
        }

        protected virtual void VisitTyped<TInput, TOutput>(TrieParser<TInput, TOutput> p)
        {
            // TODO: Iterate all entries in the Trie and output each
            // TODO: Should output with the same syntax as First
        }
    }
}
