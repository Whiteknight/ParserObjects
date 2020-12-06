﻿using System.Collections.Generic;
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

        public (bool success, TOutput value, string error) Parse(ParseState<TInput> state) => TryParse(state, 0);

        public (bool success, TOutput value, string error) TryParse(ParseState<TInput> state, int rbp)
        {
            var levelCp = state.Input.Checkpoint();
            try
            {
                return Parse(state, rbp);
            }
            catch (ParseException pe) when (pe.Severity == ParseExceptionSeverity.Level)
            {
                levelCp.Rewind();
                return (false, default, pe.Message ?? "Fail");
            }
        }

        // TODO: Need to test what happens if we have an end-of-input parselet added,
        // which will continue to succeed and consume zero input. Will probably break the
        // loop because it has the same binding power.

        private (bool success, TOutput value, string error) Parse(ParseState<TInput> state, int rbp)
        {
            var (hasLeft, leftToken, error) = GetLeft(state);
            if (!hasLeft)
                return (false, default, error);

            while (true)
            {
                var rightToken = GetRight(state, rbp, leftToken);
                if (rightToken == null)
                    break;

                // Set the next left value to be the current combined right value and continue
                // the loop
                leftToken = rightToken;
            }

            return (true, leftToken.Value, null);
        }

        private IToken<TOutput> GetRight(ParseState<TInput> state, int rbp, IToken<TOutput> leftToken)
        {
            var cp = state.Input.Checkpoint();
            foreach (var parselet in _ledableParselets)
            {
                var (success, token) = parselet.TryGetNext(state);
                if (!success)
                    continue;
                if (rbp >= token.LeftBindingPower)
                {
                    cp.Rewind();
                    continue;
                }

                var rightContext = new ParseContext<TInput, TOutput>(state, this, parselet.Rbp);

                // Transform the IToken into IToken<TOutput> using the LeftDenominator rule and
                // the current left value
                var (hasRight, rightToken) = token.LeftDenominator(rightContext, leftToken);
                if (!hasRight)
                {
                    cp.Rewind();
                    continue;
                }

                return rightToken;
            }

            return null;
        }

        private (bool success, IToken<TOutput> leftToken, string error) GetLeft(ParseState<TInput> state)
        {
            var cp = state.Input.Checkpoint();
            foreach (var parselet in _nudableParselets)
            {
                var (success, token) = parselet.TryGetNext(state);
                if (!success)
                    continue;

                var leftContext = new ParseContext<TInput, TOutput>(state, this, parselet.Rbp);

                // Transform the IToken into IToken<TInput> using the NullDenominator rule
                var (hasLeft, leftToken) = token.NullDenominator(leftContext);
                if (!hasLeft)
                {
                    cp.Rewind();
                    continue;
                }

                return (true, leftToken, null);
            }

            return (false, default, "No parselets matched and transformed at the current position.");
        }
    }
}