using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Looks up a parser at parse time, to avoid circular references in the grammar. The parser
/// looked up is expected to be constant for the duration of the parse and may be cached.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <typeparam name="TParser"></typeparam>
/// <param name="GetParser"></param>
/// <param name="Name"></param>
public sealed record DeferredParser<TInput, TOutput, TParser>(
    Func<TParser> GetParser,
    string Name = ""
) : SimpleRecordParser<TInput, TOutput>(Name), IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>
        where TParser : class, IParser
{
    public override Result<TOutput> Parse(IParseState<TInput> state)
    {
        var parser = GetParserFromCacheOrCallback(state) as IParser<TInput, TOutput>
            ?? throw new InvalidOperationException("Invalid parser type");
        return parser.Parse(state);
    }

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state)
    {
        var parser = GetParserFromCacheOrCallback(state) as IParser<TInput>
            ?? throw new InvalidOperationException("Invalid parser type");
        return parser.Parse(state);
    }

    MultiResult<TOutput> IMultiParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        var parser = GetParserFromCacheOrCallback(state) as IMultiParser<TInput, TOutput>
            ?? throw new InvalidOperationException("Invalid parser type");
        return parser.Parse(state);
    }

    MultiResult<object> IMultiParser<TInput>.Parse(IParseState<TInput> state)
    {
        var parser = GetParserFromCacheOrCallback(state) as IMultiParser<TInput>
            ?? throw new InvalidOperationException("Invalid parser type");
        return parser.Parse(state);
    }

    public override bool Match(IParseState<TInput> state)
    {
        var parser = GetParserFromCacheOrCallback(state) as IParser<TInput, TOutput>
           ?? throw new InvalidOperationException("Invalid parser type");
        return parser.Match(state);
    }

    public override IEnumerable<IParser> GetChildren() => new IParser[] { GetParser() };

    public override string ToString() => DefaultStringifier.ToString("Deferred", Name, Id);

    public override void Visit<TVisitor, TState>(TVisitor visitor, TState state)
    {
        visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
    }

    private TParser GetParserFromCacheOrCallback(IParseState<TInput> state)
    {
        var existing = state.Cache.Get<TParser>(this, default);
        if (existing.Success)
            return existing.Value;

        var parser = GetParser() ?? throw new InvalidOperationException("Deferred parser value must not be null");
        state.Cache.Add(this, default, parser);
        return parser;
    }
}
