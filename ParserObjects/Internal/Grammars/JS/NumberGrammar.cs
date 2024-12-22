using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.JS;

public static class NumberGrammar
{
    public static IParser<char, string> CreateParser()
    {
        var maybeMinus = MatchChar('-').Transform(static _ => "-").Optional(static () => "");
        var zero = MatchChar('0').Transform(static _ => "0");
        var maybeDigits = Digit().ListCharToString();
        var empty = Produce(static () => "");

        // wholePart := '0' | <nonZeroDigit> <digits>*
        var wholePart = First(
            zero,
            Rule(
                NonZeroDigit(),
                maybeDigits,
                static (first, rest) => first + rest
            )
        );

        // fractPart := '.' <digit>+ | <empty>
        var fractPart = First(
            Rule(
                MatchChar('.').Transform(static _ => "."),
                DigitString(),
                static (dot, fract) => dot + fract
            ),
            empty
        );

        // expExpr := ('e' | 'E') ('+' | '-' | <empty>) <digit>+
        var expExpr = Rule(
            First(
                MatchChar('e').Transform(static _ => "e"),
                MatchChar('E').Transform(static _ => "E")
            ),
            First(
                MatchChar('+').Transform(static _ => "+"),
                MatchChar('-').Transform(static _ => "-"),
                Produce(static () => "+")
            ),
            DigitString(),
            static (e, sign, value) => e + sign + value
        );

        // expPart := <exprExpr> | <empty>
        var expPart = First(
            expExpr,
            empty
        );

        // number := <minus>? <wholePart> <fractPart> <expPart>
        return (maybeMinus, wholePart, fractPart, expPart)
            .Rule(static (sign, whole, fract, exp) => sign + whole + fract + exp);
    }
}
