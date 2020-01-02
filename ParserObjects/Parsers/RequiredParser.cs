using System;
using System.Collections.Generic;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Parser to require a valid output. If the inner parser fails, a default fallback value
    /// is constructed and returned.
    /// The default fallback value might be an error/diagnostic object to help with error reporting
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class RequiredParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _inner;
        private readonly Func<ISequence<TInput>, TOutput> _otherwise;

        public RequiredParser(IParser<TInput, TOutput> inner, Func<ISequence<TInput>, TOutput> otherwise)
        {
            _inner = inner;
            _otherwise = otherwise;
            if (!string.IsNullOrEmpty(_inner.Name))
                Name = _inner.Name + "!";
        }

        public IParseResult<TOutput> Parse(ISequence<TInput> t)
        {
            var location = t.CurrentLocation;
            var result = _inner.Parse(t);
            if (result.Success)
                return result;

            // Otherwise, create the fallback production at the location where _inner would have started
            return new SuccessResult<TOutput>(_otherwise(t), location);
        }

        IParseResult<object> IParser<TInput>.ParseUntyped(ISequence<TInput> t) => Parse(t).Untype();

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (_inner == find && replace is IParser<TInput, TOutput> realReplace)
                return new RequiredParser<TInput, TOutput>(realReplace, _otherwise);
            return this;
        }

        public override string ToString()
        {
            return $"Required.{_inner}";
        }
    }
}