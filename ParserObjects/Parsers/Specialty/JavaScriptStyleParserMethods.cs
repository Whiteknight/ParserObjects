using System;
using System.Globalization;
using static ParserObjects.Parsers.ParserMethods<char>;
using static ParserObjects.Parsers.Specialty.DigitParserMethods;

namespace ParserObjects.Parsers.Specialty
{
    public static class JavaScriptStyleParserMethods
    {
        /// <summary>
        /// JavaScript-style number literal, returned as a string
        /// </summary>
        /// <returns></returns>
        public static IParser<char, string> NumberString() => _numberString.Value;
        private static readonly Lazy<IParser<char, string>> _numberString = new Lazy<IParser<char, string>>(
            () =>
            {
                var maybeMinus = Match('-').Transform(c => "-").Optional(() => "");
                var zero = Match('0').Transform(c => "0");
                var maybeDigits = Digit().ListCharToString();
                var empty = Produce(() => "");

                // wholePart := '0' | <nonZeroDigit> <digits>*
                var wholePart = First(
                    zero,
                    Rule(
                        NonZeroDigit(),
                        maybeDigits,
                        (first, rest) => first + rest
                    )
                );

                // fractPart := '.' <digit>+ | <empty>
                var fractPart = First(
                    Rule(
                        Match('.').Transform(c => "."),
                        DigitString(),
                        (dot, fract) => dot + fract
                    ),
                    empty
                );

                // expExpr := ('e' | 'E') ('+' | '-' | <empty>) <digit>+
                var expExpr = Rule(
                    First(
                        Match('e').Transform(c => "e"),
                        Match('E').Transform(c => "E")
                    ),
                    First(
                        Match('+').Transform(c => "+"),
                        Match('-').Transform(c => "-"),
                        Produce(() => "+")
                    ),
                    DigitString(),

                    (e, sign, value) => e + sign + value
                );

                // expPart := <exprExpr> | <empty>
                var expPart = First(
                    expExpr,
                    empty
                );

                // number := <minus>? <wholePart> <fractPart> <expPart>
                return (maybeMinus, wholePart, fractPart, expPart)
                    .Produce((sign, whole, fract, exp) => sign + whole + fract + exp)
                    .Named("JavaScript-Style Number String");
            }
        );

        /// <summary>
        /// JavaScript-style number literal returned as a parsed Double
        /// </summary>
        /// <returns></returns>
        public static IParser<char, double> Number() => _number.Value;
        private static readonly Lazy<IParser<char, double>> _number = new Lazy<IParser<char, double>>(
            () => NumberString()
                .Transform(s => double.Parse(s, NumberStyles.Float))
                .Named("JavaScript-Style Number Literal")
        );
    }
}