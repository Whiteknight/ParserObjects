using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser and related machinery to create a constant value during a parse.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class Produce<TInput, TOutput>
    {
        /// <summary>
        /// Delegate to create the value given the current state of the parse as input.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public delegate TOutput Function(ISequence<TInput> input, IDataStore data);

        public delegate IEnumerable<TOutput> MultiFunction(ISequence<TInput> input, IDataStore data);

        /// <summary>
        /// Produces an output value unconditionally. Consumes no input. The callback has access to
        /// both the input sequence and the current contextual data, to help crafting the value.
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly Function _produce;

            public Parser(Function produce)
            {
                Assert.ArgumentNotNull(produce, nameof(produce));
                _produce = produce;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var startLocation = state.Input.CurrentLocation;
                var startConsumed = state.Input.Consumed;
                var value = _produce(state.Input, state.Data);
                var endConsumed = state.Input.Consumed;
                return state.Success(this, value, endConsumed - startConsumed, startLocation);
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        public class MultiParser : IMultiParser<TInput, TOutput>
        {
            private readonly MultiFunction _produce;

            public MultiParser(MultiFunction produce)
            {
                Assert.ArgumentNotNull(produce, nameof(produce));
                _produce = produce;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);

            public IMultiResult<TOutput> Parse(IParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var startLocation = state.Input.CurrentLocation;
                var startCheckpoint = state.Input.Checkpoint();
                var values = _produce(state.Input, state.Data);
                var alternatives = values.Select(v => new SuccessResultAlternative<TOutput>(v, 0, startCheckpoint));
                return new MultiResult<TOutput>(this, startLocation, startCheckpoint, alternatives);
            }

            IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);
        }
    }
}
