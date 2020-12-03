using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Transforms the output of one parser into a different value. Only used to transform success
    /// values.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class TransformParser<TInput, TMiddle, TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TMiddle> _parser;
        private readonly Func<TMiddle, TOutput> _transform;

        public TransformParser(IParser<TInput, TMiddle> parser, Func<TMiddle, TOutput> transform)
        {
            Assert.ArgumentNotNull(parser, nameof(parser));
            Assert.ArgumentNotNull(transform, nameof(transform));
            _parser = parser;
            _transform = transform;
        }

        public string Name { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> state) => _parser.Parse(state).Transform(_transform);

        IResult IParser<TInput>.Parse(ParseState<TInput> state) => _parser.Parse(state).Transform(_transform);

        public IEnumerable<IParser> GetChildren() => new[] { _parser };

        public override string ToString() => ParserDefaultStringifier.ToString(this);
    }
}