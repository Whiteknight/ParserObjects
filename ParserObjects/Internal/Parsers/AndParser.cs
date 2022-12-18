﻿using System.Collections.Generic;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Tests several parsers sequentially. If all of them succeed return Success. If any Fail,
/// return Failure. Consumes input but returns no explicit output.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record AndParser<TInput>(
    IReadOnlyList<IParser<TInput>> Parsers,
    string Name = ""
) : IParser<TInput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        for (int i = 0; i < Parsers.Count; i++)
        {
            var parser = Parsers[i];
            var result = parser.Parse(state);
            if (!result.Success)
            {
                startCheckpoint.Rewind();
                return result;
            }
        }

        var consumed = state.Input.Consumed - startCheckpoint.Consumed;
        return state.Success(this, Defaults.ObjectInstance, consumed, startCheckpoint.Location);
    }

    public bool Match(IParseState<TInput> state)
    {
        var startCheckpoint = state.Input.Checkpoint();
        for (int i = 0; i < Parsers.Count; i++)
        {
            var parser = Parsers[i];
            var result = parser.Match(state);
            if (!result)
            {
                startCheckpoint.Rewind();
                return false;
            }
        }

        return true;
    }

    public IEnumerable<IParser> GetChildren() => Parsers;

    public override string ToString() => DefaultStringifier.ToString("And", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
