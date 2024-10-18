using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Pushes a data frame onto the contextual data store for the current parse, and pops the frame
/// off again when the inner parser has completed.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public static class DataFrame<TInput>
{
    public sealed class Parser<TOutput> : IParser<TInput, TOutput>
    {
        private readonly IParser<TInput, TOutput> _inner;
        private readonly IReadOnlyDictionary<string, object>? _values;

        public Parser(IParser<TInput, TOutput> inner, IReadOnlyDictionary<string, object>? values = null, string name = "")
        {
            _inner = inner;
            _values = values;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public bool Match(IParseState<TInput> state)
        {
            return state.WithDataFrame(_inner, static (s, p) => p.Match(s), _values);
        }

        public IResult<TOutput> Parse(IParseState<TInput> state)
        {
            return state.WithDataFrame(_inner, static (s, p) => p.Parse(s), _values);
        }

        public INamed SetName(string name) => new Parser<TOutput>(_inner, _values, name);

        IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }

    public sealed class MultiParser<TOutput> : IMultiParser<TInput, TOutput>
    {
        private readonly IMultiParser<TInput, TOutput> _inner;
        private readonly IReadOnlyDictionary<string, object>? _values;

        public MultiParser(IMultiParser<TInput, TOutput> inner, IReadOnlyDictionary<string, object>? values = null, string name = "")
        {
            _inner = inner;
            _values = values;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public string Name { get; }

        public IEnumerable<IParser> GetChildren() => new[] { _inner };

        public IMultiResult<TOutput> Parse(IParseState<TInput> state)
        {
            return state.WithDataFrame(_inner, static (s, p) => p.Parse(s), _values);
        }

        public INamed SetName(string name) => new MultiParser<TOutput>(_inner, _values, name);

        IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

        public override string ToString() => DefaultStringifier.ToString(this);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
