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

            public IResult<TOutput> Parse(ParseState<TInput> state)
            {
                Assert.ArgumentNotNull(state, nameof(state));
                var value = _produce(state.Input, state.Data);
                return state.Success(this, value, 0, state.Input.CurrentLocation);
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
