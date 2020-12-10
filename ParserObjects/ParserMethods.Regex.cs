using System;
using System.Collections.Generic;
using ParserObjects.Parsers;
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
        private static readonly HashSet<char> _charsRequiringEscape = new HashSet<char> { '\\', '(', ')', '$', '|' };
        private static readonly HashSet<char> _classCharsRequiringEscape = new HashSet<char> { '\\', '^', ']' };

        private static IParser<char, Regex> GetRegexPatternParser()
        {
            IParser<char, IRegexNode>? alternationInner = null;
            var alternation = Deferred(() => alternationInner!);

            var characterRange = Rule(
                Any(),
                Match('-'),
                Any(),
                (low, dash, high) => (low, high)
            );
            var characterOrRange = First(
                characterRange,
                Match("\\^").Transform(_ => (low: '^', high: '^')),
                Match("\\]").Transform(_ => (low: ']', high: ']')),
                Match("\\\\").Transform(_ => (low: '\\', high: '\\')),
                Match(c => !_classCharsRequiringEscape.Contains(c)).Transform(c => (low: c, high: c))
            );
            var characterClass = Rule(
                Match('['),
                Match('^').Optional(),
                characterOrRange.List(true),
                Match(']'),
                (open, maybeNot, contents, close) => RegexNodes.CharacterClass(maybeNot.Is('^'), contents)
            );

            // These groupings will not be captured like in a normal regex
            var groupedAlternation = Rule(
                Match('('),
                alternation,
                Match(')'),
                (open, atoms, close) => RegexNodes.Group(atoms)
            );

            // Literal match of any non-slash and non-control character
            var normalChar = Match(c => !_charsRequiringEscape.Contains(c) && !char.IsControl(c)).Transform(c => RegexNodes.Match(c));

            var atom = First(
                Match('.').Transform(c => RegexNodes.Wildcard()),
                Match("\\d").Transform(_ => RegexNodes.Match(c => char.IsDigit(c), "digit")),
                Match("\\D").Transform(_ => RegexNodes.Match(c => !char.IsDigit(c), "non-digit")),
                Match("\\w").Transform(_ => RegexNodes.Match(c => char.IsLetterOrDigit(c) || c == '_', "word")),
                Match("\\W").Transform(_ => RegexNodes.Match(c => char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c), "non-word")),
                Match("\\s").Transform(_ => RegexNodes.Match(c => char.IsWhiteSpace(c), "whitespace")),
                Match("\\S").Transform(_ => RegexNodes.Match(c => !char.IsWhiteSpace(c), "non-whitespace")),
                (Match('\\'), Any()).Produce((slash, c) => RegexNodes.Match(c)),
                groupedAlternation,
                characterClass,
                normalChar
            );
            var digits = CStyleParserMethods.UnsignedInteger();

            var range = First(
                Rule(
                    Match("{"),
                    digits,
                    Match("}"),
                    (open, number, close) => (number, number)
                ),
                Rule(
                    Match("{"),
                    digits,
                    Match(','),
                    Match("}"),
                    (open, number, comma, close) => (number, -1)
                ),
                Rule(
                    Match("{"),
                    Match(','),
                    digits,
                    Match("}"),
                    (open, comma, number, close) => (0, number)
                ),
                Rule(
                    Match("{"),
                    digits,
                    Match(','),
                    digits,
                    Match("}"),
                    (open, min, comma, max, close) => (min, max)
                )
            );

            var quantifiedAtom = LeftApply(
                atom,
                left => First(
                    (left, Match('?')).Produce((a, p) => RegexNodes.ZeroOrOne(a)),
                    (left, Match('+')).Produce((a, p) => RegexNodes.OneOrMore(a)),
                    (left, Match('*')).Produce((a, p) => RegexNodes.ZeroOrMore(a)),
                    (left, range).Produce((a, r) => RegexNodes.Range(a, r.Item1, r.Item2))
                )
            );
            var atomString = quantifiedAtom
                .List(true)
                .Transform(qa => RegexNodes.Sequence(qa));

            alternationInner = SeparatedList(atomString, Match('|'), true)
                .Transform(nodes => RegexNodes.Or(nodes));

            var maybeEndAnchor = First(
                Match('$').Transform(dollar => RegexNodes.EndAnchor()),
                Empty().Transform(_ => RegexNodes.Nothing())
            );
            var requiredEnd = If(End(), Empty(), Produce(ThrowEndOfPatternException));
            var regex = (alternation, maybeEndAnchor, requiredEnd).Produce((f, s, e) => RegexNodes.Sequence(new[] { f, s }));

            return regex
                .Transform(r => new Regex(r))
                .Named("RegexPattern");
        }

        private static object ThrowEndOfPatternException(ISequence<char> t, IDataStore data)
            => throw new RegexException("Expected end of pattern but found '" + t.GetNext());
    }
}
