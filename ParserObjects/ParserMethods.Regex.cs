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
        private static readonly HashSet<char> _classCharsRequiringEscape = new HashSet<char> { '\\', '^', ']', '-' };

        private static IParser<char, Regex> GetRegexPatternParser()
        {
            var openBracket = Match('[');
            var any = Any();
            var peek = Peek();

            var characterClass = Sequential(state =>
            {
                var start = state.Parse(openBracket);
                var invertResult = state.TryParse(Match('^'));
                var ranges = new List<(char low, char high)>();
                while (true)
                {
                    var c = state.Parse(any);
                    if (c == ']')
                        break;
                    if (c == '\\')
                        c = state.Parse(any);
                    var low = c;
                    var high = c;
                    var next = state.Parse(peek);
                    if (next == '-')
                    {
                        state.Parse(any);
                        c = state.Parse(any);
                        if (c == ']')
                            throw new RegexException("Unexpected ] after -. Expected end of range. Did you mean '\\]'?");
                        if (c == '\\')
                            c = state.Parse(any);
                        high = c;
                    }

                    if (high < low)
                        throw new RegexException($"Invalid range {high}-{low} should be {low}-{high}");
                    ranges.Add((low, high));
                }

                return new CharacterMatcher(invertResult.Success, ranges);
            });

            var digits = CStyleParserMethods.UnsignedInteger();

            // Literal match of any non-slash and non-control character
            var normalChar = Match(c => !_charsRequiringEscape.Contains(c) && !char.IsControl(c));

            var regex = Pratt<List<RegexState>>(config => config
                .Add(normalChar, p => p
                    .ProduceRight(2, (_, c) => RegexState.AddMatch(null, x => x == c.Value, $"Match {c}"))
                    .ProduceLeft(2, (_, states, c) => RegexState.AddMatch(states.Value, x => x == c.Value, $"Match {c}"))
                )
                .Add(characterClass, p => p
                    .ProduceRight(2, (_, matcher) => RegexState.AddMatch(null, c => matcher.Value.IsMatch(c), "class"))
                    .ProduceLeft(2, (_, states, matcher) => RegexState.AddMatch(states.Value, c => matcher.Value.IsMatch(c), "class"))
                )
                .Add(Match('.'), p => p
                    .ProduceRight(2, (_, _) => RegexState.AddMatch(null, c => c != '\0', "Any"))
                    .ProduceLeft(2, (_, states, _) => RegexState.AddMatch(states.Value, c => c != '\0', "Any"))
                )
                .Add(Match("\\"), p => p
                    .ProduceRight(2, (ctx, _) => RegexState.AddSpecialMatch(null, ctx.Parse(Any())))
                    .ProduceLeft(2, (ctx, states, _) => RegexState.AddSpecialMatch(states.Value, ctx.Parse(Any())))
                )
                .Add(Match('('), p => p
                    .ProduceRight(2, (ctx, _) =>
                    {
                        var group = ctx.Parse(0);
                        ctx.Expect(Match(')'));
                        return RegexState.AddGroupState(null, group);
                    })
                    .ProduceLeft(2, (ctx, states, _) =>
                    {
                        var group = ctx.Parse(0);
                        ctx.Expect(Match(')'));
                        return RegexState.AddGroupState(states.Value, group);
                    })
                )
                .Add(Match('{'), p => p
                    .ProduceLeft(2, (ctx, states, _) => ParseRange(ctx, states.Value, digits))
                )
                .Add(Match('?'), p => p
                    .ProduceLeft(2, (_, states, _) => RegexState.QuantifyPrevious(states.Value, Quantifier.ZeroOrOne))
                )
                .Add(Match('+'), p => p
                    .ProduceLeft(2, (_, states, _) => RegexState.SetPreviousStateRange(states.Value, 1, int.MaxValue))
                )
                .Add(Match('*'), p => p
                    .ProduceLeft(2, (_, states, _) => RegexState.QuantifyPrevious(states.Value, Quantifier.ZeroOrMore))
                )
                .Add(Match('|'), p => p
                    .ProduceLeft(2, (ctx, states, _) => ParseAlternation(ctx, states.Value))
                )
                .Add(Match('$'), p => p
                    .ProduceLeft(4, (_, states, _) =>
                    {
                        states.Value.Add(RegexState.EndOfInput);
                        return states.Value;
                    })
                )
            );

            var requiredEnd = If(End(), Produce(() => Utility.Defaults.ObjectInstance), Produce(ThrowEndOfPatternException));

            return (regex, requiredEnd).Produce((r, _) => r)
                .Transform(r => new Regex(r))
                .Named("RegexPattern");
        }

        private static object ThrowEndOfPatternException(ISequence<char> t, IDataStore data)
            => throw new RegexException("Expected end of pattern but found '" + t.GetNext());

        private static List<RegexState> ParseRange(IParseContext<char> ctx, List<RegexState> states, IParser<char, int> digits)
        {
            if (states.Last().Type == RegexStateType.EndOfInput)
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
                return RegexState.SetPreviousStateRange(states, min, min);
            }

            // At this point we might have {X,} {X,Y} or {,Y}
            // In any case, min is filled in now with either a value or 0
            var second = ctx.TryParse(digits);
            ctx.Expect(Match('}'));
            return RegexState.SetPreviousStateRange(states, min, second.Success ? second.Value : int.MaxValue);
        }

        private static List<RegexState> ParseAlternation(IParseContext<char, List<RegexState>> ctx, List<RegexState> states)
        {
            var options = new List<List<RegexState>>() { states };
            while (true)
            {
                var option = ctx.TryParse(0);
                if (!option.Success || option.Value.Count == 0)
                    break;
                options.Add(option.Value);
            }

            if (options.Count == 1)
                return states;

            return new List<RegexState>
            {
                new RegexState("alternation")
                {
                    Type = RegexStateType.Alternation,
                    Alternations = options
                }
            };
        }
    }
}
