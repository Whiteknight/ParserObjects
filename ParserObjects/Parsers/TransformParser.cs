﻿using System;
using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Transforms the output of one parser into a different form based on context
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TMiddle"></typeparam>
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

        public IParser ReplaceChild(IParser find, IParser replace)
        {
            if (find == _parser && replace is IParser<TInput, TMiddle> realReplace)
                return new TransformParser<TInput, TMiddle, TOutput>(realReplace, _transform);
            return this;
        }

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}