using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public class ExamineParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _parser;
        private readonly Action<ExamineParseState<TInput, TOutput>> _before;
        private readonly Action<ExamineParseState<TInput, TOutput>> _after;

        public ExamineParser(IParser<TInput, TOutput> parser, Action<ExamineParseState<TInput, TOutput>> before, Action<ExamineParseState<TInput, TOutput>> after)
        {
            _parser = parser;
            _before = before;
            _after = after;
        }

        public string Name { get; set; }

        public Result<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public Result<TOutput> Parse(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            _before?.Invoke(new ExamineParseState<TInput, TOutput>(_parser, t.Input, default));
            var result = _parser.Parse(t);
            _after?.Invoke(new ExamineParseState<TInput, TOutput>(_parser, t.Input, result));
            return result;
        }

        public IEnumerable<IParser> GetChildren() => new List<IParser> { _parser };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _parser && replace is IParser<TInput, TOutput> typedReplace)
                return new ExamineParser<TInput, TOutput>(typedReplace, _before, _after);
            return this;
        }
    }
}
