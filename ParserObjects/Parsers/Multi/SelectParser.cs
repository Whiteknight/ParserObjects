using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers.Multi
{
    /// <summary>
    /// Parser to convert an IMultiResult into an IResult by selecting the best result alternative
    /// using user-supplied criteria.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class Select<TInput, TOutput>
    {
        public delegate IOption<IResultAlternative<TOutput>> SuccessFactory(IResultAlternative<TOutput> alt);

        public delegate IOption<IResultAlternative<TOutput>> FailureFactory();

        public delegate IOption<IResultAlternative<TOutput>> Function(IMultiResult<TOutput> result, SuccessFactory success, FailureFactory fail);

        public class Parser : IParser<TInput, TOutput>
        {
            private readonly Function _selector;
            private readonly IMultiParser<TInput, TOutput> _parser;

            public Parser(IMultiParser<TInput, TOutput> parser, Function selector)
            {
                Assert.ArgumentNotNull(parser, nameof(parser));
                Assert.ArgumentNotNull(selector, nameof(selector));
                _selector = selector;
                _parser = parser;
                Name = "";
            }

            public string Name { get; set; }

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

                var selected = _selector(multi, Success, Fail);
                if (selected == null || !selected.Success)
                    return state.Fail(this, "No alternative selected, or no matching value could be found");

                var alt = selected.Value;
                alt.Continuation.Rewind();
                return multi.ToResult(alt);
            }

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
