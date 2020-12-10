using ParserObjects.Utility;

namespace ParserObjects.Pratt
{
    /// <summary>
    /// User-configured rule, which acts as an adaptor from IParser to IParselet.
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
            Assert.ArgumentNotNull(match, nameof(match));
            TokenTypeId = tokenTypeId;
            _match = match;
            Nud = nud;
            Led = led;
            Lbp = lbp;
            Rbp = rbp;
            Name = name ?? _match.Name ?? ((TokenTypeId > 0) ? TokenTypeId.ToString() : match.ToString());
        }

        public int TokenTypeId { get; }
        public int Lbp { get; }
        public int Rbp { get; }
        public string Name { get; set; }
        public IParser Parser => _match;

        public NudFunc<TInput, TValue, TOutput> Nud { get; }

        public LedFunc<TInput, TValue, TOutput> Led { get; }

        public bool CanNud => Nud != null;

        public bool CanLed => Led != null;

        public (bool success, IToken<TInput, TOutput> token, int consumed) TryGetNext(ParseState<TInput> state)
        {
            var result = _match.Parse(state);
            if (!result.Success)
                return default;
            return (true, new Token<TInput, TValue, TOutput>(this, result.Value), result.Consumed);
        }

        public override string ToString() => Name;
    }
}
