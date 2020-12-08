namespace ParserObjects.Pratt
{
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
            _state = state;
            _engine = engine;
            _rbp = rbp;
            _consumed = 0;
        }

        public IDataStore Data => _state.Data;

        public TOutput Parse() => Parse(_rbp);

        public int Consumed => _consumed;

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
            var result = parser.Parse(_state);
            if (!result.Success)
                throw new ParseException(ParseExceptionSeverity.Rule, result.Message, parser, result.Location);
            _consumed += result.Consumed;
            return result.Value;
        }

        public void Expect(IParser<TInput> parser)
        {
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
            var result = parser.Parse(_state);
            _consumed += result.Consumed;
            return (_, _) = result;
        }
    }
}
