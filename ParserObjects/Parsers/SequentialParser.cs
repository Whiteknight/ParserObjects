using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    public class SequentialState<TInput>
    {
        private readonly ParseState<TInput> _input;

        public SequentialState(ParseState<TInput> input)
        {
            _input = input;
        }

        public TValue Parse<TValue>(IParser<TInput, TValue> p)
        {
            var result = p.Parse(_input);
            if (!result.Success)
                throw new SequentialParserException(result.Location);
            return result.Value;
        }
    }

    public class SequentialParserException : Exception
    {
        public SequentialParserException(Location location)
        {
            Location = location;
        }

        public Location Location { get; }
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
            var startLocation = t.Input.CurrentLocation;
            var checkpoint = t.Input.Checkpoint();
            try
            {
                var state = new SequentialState<TInput>(t);
                var result = _func(state);
                return t.Success(result, startLocation);
            }
            catch (SequentialParserException spe)
            {
                // This exception is part of normal flow-control for this parser
                // Other exceptions bubble up like normal.
                checkpoint.Rewind();
                return Result.Fail<TOutput>(spe.Location);
            }
        }

        public IResult<object> ParseUntyped(ParseState<TInput> t) => Parse(t).Untype();

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}
