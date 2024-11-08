using System.Linq;
using static ParserObjects.Parsers;
using static ParserObjects.Parsers<char>;

namespace ParserObjects.Tests.Examples.Parenthesis;

public static class ParenthesisGrammar
{
    public static IParser<char, bool> CreateParser()
    {
        IParser<char, bool> validatorInternal = null;
        var validator = Deferred(() => validatorInternal);

        var parenthesis = Rule(
            MatchChar('('),
            validator,
            MatchChar(')'),
            (_, inner, _) => inner && true
        );

        var braces = Rule(
            MatchChar('{'),
            validator,
            MatchChar('}'),
            (_, inner, _) => inner && true
        );

        var brackets = Rule(
            MatchChar('['),
            validator,
            MatchChar(']'),
            (_, inner, _) => inner && true
        );

        validatorInternal = First(
            parenthesis,
            braces,
            brackets,
            Produce(() => true)
        ).List().Transform(x => x.All(y => y));

        return Rule(
            validatorInternal,
            End(),
            (r, _) => r
        );
    }
}
