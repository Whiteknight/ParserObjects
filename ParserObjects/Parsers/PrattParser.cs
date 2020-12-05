using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Expression-oriented implementation of the Pratt parsing algorithm. Contains the parser,
    /// configuration and related machinery.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static partial class Pratt<TInput, TOutput>
    {
        // See https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html
        // See https://eli.thegreenplace.net/2010/01/02/top-down-operator-precedence-parsing

        /// <summary>
        /// Pratt parser implementation. Uses configuration values to control the parse.
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly Engine _engine;
            private readonly Configuration _config;

            public Parser(Action<IConfiguration> setup)
            {
                Assert.ArgumentNotNull(setup, nameof(setup));
                var config = new Configuration();
                setup(config);
                _config = config;
                _engine = new Engine(_config.Parselets);
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state)
            {
                var startCp = state.Input.Checkpoint();
                try
                {
                    Assert.ArgumentNotNull(state, nameof(state));
                    var (success, value, error) = _engine.Parse(state);
                    if (success)
                        return state.Success(this, value);
                    return state.Fail(this, error);
                }
                catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Parser)
                {
                    startCp.Rewind();
                    return state.Fail<TOutput>(pe.Parser ?? this, pe.Message, pe.Location);
                }
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => _config.GetParsers();

            public override string ToString() => ParserDefaultStringifier.ToString(this);
        }
    }
}
