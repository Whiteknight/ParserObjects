using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Pratt;
using ParserObjects.Internal.Visitors;
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
    /* The Pratt implementation is located in Internals/Pratt/* and most of the core parsing logic
     * is in the Engine class. This Parser type wraps an Engine instance and delegates most of the
     * work to that. The Pratt.ParseException exception type is used for non-local control flow
     * within the Pratt parser, and one of the conditions of it is handled here. ParseException
     * should not be allowed to escape beyond this class.
     */

    public static PrattParser<TInput, TOutput> Configure(Action<Configuration<TInput, TOutput>> setup, string name = "")
    {
        Assert.ArgumentNotNull(setup);
        var parselets = new List<IParselet<TInput, TOutput>>();
        var references = new List<IParser>();
        var config = new Configuration<TInput, TOutput>(parselets, references);
        setup(config);
        var engine = new Engine<TInput, TOutput>(parselets);
        return new PrattParser<TInput, TOutput>(parselets, references, engine, name);
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public Result<TOutput> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        var frame = state.Data.PushDataFrame();
        var startCp = state.Input.Checkpoint();
        try
        {
            var result = Engine.TryParse(state, 0);
            return result.ToResult(this);
        }
        catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Parser)
        {
            startCp.Rewind();
            return state.Fail<TOutput>(pe.Parser ?? this, pe.Message);
        }
        finally
        {
            state.Data.PopDataFrame(frame);
        }
    }

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

    // Because of the implementation of the Pratt engine, there's no (obvious) way to match it
    // without just doing a full parse and allocating results.
    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public IEnumerable<IParser> GetChildren() => Parselets.Select(static p => p.Parser).Concat(References);

    public override string ToString() => DefaultStringifier.ToString("Pratt", Name, Id);

    public INamed SetName(string name) => this with { Name = name };

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IPrattPartialVisitor<TState>>()?.Accept(this, state);
    }
}
