using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Attempts to parse the production and returns a default value if it does not succeed
    /// </summary>
    public class OptionalParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        // TODO: Can this be replaced with First(parser, Produce(getDefault))?
        // TODO: In either case, merge with RequiredParser
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Func<TOutput> _getDefault;

        public OptionalParser(IParser<TInput, TOutput> parser, Func<TOutput> getDefault = null)
        {
            _parser = parser;
            _getDefault = getDefault ?? (() => default);
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var result = _parser.Parse(t);
            return result.Success ? result : new SuccessResult<TOutput>(_getDefault(), t.CurrentLocation);
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_parser == find)
                return new OptionalParser<TInput, TOutput>(replace as IParser<TInput, TOutput>, _getDefault);
            return this;
        }

        public override string ToString() 
            => Name == null ? $"Optional.{_parser}" : $"Optional={Name}.{_parser}";
    }
}