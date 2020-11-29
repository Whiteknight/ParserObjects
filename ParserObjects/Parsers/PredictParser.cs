using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public static class Predict<TInput, TOutput>
    {
        public static IConfiguration CreateConfiguration() => new Configuration();

        public interface IConfiguration
        {
            IConfiguration Add(Func<TInput, bool> equals, IParser<TInput, TOutput> parser);
        }

        public class Configuration : IConfiguration
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
                var next = state.Input.Peek();
                var parser = _config.Pick(next);
                if (parser == null)
                    return state.Fail(this, $"Could not pick suitable continuation with lookahead {next}");
                return parser.Parse(state);
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => _config.GetChildren();
        }
    }
}
