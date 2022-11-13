using System;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Sequences;

/// <summary>
/// An adaptor to change output values from an IParser into an ISequence of results.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class ParseResultSequence<TInput, TOutput> : ISequence<IResult<TOutput>>
{
    private readonly ISequence<TInput> _input;
    private readonly ParseState<TInput> _state;
    private readonly IParser<TInput, TOutput> _parser;

    public ParseResultSequence(ISequence<TInput> input, IParser<TInput, TOutput> parser, Action<string> log)
    {
        Assert.ArgumentNotNull(input, nameof(input));
        Assert.ArgumentNotNull(parser, nameof(parser));
        _state = new ParseState<TInput>(input, log);
        _input = input;
        _parser = parser;
    }

    public IResult<TOutput> GetNext() => GetNext(true);

    public IResult<TOutput> Peek() => GetNext(false);

    public Location CurrentLocation => _input.CurrentLocation;

    public bool IsAtEnd => _input.IsAtEnd;

    public int Consumed => _input.Consumed;

    public ISequenceCheckpoint Checkpoint() => _input.Checkpoint();

    private IResult<TOutput> GetNext(bool advance)
    {
        if (!advance)
        {
            var cp = _input.Checkpoint();
            var peek = _parser.Parse(_state);
            cp.Rewind();
            return peek;
        }

        return _parser.Parse(_state);
    }

    public ISequenceStatistics GetStatistics() => _input.GetStatistics();
}
