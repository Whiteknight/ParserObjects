using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Attempts to match a predicate condition and, invokes a specified parser on success or
    /// failure
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class IfParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput> _predicate;
        private readonly IParser<TInput, TOutput> _onSuccess;
        private readonly IParser<TInput, TOutput> _onFail;

        public IfParser(IParser<TInput> predicate, IParser<TInput, TOutput> onSuccess, IParser<TInput, TOutput> onFail)
        {
            Assert.ArgumentNotNull(predicate, nameof(predicate));
            Assert.ArgumentNotNull(onSuccess, nameof(onSuccess));
            Assert.ArgumentNotNull(onFail, nameof(onFail));
            _predicate = predicate;
            _onSuccess = onSuccess;
            _onFail = onFail;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var checkpoint = state.Input.Checkpoint();
            var result = _predicate.Parse(state);
            checkpoint.Rewind();
            if (result.Success)
                return _onSuccess.Parse(state);
            return _onFail.Parse(state);
        }

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => new IParser[] { _predicate, _onSuccess };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _predicate && replace is IParser<TInput> predicate)
                return new IfParser<TInput, TOutput>(predicate, _onSuccess, _onFail);
            if (find == _onSuccess && replace is IParser<TInput, TOutput> onSuccess)
                return new IfParser<TInput, TOutput>(_predicate, onSuccess, _onFail);
            if (find == _onFail && replace is IParser<TInput, TOutput> onFail)
                return new IfParser<TInput, TOutput>(_predicate, _onSuccess, onFail);
            return this;
        }
    }
}