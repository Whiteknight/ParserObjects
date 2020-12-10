using System;
using System.Collections.Generic;
using ParserObjects.Pratt;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class PrattParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Configuration<TInput, TOutput> _config;
        private readonly Engine<TInput, TOutput> _engine;

        public PrattParser(Action<IConfiguration<TInput, TOutput>> setup)
        {
            Assert.ArgumentNotNull(setup, nameof(setup));
            var config = new Configuration<TInput, TOutput>();
            setup(config);
            _config = config;
            _engine = new Engine<TInput, TOutput>(_config.Parselets);
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var startCp = state.Input.Checkpoint();
            try
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var result = _engine.Parse(state);
                return state.Result(this, result);
            }
            catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Parser)
            {
                startCp.Rewind();
                return state.Fail<TOutput>(pe.Parser ?? this, pe.Message, pe.Location);
            }
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => _config.GetParsers();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
