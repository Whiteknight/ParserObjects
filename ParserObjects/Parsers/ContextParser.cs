using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public static class Context<TInput, TOutput>
    {
        public delegate void Function(ISequence<TInput> input, IDataStore data);

        public class Parser : IParser<TInput, TOutput>
        {
            private readonly IParser<TInput, TOutput> _inner;
            private readonly Context<TInput, TOutput>.Function _setup;
            private readonly Context<TInput, TOutput>.Function _cleanup;

            public Parser(IParser<TInput, TOutput> inner, Function setup, Function cleanup)
            {
                Assert.ArgumentNotNull(inner, nameof(inner));
                Assert.ArgumentNotNull(setup, nameof(setup));
                Assert.ArgumentNotNull(cleanup, nameof(cleanup));

                _inner = inner;
                _setup = setup;
                _cleanup = cleanup;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                try
                {
                    _setup(state.Input, state.Data);
                    return _inner.Parse(state);
                }
                finally
                {
                    _cleanup(state.Input, state.Data);
                }
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => new[] { _inner };

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
