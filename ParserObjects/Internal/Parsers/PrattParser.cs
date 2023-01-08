using System;
using System.Collections.Generic;
using System.Linq;
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
    IReadOnlyList<IParselet<TInput, TOutput>> Parselets,
    IReadOnlyList<IParser> References,
    Engine<TInput, TOutput> Engine,
    string Name = ""
) : IParser<TInput, TOutput>
{
    public static PrattParser<TInput, TOutput> Configure(Action<Configuration<TInput, TOutput>> setup, string name = "")
    {
        Assert.ArgumentNotNull(setup, nameof(setup));
        var parselets = new List<IParselet<TInput, TOutput>>();
        var references = new List<IParser>();
        var config = new Configuration<TInput, TOutput>(parselets, references);
        setup(config);
        var engine = new Engine<TInput, TOutput>(parselets);
        return new PrattParser<TInput, TOutput>(parselets, references, engine, name);
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public IResult<TOutput> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        return state.WithDataFrame(this, static (s, p) =>
        {
            var startCp = s.Input.Checkpoint();
            try
            {
                Assert.ArgumentNotNull(s, nameof(s));
                var result = p.Engine.TryParse(s, 0);
                return s.Result(p, result);
            }
            catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Parser)
            {
                startCp.Rewind();
                return s.Fail<TInput, TOutput>(pe.Parser ?? p, pe.Message);
            }
        });
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state);

    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public IEnumerable<IParser> GetChildren() => Parselets.Select(static p => p.Parser).Concat(References);

    public override string ToString() => DefaultStringifier.ToString("Pratt", Name, Id);

    public INamed SetName(string name) => this with { Name = name };
}
