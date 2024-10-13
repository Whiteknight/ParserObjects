using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

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
        public NoDefaultParser(IParser<TInput, TOutput> inner, string name = "")
        {
            Inner = inner;
            Name = name;
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

        public Result<Option<TOutput>> Parse(IParseState<TInput> state)
        {
            var result = Inner.Parse(state);
            if (!result.Success)
                return Result<Option<TOutput>>.Ok(this, default, 0);
            return state.Success(this, new Option<TOutput>(true, result.Value), result.Consumed);
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public INamed SetName(string name) => new NoDefaultParser(Inner, name);

        public override string ToString() => DefaultStringifier.ToString("Optional", Name, Id);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
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

        public Result<TOutput> Parse(IParseState<TInput> state)
        {
            int startConsumed = state.Input.Consumed;
            var result = Inner.Parse(state);
            var value = result.Success ? result.Value : GetDefault(state);
            var endConsumed = state.Input.Consumed;
            return state.Success(this, value, endConsumed - startConsumed);
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public INamed SetName(string name) => this with { Name = name };

        public override string ToString() => DefaultStringifier.ToString("Optional", Name, Id);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
