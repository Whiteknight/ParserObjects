using System.Collections.Generic;
using ParserObjects.Internal.Visitors;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Parser to help with left-associative or left-recursive parse situations. Executes an
/// initial parser, and then passes that value to the right-hand-side production. The right
/// value is then used as the new left value and the loop repeats. Contains the parser and
/// related machinery.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class LeftApplyParser<TInput, TOutput> : IParser<TInput, TOutput>
{
    private readonly IParser<TInput, TOutput> _initial;
    private readonly Quantifier _quantifier;
    private readonly IParser<TInput, TOutput> _right;
    private readonly LeftValue<TInput, TOutput> _left;
    private readonly GetParserFromParser<TInput, TOutput> _getRight;

    public LeftApplyParser(IParser<TInput, TOutput> initial, GetParserFromParser<TInput, TOutput> getRight, Quantifier arity, string name = "")
    {
        Assert.ArgumentNotNull(initial);
        Assert.ArgumentNotNull(getRight);

        _initial = initial;
        _quantifier = arity;
        _left = new LeftValue<TInput, TOutput>(name);
        _right = getRight(_left);
        _getRight = getRight;
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public Result<TOutput> Parse(IParseState<TInput> state)
    {
        Assert.ArgumentNotNull(state);
        return _quantifier switch
        {
            Quantifier.ExactlyOne => ParseExactlyOne(state),
            Quantifier.ZeroOrOne => ParseZeroOrOne(state),
            Quantifier.ZeroOrMore => ParseZeroOrMore(state),
            _ => state.Fail(this, $"Quantifier value {_quantifier} not supported"),
        };
    }

    private Result<TOutput> ParseExactlyOne(IParseState<TInput> state)
    {
        // Parse the left. Parse the right exactly once. Return the result
        var checkpoint = state.Input.Checkpoint();

        var leftResult = _initial.Parse(state);
        if (!leftResult.Success)
            return leftResult;

        _left.Value = leftResult.Value;

        var rightResult = _right.Parse(state);
        if (rightResult.Success)
            return state.Success(this, rightResult.Value, leftResult.Consumed + rightResult.Consumed);

        checkpoint.Rewind();
        return state.Fail(this, "Expected exactly one right-hand side, but right parser failed: " + rightResult.ErrorMessage);
    }

    private Result<TOutput> ParseZeroOrMore(IParseState<TInput> state)
    {
        // Parse <left> then attempt to parse <right> in a loop. If <right> fails at any
        // point, return whatever is the last value we had
        var result = _initial.Parse(state);
        if (!result.Success)
            return result;

        var current = result.Value;
        _left.Value = result.Value;
        int consumed = result.Consumed;
        while (true)
        {
            var rhsResult = _right.Parse(state);
            if (!rhsResult.Success)
                return state.Success(this, current, consumed);

            consumed += rhsResult.Consumed;
            current = rhsResult.Value;
            _left.Value = current;
        }
    }

    private Result<TOutput> ParseZeroOrOne(IParseState<TInput> state)
    {
        // Parse the left. Maybe parse the right. If <right>, return it. Otherwise <left>
        var leftResult = _initial.Parse(state);
        if (!leftResult.Success)
            return leftResult;

        _left.Value = leftResult.Value;

        var rightResult = _right.Parse(state);
        if (!rightResult.Success)
            return leftResult;
        return state.Success(this, rightResult.Value, leftResult.Consumed + rightResult.Consumed);
    }

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public IEnumerable<IParser> GetChildren() => new IParser[] { _initial, _right };

    public override string ToString() => DefaultStringifier.ToString("LeftApply", Name, Id);

    public INamed SetName(string name) => new LeftApplyParser<TInput, TOutput>(_initial, _getRight, _quantifier, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IAssociativePartialVisitor<TState>>()?.Accept(this, state);
    }
}
