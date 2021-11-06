using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ParserObjects.Parsers;
using ParserObjects.Parsers.Multi;
using ParserObjects.Utility;

namespace ParserObjects.Visitors
{
    /// <summary>
    /// Parser-visitor to traverse the parser-graph and attempt to produce a string of approximate
    /// pseudo-BNF to describe the grammar. Proper execution of this visitor depends on parsers having
    /// the .Name value set. If you have custom parser types you can create a subclass of this visitor
    /// type with signature 'public VisitChild(MyParserType parser, State state)' and it should dispatch to
    /// them as required.
    /// </summary>
    public class BnfStringifyVisitor
    {
        // Uses a syntax inspired by W3C EBNF (https://www.w3.org/TR/REC-xml/#sec-notation) and Regex
        // (for extensions beyond what EBNF normally handles). This NOT intended for round-trip operations
        // or formal analysis purposes.

        public class State
        {
            public StringBuilder Builder { get; }
            public Stack<StringBuilder> History { get; }
            public HashSet<IParser> Seen { get; }
            public StringBuilder Current { get; set; }

            public State(StringBuilder sb)
            {
                Builder = sb;
                History = new Stack<StringBuilder>();
                Seen = new HashSet<IParser>();
                Current = new StringBuilder();
            }
        }

        public string ToBnf(IParser parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));
            var sb = new StringBuilder();
            var state = new State(sb);
            Visit(parser, state);
            return sb.ToString();
        }

        private void Visit(IParser parser, State state)
        {
            // Top-level sb
            VisitChild(parser, state);
        }

        protected virtual void VisitChild(IParser parser, State state)
        {
            if (parser == null)
                return;

            // If we have already seen this parser, just put in a tag to represent it
            if (state.Seen.Contains(parser))
            {
                if (!string.IsNullOrEmpty(parser.Name))
                {
                    state.Current.Append('<').Append(parser.Name).Append('>');
                    return;
                }

                if (!parser.GetChildren().Any())
                {
                    // if it's a simple parser with no recursion, we can just visit it again.
                    // No harm because it won't cause a loop
                    ((dynamic)this).Accept((dynamic)parser, state);
                    return;
                }

                state.Current.Append("<ALREADY SEEN UNNAMED PARSER>");
                return;
            }

            state.Seen.Add(parser);

            // If the parser doesn't have a name, recursively visit it in-place
            if (string.IsNullOrEmpty(parser.Name))
            {
                ((dynamic)this).Accept((dynamic)parser, state);
                return;
            }

            // If the parser does have a name, write a tag for it
            state.Current.Append('<');
            state.Current.Append(parser.Name);
            state.Current.Append('>');

            // Start a new builder, so we can start stringifying this new parser on it's own line.
            state.History.Push(state.Current);
            state.Current = new StringBuilder();

            // Visit the parser recursively to fill in the builder
            ((dynamic)this).Accept((dynamic)parser, state);

            // Append the current builder to the overall builder
            var rule = state.Current.ToString();
            if (!string.IsNullOrEmpty(rule))
            {
                state.Builder.Append(parser.Name);
                state.Builder.Append(" := ");
                state.Builder.Append(state.Current);
                state.Builder.AppendLine(";");
            }

            state.Current = state.History.Pop();
        }

        // fallback method if a better match can't be found. This method should never be called if
        // we properly define a .Accept method variant for every type of parser
        protected virtual void Accept(IParser p, State state)
        {
            Debug.WriteLine($"No override match found for {p.GetType().Name}");
            state.Current.Append("UNSUPPORTED_TYPE");
        }

        // Some regex syntax:
        // (?= ) is "positive lookahead" syntax. We use it to shoe something doesn't consume input
        // (?! ) is "negative lookahead" syntax. We use it for non-follows situations

        protected virtual void Accept<TInput>(AndParser<TInput> p, State state)
        {
            var children = p.GetChildren().ToArray();
            VisitChild(children[0], state);
            state.Current.Append(" && ");
            VisitChild(children[1], state);
        }

        protected virtual void Accept<TInput>(AnyParser<TInput> p, State state)
        {
            state.Current.Append('.');
        }

        protected virtual void Accept<TInput, TOutput>(Cache.MultiParser<TInput, TOutput> p, State state)
        {
            VisitChild(p.Inner, state);
        }

        protected virtual void Accept<TInput, TOutput>(Cache.NoOutputParser<TInput> p, State state)
        {
            VisitChild(p.Inner, state);
        }

        protected virtual void Accept<TInput, TOutput>(Cache.OutputParser<TInput, TOutput> p, State state)
        {
            VisitChild(p.Inner, state);
        }

        protected virtual void Accept<TInput, TMiddle, TOutput>(Chain<TInput, TMiddle, TOutput>.Parser p, State state)
        {
            var child = p.GetChildren().Single();
            VisitChild(child, state);
            state.Current.Append("->Chain");
        }

        protected virtual void Accept<TInput, TOutput>(Chain<TInput, TOutput>.Parser p, State state)
        {
            var child = p.GetChildren().Single();
            VisitChild(child, state);
            state.Current.Append("->Chain");
        }

        protected virtual void Accept<TInput, TOutput>(Context<TInput, TOutput>.Parser p, State state)
        {
            VisitChild(p.GetChildren().Single(), state);
        }

        protected virtual void Accept<TInput, TMulti, TOutput>(ContinueWith.MultiParser<TInput, TMulti, TOutput> p, State state)
        {
            VisitChild(p.GetChildren().Single(), state);
            state.Current.Append(" CONTINUE");
        }

        protected virtual void Accept<TInput, TMulti, TOutput>(ContinueWith.SingleParser<TInput, TMulti, TOutput> p, State state)
        {
            VisitChild(p.GetChildren().Single(), state);
            state.Current.Append(" CONTINUE");
        }

        protected virtual void Accept<TInput, TOutput>(Create<TInput, TOutput>.Parser p, State state)
        {
            state.Current.Append("CREATE");
        }

        protected virtual void Accept<TInput, TOutput>(DeferredParser<TInput, TOutput> p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }

        protected virtual void Accept<TInput, TOutput>(Earley<TInput, TOutput>.Parser p, State state)
        {
            state.Current.Append(p.GetBnf());
        }

        protected virtual void Accept<TInput>(EmptyParser<TInput> p, State state)
        {
            state.Current.Append("()");
        }

        protected virtual void Accept<TInput>(EndParser<TInput> p, State state)
        {
            state.Current.Append("END");
        }

        protected virtual void Accept<TInput>(Examine<TInput>.Parser p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }

        protected virtual void Accept<TInput, TOutput>(Examine<TInput, TOutput>.Parser p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }

        protected virtual void Accept<TInput, TOutput>(FailParser<TInput, TOutput> p, State state)
        {
            state.Current.Append("FAIL");
        }

        protected virtual void Accept<TInput, TOutput>(FirstParser<TInput, TOutput> p, State state)
        {
            var children = p.GetChildren().ToList();
            if (children.Count == 1)
            {
                VisitChild(children[0], state);
                return;
            }

            state.Current.Append('(');
            VisitChild(children[0], state);

            for (int i = 1; i <= children.Count - 2; i++)
            {
                state.Current.Append(" | ");
                VisitChild(children[i], state);
            }

            if (children.Count >= 2)
            {
                var last = children[children.Count - 1];
                if (last is Produce<TInput, TOutput>.Parser)
                {
                    state.Current.Append(")?");
                    return;
                }

                state.Current.Append(" | ");
                VisitChild(last, state);
            }

            state.Current.Append(')');
        }

        protected virtual void Accept<TInput, TOutput>(FuncParser<TInput, TOutput> p, State state)
        {
            state.Current.Append("User Function");
        }

        protected virtual void Accept<TInput, TOutput>(IfParser<TInput, TOutput> p, State state)
        {
            var children = p.GetChildren().ToArray();
            state.Current.Append("IF ");
            VisitChild(children[0], state);
            state.Current.Append(" THEN ");
            VisitChild(children[1], state);
            state.Current.Append(" ELSE ");
            VisitChild(children[2], state);
        }

        protected virtual void Accept<TInput, TOutput>(LeftApply<TInput, TOutput>.Parser p, State state)
        {
            var children = p.GetChildren().ToArray();
            var initial = children[0];
            var middle = children[1];
            VisitChild(middle, state);
            state.Current.Append(" | ");
            VisitChild(initial, state);
        }

        protected virtual void Accept<TInput, TOutput>(LimitedListParser<TInput, TOutput> p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
            if (p.Maximum.HasValue)
            {
                if (p.Maximum == p.Minimum)
                    state.Current.Append('{').Append(p.Minimum).Append('}');
                else
                    state.Current.Append('{').Append(p.Minimum).Append(", ").Append(p.Maximum).Append('}');
            }
            else
            {
                if (p.Minimum == 0)
                    state.Current.Append('*');
                else if (p.Minimum == 1)
                    state.Current.Append('+');
                else
                    state.Current.Append('{').Append(p.Minimum).Append(",}");
            }
        }

        protected virtual void Accept<TInput>(MatchPredicateParser<TInput> p, State state)
        {
            state.Current.Append("MATCH");
        }

        protected virtual void Accept<TInput>(MatchPatternParser<TInput> p, State state)
        {
            var pattern = string.Join(" ", p.Pattern.Select(i => $"'{i}'"));
            state.Current.Append(pattern);
        }

        protected virtual void Accept<TInput>(NegativeLookaheadParser<TInput> p, State state)
        {
            state.Current.Append("(?! ");
            VisitChild(p.GetChildren().First(), state);
            state.Current.Append(" )");
        }

        protected virtual void Accept<TInput>(NoneParser<TInput> p, State state)
        {
            var child = p.GetChildren().Single();
            state.Current.Append("(?=");
            VisitChild(child, state);
            state.Current.Append(')');
        }

        protected virtual void Accept<TInput, TOutput>(NoneParser<TInput, TOutput> p, State state)
        {
            var child = p.GetChildren().Single();
            state.Current.Append("(?=");
            VisitChild(child, state);
            state.Current.Append(')');
        }

        protected virtual void Accept<TInput>(NotParser<TInput> p, State state)
        {
            var child = p.GetChildren().First();
            state.Current.Append('!');
            VisitChild(child, state);
        }

        protected virtual void Accept<TInput>(OrParser<TInput> p, State state)
        {
            var children = p.GetChildren().ToArray();
            VisitChild(children[0], state);
            state.Current.Append(" || ");
            VisitChild(children[1], state);
        }

        protected virtual void Accept<TInput>(PeekParser<TInput> p, State state)
        {
            state.Current.Append("(?=.)");
        }

        protected virtual void Accept<TInput, TOutput>(PrattParser<TInput, TOutput> p, State state)
        {
            var children = p.GetChildren().ToArray();
            if (children.Length == 0)
            {
                state.Current.Append("PRATT()");
                return;
            }

            state.Current.Append("PRATT(");
            VisitChild(children[0], state);
            for (int i = 1; i < children.Length; i++)
            {
                state.Current.Append(", ");
                VisitChild(children[i], state);
            }

            state.Current.Append(')');
        }

        protected virtual void Accept<TInput, TOutput>(Pratt.ParseContext<TInput, TOutput> p, State state)
        {
            state.Current.Append("PRATT RECURSE");
        }

        protected virtual void Accept<TInput>(PositiveLookaheadParser<TInput> p, State state)
        {
            state.Current.Append("(?= ");
            VisitChild(p.GetChildren().First(), state);
            state.Current.Append(" )");
        }

        protected virtual void Accept<TInput, TOutput>(Produce<TInput, TOutput>.Parser p, State state)
        {
            state.Current.Append("PRODUCE");
        }

        protected virtual void Accept<TInput, TOutput>(Produce<TInput, TOutput>.MultiParser p, State state)
        {
            state.Current.Append("PRODUCE");
        }

        protected virtual void Accept<TInput, TMulti, TOutput>(ReduceParser<TInput, TMulti, TOutput> p, State state)
        {
            state.Current.Append("REDUCE ");
            VisitChild(p.GetChildren().Single(), state);
        }

        protected virtual void Accept(RegexParser p, State state)
        {
            state.Current.Append('/').Append(p.Pattern).Append('/');
        }

        protected virtual void Accept<TInput, TOutput>(ReplaceableParser<TInput, TOutput> p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }

        protected virtual void Accept<TInput, TMiddle, TOutput>(RightApply<TInput, TMiddle, TOutput>.Parser p, State state)
        {
            var children = p.GetChildren().ToArray();
            VisitChild(children[0], state);
            state.Current.Append(" (");
            VisitChild(children[1], state);
            state.Current.Append(string.IsNullOrEmpty(p.Name) ? " SELF" : $" <{p.Name}>)*");
        }

        protected virtual void Accept<TInput, TOutput>(RuleParser<TInput, TOutput> p, State state)
        {
            state.Current.Append('(');
            var children = p.GetChildren().ToArray();
            VisitChild(children[0], state);
            foreach (var child in children.Skip(1))
            {
                state.Current.Append(' ');
                VisitChild(child, state);
            }

            state.Current.Append(')');
        }

        protected virtual void Accept<TInput, TOutput>(SelectSingleResultParser<TInput, TOutput> p, State state)
        {
            state.Current.Append("SELECT ");
            VisitChild(p.GetChildren().Single(), state);
        }

        protected virtual void Accept<TInput, TOutput>(Sequential.Parser<TInput, TOutput> p, State state)
        {
            state.Current.Append("User Function");
        }

        protected virtual void Accept<TInput, TMiddle, TOutput>(Transform<TInput, TMiddle, TOutput>.Parser p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }

        protected virtual void Accept<TInput, TOutput>(TrieParser<TInput, TOutput> p, State state)
        {
            var allPatterns = p.Trie.GetAllPatterns().ToList();
            if (allPatterns.Count == 0)
                return;

            static void PrintPattern(IEnumerable<TInput> pattern, State s)
            {
                s.Current.Append('(');
                var spaceSeparatedQuotedItems = string.Join(" ", pattern.Select(item => $"'{item}'"));
                s.Current.Append(spaceSeparatedQuotedItems);
                s.Current.Append(')');
            }

            PrintPattern(allPatterns[0], state);

            foreach (var pattern in allPatterns.Skip(1))
            {
                state.Current.Append(" | ");
                PrintPattern(pattern, state);
            }
        }

        protected virtual void Accept<TInput, TOutput>(Try.Parser<TInput, TOutput> p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }

        protected virtual void Accept<TInput>(Try.Parser<TInput> p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }

        protected virtual void Accept<TInput, TOutput>(Try.MultiParser<TInput, TOutput> p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }

        protected virtual void Accept<TInput>(Try.MultiParser<TInput> p, State state)
        {
            VisitChild(p.GetChildren().First(), state);
        }
    }
}
