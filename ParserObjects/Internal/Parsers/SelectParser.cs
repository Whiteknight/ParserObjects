﻿using System;
using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parser to convert an IMultiResult into an IResult by selecting the best result alternative
/// using user-supplied criteria.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Select<TInput, TOutput>
{
    public record struct Arguments(
        IMultiResult<TOutput> Result,
        Func<IResultAlternative<TOutput>, Option<IResultAlternative<TOutput>>> Success,
        Func<Option<IResultAlternative<TOutput>>> Failure
    );

    public sealed record Parser(
        IMultiParser<TInput, TOutput> Initial,
        Func<Arguments, Option<IResultAlternative<TOutput>>> Selector,
        string Name = ""
    ) : IParser<TInput, TOutput>
    {
        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IEnumerable<IParser> GetChildren() => new[] { Initial };

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var multi = Initial.Parse(state);
            if (!multi.Success)
                return state.Fail(this, "Parser returned no valid results");

            static Option<IResultAlternative<TOutput>> Success(IResultAlternative<TOutput> alt)
            {
                if (alt == null)
                    return default;
                return new Option<IResultAlternative<TOutput>>(true, alt);
            }

            static Option<IResultAlternative<TOutput>> Fail()
                => default;

            var args = new Arguments(multi, Success, Fail);
            var selected = Selector(args);
            if (selected == null || !selected.Success)
                return state.Fail(this, "No alternative selected, or no matching value could be found");

            var alt = selected.Value;
            alt.Continuation.Rewind();
            return multi.ToResult(alt);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString("Select", Name, Id);

        public INamed SetName(string name) => this with { Name = name };
    }
}
