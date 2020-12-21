using System;
using System.Collections.Generic;
using System.Linq;
using ParserObjects.Parsers;
using ParserObjects.Pratt;
using ParserObjects.Regexes;
using static ParserObjects.ParserMethods<char>;

namespace ParserObjects
{
    public static partial class ParserMethods
    {
        /// <summary>
        /// Creates a parser which attempts to match the given regular expression from the current
        /// position of the input stream.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static IParser<char, string> Regex(string pattern)
        {
            var regexParser = RegexPattern();
            var result = regexParser.Parse(pattern);
            if (!result.Success)
                throw new RegexException("Could not parse pattern " + pattern);

            return new RegexParser(result.Value, pattern);
        }

        /// <summary>
        /// Creates a parser which parses a regex pattern string into a Regex object for work with
        /// the RegexParser and RegexEngine.
        /// </summary>
        /// <returns></returns>
        public static IParser<char, Regex> RegexPattern() => _regexPattern.Value;

        private static readonly Lazy<IParser<char, Regex>> _regexPattern = new Lazy<IParser<char, Regex>>(GetRegexPatternParser);
        private static readonly HashSet<char> _charsRequiringEscape = new HashSet<char> { '\\', '(', ')', '$', '|', '[', '.', '?', '+', '*', '{', '}' };

        private static IParser<char, Regex> GetRegexPatternParser()
        {
            var digits = CStyleParserMethods.UnsignedInteger();

            // Literal match of any non-slash and non-control character
            var normalChar = Match(c => !_charsRequiringEscape.Contains(c) && !char.IsControl(c));

            var regex = Pratt<List<State>>(config => config

                // Atoms
                .Add(normalChar, p => p
                    .ProduceRight(2, (_, c) => State.AddMatch(null, x => x == c.Value, $"Match {c}"))
                    .ProduceLeft(2, (_, states, c) => State.AddMatch(states.Value, x => x == c.Value, $"Match {c}"))
                )
                .Add(GetCharacterClassParser(), p => p
                    .ProduceRight(2, (_, matcher) => State.AddMatch(null, c => matcher.Value.IsMatch(c), "class"))
                    .ProduceLeft(2, (_, states, matcher) => State.AddMatch(states.Value, c => matcher.Value.IsMatch(c), "class"))
                )
                .Add(Match('.'), p => p
                    .ProduceRight(2, (_, _) => State.AddMatch(null, c => c != '\0', "Any"))
                    .ProduceLeft(2, (_, states, _) => State.AddMatch(states.Value, c => c != '\0', "Any"))
                )
                .Add(Match('\\'), p => p
                    .ProduceRight(2, (ctx, _) => State.AddSpecialMatch(null, ctx.Parse(Any())))
                    .ProduceLeft(2, (ctx, states, _) => State.AddSpecialMatch(states.Value, ctx.Parse(Any())))
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
                    .ProduceLeft(2, (ctx, states, _) => ParseRange(ctx, states.Value, digits))
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

        private static IParser<char, CharacterMatcher> GetCharacterClassParser()
        {
            return Sequential(state =>
            {
                state.Parse(Match('['));
                var invertResult = state.TryParse(Match('^'));
                var ranges = new List<(char low, char high)>();
                while (true)
                {
                    var c = state.Input.GetNext();
                    if (c == ']')
                        break;
                    if (c == '\\')
                        c = state.Input.GetNext();
                    var low = c;
                    var next = state.Input.Peek();
                    if (next != '-')
                    {
                        ranges.Add((low, low));
                        continue;
                    }

                    // We're keeping the peek'd char (dash) so advance input and keep going
                    state.Input.GetNext();
                    c = state.Input.GetNext();
                    if (c == ']')
                        throw new RegexException("Unexpected ] after -. Expected end of range. Did you mean '\\]'?");
                    if (c == '\\')
                        c = state.Input.GetNext();
                    var high = c;

                    if (high < low)
                        throw new RegexException($"Invalid range {high}-{low} should be {low}-{high}");

                    ranges.Add((low, high));
                }

                return new CharacterMatcher(invertResult.Success, ranges);
            });
        }

        private static object ThrowEndOfPatternException(ISequence<char> t, IDataStore data)
            => throw new RegexException("Expected end of pattern but found '" + t.GetNext());

        private static List<State> ParseRange(IParseContext<char> ctx, List<State> states, IParser<char, int> digits)
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
