using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public abstract class Examine<TInput, TOutput>
    {
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _parser;
            private readonly Action<Context> _before;
            private readonly Action<Context> _after;

            public Parser(IParser<TInput, TOutput> parser, Action<Context> before, Action<Context> after)
            {
                _parser = parser;
                _before = before;
                _after = after;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                _before?.Invoke(new Context(_parser, state, null));
                var result = _parser.Parse(state);
                _after?.Invoke(new Context(_parser, state, result));
                return result;
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new List<IParser> { _parser };

            public IParser ReplaceChild(IParser find, IParser replace)
            {
                if (find == _parser && replace is IParser<TInput, TOutput> typedReplace)
                    return new Parser(typedReplace, _before, _after);
                return this;
            }
        }

        public class Context
        {
            public Context(IParser<TInput, TOutput> parser, ParseState<TInput> state, IResult<TOutput> result)
            {
                State = state;
                Parser = parser;
                Result = result;
            }

            public ParseState<TInput> State { get; }
            public IDataStore Data => State.Data;
            public IParser<TInput, TOutput> Parser { get; }
            public ISequence<TInput> Input => State.Input;
            public IResult<TOutput> Result { get; }
        }
    }

    public abstract class Examine<TInput>
    {
        public class Parser : IParser<TInput>
        {
            private readonly IParser<TInput> _parser;
            private readonly Action<Context> _before;
            private readonly Action<Context> _after;

            public Parser(IParser<TInput> parser, Action<Context> before, Action<Context> after)
            {
                _parser = parser;
                _before = before;
                _after = after;
            }

            public string Name { get; set; }

            public IResult Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                _before?.Invoke(new Context(_parser, state, null));
                var result = _parser.Parse(state);
                _after?.Invoke(new Context(_parser, state, result));
                return result;
            }

            public IEnumerable<IParser> GetChildren() => new List<IParser> { _parser };

            public IParser ReplaceChild(IParser find, IParser replace)
            {
                if (find == _parser && replace is IParser<TInput> typedReplace)
                    return new Parser(typedReplace, _before, _after);
                return this;
            }
        }

        public class Context
        {
            public Context(IParser<TInput> parser, ParseState<TInput> state, IResult result)
            {
                State = state;
                Parser = parser;
                Result = result;
            }

            public ParseState<TInput> State { get; }
            public IDataStore Data => State.Data;
            public IParser<TInput> Parser { get; }
            public ISequence<TInput> Input => State.Input;
            public IResult Result { get; }
        }
    }
}
