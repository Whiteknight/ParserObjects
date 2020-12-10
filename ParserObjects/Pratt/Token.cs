namespace ParserObjects.Pratt
{
    // Simple token type which wraps a value from the input stream and metadata necessary to
    // work with it inside the Engine. This class is for (mostly) internal use only and shouldn't
    // be used directly except through the provided abstractions.
    public class Token<TInput, TValue, TOutput> : IToken<TValue>, IToken<TInput, TOutput>
    {
        private readonly Parselet<TInput, TValue, TOutput> _parselet;

        public Token(Parselet<TInput, TValue, TOutput> parselet, TValue value)
        {
            _parselet = parselet;
            Value = value;
            TokenTypeId = _parselet?.TokenTypeId ?? 0;
            LeftBindingPower = _parselet?.Lbp ?? 0;
            RightBindingPower = _parselet?.Rbp ?? 0;
            Name = _parselet?.Name ?? string.Empty;
            IsValid = _parselet != null;
        }

        public Token(int typeId, TValue value, int lbp, int rbp, string name)
        {
            TokenTypeId = typeId;
            Value = value;
            LeftBindingPower = lbp;
            RightBindingPower = rbp;
            Name = name;
            IsValid = true;
        }

        public int TokenTypeId { get; }
        public TValue Value { get; }

        public int LeftBindingPower { get; }
        public int RightBindingPower { get; }
        public bool IsValid { get; }
        public string Name { get; }

        public (bool success, IToken<TOutput> value) NullDenominator(IParseContext<TInput, TOutput> context)
        {
            var nud = _parselet?.Nud;
            if (nud == null)
                return (false, default);
            try
            {
                var resultValue = nud(context, this);
                return (true, new Token<TInput, TOutput, TOutput>(TokenTypeId, resultValue, _parselet.Lbp, _parselet.Rbp, Name));
            }
            catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Rule)
            {
                return (false, default);
            }
        }

        public (bool success, IToken<TOutput> value) LeftDenominator(IParseContext<TInput, TOutput> context, IToken left)
        {
            if (_parselet?.Led == null || left is not IToken<TOutput> leftTyped)
                return (false, default);

            var led = _parselet.Led;
            if (led == null)
                return (false, default);
            try
            {
                var resultValue = led(context, leftTyped, this);
                return (true, new Token<TInput, TOutput, TOutput>(TokenTypeId, resultValue, _parselet.Lbp, _parselet.Rbp, Name));
            }
            catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Rule)
            {
                return (false, default);
            }
        }

        public override string ToString() => Value.ToString();
    }
}
