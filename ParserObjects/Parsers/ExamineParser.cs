using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Inserts a callback before and after the specified parser. Useful for debugging purposes
    /// and to adjust the input/output of a parser. Contains parsers and related machinery.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public abstract class Examine<TInput, TOutput>
    {
        /// <summary>
        /// Examine parser for parsers which return typed output. Executes callbacks before and
        /// after the parse.
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _parser;
            private readonly Action<Context>? _before;
            private readonly Action<Context>? _after;

            public Parser(IParser<TInput, TOutput> parser, Action<Context>? before, Action<Context>? after)
            {
                Assert.ArgumentNotNull(parser, nameof(parser));
                _parser = parser;
                _before = before;
                _after = after;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var startConsumed = state.Input.Consumed;
                _before?.Invoke(new Context(_parser, state, null));
                var result = _parser.Parse(state);
                _after?.Invoke(new Context(_parser, state, result));
                var totalConsumed = state.Input.Consumed - startConsumed;

                // The callbacks have access to Input, so the user might consume data. Make sure
                // to report that if it happens.
                if (!result.Success || result.Consumed == totalConsumed)
                    return result;
                return state.Success(this, result.Value, totalConsumed, result.Location);
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new List<IParser> { _parser };

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        /// <summary>
        /// Context information available during an examination.
        /// </summary>
        public record Context(
            IParser<TInput, TOutput> Parser,
            IParseState<TInput> State,
            IResult<TOutput>? Result
        )
        {
            public IDataStore Data => State.Data;
            public ISequence<TInput> Input => State.Input;
        }
    }

    /// <summary>
    /// Inserts a callback before and after parser execution. Used for parsers with untyped output.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public abstract class Examine<TInput>
    {
        /// <summary>
        /// The examine parser. Executes callbacks before and after the parser. Does not return
        /// typed output.
        /// </summary>
        public class Parser : IParser<TInput>
        {
            private readonly IParser<TInput> _parser;
            private readonly Action<Context>? _before;
            private readonly Action<Context>? _after;

            public Parser(IParser<TInput> parser, Action<Context>? before, Action<Context>? after)
            {
                _parser = parser;
                _before = before;
                _after = after;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IResult Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                _before?.Invoke(new Context(_parser, state, null));
                var result = _parser.Parse(state);
                _after?.Invoke(new Context(_parser, state, result));
                return result;
            }

            public IEnumerable<IParser> GetChildren() => new List<IParser> { _parser };

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        /// <summary>
        /// The context object which holds information able to be examined.
        /// </summary>
        public record Context(
            IParser<TInput> Parser,
            IParseState<TInput> State,
            IResult? Result
        )
        {
            public IDataStore Data => State.Data;
            public ISequence<TInput> Input => State.Input;
        }
    }
}
