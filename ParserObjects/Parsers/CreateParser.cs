﻿using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    public static class Create<TInput, TOutput>
    {
        public delegate IParser<TInput, TOutput> Function(ParseState<TInput> state);

        /// <summary>
        /// Create a parser dynamically using information from the parse state. The parser created is 
        /// not expected to be constant and will not be cached.
        /// </summary>
        public class Parser : IParser<TInput, TOutput>
        {
            private readonly Function _getParser;

            public Parser(Function getParser)
            {
                Assert.ArgumentNotNull(getParser, nameof(getParser));
                _getParser = getParser;
            }

            public string Name { get; set; }

            public IResult<TOutput> Parse(ParseState<TInput> state)
            {
                var parser = _getParser(state);
                if (parser == null)
                    throw new InvalidOperationException("Create parser value must not be null");
                return parser.Parse(state);
            }

            IResult IParser<TInput>.Parse(ParseState<TInput> state) => Parse(state);

            public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

            public IParser ReplaceChild(IParser find, IParser replace) => this;

            public override string ToString()
            {
                var typeName = this.GetType().Name;
                return Name == null ? base.ToString() : $"{typeName} {Name}";
            }
        }
    }
}
