namespace ParserObjects.Pratt
{
    public class ParseletConfiguration<TInput, TValue, TOutput> : IParseletConfiguration<TInput, TValue, TOutput>
    {
        private readonly IParser<TInput, TValue> _matcher;

        private int _typeId;
        private int _lbp;
        private int _rbp;
        private NudFunc<TInput, TValue, TOutput> _getNud;
        private LedFunc<TInput, TValue, TOutput> _getLed;
        private string _name;

        public ParseletConfiguration(IParser<TInput, TValue> matcher)
        {
            _matcher = matcher;
        }

        public string Name { get; set; }

        public IParselet<TInput, TOutput> Build()
        {
            return new Parselet<TInput, TValue, TOutput>(
                _typeId,
                _matcher,
                _getNud,
                _getLed,
                _lbp,
                _rbp <= 0 ? _lbp + 1 : _rbp,
                _name ?? _matcher.Name ?? _typeId.ToString()
            );
        }

        public IParseletConfiguration<TInput, TValue, TOutput> TypeId(int id)
        {
            _typeId = id;
            return this;
        }

        public IParseletConfiguration<TInput, TValue, TOutput> ProduceRight(NudFunc<TInput, TValue, TOutput> getNud)
        {
            _getNud = getNud;
            return this;
        }

        public IParseletConfiguration<TInput, TValue, TOutput> ProduceLeft(LedFunc<TInput, TValue, TOutput> getLed)
        {
            _getLed = getLed;
            return this;
        }

        public IParseletConfiguration<TInput, TValue, TOutput> LeftBindingPower(int lbp)
        {
            _lbp = lbp;
            return this;
        }

        public IParseletConfiguration<TInput, TValue, TOutput> RightBindingPower(int rbp)
        {
            _rbp = rbp;
            return this;
        }

        public IParseletConfiguration<TInput, TValue, TOutput> Named(string name)
        {
            _name = name;
            return this;
        }
    }
}
