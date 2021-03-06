﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Matches at the end of the input sequence. Fails if the input sequence is at any point
    /// besides the end.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class EndParser<TInput> : IParser<TInput>
    {
        public EndParser()
        {
            Name = string.Empty;
        }

        public string Name { get; set; }

        public IResult Parse(IParseState<TInput> state)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            return state.Input.IsAtEnd
                ? state.Success(this, Defaults.ObjectInstance, 0)
                : state.Fail(this, "Expected end of Input but found " + state.Input.Peek()!.ToString());
        }

        public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

        public override string ToString() => DefaultStringifier.ToString(this);
    }
}
