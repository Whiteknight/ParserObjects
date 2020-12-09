using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Pratt
{
    // TODO: Should ParseContext implement IParser<TInput, TOutput> so that it can be passed to
    // another parser and called recursively? For example, if we wanted a list of values from
    // the Pratt, we could call List(ctx) and get it

    // Simple contextual wrapper, so that private Engine methods can be
    // exposed to user callbacks
    public class ParseContext<TInput, TOutput> : IParseContext<TInput, TOutput>
    {
        private readonly ParseState<TInput> _state;
        private readonly Engine<TInput, TOutput> _engine;

        private readonly int _rbp;

        private int _consumed;

        public ParseContext(ParseState<TInput> state, Engine<TInput, TOutput> engine, int rbp)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            Assert.ArgumentNotNull(engine, nameof(engine));
            _state = state;
            _engine = engine;
            _rbp = rbp;
            _consumed = 0;
        }

        public IDataStore Data => _state.Data;

        public TOutput Parse() => Parse(_rbp);

        public int Consumed => _consumed;

        public string Name { get; set; }

        public TOutput Parse(int rbp)
        {
            var (success, value, error, consumed) = _engine.TryParse(_state, rbp);
            if (!success)
                throw new ParseException(ParseExceptionSeverity.Rule, error, null, null);
            _consumed += consumed;
            return value;
        }

        public TValue Parse<TValue>(IParser<TInput, TValue> parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));
            var result = parser.Parse(_state);
            if (!result.Success)
                throw new ParseException(ParseExceptionSeverity.Rule, result.Message, parser, result.Location);
            _consumed += result.Consumed;
            return result.Value;
        }

        IResult<TOutput> IParser<TInput, TOutput>.Parse(ParseState<TInput> state)
        {
            var (success, value, error, consumed) = _engine.TryParse(_state, _rbp);
            if (!success)
                return state.Fail(this, error);
            return state.Success(this, value, consumed);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => ((IParser<TInput, TOutput>)this).Parse(state);

        public void Expect(IParser<TInput> parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));
            var result = parser.Parse(_state);
            if (!result.Success)
                throw new ParseException(ParseExceptionSeverity.Rule, result.Message, parser, result.Location);
            _consumed += result.Consumed;
        }

        public void FailRule(string message = null)
            => throw new ParseException(ParseExceptionSeverity.Rule, message ?? "Fail", null, _state.Input.CurrentLocation);

        public void FailLevel(string message = null)
            => throw new ParseException(ParseExceptionSeverity.Level, message ?? "", null, _state.Input.CurrentLocation);

        public void FailAll(string message = null)
            => throw new ParseException(ParseExceptionSeverity.Parser, message ?? "", null, _state.Input.CurrentLocation);

        public (bool success, TOutput value) TryParse() => TryParse(_rbp);

        public (bool success, TOutput value) TryParse(int rbp)
        {
            var (success, value, _, consumed) = _engine.TryParse(_state, rbp);
            _consumed += consumed;
            return (success, value);
        }

        public (bool success, TValue value) TryParse<TValue>(IParser<TInput, TValue> parser)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));
            var result = parser.Parse(_state);
            _consumed += result.Consumed;
            return (_, _) = result;
        }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
