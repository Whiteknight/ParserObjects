using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

// NOTE: Replaceable parsers should be the only parser types which have mutable data.
// Keep it localized to just one place.

namespace ParserObjects.Internal.Parsers;

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
            Assert.ArgumentNotNull(defaultValue);
            _value = defaultValue;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IParser ReplaceableChild => _value;

        public Result<object> Parse(IParseState<TInput> state) => _value.Parse(state);

        public bool Match(IParseState<TInput> state) => _value.Match(state);

        public IEnumerable<IParser> GetChildren() => new[] { _value };

        public SingleReplaceResult SetParser(IParser parser)
        {
            var previous = _value;
            if (parser is IParser<TInput> typed)
                _value = typed;
            return new SingleReplaceResult(this, previous, _value);
        }

        public override string ToString() => DefaultStringifier.ToString("Replaceable", Name, Id);

        public INamed SetName(string name) => new SingleParser(_value, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
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
            Assert.ArgumentNotNull(defaultValue);
            _value = defaultValue;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IParser ReplaceableChild => _value;

        public Result<TOutput> Parse(IParseState<TInput> state) => _value.Parse(state);

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => _value.Parse(state).AsObject();

        public bool Match(IParseState<TInput> state) => _value.Match(state);

        public IEnumerable<IParser> GetChildren() => new[] { _value };

        public SingleReplaceResult SetParser(IParser parser)
        {
            var previous = _value;
            if (parser is IParser<TInput, TOutput> typed)
                _value = typed;
            return new SingleReplaceResult(this, previous, _value);
        }

        public override string ToString() => DefaultStringifier.ToString("Replaceable", Name, Id);

        public INamed SetName(string name) => new SingleParser(_value, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class MultiParser : IMultiParser<TInput, TOutput>, IReplaceableParserUntyped
    {
        private IMultiParser<TInput, TOutput> _value;

        public MultiParser(IMultiParser<TInput, TOutput> defaultValue, string name = "")
        {
            Assert.ArgumentNotNull(defaultValue);
            _value = defaultValue;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IParser ReplaceableChild => _value;

        public IMultResult<TOutput> Parse(IParseState<TInput> state) => _value.Parse(state);

        IMultResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => _value.Parse(state);

        public IEnumerable<IParser> GetChildren() => new[] { _value };

        public SingleReplaceResult SetParser(IParser parser)
        {
            var previous = _value;
            if (parser is IMultiParser<TInput, TOutput> typed)
                _value = typed;
            return new SingleReplaceResult(this, previous, _value);
        }

        public override string ToString() => DefaultStringifier.ToString("Replaceable", Name, Id);

        public INamed SetName(string name) => new MultiParser(_value, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
