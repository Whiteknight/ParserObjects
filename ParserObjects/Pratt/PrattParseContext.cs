using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ParserObjects.Internal;
using ParserObjects.Internal.Pratt;

namespace ParserObjects.Pratt;

/// <summary>
/// The current context of the Pratt parse, passed to user LED and NUD callbacks. This type provides
/// methods for recursing into the Pratt engine and executing other parsers in a controlled way.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public readonly struct PrattParseContext<TInput, TOutput> : IParser<TInput, TOutput>
{
    private readonly IParseState<TInput> _state;
    private readonly Engine<TInput, TOutput> _engine;
    private readonly int _rbp;
    private readonly bool _canRecurse;
    private readonly ParseControl _parseControl;
    private readonly int _startConsumed;

    public PrattParseContext(IParseState<TInput> state, Engine<TInput, TOutput> engine, int rbp, bool canRecurse, string name, ParseControl parseControl)
    {
        Assert.ArgumentNotNull(state);
        Assert.ArgumentNotNull(engine);
        Assert.ArgumentNotNull(parseControl);

        _state = state;
        _engine = engine;
        _rbp = rbp;
        _startConsumed = state.Input.Consumed;
        Name = name;
        _parseControl = parseControl;
        _canRecurse = canRecurse;
    }

    /// <summary>
    /// Gets the current parse state data.
    /// </summary>
    public DataStore Data => _state.Data;

    /// <summary>
    /// Gets the number of input items consumed so far.
    /// </summary>
    public int Consumed => _state.Input.Consumed - _startConsumed;

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    /// <summary>
    /// Gets the input parse sequence.
    /// </summary>
    public ISequence<TInput> Input => _state.Input;

    /// <summary>
    /// Recurse into the Pratt engine to obtain a new value with the same Right Binding Power
    /// as the current rule.
    /// </summary>
    /// <returns></returns>
    public TOutput Parse() => Parse(_rbp);

    /// <summary>
    /// Recurse into the Pratt engine to obtain a new value with the given Right Binding Power.
    /// </summary>
    /// <param name="rbp"></param>
    /// <returns></returns>
    /// <exception cref="ParseException">Thrown if the recursion fails.</exception>
    public TOutput Parse(int rbp)
    {
        EnsureIsNotComplete();
        EnsureRecursionIsPermitted();
        var result = _engine.TryParse(_state, rbp, _parseControl);
        return result.Success
            ? result.Value!
            : throw new ParseException(ParseExceptionSeverity.Rule, result.ErrorMessage!, this, _state.Input.CurrentLocation);
    }

    /// <summary>
    /// Invoke the given parser and return the successful result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="parser"></param>
    /// <returns></returns>
    /// <exception cref="ParseException">Thrown if the parser fails.</exception>
    public TValue Parse<TValue>(IParser<TInput, TValue> parser)
    {
        Assert.ArgumentNotNull(parser);
        EnsureIsNotComplete();
        return parser.Parse(_state) switch
        {
            (true, var value, _) => value!,
            (false, _, var error) => throw new ParseException(ParseExceptionSeverity.Rule, error!, parser, _state.Input.CurrentLocation)
        };
    }

    Result<TOutput> IParser<TInput, TOutput>.Parse(IParseState<TInput> state)
    {
        EnsureIsNotComplete();
        EnsureRecursionIsPermitted();
        var result = _engine.TryParse(_state, _rbp, _parseControl);
        return result.ToResult(this);
    }

    // There's not really any way to access this method, because we can't use the IParseState<>
    // inside the user callback method. It exists to satisfy the interface and maybe there's some
    // way to compose parsers in a way that makes this callable, but for now we exclude it from tests
    [ExcludeFromCodeCoverage]
    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => ((IParser<TInput, TOutput>)this).Parse(state).AsObject();

    /// <summary>
    /// Attempt to recurse into the Pratt engine to match the next item. Returns true if the match
    /// succeeds, false otherwise.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool Match(IParseState<TInput> state)
    {
        EnsureIsNotComplete();
        EnsureRecursionIsPermitted();
        var result = _engine.TryParse(_state, _rbp, _parseControl);
        return result.Success;
    }

    /// <summary>
    /// Attempt to match the given parser. Returns true if the match succeeds, false otherwise.
    /// </summary>
    /// <param name="parser"></param>
    /// <returns></returns>
    public bool Match(IParser<TInput> parser)
    {
        Assert.ArgumentNotNull(parser);
        EnsureIsNotComplete();
        return parser.Match(_state);
    }

    /// <summary>
    /// Expect the given parser to match at the current position. Immediately abandons the user
    /// callback if it fails.
    /// </summary>
    /// <param name="parser"></param>
    /// <exception cref="ParseException">Thrown it the match fails to immediately abandon the user
    /// callback.</exception>
    public void Expect(IParser<TInput> parser)
    {
        Assert.ArgumentNotNull(parser);
        EnsureIsNotComplete();
        var result = parser.Match(_state);
        if (!result)
            throw new ParseException(ParseExceptionSeverity.Rule, $"Could not match parser {parser} at position {_state.Input.Consumed}", parser, _state.Input.CurrentLocation);
    }

    /// <summary>
    /// Fail the current parse rule, but allow the Pratt engine to continue other attempts at the
    /// same level.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="ParseException">Immediately abandons the rule.</exception>
    public void FailRule(string message = "")
        => throw new ParseException(ParseExceptionSeverity.Rule, message ?? "Fail", this, _state.Input.CurrentLocation);

    /// <summary>
    /// Fail the current parse level. The Pratt engine will end it's attempts at this level and
    /// move up to the next higher level of recursion.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="ParseException">Immediately abandons the level.</exception>
    public void FailLevel(string message = "")
        => throw new ParseException(ParseExceptionSeverity.Level, message ?? "", this, _state.Input.CurrentLocation);

    /// <summary>
    /// Fail the entire Pratt parse. The Pratt engine will immediately terminate and return
    /// a failure result.
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="ParseException">Immediately abandon the Pratt parse attempt.</exception>
    public void FailAll(string message = "")
        => throw new ParseException(ParseExceptionSeverity.Parser, message ?? "", this, _state.Input.CurrentLocation);

    /// <summary>
    /// Mark that the Pratt parse attempt is complete. The current result value will be returned
    /// as the success result value.
    /// </summary>
    public void Complete()
    {
        _parseControl.IsComplete = true;
    }

    /// <summary>
    /// Attempt to recurse into the Pratt engine to obtain another value at the same Right
    /// Binding Power. Returns a successful option on success, failure othewise.
    /// </summary>
    /// <returns></returns>
    public Option<TOutput> TryParse() => TryParse(_rbp);

    /// <summary>
    /// Attempt to recurse into the Pratt engine to obtain another value at the given Right
    /// Binding Power. Returns a successul option on success, failure otherwise.
    /// </summary>
    /// <param name="rbp"></param>
    /// <returns></returns>
    public Option<TOutput> TryParse(int rbp)
        => _canRecurse && !_parseControl.IsComplete
        ? _engine.TryParse(_state, rbp, _parseControl).ToOption()
        : default;

    /// <summary>
    /// Attempt to parse with the given parser. Returns a successful option on success, false
    /// otherwise.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="parser"></param>
    /// <returns></returns>
    public Option<TValue> TryParse<TValue>(IParser<TInput, TValue> parser)
    {
        Assert.ArgumentNotNull(parser);
        return _parseControl.IsComplete
            ? default
            : parser.Parse(_state).ToOption();
    }

    [ExcludeFromCodeCoverage]
    public IEnumerable<IParser> GetChildren() => [];

    [ExcludeFromCodeCoverage]
    public override string ToString() => DefaultStringifier.ToString(this);

    private void EnsureRecursionIsPermitted()
    {
        if (!_canRecurse)
            throw new ParseException(ParseExceptionSeverity.Rule, "The parser consumed zero input, so an attempt to recurse was denied to avoid an infinite loop.", this, _state.Input.CurrentLocation);
    }

    private void EnsureIsNotComplete()
    {
        if (_parseControl.IsComplete)
            throw new ParseException(ParseExceptionSeverity.Parser, "The parser is marked as being 'Complete' and cannot consume more inputs", this, _state.Input.CurrentLocation);
    }

    [ExcludeFromCodeCoverage]
    public INamed SetName(string name) => throw new InvalidOperationException("Cannot rename an internal parse context");

    [ExcludeFromCodeCoverage]
    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
            where TVisitor : IVisitor<TState>
        => throw new NotImplementedException();
}
