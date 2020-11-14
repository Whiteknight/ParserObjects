using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
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

    public class SequentialParserException : Exception
    {
        public SequentialParserException(IResult result)
        {
            Location = result.Location;
            Result = result;
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

        public IResult<TOutput> Parse(ParseState<TInput> t)
        {
            Assert.ArgumentNotNull(t, nameof(t));
            var startLocation = t.Input.CurrentLocation;
            var checkpoint = t.Input.Checkpoint();
            try
            {
                var state = new SequentialState<TInput>(t);
                var result = _func(state);
                return t.Success(this, result, startLocation);
            }
            catch (SequentialParserException spe)
            {
                // This exception is part of normal flow-control for this parser
                // Other exceptions bubble up like normal.
                checkpoint.Rewind();
                var result = spe.Result;
                return t.Fail(this, "Error during parsing: " + result.Message, spe.Location);
            }
        }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
