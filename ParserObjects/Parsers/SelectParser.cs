using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Parser to convert an IMultiResult into an IResult by selecting the best result alternative
/// using user-supplied criteria.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public static class Select<TInput, TOutput>
{
    public struct Arguments
    {
        public Arguments(IMultiResult<TOutput> result, Func<IResultAlternative<TOutput>, IOption<IResultAlternative<TOutput>>> success, Func<IOption<IResultAlternative<TOutput>>> failure)
        {
            Result = result;
            Success = success;
            Failure = failure;
        }

        public IMultiResult<TOutput> Result { get; }
        public Func<IResultAlternative<TOutput>, IOption<IResultAlternative<TOutput>>> Success { get; }
        public Func<IOption<IResultAlternative<TOutput>>> Failure { get; }
    }

    public sealed class Parser : IParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TOutput> _parser;

        private readonly Func<Arguments, IOption<IResultAlternative<TOutput>>> _selector;

        public Parser(IMultiParser<TInput, TOutput> parser, Func<Arguments, IOption<IResultAlternative<TOutput>>> selector, string name = "")
        {
            Assert.ArgumentNotNull(parser, nameof(parser));
            Assert.ArgumentNotNull(selector, nameof(selector));
            _selector = selector;
            _parser = parser;
            Name = name;
        }

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            var multi = _parser.Parse(state);
            if (!multi.Success)
                return state.Fail(this, "Parser returned no valid results");

            static IOption<IResultAlternative<TOutput>> Success(IResultAlternative<TOutput> alt)
            {
                if (alt == null)
                    return FailureOption<IResultAlternative<TOutput>>.Instance;
                return new SuccessOption<IResultAlternative<TOutput>>(alt);
            }

            static IOption<IResultAlternative<TOutput>> Fail()
                => FailureOption<IResultAlternative<TOutput>>.Instance;

            var args = new Arguments(multi, Success, Fail);
            var selected = _selector(args);
            if (selected == null || !selected.Success)
                return state.Fail(this, "No alternative selected, or no matching value could be found");

            var alt = selected.Value;
            alt.Continuation.Rewind();
            return multi.ToResult(alt);
        }

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);

        public INamed SetName(string name) => new Select<TInput, TOutput>.Parser(_parser, _selector, name);
    }
}
