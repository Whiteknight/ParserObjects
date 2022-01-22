using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Pratt;

// Simple contextual wrapper, so that private Engine methods can be
// exposed to user callbacks. This class is for internal use only. Users should interact with
// the provided abstractions.
public sealed class ParseContext<TInput, TOutput> : IParseContext<TInput, TOutput>
{
    private readonly IParseState<TInput> _state;
    private readonly Engine<TInput, TOutput> _engine;

    private readonly int _rbp;
    private readonly bool _canRecurse;

    private readonly int _startConsumed;

    public ParseContext(IParseState<TInput> state, Engine<TInput, TOutput> engine, int rbp, bool canRecurse, string name)
    {
        Assert.ArgumentNotNull(state, nameof(state));
        Assert.ArgumentNotNull(engine, nameof(engine));
        _state = state;
        _engine = engine;
        _rbp = rbp;
        _startConsumed = state.Input.Consumed;
        Name = name;
        _canRecurse = canRecurse;
    }

    public IDataStore Data => _state.Data;

    public int Consumed => _state.Input.Consumed - _startConsumed;

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public ISequence<TInput> Input => _state.Input;

    public TOutput Parse() => Parse(_rbp);

    public TOutput Parse(int rbp)
    {
        EnsureRecursionIsPermitted();
        var result = _engine.TryParse(_state, rbp);
        if (!result.Success)
            throw new ParseException(ParseExceptionSeverity.Rule, result.ErrorMessage!, this, result.Location);
        return result.Value!;
    }

    public TValue Parse<TValue>(IParser<TInput, TValue> parser)
    {
        Assert.ArgumentNotNull(parser, nameof(parser));
        var result = parser.Parse(_state);
        if (!result.Success)
            throw new ParseException(ParseExceptionSeverity.Rule, result.ErrorMessage, parser, result.Location);
        return result.Value;
    }

    IResult<TOutput> IParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        EnsureRecursionIsPermitted();
        var result = _engine.TryParse(_state, _rbp);
        return state.Result(this, result);
    }

    IResult IParser<TInput>.Parse(IParseState<TInput> state) => ((IParser<TInput, TOutput>)this).Parse(state);

    public bool Match(IParser<TInput> parser)
    {
        Assert.ArgumentNotNull(parser, nameof(parser));
        var result = parser.Parse(_state);
        return result.Success;
    }

    public void Expect(IParser<TInput> parser)
    {
        Assert.ArgumentNotNull(parser, nameof(parser));
        var result = parser.Parse(_state);
        if (!result.Success)
            throw new ParseException(ParseExceptionSeverity.Rule, result.ErrorMessage, parser, result.Location);
    }

    public void FailRule(string message = "")
        => throw new ParseException(ParseExceptionSeverity.Rule, message ?? "Fail", this, _state.Input.CurrentLocation);

    public void FailLevel(string message = "")
        => throw new ParseException(ParseExceptionSeverity.Level, message ?? "", this, _state.Input.CurrentLocation);

    public void FailAll(string message = "")
        => throw new ParseException(ParseExceptionSeverity.Parser, message ?? "", this, _state.Input.CurrentLocation);

    public IOption<TOutput> TryParse() => TryParse(_rbp);

    public IOption<TOutput> TryParse(int rbp)
    {
        if (!_canRecurse)
            return FailureOption<TOutput>.Instance;
        var result = _engine.TryParse(_state, rbp);
        return result.Match(FailureOption<TOutput>.Instance, value => new SuccessOption<TOutput>(value));
    }

    public IOption<TValue> TryParse<TValue>(IParser<TInput, TValue> parser)
    {
        Assert.ArgumentNotNull(parser, nameof(parser));
        var result = parser.Parse(_state);
        return result!.Success ? new SuccessOption<TValue>(result!.Value) : FailureOption<TValue>.Instance;
    }

    public IEnumerable<IParser> GetChildren() => Enumerable.Empty<IParser>();

    public override string ToString() => DefaultStringifier.ToString(this);

    private void EnsureRecursionIsPermitted()
    {
        if (!_canRecurse)
            throw new ParseException(ParseExceptionSeverity.Rule, "The parser consumed zero input, so an attempt to recurse was denied to avoid an infinite loop.", this, _state.Input.CurrentLocation);
    }

    public INamed SetName(string name) => throw new InvalidOperationException("Cannot name an internal parse context");
}
