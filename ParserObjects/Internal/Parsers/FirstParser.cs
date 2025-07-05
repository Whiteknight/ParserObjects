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
    /* ParseInternal and MatchInternal are the implementations of Parse() and Match() methods,
     * respectively. The Parser classes are adaptors between these methods and the IParser
     * interface variants
     */

    private static Result<TResult> ParseInternal<TParser, TResult>(
        IParseState<TInput> state,
        IReadOnlyList<TParser> parsers,
        Func<IParseState<TInput>, TParser, Result<TResult>> getResult
    )
    {
        Assert.NotNull(state);
        Debug.Assert(parsers.Count >= 2);

        for (int i = 0; i < parsers.Count - 1; i++)
        {
            var result = getResult(state, parsers[i]);
            if (result.Success)
                return result;
        }

        return getResult(state, parsers[parsers.Count - 1]);
    }

    public sealed record WithoutOutput(
        IReadOnlyList<IParser<TInput>> Parsers,
        string Name = ""
    ) : SimpleRecordParser<TInput>(Name), IParser<TInput>
    {
        public override Result<object> Parse(IParseState<TInput> state)
            => ParseInternal(state, Parsers, static (s, p) => p.Parse(s));

        public override bool Match(IParseState<TInput> state)
        {
            Assert.NotNull(state);
            Debug.Assert(Parsers.Count >= 2);

            for (int i = 0; i < Parsers.Count - 1; i++)
            {
                var parser = Parsers[i];
                var result = parser.Match(state);
                if (result)
                    return result;
            }

            return Parsers[Parsers.Count - 1].Match(state);
        }

        public override IEnumerable<IParser> GetChildren() => Parsers;

        public override string ToString() => DefaultStringifier.ToString("First", Name, Id);

        public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed record WithOutput<TOutput>(
        IReadOnlyList<IParser<TInput, TOutput>> Parsers,
        string Name = ""
    ) : SimpleRecordParser<TInput, TOutput>(Name), IParser<TInput, TOutput>
    {
        public override Result<TOutput> Parse(IParseState<TInput> state)
            => ParseInternal(state, Parsers, static (s, p) => p.Parse(s));

        public override bool Match(IParseState<TInput> state)
        {
            Assert.NotNull(state);
            Debug.Assert(Parsers.Count >= 2);

            for (int i = 0; i < Parsers.Count; i++)
            {
                var parser = Parsers[i];
                var result = parser.Match(state);
                if (result)
                    return true;
            }

            return false;
        }

        public override IEnumerable<IParser> GetChildren() => Parsers;

        public override string ToString() => DefaultStringifier.ToString("First", Name, Id);

        public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
