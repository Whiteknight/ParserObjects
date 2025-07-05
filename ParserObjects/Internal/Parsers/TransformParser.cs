using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;
using static ParserObjects.Internal.Assert;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parsers to transform the result value of an inner parser.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class Transform<TInput>
{
    public static IParser<TInput, TOutput> Create<TMiddle, TOutput, TData>(
        IParser<TInput, TMiddle> inner,
        TData data,
        Func<TData, TMiddle, TOutput> transform,
        string name = ""
    ) => new Parser<TMiddle, TOutput, TData>(inner, data, transform, name);

    public static IMultiParser<TInput, TOutput> Create<TMiddle, TOutput, TData>(
        IMultiParser<TInput, TMiddle> inner,
        TData data,
        Func<TData, TMiddle, TOutput> transform,
        string name = ""
    ) => new MultiParser<TMiddle, TOutput, TData>(inner, data, transform, name);

    public sealed record Parser<TMiddle, TOutput, TData>(
        IParser<TInput, TMiddle> Inner,
        TData Data,
        Func<TData, TMiddle, TOutput> Transform,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public Result<TOutput> Parse(IParseState<TInput> state)
        {
            var result = Inner.Parse(state);
            return result.Success
                ? Result.Ok(this, Transform(Data, result.Value), result.Consumed, result.Data)
                : Result.Fail<TOutput>(Inner, result.ErrorMessage, result.Data);
        }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public bool Match(IParseState<TInput> state) => Inner.Match(state);

        public IEnumerable<IParser> GetChildren() => [Inner];

        public override string ToString() => DefaultStringifier.ToString("Transform", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed record MultiParser<TMiddle, TOutput, TData>(
        IMultiParser<TInput, TMiddle> Inner,
        TData Data,
        Func<TData, TMiddle, TOutput> Transform,
        string Name = ""
    ) : IMultiParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public MultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            // Execute the parse and transform the result
            var startCp = NotNull(state).Input.Checkpoint();
            var result = Inner.Parse(state);
            startCp.Rewind();

            return result.Transform(Data, Transform);
        }

        MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

        public IEnumerable<IParser> GetChildren() => [Inner];

        public override string ToString() => DefaultStringifier.ToString("Transform", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
