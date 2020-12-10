using System.Collections.Generic;
using System.Linq;
using ParserObjects.Utility;

namespace ParserObjects.Pratt
{
    // Implementation of the Pratt algorithm. For internal use only, use PrattParser instead.
    public class Engine<TInput, TOutput>
    {
        // See https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html
        // See https://eli.thegreenplace.net/2010/01/02/top-down-operator-precedence-parsing

        private readonly IReadOnlyList<IParselet<TInput, TOutput>> _nudableParselets;
        private readonly IReadOnlyList<IParselet<TInput, TOutput>> _ledableParselets;

        public Engine(IReadOnlyList<IParselet<TInput, TOutput>> parselets)
        {
            Assert.ArrayNotNullAndContainsNoNulls(parselets, nameof(parselets));
            _nudableParselets = parselets.Where(p => p.CanNud).ToList();
            _ledableParselets = parselets.Where(p => p.CanLed).ToList();
        }

        public PartialResult<TOutput> Parse(ParseState<TInput> state) => TryParse(state, 0);

        public PartialResult<TOutput> TryParse(ParseState<TInput> state, int rbp)
        {
            Assert.ArgumentNotNull(state, nameof(state));
            var levelCp = state.Input.Checkpoint();
            try
            {
                return Parse(state, rbp);
            }
            catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Level)
            {
                levelCp.Rewind();
                return default;
            }
        }

        // TODO: Need to test what happens if we have an end-of-input parselet added,
        // which will continue to succeed and consume zero input. Will probably break the
        // loop because it has the same binding power.

        private PartialResult<TOutput> Parse(ParseState<TInput> state, int rbp)
        {
            var startLocation = state.Input.CurrentLocation;
            var leftResult = GetLeft(state);
            if (!leftResult.Success)
                return default;
            var leftToken = leftResult.Value;
            int consumed = leftResult.Consumed;

            while (true)
            {
                var rightResult = GetRight(state, rbp, leftToken);
                if (!rightResult.Success || rightResult.Value == null)
                    break;

                // Set the next left value to be the current combined right value and continue
                // the loop
                consumed += rightResult.Consumed;
                leftToken = rightResult.Value;

                // If we have success, but did not consume any input, we will get into an infinite
                // loop if we don't break. One zero-length suffix rule is the maximum
                if (rightResult.Consumed == 0)
                    break;
            }

            return PartialResult<TOutput>.Succeed(leftToken.Value, consumed, startLocation);
        }

        private PartialResult<IToken<TOutput>> GetRight(ParseState<TInput> state, int rbp, IToken<TOutput> leftToken)
        {
            var cp = state.Input.Checkpoint();
            foreach (var parselet in _ledableParselets)
            {
                var (success, token, consumed) = parselet.TryGetNext(state);
                if (!success)
                    continue;

                if (rbp >= token.LeftBindingPower)
                {
                    cp.Rewind();
                    continue;
                }

                var rightContext = new ParseContext<TInput, TOutput>(state, this, parselet.Rbp)
                {
                    Name = parselet.Name
                };

                // Transform the IToken into IToken<TOutput> using the LeftDenominator rule and
                // the current left value
                var (hasRight, rightToken) = token.LeftDenominator(rightContext, leftToken);
                if (!hasRight)
                {
                    cp.Rewind();
                    continue;
                }

                return PartialResult<IToken<TOutput>>.Succeed(rightToken, consumed + rightContext.Consumed);
            }

            return default;
        }

        private PartialResult<IToken<TOutput>> GetLeft(ParseState<TInput> state)
        {
            var cp = state.Input.Checkpoint();
            foreach (var parselet in _nudableParselets)
            {
                var (success, token, consumed) = parselet.TryGetNext(state);
                if (!success)
                    continue;

                // get-left rules which succeed but consume zero input are treated as errors. This
                // would lead to infinite left recursion with no obvious programmatic ways to
                // prevent it.
                if (consumed == 0)
                    return PartialResult<IToken<TOutput>>.Fail($"Parselet {parselet} consumed no input and would have caused infinite recursion");

                var leftContext = new ParseContext<TInput, TOutput>(state, this, parselet.Rbp)
                {
                    Name = parselet.Name
                };

                // Transform the IToken into IToken<TInput> using the NullDenominator rule
                var (hasLeft, leftToken) = token.NullDenominator(leftContext);
                if (!hasLeft)
                {
                    cp.Rewind();
                    continue;
                }

                consumed += leftContext.Consumed;
                return PartialResult<IToken<TOutput>>.Succeed(leftToken, consumed, null);
            }

            return PartialResult<IToken<TOutput>>.Fail("No parselets matched and transformed at the current position.");
        }
    }
}
