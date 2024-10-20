using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.JS;

public static class NumberGrammar
{
    public static IParser<char, string> CreateParser()
    {
        var maybeMinus = MatchChar('-').Transform(_ => "-").Optional(() => "");
        var zero = MatchChar('0').Transform(_ => "0");
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
                MatchChar('.').Transform(_ => "."),
                DigitString(),
                (dot, fract) => dot + fract
            ),
            empty
        );

        // expExpr := ('e' | 'E') ('+' | '-' | <empty>) <digit>+
        var expExpr = Rule(
            First(
                MatchChar('e').Transform(_ => "e"),
                MatchChar('E').Transform(_ => "E")
            ),
            First(
                MatchChar('+').Transform(_ => "+"),
                MatchChar('-').Transform(_ => "-"),
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
            .Rule(static (sign, whole, fract, exp) => sign + whole + fract + exp)
            .Named("JavaScript-Style Number String");
    }
}
