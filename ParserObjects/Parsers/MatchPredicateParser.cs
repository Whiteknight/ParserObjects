﻿using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Returns the next input item if it satisfies a predicate, failure otherwise.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MatchPredicateParser<T> : IParser<T, T>
    {
        private readonly Func<T, bool> _predicate;

        public MatchPredicateParser(Func<T, bool> predicate)
        {
            Assert.ArgumentNotNull(predicate, nameof(predicate));
            _predicate = predicate;
        }

        public string Name { get; set; }

        public IResult<T> Parse(ParseState<T> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));

            var location = state.Input.CurrentLocation;

            if (state.Input.IsAtEnd)
                return state.Fail(this, "Expected a matching item, but found End");

            var next = state.Input.Peek();
            if (!_predicate(next))
                return state.Fail(this, "Next item does not match the predicate");

            return state.Success(this, state.Input.GetNext(), location);
        }

        IResult IParser<T>.Parse(ParseState<T> state) => Parse(state);

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public IParser ReplaceChild(IParser find, IParser replace) => this;

        public override string ToString()
        {
            var typeName = GetType().Name;
            return Name == null ? base.ToString() : $"{typeName} {Name}";
        }
    }
}