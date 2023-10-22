using System;
using System.Collections.Generic;
using System.Diagnostics;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Takes a list of parsers and attempts each one in order. Returns as soon as the first parser
/// succeeds.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class FirstParser<TInput>
{
    private static TResult ParseInternal<TParser, TResult>(IParseState<TInput> state, IReadOnlyList<TParser> parsers, Func<IParseState<TInput>, TParser, TResult> getResult)
        where TParser : IParser<TInput>
        where TResult : IResult
    {
        Assert.ArgumentNotNull(state);
        Debug.Assert(parsers.Count >= 2, "We shouldn't have fewer than 2 parsers here");

        for (int i = 0; i < parsers.Count - 1; i++)
        {
            var parser = parsers[i];
            var result = getResult(state, parser);
            if (result.Success)
                return result;
        }

        return getResult(state, parsers[parsers.Count - 1]);
    }

    private static bool MatchInternal<TParser>(IParseState<TInput> state, IReadOnlyList<TParser> parsers)
        where TParser : IParser<TInput>
    {
        Assert.ArgumentNotNull(state);
        Debug.Assert(parsers.Count >= 2, "We shouldn't have fewer than 2 parsers here");

        for (int i = 0; i < parsers.Count - 1; i++)
        {
            var parser = parsers[i];
            var result = parser.Match(state);
            if (result)
                return result;
        }

        return parsers[parsers.Count - 1].Match(state);
    }

    public sealed record WithoutOutput(
        IReadOnlyList<IParser<TInput>> Parsers,
        string Name = ""
    ) : IParser<TInput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        IResult IParser<TInput>.Parse(IParseState<TInput> state)
            => ParseInternal(state, Parsers, static (s, p) => p.Parse(s));

        public bool Match(IParseState<TInput> state)
            => MatchInternal(state, Parsers);

        public IEnumerable<IParser> GetChildren() => Parsers;

        public override string ToString() => DefaultStringifier.ToString("First", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed record WithOutput<TOutput>(
        IReadOnlyList<IParser<TInput, TOutput>> Parsers,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IResult<TOutput> Parse(IParseState<TInput> state)
            => ParseInternal(state, Parsers, static (s, p) => p.Parse(s));

        IResult IParser<TInput>.Parse(IParseState<TInput> state)
            => ParseInternal(state, Parsers, static (s, p) => p.Parse(s));

        public bool Match(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state);
            Debug.Assert(Parsers.Count >= 2, "We shouldn't have fewer than 2 parsers here");

            for (int i = 0; i < Parsers.Count; i++)
            {
                var parser = Parsers[i];
                var result = parser.Match(state);
                if (result)
                    return true;
            }

            return false;
        }

        public IEnumerable<IParser> GetChildren() => Parsers;

        public override string ToString() => DefaultStringifier.ToString("First", Name, Id);

        public INamed SetName(string name) => this with { Name = name };

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
