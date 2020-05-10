using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    public class ExamineParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Action<ISequence<TInput>> _before;
        private readonly Action<ISequence<TInput>, IParseResult<TOutput>> _after;

        public ExamineParser(IParser<TInput, TOutput> parser, Action<ISequence<TInput>> before, Action<ISequence<TInput>, IParseResult<TOutput>> after)
        {
            _parser = parser;
            _before = before;
            _after = after;
        }

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new List<IParser> { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _parser && replace is IParser<TInput, TOutput> typedReplace)
                return new ExamineParser<TInput, TOutput>(typedReplace, _before, _after);
            return this;
        }

        public IParseResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            _before?.Invoke(t);
            var result = _parser.Parse(t);
            _after?.Invoke(t, result);
            return result;
        }
    }
}
