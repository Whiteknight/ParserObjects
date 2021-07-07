using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers.Multi
{
    public class ReduceParser<TInput, TMulti, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<IMultiResult<TMulti>, IResult<TOutput>> _reduce;
        private readonly IMultiParser<TInput, TMulti> _parser;

        public ReduceParser(IMultiParser<TInput, TMulti> parser, Func<IMultiResult<TMulti>, IResult<TOutput>> reduce)
        {
            _reduce = reduce;
            _parser = parser;
            Name = "";
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var multi = _parser.Parse(state);
            return _reduce(multi);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
