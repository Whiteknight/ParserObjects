namespace ParserObjects.Parsers
{
    public static partial class Pratt<TInput, TOutput>
    {
        public interface IParselet
        {
            int TokenTypeId { get; }
            int Lbp { get; }
            int Rbp { get; }
            string Name { get; }
            (bool success, IToken token) TryGetNext(ParseState<TInput> state);
            IParser Parser { get; }
        }

        // user-configured Parselet rule, which contains a parser to match an input token and 
        // some rules about precidence, associativity and converstion into output tokens
        private class Parselet<TValue> : IParselet
        {
            private readonly NudFunc<TValue> _nud;
            private readonly LedFunc<TValue> _led;
            private readonly IParser<TInput, TValue> _match;

            public Parselet(int tokenTypeId, IParser<TInput, TValue> match, NudFunc<TValue> nud, LedFunc<TValue> led, int lbp, int rbp, string name)
            {
                TokenTypeId = tokenTypeId;
                _match = match;
                _nud = nud;
                _led = led;
                Lbp = lbp;
                Rbp = rbp;
                Name = name;
            }

            public int TokenTypeId { get; }
            public int Lbp { get; }
            public int Rbp { get; }
            public string Name { get; }
            public IParser Parser => _match;

            public (bool success, IToken token) TryGetNext(ParseState<TInput> state)
            {
                var result = _match.Parse(state);
                if (!result.Success)
                    return (false, null);
                return (true, new Token<TValue>(this, result.Value));
            }

            public (bool success, IToken<TOutput> value) NullDenominator(IParseContext context, IToken<TValue> value)
            {
                var nud = _nud;
                if (nud == null)
                    return (false, default);
                try
                {
                    var resultValue = nud(context, value);
                    return (true, new Token<TOutput>(TokenTypeId, resultValue, Lbp, Rbp, Name));
                }
                catch (ParseException)
                {
                    return (false, default);
                }
            }

            public (bool success, IToken<TOutput> value) LeftDenominator(IParseContext context, IToken<TOutput> left, IToken<TValue> value)
            {
                var led = _led;
                if (led == null)
                    return (false, default);
                try
                {
                    var resultValue = led(context, left, value);
                    return (true, new Token<TOutput>(TokenTypeId, resultValue, Lbp, Rbp, Name));
                }
                catch (ParseException)
                {
                    return (false, default);
                }
            }
        }
    }
}
