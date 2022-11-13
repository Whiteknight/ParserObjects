﻿using System;
using System.Collections.Generic;
using ParserObjects.Internal.Pratt;
using ParserObjects.Internal.Utility;
using ParserObjects.Pratt;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Implementation of the Pratt parsing algorithm. Uses special Right production rules ("NUD")
/// and Left production rules (LED) to recursively parse an input. Especially useful for
/// mathematical expressions.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed record PrattParser<TInput, TOutput>(
    Configuration<TInput, TOutput> Config,
    Engine<TInput, TOutput> Engine,
    string Name = ""
) : IParser<TInput, TOutput>
{
    public static PrattParser<TInput, TOutput> Configure(Action<IConfiguration<TInput, TOutput>> setup, string name = "")
    {
        Assert.ArgumentNotNull(setup, nameof(setup));
        var config = new Configuration<TInput, TOutput>();
        setup(config);
        var engine = new Engine<TInput, TOutput>(config.Parselets);
        return new PrattParser<TInput, TOutput>(config, engine, name);
    }

    public static PrattParser<TInput, TOutput> Create(Configuration<TInput, TOutput> config, string name = "")
    {
        var engine = new Engine<TInput, TOutput>(config.Parselets);
        return new PrattParser<TInput, TOutput>(config, engine, name);
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));

        var startCp = state.Input.Checkpoint();
        var dataStore = state.Data as CascadingKeyValueStore;
        dataStore?.PushFrame();
        try
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var result = Engine.TryParse(state, 0);
            return state.Result(this, result);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Parser)
        {
            startCp.Rewind();
            return state.Fail<TInput, TOutput>(pe.Parser ?? this, pe.Message, pe.Location ?? state.Input.CurrentLocation);
        }
        finally
        {
            dataStore?.PopFrame();
        }
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public IEnumerable<IParser> GetChildren() => Config.GetParsers();

    public override string ToString() => DefaultStringifier.ToString("Pratt", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}