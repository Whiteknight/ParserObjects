using System.Collections.Generic;
using System.Linq;
using ParserObjects.Gll;
using ParserObjects.Utility;

namespace ParserObjects.Parsers;

public sealed record GllParserAdaptor<TInput, TOutput>(
    IGllParser<TInput, TOutput> Inner,
    string Name = ""
) : IMultiParser<TInput, TOutput>
{
    // TODO: Fix this once IGllParser implements IParser
    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public IMultiResult<TOutput> Parse(IParseState<TInput> state)
    {
        var startContinuation = state.Input.Checkpoint();
        var matches = Engine.Instance.Execute(state.Input, Inner);
        var alternatives = matches.Select(m => MapMatchToResultAlternative(startContinuation, m));
        return new MultiResult<TOutput>(this, startContinuation.Location, startContinuation, alternatives);
    }

    IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    private IResultAlternative<TOutput> MapMatchToResultAlternative(ISequenceCheckpoint startContinuation, IMatch match)
    {
        if (!match.Success)
            return new FailureResultAlternative<TOutput>(match.ErrorMessage, startContinuation);
        var valueOption = match.GetValue<TOutput>();
        if (!valueOption.Success)
            return new FailureResultAlternative<TOutput>("Wrong type", startContinuation);
        return new SuccessResultAlternative<TOutput>(valueOption.Value, 0, match.State.StartCheckpoint);
    }

    public override string ToString() => DefaultStringifier.ToString(this);

    public INamed SetName(string name) => this with { Name = name };
}

// TODO: IParser<TIn, TOut> -> IGllParser<TIn, TOut> ?
// TODO: IMultiParser<TIn, TOut> -> IGllParser<TIn, TOut> ?
// Do we need adaptors like this?
// public static class ParserToGllAdaptor<TInput, TOutput>
// {
//    public class SingleParser : IGllParser<TInput, TOutput>
//    {
//        public SingleParser(IParser<TInput, TOutput> inner)
//        {
//        }
//    }
// }
