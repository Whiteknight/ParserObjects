using System;
using System.Collections.Generic;
using ParserObjects.Internal.Visitors;
using static ParserObjects.Internal.Assert;

namespace ParserObjects.Internal.Parsers;

/// <summary>
/// Attempts to parse a right-recursive or right-associative parse rule. Useful for limited
/// situations, especially for parsing expressions.
/// </summary>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TMiddle"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public sealed class RightApplyParser<TInput, TMiddle, TOutput> : IParser<TInput, TOutput>
{
    private readonly IParser<TInput, TOutput> _item;
    private readonly IParser<TInput, TMiddle> _middle;
    private readonly Func<RightApplyArguments<TOutput, TMiddle>, TOutput> _produce;
    private readonly Func<IParseState<TInput>, TOutput>? _getMissingRight;
    private readonly Quantifier _quantifier;

    // <item> (<middle> <item>)* with right-associativity in the production method

    public RightApplyParser(
        IParser<TInput, TOutput> item,
        IParser<TInput, TMiddle> middle,
        Func<RightApplyArguments<TOutput, TMiddle>, TOutput> produce,
        Quantifier quantifier,
        Func<IParseState<TInput>, TOutput>? getMissingRight = null,
        string name = "")
    {
        _item = NotNull(item);
        _middle = NotNull(middle);
        _produce = NotNull(produce);
        _getMissingRight = getMissingRight;
        _quantifier = quantifier;
        Name = name;
    }

    public int Id { get; } = UniqueIntegerGenerator.GetNext();

    public string Name { get; }

    public Result<TOutput> Parse(IParseState<TInput> state)
    {
        // Exactly one parses either <left> or <left> <middle> <right>. We only need the
        // start checkpoint in this case
        if (_quantifier == Quantifier.ExactlyOne)
        {
            var start = NotNull(state).Input.Checkpoint();
            var lr = _item.Parse(state);
            if (!lr.Success)
                return lr;
            return ParseExactlyOne(state, lr, start);
        }

        // In other cases, the term "Zero" means "maybe match <left> by itself"
        // If <left> fails, the parser fails.
        // If <left> succeeds, this is "Zero" and we continue from there
        // Every reduction of <left> <middle> <right> is considered one iteration.
        var leftResult = _item.Parse(state);
        if (!leftResult.Success)
            return leftResult;

        return _quantifier switch
        {
            Quantifier.ZeroOrOne => ParseZeroOrOne(state, leftResult),
            Quantifier.ZeroOrMore => ParseZeroOrMore(state, leftResult),
            _ => throw new InvalidOperationException("Unsupported quantifier"),
        };
    }

    private Result<TOutput> ParseZeroOrMore(IParseState<TInput> state, Result<TOutput> leftResult)
    {
        static Result<TOutput> ProduceSuccess(IParser self, Func<RightApplyArguments<TOutput, TMiddle>, TOutput> produce, Stack<(TOutput Left, TMiddle Middle)> resultStack, TOutput right, int consumed)
        {
            while (resultStack.Count > 0)
            {
                var (left, middle) = resultStack.Pop();
                var args = new RightApplyArguments<TOutput, TMiddle>(left, middle, right);
                right = produce(args);
            }

            return Result.Ok(self, right, consumed);
        }

        var left = leftResult.Value;
        int consumed = leftResult.Consumed;

        var resultStack = new Stack<(TOutput Left, TMiddle Middle)>();

        while (true)
        {
            var checkpoint = state.Input.Checkpoint();

            // We have the left, so parse the middle. If not found, just return left
            var middleResult = _middle.Parse(state);
            if (!middleResult.Success)
                return ProduceSuccess(this, _produce, resultStack, left, consumed);

            int rightConsumed = middleResult.Consumed;

            // We have <left> <middle>, now we have to look for <right>.
            // if we have it, push state, set right as the new left, and repeat loop
            var rightResult = _item.Parse(state);
            if (rightResult.Success)
            {
                // Add left and middle to the stack, and we'll loop again with the left
                rightConsumed += rightResult.Consumed;
                consumed += rightConsumed;
                resultStack.Push((left, middleResult.Value));
                left = rightResult.Value;
                if (rightConsumed == 0)
                    return ProduceSuccess(this, _produce, resultStack, rightResult.Value, consumed);
                continue;
            }

            // We have <left> <middle> but no <right>. See if we can make a synthetic one
            if (_getMissingRight != null)
            {
                // create a synthetic right item and short-circuit exit (we could go around
                // again, fail the <middle> and exit at that point, but this is faster and
                // doesn't require allocating a new Result<T>
                consumed += middleResult.Consumed;
                var syntheticRight = _getMissingRight(state);
                resultStack.Push((left, middleResult.Value));
                return ProduceSuccess(this, _produce, resultStack, syntheticRight, consumed);
            }

            // We can't make a synthetic right, so rewind to give back the <middle>
            checkpoint.Rewind();
            return ProduceSuccess(this, _produce, resultStack, left, consumed);
        }
    }

    // Parse zero or one reductions of the form <left> <middle> <right>.
    // Zero reductions of that form is just <left>
    // So a better name for this might be "ParseLeftOnlyOrLeftMiddleRight" but that feels verbose.
    private Result<TOutput> ParseZeroOrOne(IParseState<TInput> state, Result<TOutput> leftResult)
    {
        var checkpoint = state.Input.Checkpoint();
        var middleResult = _middle.Parse(state);
        if (!middleResult.Success)
            return leftResult;

        var rightResult = _item.Parse(state);
        if (rightResult.Success)
        {
            var args = new RightApplyArguments<TOutput, TMiddle>(leftResult.Value, middleResult.Value, rightResult.Value);
            var result = _produce(args);
            return Result.Ok(this, result, leftResult.Consumed + middleResult.Consumed + rightResult.Consumed);
        }

        checkpoint.Rewind();

        // We have <left> <middle> but no <right>. See if we can make a synthetic one
        if (_getMissingRight != null)
        {
            // create a synthetic right item and short-circuit exit (we could go around
            // again, fail the <middle> and exit at that point, but this is faster and
            // doesn't require allocating a new Result<T>
            var syntheticRight = _getMissingRight(state);
            var args = new RightApplyArguments<TOutput, TMiddle>(leftResult.Value, middleResult.Value, syntheticRight);
            var result = _produce(args);
            return Result.Ok(this, result, leftResult.Consumed + middleResult.Consumed);
        }

        return leftResult;
    }

    // Parse exactly one reduction of the form <left> <middle> <right>.
    // If we only have <left>, we do not have a full reduction of this form and call it a failure
    // Rewind back to the point before the <left> matches in that case.
    private Result<TOutput> ParseExactlyOne(IParseState<TInput> state, Result<TOutput> leftResult, SequenceCheckpoint startCp)
    {
        var middleCp = state.Input.Checkpoint();
        var middleResult = _middle.Parse(state);
        if (!middleResult.Success)
            return Result.Fail(this, "Expected exactly one production but found zero");

        var rightResult = _item.Parse(state);
        if (rightResult.Success)
        {
            var args = new RightApplyArguments<TOutput, TMiddle>(leftResult.Value, middleResult.Value, rightResult.Value);
            var result = _produce(args);
            return Result.Ok(this, result, leftResult.Consumed + middleResult.Consumed + rightResult.Consumed);
        }

        // We have <left> <middle> but no <right>. See if we can make a synthetic one
        if (_getMissingRight != null)
        {
            // create a synthetic right item and short-circuit exit (we could go around
            // again, fail the <middle> and exit at that point, but this is faster and
            // doesn't require allocating a new Result<T>
            middleCp.Rewind();
            var syntheticRight = _getMissingRight(state);
            var args = new RightApplyArguments<TOutput, TMiddle>(leftResult.Value, middleResult.Value, syntheticRight);
            var result = _produce(args);
            return Result.Ok(this, result, leftResult.Consumed + middleResult.Consumed);
        }

        startCp.Rewind();
        return Result.Fail(this, "Expected exactly one production but found zero");
    }

    Result<object> IParser<TInput>.Parse(IParseState<TInput> state) => Parse(state).AsObject();

    public bool Match(IParseState<TInput> state) => Parse(state).Success;

    public IEnumerable<IParser> GetChildren() => [_item, _middle];

    public override string ToString() => DefaultStringifier.ToString("RightApply", Name, Id);

    public INamed SetName(string name)
        => new RightApplyParser<TInput, TMiddle, TOutput>(_item, _middle, _produce, _quantifier, _getMissingRight, name);

    public void Visit<TVisitor, TState>(TVisitor visitor, TState state)
        where TVisitor : IVisitor<TState>
    {
        visitor.Get<IAssociativePartialVisitor<TState>>()?.Accept(this, state);
    }
}
