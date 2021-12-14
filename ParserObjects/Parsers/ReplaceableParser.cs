using System.Collections.Generic;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

/// <summary>
/// Delegates to an internal parser, and allows the internal parser to be replaced in-place
/// after the parser graph has been created. Useful for cases where grammar extensions or
/// modifications need to be made after the parser has been created.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class Replaceable<TInput>
{
    public sealed class SingleParser : IParser<TInput>, IReplaceableParserUntyped
    {
        private IParser<TInput> _value;

        public SingleParser(IParser<TInput> defaultValue, string name = "")
        {
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));
            _value = defaultValue;
            Name = name;
        }

        public string Name { get; }

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

        public INamed SetName(string name) => new SingleParser(_value, name);
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
    public sealed class SingleParser : IParser<TInput, TOutput>, IReplaceableParserUntyped
    {
        private IParser<TInput, TOutput> _value;

        public SingleParser(IParser<TInput, TOutput> defaultValue, string name = "")
        {
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));
            _value = defaultValue;
            Name = name;
        }

        public string Name { get; }

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

        public INamed SetName(string name) => new SingleParser(_value, name);
    }

    public sealed class MultiParser : IMultiParser<TInput, TOutput>, IReplaceableParserUntyped
    {
        private IMultiParser<TInput, TOutput> _value;

        public MultiParser(IMultiParser<TInput, TOutput> defaultValue, string name = "")
        {
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));
            _value = defaultValue;
            Name = name;
        }

        public string Name { get; }

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

        public INamed SetName(string name) => new MultiParser(_value, name);
    }
}
