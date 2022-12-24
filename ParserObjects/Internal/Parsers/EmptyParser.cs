﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Utility;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// The empty parser, consumes no input and always returns success.
/// </summary>
/// <typeparam name="TInput"></typeparam>
public sealed record EmptyParser<TInput>(
    string Name = ""
) : IParser<TInput>
{
    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        return state.Success(this, Defaults.ObjectInstance, 0, state.Input.CurrentLocation);
    }

    public bool Match(IParseState<TInput> state) => true;

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Empty", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
