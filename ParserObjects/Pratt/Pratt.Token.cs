namespace ParserObjects.Parsers
{
    public static partial class Pratt<TInput, TOutput>
    {
        public interface IToken
        {
            int TokenTypeId { get; }
            int LeftBindingPower { get; }
            int RightBindingPower { get; }
            (bool success, IToken<TOutput> value) NullDenominator(IParseContext context);
            (bool success, IToken<TOutput> value) LeftDenominator(IParseContext context, IToken left);
            bool IsValid { get; }
            IParselet Parselet { get; }
        }

        public interface IToken<TValue> : IToken
        {
            TValue Value { get; }
        }

        // Simple token type which wraps a value from the input stream and metadata necessary to
        // work with it inside the Engine.
        private class Token<TValue> : IToken<TValue>
        {
            private readonly Parselet<TValue> _parselet;

            public Token(Parselet<TValue> parselet, TValue value)
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
            public IParselet Parselet => _parselet;
            public TValue Value { get; }

            public int LeftBindingPower { get; }
            public int RightBindingPower { get; }
            public bool IsValid { get; }
            public string Name { get; }

            public (bool success, IToken<TOutput> value) NullDenominator(IParseContext context)
            {
                if (_parselet == null)
                    return (false, default);
                return _parselet.NullDenominator(context, this);
            }

            public (bool success, IToken<TOutput> value) LeftDenominator(IParseContext context, IToken left)
            {
                if (_parselet == null || left is not Token<TOutput> leftTyped)
                    return (false, default);
                return _parselet.LeftDenominator(context, leftTyped, this);
            }

            public override string ToString() => Value.ToString();
        }
    }
}
