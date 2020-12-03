using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Delegates to an internal parser, and allows the internal parser to be replaced in-place
    /// after the parser graph has been created. Useful for cases where grammar extensions or
    /// modifications need to be made after the parser has been created.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class ReplaceableParser<TInput, TOutput> : IParser<TInput, TOutput>, IReplaceableParserUntyped
    {
        private IParser<TInput, TOutput> _value;

        public ReplaceableParser(IParser<TInput, TOutput> defaultValue)
        {
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));
            _value = defaultValue;
        }

        public string Name { get; set; }

        public IParser ReplaceableChild => _value;

        public IResult<TOutput> Parse(ParseState<TInput> state) => _value.Parse(state);

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => _value.Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _value };

        public SingleReplaceResult SetParser(IParser parser)
        {
            var previous = _value;
            if (parser is IParser<TInput, TOutput> typed)
                _value = typed;
            return new SingleReplaceResult(this, previous, _value);
        }

        public override string ToString() => ParserDefaultStringifier.ToString(this);
    }
}