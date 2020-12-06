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

        private (bool success, TOutput value, string error) Parse(ParseState<TInput> state, int rbp)
        {
            // Get the first "left" token. This token may have any type output by the parser
            var (hasLeftToken, leftToken, leftContext) = GetNextToken(state, _nudableParselets);
            if (!hasLeftToken)
                return (false, default, "Could not match any tokens");

            // Transform the IToken into IToken<TInput> using the NullDenominator rule
            var (hasLeft, left) = leftToken.NullDenominator(leftContext);
            if (!hasLeft)
                return (false, default, "Left Denominator failed");

            while (true)
            {
                var cp = state.Input.Checkpoint();

                // Now get the next "right" token. This token may have any type output by the
                // parser.
                var (hasRightToken, rightToken, rightContext) = GetNextToken(state, _ledableParselets);
                if (!hasRightToken || rbp >= rightToken.LeftBindingPower)
                {
                    cp.Rewind();
                    break;
                }

                // Transform the IToken into IToken<TOutput> using the LeftDenominator rule and
                // the current left value
                var (hasRight, right) = rightToken.LeftDenominator(rightContext, left);
                if (!hasRight)
                {
                    cp.Rewind();
                    break;
                }

                // Set the next left value to be the current combined right value and continue
                // the loop
                left = right;
            }

            return (true, left.Value, null);
        }

        private (bool, IToken<TInput, TOutput>, ParseContext<TInput, TOutput>) GetNextToken(ParseState<TInput> state, IEnumerable<IParselet<TInput, TOutput>> parselets)
        {
            foreach (var parselet in parselets)
            {
                var (success, token) = parselet.TryGetNext(state);
                if (success)
                    return (true, token, new ParseContext<TInput, TOutput>(state, this, parselet.Rbp));
            }

            // This is both the "no match" result. It will also be the "end of input" result
            // unless an explicit "end of input" parselet has been added.
            // TODO: Need to test what happens if we have an end-of-input parselet added,
            // which will continue to succeed and consume zero input. Will probably break the
            // loop because it has the same binding power.
            return (false, null, null);
        }
    }
}
