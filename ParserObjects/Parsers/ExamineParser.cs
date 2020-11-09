using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    public class ExamineParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Action<ParseState<TInput, TOutput>> _before;
        private readonly Action<ParseState<TInput, TOutput>> _after;

        public ExamineParser(IParser<TInput, TOutput> parser, Action<ParseState<TInput, TOutput>> before, Action<ParseState<TInput, TOutput>> after)
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

        public IResult<object> ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public IResult<TOutput> Parse(ISequence<TInput> t)
        {
            _before?.Invoke(new ParseState<TInput, TOutput>(_parser, t, null));
            var result = _parser.Parse(t);
            _after?.Invoke(new ParseState<TInput, TOutput>(_parser, t, result));
            return result;
        }
    }
}
