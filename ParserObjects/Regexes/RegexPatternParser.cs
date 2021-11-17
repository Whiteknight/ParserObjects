﻿using System.Collections.Generic;
using System.Linq;
using ParserObjects.Pratt;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects.Regexes
{
    public static class RegexPatternParser
    {
        private static readonly HashSet<char> _charsRequiringEscape = new HashSet<char> { '\\', '(', ')', '$', '|', '[', '.', '?', '+', '*', '{', '}' };

        public static IParser<char, Regex> Create()
        {
            var digits = CStyleParserMethods.UnsignedInteger();

            // Literal match of any non-slash and non-control character
            var normalChar = Match(c => !_charsRequiringEscape.Contains(c) && !char.IsControl(c));
            var any = Any();

            var regex = Pratt<List<State>>(config => config

                // Atoms
                .Add(normalChar, p => p
                    .ProduceRight(2, (_, c) => State.AddMatch(null, x => x == c.Value, $"Match {c}"))
                    .ProduceLeft(2, (_, states, c) => State.AddMatch(states.Value, x => x == c.Value, $"Match {c}"))
                )
                .Add(Match('['), p => p
                    .ProduceRight(2, (ctx, _) => ParseCharacterClass(ctx, null))
                    .ProduceLeft(2, (ctx, states, _) => ParseCharacterClass(ctx, states.Value))
                )
                .Add(Match('.'), p => p
                    .ProduceRight(2, (_, _) => State.AddMatch(null, c => c != '\0', "Any"))
                    .ProduceLeft(2, (_, states, _) => State.AddMatch(states.Value, c => c != '\0', "Any"))
                )
                .Add(Match('\\'), p => p
                    .ProduceRight(2, (ctx, _) => State.AddSpecialMatch(null, ctx.Parse(any)))
                    .ProduceLeft(2, (ctx, states, _) => State.AddSpecialMatch(states.Value, ctx.Parse(any)))
                )
                .Add(Match('('), p => p
                    .ProduceRight(2, (ctx, _) =>
                    {
                        var group = ctx.Parse(0);
                        ctx.Expect(Match(')'));
                        return State.AddGroupState(null, group);
                    })
                    .ProduceLeft(2, (ctx, states, _) =>
                    {
                        var group = ctx.Parse(0);
                        ctx.Expect(Match(')'));
                        return State.AddGroupState(states.Value, group);
                    })
                )

                // Quantifiers
                .Add(Match('{'), p => p
                    .ProduceLeft(2, (ctx, states, _) => ParseRepetitionRange(ctx, states.Value, digits))
                )
                .Add(Match('?'), p => p
                    .ProduceLeft(2, (_, states, _) => State.QuantifyPrevious(states.Value, Quantifier.ZeroOrOne))
                )
                .Add(Match('+'), p => p
                    .ProduceLeft(2, (_, states, _) => State.SetPreviousStateRange(states.Value, 1, int.MaxValue))
                )
                .Add(Match('*'), p => p
                    .ProduceLeft(2, (_, states, _) => State.QuantifyPrevious(states.Value, Quantifier.ZeroOrMore))
                )

                // Alternation
                .Add(Match('|'), p => p
                    .ProduceLeft(2, (ctx, states, _) => ParseAlternation(ctx, states.Value))
                )

                // End Anchor
                .Add(Match('$'), p => p
                    .ProduceLeft(4, (_, states, _) =>
                    {
                        states.Value.Add(State.EndOfInput);
                        return states.Value;
                    })
                )
            );

            var requiredEnd = If(End(), Produce(() => Utility.Defaults.ObjectInstance), Produce(ThrowEndOfPatternException));

            return (regex, requiredEnd).Produce((r, _) => r)
                .Transform(r => new Regex(r))
                .Named("RegexPattern");
        }

        private static char GetUnescapedCharacter(IParseContext<char> ctx, char c)
        {
            if (c != '\\')
                return c;

            if (ctx.Input.IsAtEnd)
                throw new RegexException("Expected character, found end");

            return ctx.Input.GetNext();
        }

        private static List<State> ParseCharacterClass(IParseContext<char> ctx, List<State>? states)
        {
            var invertResult = ctx.TryParse(Match('^'));
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

            var matcher = new CharacterMatcher(invertResult.Success, ranges);
            return State.AddMatch(states, c => matcher.IsMatch(c), "class");
        }

        private static (char low, char high) ParseCharacterRange(IParseContext<char> ctx, char c)
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

        private static object ThrowEndOfPatternException(ISequence<char> t, IDataStore data)
            => throw new RegexException("Expected end of pattern but found '" + t.GetNext());

        private static List<State> ParseRepetitionRange(IParseContext<char> ctx, List<State> states, IParser<char, int> digits)
        {
            if (states.Last().Type == StateType.EndOfInput)
                throw new RegexException("Cannot quantify the end anchor $");
            int min = 0;
            var first = ctx.TryParse(digits);
            if (first.Success)
                min = first.Value;
            var comma = ctx.TryParse(Match(','));
            if (!comma.Success)
            {
                ctx.Expect(Match('}'));
                // No comma, so we must have {X} form
                if (!first.Success)
                    throw new RegexException("Invalid range specifier. Must be one of {X} {X,} {,Y} or {X,Y}");
                return State.SetPreviousStateRange(states, min, min);
            }

            // At this point we might have X, X,Y or ,Y
            // In any case, min is filled in now with either a value or 0
            var second = ctx.TryParse(digits);
            ctx.Expect(Match('}'));
            return State.SetPreviousStateRange(states, min, second.Success ? second.Value : int.MaxValue);
        }

        private static List<State> ParseAlternation(IParseContext<char, List<State>> ctx, List<State> states)
        {
            var options = new List<List<State>>() { states };
            while (true)
            {
                var option = ctx.TryParse(0);
                if (!option.Success || option.Value.Count == 0)
                    break;
                options.Add(option.Value);
            }

            if (options.Count == 1)
                return states;

            return new List<State>
            {
                new State("alternation")
                {
                    Type = StateType.Alternation,
                    Alternations = options
                }
            };
        }
    }
}