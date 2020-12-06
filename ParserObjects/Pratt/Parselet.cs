namespace ParserObjects.Pratt
{
    /// <summary>
    /// User-configured parselet rule, which acts as an adaptor for IParser to IParselet.
    /// Is mostly used as a collection of configured internal values and should not be accessed
    /// directly from user code.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class Parselet<TInput, TValue, TOutput> : IParselet<TInput, TOutput>
    {
        private readonly IParser<TInput, TValue> _match;

        public Parselet(int tokenTypeId, IParser<TInput, TValue> match, NudFunc<TInput, TValue, TOutput> nud, LedFunc<TInput, TValue, TOutput> led, int lbp, int rbp, string name)
        {
            TokenTypeId = tokenTypeId;
            _match = match;
            Nud = nud;
            Led = led;
            Lbp = lbp;
            Rbp = rbp;
            Name = name;
        }

        public int TokenTypeId { get; }
        public int Lbp { get; }
        public int Rbp { get; }
        public string Name { get; set; }
        public IParser Parser => _match;

        public NudFunc<TInput, TValue, TOutput> Nud { get; }

        public LedFunc<TInput, TValue, TOutput> Led { get; }

        public (bool success, IToken<TInput, TOutput> token) TryGetNext(ParseState<TInput> state)
        {
            var result = _match.Parse(state);
            if (!result.Success)
                return (false, null);
            return (true, new Token<TInput, TValue, TOutput>(this, result.Value));
        }

        public override string ToString() => Name ?? _match.Name ?? base.ToString();
    }
}
