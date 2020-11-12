﻿using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// A parser for holding a parsed result from left application. Do not use this type
    /// directly.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class LeftValueParser<TInput, TOutput> : IParser<TInput, TOutput>
    {
        public TOutput Value { get; set; }
        public Location Location { get; set; }

        public IResult<TOutput> Parse(ParseState<TInput> t) => t.Success(Value, Location);

        IResult<object> IParser<TInput>.ParseUntyped(ParseState<TInput> t) => t.Success<object>(Value, Location);

        public string Name { get; set; }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;
    }
}