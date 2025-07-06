using System;
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
/// <typeparam name="TOutput"></typeparam>
public static class Replaceable<TInput, TOutput>
{
    private const string _errorParserWrongType = "Parser is not the correct type";

    public static TParser From<TParser>(TParser parser)
        where TParser : class, IParser
        => (new Parser<TParser>(parser) as TParser)!;

    public sealed class Parser<TParser>
        : IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>, IReplaceableParserUntyped
        where TParser : class, IParser
    {
        private TParser _parser;

        public Parser(TParser parser, string name = "")
        {
            _parser = parser;
            Name = name;
        }

        public int Id { get; } = UniqueIntegerGenerator.GetNext();

        public IParser ReplaceableChild => _parser;

        public string Name { get; }

        Result<object> IParser<TInput>.Parse(IParseState<TInput> state)
        {
            var parser = _parser as IParser<TInput>
                ?? throw new InvalidOperationException(_errorParserWrongType);
            return parser.Parse(state);
        }

        Result<TOutput> IParser<TInput, TOutput>.Parse(IParseState<TInput> state)
        {
            var parser = _parser as IParser<TInput, TOutput>
                ?? throw new InvalidOperationException(_errorParserWrongType);
            return parser.Parse(state);
        }

        MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state)
        {
            var parser = _parser as IMultiParser<TInput>
                ?? throw new InvalidOperationException(_errorParserWrongType);
            return parser.Parse(state);
        }

        MultiResult<TOutput> IMultiParser<TInput, TOutput>.Parse(IParseState<TInput> state)
        {
            var parser = _parser as IMultiParser<TInput, TOutput>
                ?? throw new InvalidOperationException(_errorParserWrongType);
            return parser.Parse(state);
        }

        public bool Match(IParseState<TInput> state)
        {
            var parser = _parser as IParser<TInput>
                ?? throw new InvalidOperationException(_errorParserWrongType);
            return parser.Match(state);
        }

        public IEnumerable<IParser> GetChildren() => [_parser];

        public SingleReplaceResult SetParser(IParser parser)
        {
            var previous = _parser;
            if (parser is TParser typed)
                _parser = typed;
            return new SingleReplaceResult(this, previous, _parser);
        }

        public override string ToString() => DefaultStringifier.ToString("Replaceable", Name, Id);

        public INamed SetName(string name) => new Parser<TParser>(_parser, name);

        public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        {
            visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
        }
    }
}
