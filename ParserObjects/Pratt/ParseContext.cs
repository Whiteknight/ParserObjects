namespace ParserObjects.Pratt
{
    // Simple contextual wrapper, so that private Engine methods can be
    // exposed to user callbacks
    public class ParseContext<TInput, TOutput> : IParseContext<TInput, TOutput>
    {
        private readonly ParseState<TInput> _state;
        private readonly Engine<TInput, TOutput> _engine;

        private readonly int _rbp;

        public ParseContext(ParseState<TInput> state, Engine<TInput, TOutput> engine, int rbp)
        {
            _state = state;
            _engine = engine;
            _rbp = rbp;
        }

        public IDataStore Data => _state.Data;

        public TOutput Parse() => Parse(_rbp);

        public TOutput Parse(int rbp)
        {
            var (success, value, error) = _engine.TryParse(_state, rbp);
            if (!success)
                throw new ParseException(ParseExceptionSeverity.Rule, error, null, null);
            return value;
        }

        public TValue Parse<TValue>(IParser<TInput, TValue> parser)
        {
            var result = parser.Parse(_state);
            if (!result.Success)
                throw new ParseException(ParseExceptionSeverity.Rule, result.Message, parser, result.Location);
            return result.Value;
        }

        public void Expect(IParser<TInput> parser)
        {
            var result = parser.Parse(_state);
            if (!result.Success)
                throw new ParseException(ParseExceptionSeverity.Rule, result.Message, parser, result.Location);
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
            var (success, value, _) = _engine.TryParse(_state, rbp);
            return (success, value);
        }

        public (bool success, TValue value) TryParse<TValue>(IParser<TInput, TValue> parser)
        {
            var result = parser.Parse(_state);
            return (_, _) = result;
        }
    }
}
