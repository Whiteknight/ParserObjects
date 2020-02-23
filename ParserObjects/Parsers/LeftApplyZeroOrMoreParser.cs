using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public class LeftApplyZeroOrMoreParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _initial;
        private readonly IParser<TInput, TOutput> _right;
        private readonly LeftValueParser _left;

        private string _name;

        public LeftApplyZeroOrMoreParser(IParser<TInput, TOutput> initial, Func<IParser<TInput, TOutput>, IParser<TInput, TOutput>> getRight)
        {
            _initial = initial;
            _left = new LeftValueParser();
            _right = getRight(_left);
        }

        private LeftApplyZeroOrMoreParser(IParser<TInput, TOutput> initial, LeftValueParser left, IParser<TInput, TOutput> right)
        {
            _initial = initial;
            _left = left;
            _right = right;
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var result = _initial.Parse(t);
            if (!result.Success)
                return new FailResult<TOutput>(t.CurrentLocation);

            var current = result.Value;
            _left.Value = result.Value;
            _left.Location = result.Location;
            while (true)
            {
                var rhsResult = _right.Parse(t);
                if (!rhsResult.Success)
                    return new SuccessResult<TOutput>(current, result.Location);

                current = rhsResult.Value;
                _left.Value = current;
            }
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name
        {
            get => _name;
            set
            {
                // TODO: Find a good way to test this
                _name = value;
                _left.Name = string.IsNullOrEmpty(_name) ? null : _name + ".Left";
            }
        }

        public IEnumerable<IParser> GetChildren() => new IParser[] { _initial, _right };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_initial == find && replace is IParser<TInput, TOutput> initialTyped)
                return new LeftApplyZeroOrMoreParser<TInput, TOutput>(initialTyped, _left, _right);

            if (_right == find && replace is IParser<TInput, TOutput> rightTyped)
                return new LeftApplyZeroOrMoreParser<TInput, TOutput>(_initial, _left, rightTyped);

            return this;
        }

        public IParser Accept(IParserVisitor visitor) => (visitor as ICoreVisitorDispatcher)?.VisitLeftApplyZeroOrMore(this) ?? this;

        public override string ToString()
        {
            var typeName = this.GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }

        private class LeftValueParser : IParser<TInput, TOutput>
        {
            public TOutput Value { get; set; }
            public Location Location { get; set; }

            public IParseResult<TOutput> Parse(ISequence<TInput> t) => new SuccessResult<TOutput>(Value, Location);

            IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => new SuccessResult<object>(Value, Location);

            public string Name { get; set; }

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public IParser ReplaceChild(IParser find, IParser replace) => this;

            public IParser Accept(IParserVisitor visitor) => this;
        }
    }
}
