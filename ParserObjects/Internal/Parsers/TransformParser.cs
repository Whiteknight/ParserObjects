using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parsers to transform the result value of an inner parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TData"></typeparam>
public static class Transform<TInput, TMiddle, TOutput, TData>
{
    public sealed record Parser(
        IParser<TInput, TMiddle> Inner,
        TData Data,
        Func<TData, TMiddle, TOutput> Transform,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

            // Execute the parse and transform the result
            var result = Inner.Parse(state);
            if (!result.Success)
                return new FailureResult<TOutput>(result.Parser, result.ErrorMessage, default);

            var transformedValue = Transform(Data, result.Value);

            return state.Success(this, transformedValue, result.Consumed);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public bool Match(IParseState<TInput> state) => Inner.Match(state);

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public override string ToString() => DefaultStringifier.ToString("Transform", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed record MultiParser(
        IMultiParser<TInput, TMiddle> Inner,
        TData Data,
        Func<TData, TMiddle, TOutput> Transform,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);

            // Execute the parse and transform the result
            var result = Inner.Parse(state);
            result.StartCheckpoint.Rewind();

            return result.Transform(Data, Transform);
        }

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { Inner };

        public override string ToString() => DefaultStringifier.ToString("Transform", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
