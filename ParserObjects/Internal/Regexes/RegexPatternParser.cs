using System.Collections.Generic;
using ParserObjects.Pratt;
using ParserObjects.Regexes;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.C;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Regexes;

public static class RegexPatternGrammar
{
    private const int _bpAll = 0;
    private const int _bpAlt = 1;
    private const int _bpAtom = 10;
    private const int _bpQuant = 20;
    private const int _bpAnchor = 30;

    private static readonly HashSet<char> _charsRequiringEscape = new HashSet<char>
    {
        '\\',
        '(', ')',
        '$', '|',
        '[',
        '.', '?', '+', '*',
        '{', '}'
    };

    public static IParser<char, Regex> CreateParser()
    {
        // Literal match of any non-slash and non-control character
        var normalChar = Match(c => !_charsRequiringEscape.Contains(c) && !char.IsControl(c));

        var regex = Pratt<List<IState>>(config => config

            // Alternation
            .Add(MatchChar('|'), static p => p
                .BindLeft(_bpAlt, static (ctx, states, _) => ParseAlternation(ctx, states.Value))
            )

            // Atoms. NUD atoms create a new List<State>. LED atoms append to the existing List<State>
            .Add(normalChar, static p => p
                .Bind(_bpAtom, static (_, c) => State.AddMatch(null, c.Value))
                .BindLeft(_bpAtom, static (_, states, c) => State.AddMatch(states.Value, c.Value))
            )
            .Add(MatchChar('['), static p => p
                .Bind(_bpAtom, static (ctx, _) => ParseCharacterClass(ctx, null))
                .BindLeft(_bpAtom, static (ctx, states, _) => ParseCharacterClass(ctx, states.Value))
            )
            .Add(MatchChar('.'), static p => p
                .Bind(_bpAtom, static (_, _) => State.AddMatch(null, static c => c != '\0', "Any"))
                .BindLeft(_bpAtom, static (_, states, _) => State.AddMatch(states.Value, static c => c != '\0', "Any"))
            )
            .Add(MatchChar('\\'), static p => p
                .Bind(_bpAtom, static (ctx, _) => State.AddSpecialMatch(null, ctx.Parse(Any())))
                .BindLeft(_bpAtom, (ctx, states, _) => State.AddSpecialMatch(states.Value, ctx.Parse(Any())))
            )

            // non-capturing group
            .Add(MatchChars("(?:"), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return State.AddNonCapturingCloisterState(null, group);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return State.AddNonCapturingCloisterState(states.Value, group);
                })
            )
            // zero-length positive lookahead
            .Add(MatchChars("(?="), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return State.AddLookaheadState(null, true, group);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return State.AddLookaheadState(states.Value, true, group);
                })
            )
            // zero-length negative lookahead
            .Add(MatchChars("(?!"), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return State.AddLookaheadState(null, false, group);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return State.AddLookaheadState(states.Value, false, group);
                })
            )
            // Recurse into IParser
            .Add(MatchChars("(?p"), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var name = ctx.Parse(StrippedString());
                    ctx.Expect(MatchChar(')'));
                    var parser = ctx.Data.Get<IParser<char>>(name).GetValueOrDefault(Empty());
                    return State.AddParserRecurse(null, parser);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var name = ctx.Parse(StrippedString());
                    ctx.Expect(MatchChar(')'));
                    var parser = ctx.Data.Get<IParser<char>>(name).GetValueOrDefault(Empty());
                    return State.AddParserRecurse(states.Value, parser);
                })
            )
            // normal parens, capturing group
            .Add(MatchChar('('), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return State.AddCapturingGroupState(null, group);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return State.AddCapturingGroupState(states.Value, group);
                })
            )

            // Quantifiers
            .Add(MatchChar('{'), static p => p
                .BindLeft(_bpQuant, static (ctx, states, _) => ParseRepetitionRange(ctx, states.Value, UnsignedInteger()))
            )
            .Add(MatchChar('?'), static p => p
                .BindLeft(_bpQuant, static (_, states, _) => State.SetPreviousQuantifier(states.Value, Quantifier.ZeroOrOne))
            )
            .Add(MatchChar('+'), static p => p
                .BindLeft(_bpQuant, static (_, states, _) => State.SetPreviousStateRange(states.Value, 1, int.MaxValue))
            )
            .Add(MatchChar('*'), static p => p
                .BindLeft(_bpQuant, static (_, states, _) => State.SetPreviousQuantifier(states.Value, Quantifier.ZeroOrMore))
            )

            // End Anchor
            .Add(MatchChar('$'), static p => p
                .Bind(_bpAnchor, static (_, _) => new List<IState> { State.EndAnchor })
                .BindLeft(_bpAnchor, static (_, states, _) =>
                {
                    states.Value.Add(State.EndAnchor);
                    return states.Value;
                })
            )
        );

        return (regex, End()).Rule(static (r, _) => r)
            .Transform(static r => new Regex(r));
    }

    private static char GetUnescapedCharacter(PrattParseContext<char, List<IState>> ctx, char c)
    {
        if (c != '\\')
            return c;

        if (ctx.Input.IsAtEnd)
            throw new RegexException("Expected character, found end");

        return ctx.Input.GetNext();
    }

    private static List<IState> ParseCharacterClass(PrattParseContext<char, List<IState>> ctx, List<IState>? states)
    {
        var invertResult = ctx.TryParse(MatchChar('^'));
        var ranges = new List<(char low, char high)>();

        while (true)
        {
            if (ctx.Input.IsAtEnd)
                throw new RegexException("Incomplete character class");

            var c = ctx.Input.GetNext();
            if (c == ']')
                break;

            c = GetUnescapedCharacter(ctx, c);

            var range = ParseCharacterRange(ctx, c);
            ranges.Add(range);
        }

        if (ranges.Count == 0)
            throw new RegexException("Empty character class");

        return State.AddMatch(states, invertResult.Success, ranges);
    }

    private static (char low, char high) ParseCharacterRange(PrattParseContext<char, List<IState>> ctx, char c)
    {
        var low = c;
        var next = ctx.Input.Peek();
        if (next != '-')
            return (low, low);

        // We're keeping the peek'd char (dash) so advance input and keep going
        ctx.Input.GetNext();
        c = ctx.Input.GetNext();
        if (c == ']')
            throw new RegexException("Unexpected ] after -. Expected end of range. Did you mean '\\]'?");

        var high = GetUnescapedCharacter(ctx, c);
        return high < low
            ? throw new RegexException($"Invalid range {high}-{low} should be {low}-{high}")
            : (low, high);
    }

    private static List<IState> ParseRepetitionRange(PrattParseContext<char, List<IState>> ctx, List<IState> states, IParser<char, uint> digits)
    {
        if (states[^1] is EndAnchorState)
            throw new RegexException("Cannot quantify the end anchor $");

        uint min = 0;
        var first = ctx.TryParse(digits);
        if (first.Success)
            min = first.Value;

        var comma = ctx.TryParse(MatchChar(','));
        if (!comma.Success)
        {
            // No comma, so we must have {X} form
            if (!first.Success)
                throw new RegexException("Invalid range specifier. Must be one of {X} {X,} {,Y} or {X,Y}");
            ctx.Expect(MatchChar('}'));
            return State.SetPreviousStateRange(states, min, min);
        }

        // At this point we might have X, X,Y or ,Y
        // In any case, min is filled in now with either a value or 0
        var second = ctx.TryParse(digits);
        ctx.Expect(MatchChar('}'));
        return State.SetPreviousStateRange(states, min, second.Success ? second.Value : int.MaxValue);
    }

    private static List<IState> ParseAlternation(PrattParseContext<char, List<IState>> ctx, List<IState> states)
    {
        var options = new List<List<IState>>() { states };
        do
        {
            var option = ctx.TryParse(_bpAlt);
            if (!option.Success || option.Value.Count == 0)
                break;

            options.Add(option.Value);
        }
        while (ctx.Match(MatchChar('|')));

        return options.Count == 1
            ? states
            : new List<IState> { new AlternationState("alternation", options) };
    }
}
