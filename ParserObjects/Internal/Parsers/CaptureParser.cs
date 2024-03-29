﻿using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Attempt to match a list of inner parsers, in order, but do not capture the results explicitly.
/// Instead a callback method with start and end checkpoints for the entire completed match is
/// used to produce a result value. In practice this is often used to get a list of all input values
/// from the input sequence between the two checkpoints.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
/// <param name="Parsers"></param>
/// <param name="GetOutput"></param>
/// <param name="Name"></param>
public record CaptureParser<TInput, TOutput>(
    IReadOnlyList<IParser<TInput>> Parsers,
    Func<ISequence<TInput>, SequenceCheckpoint, SequenceCheckpoint, TOutput> GetOutput,
    string Name = ""
) : IParser<TInput, TOutput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        var startCp = state.Input.Checkpoint();
        for (int i = 0; i < Parsers.Count; i++)
        {
            var parser = Parsers[i];
            var result = parser.Match(state);
            if (!result)
            {
                startCp.Rewind();
                return state.Fail(this, $"Inner parser {i} failed.");
            }
        }

        var endCp = state.Input.Checkpoint();
        var contents = GetOutput(state.Input, startCp, endCp);
        return state.Success(this, contents, endCp.Consumed - startCp.Consumed);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state)
    {
        var startCp = state.Input.Checkpoint();
        for (int i = 0; i < Parsers.Count; i++)
        {
            var parser = Parsers[i];
            var result = parser.Match(state);
            if (!result)
            {
                startCp.Rewind();
                return false;
            }
        }

        return true;
    }

    public IEnumerable<IParser> GetChildren() => Parsers;

    public INamed SetName(string name) => this with { Name = name };

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
    }
}
