﻿using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers
{
    /// <summary>
    /// Delegates to an internal parser, and allows the internal parser to be replaced in-place
    /// after the parser graph has been created. Useful for cases where grammar extensions or
    /// modifications need to be made after the parser has been created.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public static class Replaceable<TInput>
    {
        public class SingleParser : IParser<TInput>, IReplaceableParserUntyped
        {
            private IParser<TInput> _value;

            public SingleParser(IParser<TInput> defaultValue)
            {
                Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));
                _value = defaultValue;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IParser ReplaceableChild => _value;

            public IResult Parse(IParseState<TInput> state) => _value.Parse(state);

            public IEnumerable<IParser> GetChildren() => new[] { _value };

            public SingleReplaceResult SetParser(IParser parser)
            {
                var previous = _value;
                if (parser is IParser<TInput> typed)
                    _value = typed;
                return new SingleReplaceResult(this, previous, _value);
            }

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }

    /// <summary>
    /// Delegates to an internal parser, and allows the internal parser to be replaced in-place
    /// after the parser graph has been created. Useful for cases where grammar extensions or
    /// modifications need to be made after the parser has been created.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public static class Replaceable<TInput, TOutput>
    {
        public class SingleParser : IParser<TInput, TOutput>, IReplaceableParserUntyped
        {
            private IParser<TInput, TOutput> _value;

            public SingleParser(IParser<TInput, TOutput> defaultValue)
            {
                Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));
                _value = defaultValue;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IParser ReplaceableChild => _value;

            public IResult<TOutput> Parse(IParseState<TInput> state) => _value.Parse(state);

            IResult IParser<TInput>.Parse(IParseState<TInput> state) => _value.Parse(state);

            public IEnumerable<IParser> GetChildren() => new[] { _value };

            public SingleReplaceResult SetParser(IParser parser)
            {
                var previous = _value;
                if (parser is IParser<TInput, TOutput> typed)
                    _value = typed;
                return new SingleReplaceResult(this, previous, _value);
            }

            public override string ToString() => DefaultStringifier.ToString(this);
        }

        public class MultiParser : IMultiParser<TInput, TOutput>, IReplaceableParserUntyped
        {
            private IMultiParser<TInput, TOutput> _value;

            public MultiParser(IMultiParser<TInput, TOutput> defaultValue)
            {
                Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));
                _value = defaultValue;
                Name = string.Empty;
            }

            public string Name { get; set; }

            public IParser ReplaceableChild => _value;

            public IMultiResult<TOutput> Parse(IParseState<TInput> state) => _value.Parse(state);

            IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => _value.Parse(state);

            public IEnumerable<IParser> GetChildren() => new[] { _value };

            public SingleReplaceResult SetParser(IParser parser)
            {
                var previous = _value;
                if (parser is IMultiParser<TInput, TOutput> typed)
                    _value = typed;
                return new SingleReplaceResult(this, previous, _value);
            }

            public override string ToString() => DefaultStringifier.ToString(this);
        }
    }
}
