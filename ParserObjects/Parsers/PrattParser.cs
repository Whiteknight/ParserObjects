using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public class Pratt<TInput, TOperator, TOutput>
    {
        // See https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html

        public static IConfiguration CreateConfiguration() => new Configuration();

        private record InfixOperator (
            IParser<TInput, TOperator> Parser,
            int LeftAssociativity,
            int RightAssociativity,
            Func<TOutput, TOperator, TOutput, TOutput> Produce
        );

        private record PrefixOperator (
            IParser<TInput, TOperator> Parser,
            int RightAssociativity,
            Func<TOperator, TOutput, TOutput> Produce
        );
        

        private record PostfixOperator (
            IParser<TInput, TOperator> Parser,
            int LeftAssociativity,
            Func<TOutput, TOperator, TOutput> Produce 
        );
        

        private record CircumfixOperator(
            IParser<TInput, TOperator> StartParser,
            IParser<TInput, TOperator> EndParser,
            Func<TOperator, TOutput, TOperator, TOutput> Produce
        );

        private record PostcircumfixOperator(
            IParser<TInput, TOperator> StartParser,
            IParser<TInput, TOperator> EndParser,
            int LeftAssociativity,
            Func<TOutput, TOperator, TOutput, TOperator, TOutput> Produce
        );

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
        }

        public class Parser : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _values;

            private readonly Configuration _config;

            public Parser(IParser<TInput, TOutput> values, IConfiguration config)
            {
                _values = values;
                _config = config as Configuration ?? throw new ArgumentException("Expected Pratt.Configuration. You cannot use a subclass for this purpose. Use Pratt.CreateConfiguration() to create a configuration object.", nameof(config));
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state) => ParsePrecidence(state, 0);

            private struct GetPostfixOperatorResult
            {
                public GetPostfixOperatorResult(int leftAssociativity, Func<TOutput, TOutput> produce)
                {
                    LeftAssociativity = leftAssociativity;
                    Produce = produce;
                }

                public bool Success => Produce != null;
                public int LeftAssociativity { get; }
                public Func<TOutput, TOutput> Produce { get;  }
            }

            private struct GetInfixOperatorResult
            {
                public GetInfixOperatorResult(int leftAssociativity, int rightAssociativity, Func<TOutput, TOutput, TOutput> produce)
                {
                    LeftAssociativity = leftAssociativity;
                    RightAssociativity = rightAssociativity;
                    Produce = produce;
                }

                public bool Success => Produce != null;
                public int LeftAssociativity { get; }
                public int RightAssociativity { get; }
                public Func<TOutput, TOutput, TOutput> Produce { get; }
            }

            private IResult<TOutput> ParsePrecidence(ParseState<TInput> state, int minBp)
            {
                if (state.Input.IsAtEnd)
                    return state.Fail(this, "Sequence is at end");

                var (hasLeft, left) = GetLeftHandSide(state);
                if (!hasLeft)
                    return state.Fail(this, "Expected terminal/value result");

                while (!state.Input.IsAtEnd)
                {
                    // If we parse an operator but it's at the wrong level of precidence, we need
                    // to rewind and adjust the level of recursion.
                    var checkpoint = state.Input.Checkpoint();

                    // First see if we can attach a postfix or postcircumfix operator at this
                    // point. 
                    var postfixResult = GetPostfixOperator(state);
                    if (postfixResult.Success)
                    {
                        if (postfixResult.LeftAssociativity < minBp)
                        {
                            checkpoint.Rewind();
                            break;
                        }
                        left = postfixResult.Produce(left);
                        continue;
                    }

                    // Second see if we can parse an infix operator at this point.
                    var infixResult = GetInfixOperator(state);
                    if (infixResult.Success)
                    {
                        if (infixResult.LeftAssociativity < minBp)
                        {
                            checkpoint.Rewind();
                            break;
                        }
                        var rhsResult = ParsePrecidence(state, infixResult.RightAssociativity);
                        if (!rhsResult.Success)
                        {
                            checkpoint.Rewind();
                            break;
                        }

                        left = infixResult.Produce(left, rhsResult.Value);
                        continue;
                    }

                    // We can't match anything at this point, so we bail out.
                    break;
                }

                return state.Success(this, left);
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
                return (_, _) = _values.Parse(state);
            }

            private GetPostfixOperatorResult GetPostfixOperator(ParseState<TInput> state)
            {
                // Try to parse the postfix operators. These are atomic, so if there's a failure
                // we don't need to rewind.
                foreach (var opParser in _config.PostfixOperators)
                {
                    var opResult = opParser.Parser.Parse(state);
                    if (!opResult.Success)
                        continue;
                    TOutput produce(TOutput left) => opParser.Produce(left, opResult.Value);
                    return new GetPostfixOperatorResult(opParser.LeftAssociativity, produce);
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
                    TOutput produce(TOutput left) => opParser.Produce(left, startResult.Value, contents.Value, endResult.Value);
                    return new GetPostfixOperatorResult(opParser.LeftAssociativity, produce);
                }

                // No postfix or postcircumfix operators, so return nothing
                return new GetPostfixOperatorResult();
            }

            private GetInfixOperatorResult GetInfixOperator(ParseState<TInput> state)
            {
                // Check the infix operators to see if we have a match. Rewinds will happen
                // in the caller if the pattern doesn't work out, because of the recursion.
                foreach (var opParser in _config.InfixOperators)
                {
                    var opResult = opParser.Parser.Parse(state);
                    if (!opResult.Success)
                        continue;
                    TOutput produce(TOutput leftAssoc, TOutput rightAssoc) => opParser.Produce(leftAssoc, opResult.Value, rightAssoc);
                    return new GetInfixOperatorResult(opParser.LeftAssociativity, opParser.RightAssociativity, produce);
                }
                return new GetInfixOperatorResult();
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren()
            {
                var list = new List<IParser> { _values };
                _config.AppendParsers(list);
                return list;
            }
        }
    }
}
