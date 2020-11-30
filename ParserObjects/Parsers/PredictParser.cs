using ParserObjects.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser and related machinery to use a lookahead value to control the parse
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class Predict<TInput, TOutput>
    {
        /// <summary>
        /// Create a new Configuration object
        /// </summary>
        /// <returns></returns>
        public static IConfiguration CreateConfiguration() => new Configuration();

        /// <summary>
        /// Configuration for a predict parser. Allows setting a list of predicates and the parser
        /// to invoke when the predicate succeeds
        /// </summary>
        public interface IConfiguration
        {
            IConfiguration Add(Func<TInput, bool> equals, IParser<TInput, TOutput> parser);
        }

        class Configuration : IConfiguration
        {
            private readonly List<(Func<TInput, bool> equals, IParser<TInput, TOutput> parser)> _parsers;

            public Configuration()
            {
                _parsers = new List<(Func<TInput, bool> equals, IParser<TInput, TOutput> parser)>();
            }

            public IConfiguration Add(Func<TInput, bool> equals, IParser<TInput, TOutput> parser)
            {
                _parsers.Add((equals, parser));
                return this;
            }

            public IParser<TInput, TOutput> Pick(TInput next) 
                => _parsers
                    .Where(rule => rule.equals(next))
                    .Select(rule => rule.parser)
                    .FirstOrDefault();

            public IEnumerable<IParser> GetChildren()
                => _parsers.Select(v => v.parser);

            public Configuration ReplaceChild(IParser find, IParser replace)
            {
                if (replace is not IParser<TInput, TOutput> typed)
                    return this;
                if (!_parsers.Any(p => ReferenceEquals(p.parser, find)))
                    return this;
                var newConfig = new Configuration();
                foreach (var (equals, parser) in _parsers)
                {
                    var newParser = ReferenceEquals(parser, find) ? typed : parser;
                    newConfig._parsers.Add((equals, newParser));
                }
                return newConfig;
            }
        }

        /// <summary>
        /// Parser to use the next lookahead item to determine which parser to invoke next. Does
        /// not consume the lookahead value.
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly Configuration _config;

            public Parser(IConfiguration config)
            {
                _config = config as Configuration ?? throw new ArgumentException("Expected Configuration object, not a custom implementation. Please use Predict.CreateConfiguration to get a correct instance", nameof(config));
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var next = state.Input.Peek();
                var parser = _config.Pick(next);
                if (parser == null)
                    return state.Fail(this, $"Could not pick suitable continuation with lookahead {next}");
                return parser.Parse(state);
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => _config.GetChildren();

            public override string ToString() => ParserDefaultStringifier.ToString(this);
        }
    }
}
