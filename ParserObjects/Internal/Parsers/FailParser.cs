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
    private readonly string _errorMessage;

    public FailParser(string errorMessage = "Fail", string name = "")
    {
        _errorMessage = errorMessage;
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public string ErrorMessage => _errorMessage;

    Result<TOutput> IParser<TInput, TOutput>.Parse(IParseState<TInput> state)
        => Result<TOutput>.Fail(this, _errorMessage);

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state)
        => Result<object>.Fail(this, _errorMessage);

    IMultResult<TOutput> IMultiParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        var startCheckpoint = state.Input.Checkpoint();
        return new MultResult<TOutput>(this, startCheckpoint, new[]
        {
            new FailureResultAlternative<TOutput>(ErrorMessage, startCheckpoint)
        });
    }

    IMultResult IMultiParser<TInput>.Parse(IParseState<TInput> state)
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
