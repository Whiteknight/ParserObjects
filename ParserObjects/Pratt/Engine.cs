using System.Collections.Generic;
using System.Linq;

namespace ParserObjects.Pratt
{
    public class Engine<TInput, TOutput>
    {
        // See https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing.html
        // See https://eli.thegreenplace.net/2010/01/02/top-down-operator-precedence-parsing

        private readonly IReadOnlyList<IParselet<TInput, TOutput>> _nudableParselets;
        private readonly IReadOnlyList<IParselet<TInput, TOutput>> _ledableParselets;

        public Engine(IReadOnlyList<IParselet<TInput, TOutput>> parselets)
        {
            _nudableParselets = parselets.Where(p => p.CanNud).ToList();
            _ledableParselets = parselets.Where(p => p.CanLed).ToList();
        }

        public (bool success, TOutput value, string error, int consumed) Parse(ParseState<TInput> state) => TryParse(state, 0);

        public (bool success, TOutput value, string error, int consumed) TryParse(ParseState<TInput> state, int rbp)
        {
            var levelCp = state.Input.Checkpoint();
            try
            {
                return Parse(state, rbp);
            }
            catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Level)
            {
                levelCp.Rewind();
                return (false, default, pe.Message ?? "Fail", 0);
            }
        }

        // TODO: Need to test what happens if we have an end-of-input parselet added,
        // which will continue to succeed and consume zero input. Will probably break the
        // loop because it has the same binding power.

        private (bool success, TOutput value, string error, int consumed) Parse(ParseState<TInput> state, int rbp)
        {
            var (hasLeft, leftToken, error, consumed) = GetLeft(state);
            if (!hasLeft)
                return (false, default, error, 0);

            while (true)
            {
                var (hasRight, rightToken, rightConsumed) = GetRight(state, rbp, leftToken);
                if (!hasRight || rightToken == null)
                    break;

                // Set the next left value to be the current combined right value and continue
                // the loop
                consumed += rightConsumed;
                leftToken = rightToken;

                // If we have success, but did not consume any input, we will get into an infinite
                // loop if we don't break. One zero-length suffix rule is the maximum
                if (rightConsumed == 0)
                    break;
            }

            return (true, leftToken.Value, null, consumed);
        }

        private (bool success, IToken<TOutput> token, int consumed) GetRight(ParseState<TInput> state, int rbp, IToken<TOutput> leftToken)
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

                return (true, rightToken, consumed + rightContext.Consumed);
            }

            return default;
        }

        private (bool success, IToken<TOutput> leftToken, string error, int consumed) GetLeft(ParseState<TInput> state)
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
                    return (false, default, $"Parselet {parselet} consumed no input and would have caused infinite recursion", 0);

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
                return (true, leftToken, null, consumed);
            }

            return (false, default, "No parselets matched and transformed at the current position.", 0);
        }
    }
}
