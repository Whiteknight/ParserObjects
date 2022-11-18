﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Pratt;
using ParserObjects.Regexes;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.C;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Regexes;

public static class RegexPatternGrammar
{
    private static readonly HashSet<char> _charsRequiringEscape = new HashSet<char> { '\\', '(', ')', '$', '|', '[', '.', '?', '+', '*', '{', '}' };

    public static IParser<char, Regex> CreateParser()
    {
        var digits = UnsignedInteger();

        // Literal match of any non-slash and non-control character
        var normalChar = Match(c => !_charsRequiringEscape.Contains(c) && !char.IsControl(c));

        var regex = Pratt<List<IState>>(config => config

            // Atoms. NUD atoms create a new List<State>. LED atoms append to the existing List<State>
            .Add(normalChar, p => p
                .Bind(1, (_, c) => State.AddMatch(null, c.Value))
                .BindLeft(1, (_, states, c) => State.AddMatch(states.Value, c.Value))
            )
            .Add(MatchChar('['), p => p
                .Bind(2, (ctx, _) => ParseCharacterClass(ctx, null))
                .BindLeft(2, (ctx, states, _) => ParseCharacterClass(ctx, states.Value))
            )
            .Add(MatchChar('.'), p => p
                .Bind(2, (_, _) => State.AddMatch(null, c => c != '\0', "Any"))
                .BindLeft(2, (_, states, _) => State.AddMatch(states.Value, c => c != '\0', "Any"))
            )
            .Add(MatchChar('\\'), p => p
                .Bind(2, (ctx, _) => State.AddSpecialMatch(null, ctx.Parse(Any())))
                .BindLeft(2, (ctx, states, _) => State.AddSpecialMatch(states.Value, ctx.Parse(Any())))
            )
            .Add(Match("(?:"), p => p
                .Bind(2, (ctx, _) =>
                {
                    var group = ctx.Parse(0);
                    ctx.Expect(MatchChar(')'));
                    return State.AddNonCapturingCloisterState(null, group);
                })
                .BindLeft(2, (ctx, states, _) =>
                {
                    var group = ctx.Parse(0);
                    ctx.Expect(MatchChar(')'));
                    return State.AddNonCapturingCloisterState(states.Value, group);
                })
            )
            .Add(MatchChar('('), p => p
                .Bind(2, (ctx, _) =>
                {
                    var group = ctx.Parse(0);
                    ctx.Expect(MatchChar(')'));
                    return State.AddCapturingGroupState(null, group);
                })
                .BindLeft(2, (ctx, states, _) =>
                {
                    var group = ctx.Parse(0);
                    ctx.Expect(MatchChar(')'));
                    return State.AddCapturingGroupState(states.Value, group);
                })
            )

            // Quantifiers
            .Add(MatchChar('{'), p => p
                .BindLeft(2, (ctx, states, _) => ParseRepetitionRange(ctx, states.Value, digits))
            )
            .Add(MatchChar('?'), p => p
                .BindLeft(2, (_, states, _) => State.SetPreviousQuantifier(states.Value, Quantifier.ZeroOrOne))
            )
            .Add(MatchChar('+'), p => p
                .BindLeft(2, (_, states, _) => State.SetPreviousStateRange(states.Value, 1, int.MaxValue))
            )
            .Add(MatchChar('*'), p => p
                .BindLeft(2, (_, states, _) => State.SetPreviousQuantifier(states.Value, Quantifier.ZeroOrMore))
            )

            // Alternation
            .Add(MatchChar('|'), p => p
                .BindLeft(3, (ctx, states, _) => ParseAlternation(ctx, states.Value))
            )

            // End Anchor
            .Add(MatchChar('$'), p => p
                .Bind(4, (_, _) => new List<IState> { State.EndAnchor })
                .BindLeft(4, (_, states, _) =>
                {
                    states.Value.Add(State.EndAnchor);
                    return states.Value;
                })
            )
        );

        return (regex, IsEnd()).Rule((r, _) => r)
            .Transform(r => new Regex(r))
            .Named("RegexPattern");
    }

    private static char GetUnescapedCharacter(IPrattParseContext<char> ctx, char c)
    {
        if (c != '\\')
            return c;

        if (ctx.Input.IsAtEnd)
            throw new RegexException("Expected character, found end");

        return ctx.Input.GetNext();
    }

    private static List<IState> ParseCharacterClass(IPrattParseContext<char> ctx, List<IState>? states)
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

            // TODO: There is a potential optimization here where, if we have subsequent characters
            // in a class like [abcde] we could replace with range [a-e]
            var range = ParseCharacterRange(ctx, c);
            ranges.Add(range);
        }

        if (ranges.Count == 0)
            throw new RegexException("Empty character class");

        return State.AddMatch(states, invertResult.Success, ranges);
    }

    private static (char low, char high) ParseCharacterRange(IPrattParseContext<char> ctx, char c)
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

        c = GetUnescapedCharacter(ctx, c);

        var high = c;
        if (high < low)
            throw new RegexException($"Invalid range {high}-{low} should be {low}-{high}");

        return (low, high);
    }

    private static List<IState> ParseRepetitionRange(IPrattParseContext<char> ctx, List<IState> states, IParser<char, int> digits)
    {
        if (states.Last() is EndAnchorState)
            throw new RegexException("Cannot quantify the end anchor $");

        int min = 0;
        var first = ctx.TryParse(digits);
        if (first.Success)
            min = first.Value;

        var comma = ctx.TryParse(MatchChar(','));
        if (!comma.Success)
        {
            ctx.Expect(MatchChar('}'));
            // No comma, so we must have {X} form
            if (!first.Success)
                throw new RegexException("Invalid range specifier. Must be one of {X} {X,} {,Y} or {X,Y}");
            return State.SetPreviousStateRange(states, min, min);
        }

        // At this point we might have {X,} {X,Y} or {,Y}
        // In any case, min is filled in now with either a value or 0
        var second = ctx.TryParse(digits);
        ctx.Expect(MatchChar('}'));
        return State.SetPreviousStateRange(states, min, second.Success ? second.Value : int.MaxValue);
    }

    // TODO: Move most of this logic into a static factory method on State.
    private static List<IState> ParseAlternation(IPrattParseContext<char, List<IState>> ctx, List<IState> states)
    {
        var options = new List<List<IState>>() { states };
        while (true)
        {
            var option = ctx.TryParse(0);
            if (!option.Success || option.Value.Count == 0)
                break;

            if (option.Value.Count == 1 && option.Value[0] is AlternationState alt)
                options.AddRange(alt.Alternations);
            else
                options.Add(option.Value);
        }

        if (options.Count == 1)
            return states;

        return new List<IState>
        {
            new AlternationState("alternation", options)
        };
    }
}
