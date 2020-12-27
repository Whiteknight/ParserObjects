using System;
using System.Collections.Generic;
using ParserObjects.Pratt;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Implementation of the Pratt parsing algorithm. Uses special Right production rules ("NUD")
    /// and Left production rules (LED) to recursively parse an input. Especially useful for
    /// mathematical expressions.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
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
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var startCp = state.Input.Checkpoint();
            var dataStore = state.Data as CascadingKeyValueStore;
            dataStore?.PushFrame();
            try
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var result = _engine.TryParse(state, 0);
                return state.Result(this, result);
            }
            catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Parser)
            {
                startCp.Rewind();
                return state.Fail<TInput, TOutput>(pe.Parser ?? this, pe.Message, pe.Location ?? state.Input.CurrentLocation);
            }
            finally
            {
                dataStore?.PopFrame();
            }
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => _config.GetParsers();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
