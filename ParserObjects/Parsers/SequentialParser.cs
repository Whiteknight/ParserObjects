using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Allows executing a parse sequentially in a special callback. Useful for debugging purposes
    /// or cases where procedural logic should be intermixed with parser logic.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class SequentialState<TInput>
    {
        private readonly ParseState<TInput> _input;

        public SequentialState(ParseState<TInput> input)
        {
            _input = input;
        }

        public IDataStore Data => _input.Data;

        public TOutput Parse<TOutput>(IParser<TInput, TOutput> p)
        {
            var result = p.Parse(_input);
            if (!result.Success)
                throw new SequentialParserException(result);
            return result.Value;
        }

        public IResult<TOutput> TryParse<TOutput>(IParser<TInput, TOutput> p)
        {
            return p.Parse(_input);
        }

        public IResult<TOutput> TryMatch<TOutput>(IParser<TInput, TOutput> p)
        {
            var checkpoint = _input.Input.Checkpoint();
            var result = p.Parse(_input);
            checkpoint.Rewind();
            return result;
        }
    }

    [Serializable]
    public class SequentialParserException : Exception
    {
        public SequentialParserException()
        {
        }

        public SequentialParserException(IResult result)
        {
            Location = result.Location;
            Result = result;
        }

        protected SequentialParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public Location Location { get; }

        public IResult Result { get; }
    }


    public class SequentialParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly Func<SequentialState<TInput>, TOutput> _func;

        public SequentialParser(Func<SequentialState<TInput>, TOutput> func)
        {
            _func = func;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var startLocation = state.Input.CurrentLocation;
            var checkpoint = state.Input.Checkpoint();
            try
            {
                var seqState = new SequentialState<TInput>(state);
                var result = _func(seqState);
                return state.Success(this, result, startLocation);
            }
            catch (SequentialParserException spe)
            {
                // This exception is part of normal flow-control for this parser
                // Other exceptions bubble up like normal.
                checkpoint.Rewind();
                var result = spe.Result;
                state.Log(this, $"Parse failed during sequential callback: {result}\n\n{spe.StackTrace}");
                return state.Fail(this, $"Error during parsing: {result.Parser} {result.Message} at {result.Location}");
            }
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
