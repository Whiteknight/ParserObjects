using System.Collections.Generic;
using ParserObjects.Internal.Regexes.States;
using ParserObjects.Pratt;
using ParserObjects.Regexes;
using static ParserObjects.Parsers<char>;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers.C;

namespace ParserObjects.Internal.Regexes.Patterns;

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

    private static readonly IParser<char, string> _parserNames = MatchChars(c => c != '}');
    private static readonly IParser<char, char> _normalChar = Match(c => !_charsRequiringEscape.Contains(c) && !char.IsControl(c));

    public static IParser<char, Regex> CreateParser()
    {
        // Literal match of any non-slash and non-control character
        var regex = Pratt<StateList>(config => config

            // Alternation
            .Add(MatchChar('|'), static p => p
                .BindLeft(_bpAlt, static (ctx, states, _) => ParseAlternation(ctx, states.Value))
            )

            // Atoms. NUD atoms create a new List<State>. LED atoms append to the existing List<State>
            .Add(_normalChar, static p => p
                .Bind(_bpAtom, static (_, c) => StateList.Create().AddMatch(c.Value))
                .BindLeft(_bpAtom, static (_, states, c) => states.Value.AddMatch(c.Value))
            )
            .Add(MatchChar('['), static p => p
                .Bind(_bpAtom, static (ctx, _) => ParseCharacterClass(ctx, StateList.Create()))
                .BindLeft(_bpAtom, static (ctx, states, _) => ParseCharacterClass(ctx, states.Value))
            )
            .Add(MatchChar('.'), static p => p
                .Bind(_bpAtom, static (_, _) => StateList.Create().AddMatch(static c => c != '\0', "Any"))
                .BindLeft(_bpAtom, static (_, states, _) => states.Value.AddMatch(static c => c != '\0', "Any"))
            )
            .Add(MatchChar('\\'), static p => p
                .Bind(_bpAtom, static (ctx, _) => StateList.Create().AddSpecialMatch(ctx.Parse(Any())))
                .BindLeft(_bpAtom, (ctx, states, _) => states.Value.AddSpecialMatch(ctx.Parse(Any())))
            )

            // non-capturing group
            .Add(MatchChars("(?:"), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return StateList.Create().AddNonCapturingCloisterState(group);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return states.Value.AddNonCapturingCloisterState(group);
                })
            )
            // zero-length positive lookahead
            .Add(MatchChars("(?="), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return StateList.Create().AddLookaheadState(true, group);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return states.Value.AddLookaheadState(true, group);
                })
            )
            // zero-length negative lookahead
            .Add(MatchChars("(?!"), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return StateList.Create().AddLookaheadState(false, group);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return states.Value.AddLookaheadState(false, group);
                })
            )
            // Recurse into IParser
            .Add(MatchChars("(?{"), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var name = ctx.Parse(_parserNames);
                    ctx.Expect(MatchChar('}'));
                    ctx.Expect(MatchChar(')'));
                    var parser = ctx.Data.Get<IParser<char>>(name).GetValueOrDefault(Empty());
                    return StateList.Create().AddParserRecurse(parser);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var name = ctx.Parse(_parserNames);
                    ctx.Expect(MatchChar('}'));
                    ctx.Expect(MatchChar(')'));
                    var parser = ctx.Data.Get<IParser<char>>(name).GetValueOrDefault(Empty());
                    return states.Value.AddParserRecurse(parser);
                })
            )
            // normal parens, capturing group
            .Add(MatchChar('('), static p => p
                .Bind(_bpAtom, static (ctx, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return StateList.Create().AddCapturingGroupState(group);
                })
                .BindLeft(_bpAtom, static (ctx, states, _) =>
                {
                    var group = ctx.Parse(_bpAll);
                    ctx.Expect(MatchChar(')'));
                    return states.Value.AddCapturingGroupState(group);
                })
            )

            // Quantifiers
            .Add(MatchChar('{'), static p => p
                .BindLeft(_bpQuant, static (ctx, states, _) => ParseRepetitionRange(ctx, states.Value, UnsignedInteger()))
            )
            .Add(MatchChar('?'), static p => p
                .BindLeft(_bpQuant, static (_, states, _) => states.Value.SetPreviousQuantifier(Quantifier.ZeroOrOne))
            )
            .Add(MatchChar('+'), static p => p
                .BindLeft(_bpQuant, static (_, states, _) => states.Value.SetPreviousStateRange(1, int.MaxValue))
            )
            .Add(MatchChar('*'), static p => p
                .BindLeft(_bpQuant, static (_, states, _) => states.Value.SetPreviousQuantifier(Quantifier.ZeroOrMore))
            )

            // End Anchor
            .Add(MatchChar('$'), static p => p
                .Bind(_bpAnchor, static (_, _) => StateList.Create().AddEndAnchor())
                .BindLeft(_bpAnchor, static (_, states, _) => states.Value.AddEndAnchor())
            )
        );

        return (regex, End()).Rule(static (r, _) => r)
            .Transform(static r => new Regex(r.States));
    }

    private static char GetUnescapedCharacter(PrattParseContext<char, StateList> ctx, char c)
    {
        if (c != '\\')
            return c;

        if (ctx.Input.IsAtEnd)
            throw new RegexException("Expected character, found end");

        return ctx.Input.GetNext();
    }

    private static StateList ParseCharacterClass(PrattParseContext<char, StateList> ctx, StateList states)
    {
        var invertResult = ctx.TryParse(MatchChar('^')).Success;
        var ranges = new CharRanges(null, null);

        while (ctx.Input.Peek() != ']')
        {
            if (ctx.Input.IsAtEnd)
                throw new RegexException("Incomplete character class");

            var c = GetUnescapedCharacter(ctx, ctx.Input.GetNext());

            var (low, high) = ParseCharacterRange(ctx, c);
            ranges = ranges.Add(low, high);
        }

        // Clear the ']'
        ctx.Input.GetNext();
        if (!ranges.HasAny)
            throw new RegexException("Empty character class");

        return states.AddMatch(invertResult, ranges);
    }

    private static (char low, char high) ParseCharacterRange(PrattParseContext<char, StateList> ctx, char c)
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
        return (low, high);
    }

    private static StateList ParseRepetitionRange(PrattParseContext<char, StateList> ctx, StateList states, IParser<char, uint> digits)
    {
        states.VerifyPreviousStateIsNotEndAnchor();

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
            return states.SetPreviousStateRange(min, min);
        }

        // At this point we might have X, X,Y or ,Y
        // In any case, min is filled in now with either a value or 0
        var second = ctx.TryParse(digits);
        ctx.Expect(MatchChar('}'));
        return states.SetPreviousStateRange(min, second.Success ? second.Value : int.MaxValue);
    }

    private static StateList ParseAlternation(PrattParseContext<char, StateList> ctx, StateList states)
    {
        var options = new List<List<IState>>() { states.States! };
        do
        {
            var option = ctx.TryParse(_bpAlt);
            if (!option.Success || option.Value.Count == 0)
                break;

            options.Add(option.Value.States);
        }
        while (ctx.Match(MatchChar('|')));

        return new StateList(options.Count == 1
            ? states.States!
            : new List<IState> { new AlternationState(options) });
    }
}
