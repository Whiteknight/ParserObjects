using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parsers to attempt to invoke the inner parser, but always return success. A default value may
/// be returned if the inner parser fails.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Optional<TInput, TOutput>
{
    public sealed class NoDefaultParser : IParser<TInput, Option<TOutput>>
    {
        private readonly IResult<Option<TOutput>> _failure;

        public NoDefaultParser(IParser<TInput, TOutput> inner, string name = "")
        {
            Inner = inner;
            Name = name;
            _failure = new SuccessResult<Option<TOutput>>(this, default, 0);
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IParser<TInput, TOutput> Inner { get; }

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public bool Match(IParseState<TInput> state)
        {
            Inner.Match(state);
            return true;
        }

        public IResult<Option<TOutput>> Parse(IParseState<TInput> state)
        {
            var result = Inner.Parse(state);
            if (!result.Success)
                return _failure;
            return state.Success(this, new Option<TOutput>(true, result.Value), result.Consumed);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public INamed SetName(string name) => new NoDefaultParser(Inner, name);

        public override string ToString() => DefaultStringifier.ToString("Optional", Name, Id);
    }

    public record DefaultValueParser(
        IParser<TInput, TOutput> Inner,
        Func<IParseState<TInput>, TOutput> GetDefault,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

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
            return state.Success(this, value, endConsumed - startConsumed);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString("Optional", Name, Id);
    }
}
