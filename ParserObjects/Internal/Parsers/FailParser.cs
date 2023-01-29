using System.Collections.Generic;
using System.Linq;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Returns unconditional failure, optionally with a helpful error message.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class FailParser<TInput, TOutput> : IParser<TInput, TOutput>, IMultiParser<TInput, TOutput>
{
    private readonly IResult<TOutput> _result;

    public FailParser(string errorMessage = "Fail", string name = "")
    {
        Name = name;
        _result = new FailureResult<TOutput>(this, errorMessage);
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public string ErrorMessage => _result.ErrorMessage;

    IResult<TOutput> IParser<TInput, TOutput>.Parse(IParseState<TInput> state)
        => _result;

    IResult IParser<TInput>.Parse(IParseState<TInput> state)
        => _result;

    IMultiResult<TOutput> IMultiParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        var startCheckpoint = state.Input.Checkpoint();
        return new MultiResult<TOutput>(this, startCheckpoint, new[]
        {
            new FailureResultAlternative<TOutput>(ErrorMessage, startCheckpoint)
        });
    }

    IMultiResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
        => ((IMultiParser<TInput, TOutput>)this).Parse(state);

    public bool Match(IParseState<TInput> state) => false;

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString("Fail", Name, Id);

    public INamed SetName(string name) => new FailParser<TInput, TOutput>(ErrorMessage, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<ICorePartialVisitor<TState>>()?.Accept(this, state);
    }
}
