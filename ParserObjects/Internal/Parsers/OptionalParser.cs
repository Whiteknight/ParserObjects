using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

public static class Optional<TInput, TOutput>
{
    public record NoDefaultParser(
        IParser<TInput, TOutput> Inner,
        string Name = ""
    ) : IParser<TInput, Option<TOutput>>
    {
        public int Id => UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public bool Match(IParseState<TInput> state)
        {
            Inner.Match(state);
            return true;
        }

        public IResult<Option<TOutput>> Parse(IParseState<TInput> state)
        {
            var result = Inner.Parse(state);
            var option = result.Success ? new Option<TOutput>(true, result.Value) : default;
            return state.Success(this, option, result.Consumed, result.Location);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString("Optional", Name, Id);
    }

    public record DefaultValueParser(
        IParser<TInput, TOutput> Inner,
        Func<IParseState<TInput>, TOutput> GetDefault,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id => UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public bool Match(IParseState<TInput> state)
        {
            Inner.Match(state);
            return true;
        }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            int startConsumed = state.Input.Consumed;
            var result = Inner.Parse(state);
            var value = result.Success ? result.Value : GetDefault(state);
            var endConsumed = state.Input.Consumed;
            return state.Success(this, value, endConsumed - startConsumed, result.Location);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString("Optional", Name, Id);
    }
}
