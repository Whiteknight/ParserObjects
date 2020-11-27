using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public class Pratt<TInput, TOperator, TOutput>
    {
        public static IConfiguration CreateConfiguration() => new Configuration();

        private class InfixOperator
        {
            public InfixOperator(IParser<TInput, TOperator> parser, int leftAssociativity, int rightAssociativity, Func<TOutput, TOperator, TOutput, TOutput> produce)
            {
                Parser = parser;
                LeftAssociativity = leftAssociativity;
                RightAssociativity = rightAssociativity;
                Produce = produce;
            }

            public IParser<TInput, TOperator> Parser { get; }
            public int LeftAssociativity { get; }
            public int RightAssociativity { get; }
            public Func<TOutput, TOperator, TOutput, TOutput> Produce { get; }
        }

        private class PrefixOperator
        {
            public PrefixOperator(IParser<TInput, TOperator> parser, int rightAssociativity, Func<TOperator, TOutput, TOutput> produce)
            {
                Parser = parser;
                RightAssociativity = rightAssociativity;
                Produce = produce;
            }

            public IParser<TInput, TOperator> Parser { get; }
            public int RightAssociativity { get; }
            public Func<TOperator, TOutput, TOutput> Produce { get; }
        }

        private class PostfixOperator
        {
            public PostfixOperator(IParser<TInput, TOperator> parser, int leftAssociativity, Func<TOutput, TOperator, TOutput> produce)
            {
                Parser = parser;
                LeftAssociativity = leftAssociativity;
                Produce = produce;
            }

            public IParser<TInput, TOperator> Parser { get; }
            public int LeftAssociativity { get; }
            public Func<TOutput, TOperator, TOutput> Produce { get; }
        }

        private class CircumfixOperator
        {
            public CircumfixOperator(IParser<TInput, TOperator> startParser, IParser<TInput, TOperator> endParser, Func<TOperator, TOutput, TOperator, TOutput> produce)
            {
                StartParser = startParser;
                EndParser = endParser;
                Produce = produce;
            }

            public IParser<TInput, TOperator> StartParser { get; }
            public IParser<TInput, TOperator> EndParser { get; }
            public Func<TOperator, TOutput, TOperator, TOutput> Produce { get; }
        }

        private class PostcircumfixOperator
        {
            public PostcircumfixOperator(IParser<TInput, TOperator> startParser, IParser<TInput, TOperator> endParser, int leftAssociativity, Func<TOutput, TOperator, TOutput, TOperator, TOutput> produce)
            {
                StartParser = startParser;
                EndParser = endParser;
                LeftAssociativity = leftAssociativity;
                Produce = produce;
            }

            public int LeftAssociativity { get; }
            public IParser<TInput, TOperator> StartParser { get; }
            public IParser<TInput, TOperator> EndParser { get; }
            public Func<TOutput, TOperator, TOutput, TOperator, TOutput> Produce { get; }
        }

        public interface IConfiguration
        {
            IConfiguration AddInfixOperator(IParser<TInput, TOperator> matchOperator, int leftAssociativity, int rightAssociativity, Func<TOutput, TOperator, TOutput, TOutput> produce);
            IConfiguration AddPrefixOperator(IParser<TInput, TOperator> matchOperator, int rightAssociativity, Func<TOperator, TOutput, TOutput> produce);
            IConfiguration AddPostfixOperator(IParser<TInput, TOperator> matchOperator, int leftAssociativity, Func<TOutput, TOperator, TOutput> produce);
            IConfiguration AddCircumfixOperator(IParser<TInput, TOperator> matchStart, IParser<TInput, TOperator> matchEnd, Func<TOperator, TOutput, TOperator, TOutput> produce);
            IConfiguration AddPostcircumfixOperator(IParser<TInput, TOperator> matchStart, IParser<TInput, TOperator> matchEnd, int leftAssociativity, Func<TOutput, TOperator, TOutput, TOperator, TOutput> produce);
        }

        private class Configuration : IConfiguration
        {
            public List<InfixOperator> InfixOperators { get; }
            public List<PrefixOperator> PrefixOperators { get; }
            public List<PostfixOperator> PostfixOperators { get; }
            public List<CircumfixOperator> CircumfixOperators { get; }
            public List<PostcircumfixOperator> PostcircumfixOperators { get; }

            public Configuration()
            {
                InfixOperators = new List<InfixOperator>();
                PrefixOperators = new List<PrefixOperator>();
                PostfixOperators = new List<PostfixOperator>();
                CircumfixOperators = new List<CircumfixOperator>();
                PostcircumfixOperators = new List<PostcircumfixOperator>();
            }

            public Configuration(List<InfixOperator> infixOperators, List<PrefixOperator> prefixOperators, List<PostfixOperator> postfixOperators, List<CircumfixOperator> circumfixOperators, List<PostcircumfixOperator> postcircumfixOperators)
            {
                InfixOperators = infixOperators;
                PrefixOperators = prefixOperators;
                PostfixOperators = postfixOperators;
                CircumfixOperators = circumfixOperators;
                PostcircumfixOperators = postcircumfixOperators;
            }

            public IConfiguration AddInfixOperator(IParser<TInput, TOperator> matchOperator, int leftAssociativity, int rightAssociativity, Func<TOutput, TOperator, TOutput, TOutput> produce)
            {
                InfixOperators.Add(new InfixOperator(matchOperator, leftAssociativity, rightAssociativity, produce));
                return this;
            }

            public IConfiguration AddPrefixOperator(IParser<TInput, TOperator> matchOperator, int rightAssociativity, Func<TOperator, TOutput, TOutput> produce)
            {
                PrefixOperators.Add(new PrefixOperator(matchOperator, rightAssociativity, produce));
                return this;
            }

            public IConfiguration AddPostfixOperator(IParser<TInput, TOperator> matchOperator, int leftAssociativity, Func<TOutput, TOperator, TOutput> produce)
            {
                PostfixOperators.Add(new PostfixOperator(matchOperator, leftAssociativity, produce));
                return this;
            }

            public IConfiguration AddCircumfixOperator(IParser<TInput, TOperator> matchStart, IParser<TInput, TOperator> matchEnd, Func<TOperator, TOutput, TOperator, TOutput> produce)
            {
                CircumfixOperators.Add(new CircumfixOperator(matchStart, matchEnd, produce));
                return this;
            }

            public IConfiguration AddPostcircumfixOperator(IParser<TInput, TOperator> matchStart, IParser<TInput, TOperator> matchEnd, int leftAssociativity, Func<TOutput, TOperator, TOutput, TOperator, TOutput> produce)
            {
                PostcircumfixOperators.Add(new PostcircumfixOperator(matchStart, matchEnd, leftAssociativity, produce));
                return this;
            }

            public void AppendParsers(List<IParser> parsers)
            {
                parsers.AddRange(CircumfixOperators.SelectMany(c => new[] { c.StartParser, c.EndParser }));
                parsers.AddRange(InfixOperators.Select(c => c.Parser));
                parsers.AddRange(PostcircumfixOperators.SelectMany(c => new[] { c.StartParser, c.EndParser }));
                parsers.AddRange(PostfixOperators.Select(c => c.Parser));
                parsers.AddRange(PrefixOperators.Select(c => c.Parser));
            }

            public Configuration TryReplace(IParser find, IParser replace)
            {
                if (replace is not IParser<TInput, TOperator> typed)
                    return this;

                var any = false;

                List<InfixOperator> infix = null;
                if (InfixOperators.Any(op => op.Parser == find))
                {
                    any = true;
                    infix = new List<InfixOperator>();
                    foreach (var op in InfixOperators)
                        infix.Add(op.Parser == find ? new InfixOperator(typed, op.LeftAssociativity, op.RightAssociativity, op.Produce) : op);
                }

                List<PrefixOperator> prefix = null;
                if (PrefixOperators.Any(op => op.Parser == find))
                {
                    any = true;
                    prefix = new List<PrefixOperator>();
                    foreach (var op in PrefixOperators)
                        prefix.Add(op.Parser == find ? new PrefixOperator(typed, op.RightAssociativity, op.Produce) : op);
                }

                List<PostfixOperator> postfix = null;
                if (PostfixOperators.Any(op => op.Parser == find))
                {
                    any = true;
                    postfix = new List<PostfixOperator>();
                    foreach (var op in PostfixOperators)
                        postfix.Add(op.Parser == find ? new PostfixOperator(typed, op.LeftAssociativity, op.Produce) : op);
                }

                List<CircumfixOperator> circumfix = null;
                if (CircumfixOperators.Any(op => op.StartParser == find || op.EndParser == find))
                {
                    any = true;
                    circumfix = new List<CircumfixOperator>();
                    foreach (var op in CircumfixOperators)
                    {
                        var start = op.StartParser == find ? typed : op.StartParser;
                        var end = op.EndParser == find ? typed : op.EndParser;
                        circumfix.Add(start != op.StartParser || end != op.EndParser ? new CircumfixOperator(start, end, op.Produce) : op);
                    }
                }

                List<PostcircumfixOperator> postcircumfix = null;
                if (PostcircumfixOperators.Any(op => op.StartParser == find || op.EndParser == find))
                {
                    any = true;
                    postcircumfix = new List<PostcircumfixOperator>();
                    foreach (var op in PostcircumfixOperators)
                    {
                        var start = op.StartParser == find ? typed : op.StartParser;
                        var end = op.EndParser == find ? typed : op.EndParser;
                        postcircumfix.Add(start != op.StartParser || end != op.EndParser ? new PostcircumfixOperator(start, end, op.LeftAssociativity, op.Produce) : op);
                    }
                }

                if (!any)
                    return this;
                return new Configuration(
                    infix ?? InfixOperators,
                    prefix ?? PrefixOperators,
                    postfix ?? PostfixOperators,
                    circumfix ?? CircumfixOperators,
                    postcircumfix ?? PostcircumfixOperators
                );
            }
        }

        public class Parser : IParser<TInput, TOutput>
        {
            private IParser<TInput, TOutput> _values;

            private Configuration _config;

            public Parser(IParser<TInput, TOutput> values, IConfiguration config)
            {
                _values = values;
                _config = config as Configuration ?? throw new ArgumentException("Expected Pratt.Configuration. You cannot use a subclass for this purpose. Use Pratt.CreateConfiguration() to create a configuration object.", nameof(config));
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state) => ParsePrecidence(state, 0);

            private IResult<TOutput> ParsePrecidence(ParseState<TInput> state, int minBp)
            {
                if (state.Input.IsAtEnd)
                    return state.Fail(this, "Sequence is at end");

                var (hasLhs, lhs) = GetLeftHandSide(state);
                if (!hasLhs)
                    return state.Fail(this, "Expected terminal/value result");

                while (true)
                {
                    if (state.Input.IsAtEnd)
                        break;

                    // If we parse an operator but it's at the wrong level of precidence, we need
                    // to rewind and adjust the level of recursion.
                    var checkpoint = state.Input.Checkpoint();

                    // First see if we can attach a postfix or postcircumfix operator at this
                    // point. 
                    var (hasPOp, plbp, pproduce) = GetPostfixOperator(state);
                    if (hasPOp)
                    {
                        if (plbp < minBp)
                        {
                            checkpoint.Rewind();
                            break;
                        }
                        lhs = pproduce(lhs);
                        continue;
                    }

                    // Second see if we can parse an infix operator at this point.
                    var (hasIOp, lbp, rbp, produce) = GetInfixOperator(state);
                    if (hasIOp)
                    {
                        if (lbp < minBp)
                        {
                            checkpoint.Rewind();
                            break;
                        }
                        var rhsResult = ParsePrecidence(state, rbp);
                        if (!rhsResult.Success)
                        {
                            checkpoint.Rewind();
                            break;
                        }

                        lhs = produce(lhs, rhsResult.Value);
                        continue;
                    }

                    // We can't match anything at this point, so we bail out.
                    break;
                }

                return state.Success(this, lhs);
            }

            private (bool success, TOutput value) GetLeftHandSide(ParseState<TInput> state)
            {
                var checkpoint = state.Input.Checkpoint();
                // Check all possible circumfix operators. Once we see the start production, we
                // have to rewind if there are any failures.
                foreach (var opParser in _config.CircumfixOperators)
                {
                    var startResult = opParser.StartParser.Parse(state);
                    if (!startResult.Success)
                        continue;
                    var lhsResult = ParsePrecidence(state, 0);
                    if (!lhsResult.Success)
                    {
                        checkpoint.Rewind();
                        continue;
                    }
                    var endResult = opParser.EndParser.Parse(state);
                    if (!endResult.Success)
                    {
                        checkpoint.Rewind();
                        continue;
                    }
                    var rhs = opParser.Produce(startResult.Value, lhsResult.Value, endResult.Value);
                    return (true, rhs);
                }
                
                // Check the prefix operators. Once we see the operator, we need to rewind on
                // failure
                foreach (var opParser in _config.PrefixOperators)
                {
                    var opResult = opParser.Parser.Parse(state);
                    if (!opResult.Success)
                        continue;
                    var rbp = opParser.RightAssociativity;
                    var rhsResult = ParsePrecidence(state, rbp);
                    if (!rhsResult.Success)
                    {
                        checkpoint.Rewind();
                        continue;
                    }
                    var rhs = opParser.Produce(opResult.Value, rhsResult.Value);
                    return (true, rhs);
                }

                // No operators, try to parse a terminal value
                var (hasValue, value) = _values.Parse(state);
                return (hasValue, value);
            }

            private (bool success, int lbp, Func<TOutput, TOutput> produce) GetPostfixOperator(ParseState<TInput> state)
            {
                // Try to parse the postfix operators. These are atomic, so if there's a failure
                // we don't need to rewind.
                foreach (var opParser in _config.PostfixOperators)
                {
                    var opResult = opParser.Parser.Parse(state);
                    if (!opResult.Success)
                        continue;
                    TOutput produce(TOutput lhs) => opParser.Produce(lhs, opResult.Value);
                    return (true, opParser.LeftAssociativity, produce);

                }

                // Check postcircumfix operators. Once we start matching things, if we see a
                // fail we have to rewind
                var checkpoint = state.Input.Checkpoint();
                foreach (var opParser in _config.PostcircumfixOperators)
                {
                    var startResult = opParser.StartParser.Parse(state);
                    if (!startResult.Success)
                        continue;
                    var contents = ParsePrecidence(state, 0);
                    if (!contents.Success)
                    {
                        checkpoint.Rewind();
                        continue;
                    }
                    var endResult = opParser.EndParser.Parse(state);
                    if (!endResult.Success)
                    {
                        checkpoint.Rewind();
                        continue;
                    }
                    TOutput produce(TOutput lhs) => opParser.Produce(lhs, startResult.Value, contents.Value, endResult.Value);
                    return (true, opParser.LeftAssociativity, produce);
                }

                // No postfix or postcircumfix operators, so return nothing
                return (false, 0, null);
            }

            private (bool success, int lbp, int rbp, Func<TOutput, TOutput, TOutput> produce) GetInfixOperator(ParseState<TInput> state)
            {
                // Check the infix operators to see if we have a match. Rewinds will happen
                // in the caller if the pattern doesn't work out, because of the recursion.
                foreach (var opParser in _config.InfixOperators)
                {
                    var opResult = opParser.Parser.Parse(state);
                    if (!opResult.Success)
                        continue;
                    TOutput produce(TOutput lhs, TOutput rhs) => opParser.Produce(lhs, opResult.Value, rhs);
                    return (true, opParser.LeftAssociativity, opParser.RightAssociativity, produce);
                }
                return (false, 0, 0, null);
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IParser ReplaceChild(IParser find, IParser replace)
            {
                if (find == _values && replace is IParser<TInput, TOutput> typed)
                    return new Parser(typed, _config);
                var newConfig = _config.TryReplace(find, replace);
                if (newConfig != null && newConfig != _config)
                    return new Parser(_values, newConfig);
                return this;
            }

            public IEnumerable<IParser> GetChildren()
            {
                var list = new List<IParser> { _values };
                _config.AppendParsers(list);
                return list;
            }
        }
    }
}
