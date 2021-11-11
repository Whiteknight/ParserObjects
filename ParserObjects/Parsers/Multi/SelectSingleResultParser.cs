using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers.Multi
{
    public class SelectSingleResultParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly SelectMultiAlternativeFunction<TOutput> _selector;
        private readonly IMultiParser<TInput, TOutput> _parser;

        public SelectSingleResultParser(IMultiParser<TInput, TOutput> parser, SelectMultiAlternativeFunction<TOutput> selector)
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
