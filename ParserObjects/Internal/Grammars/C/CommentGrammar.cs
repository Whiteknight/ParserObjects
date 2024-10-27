using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.C;

public static class CommentGrammar
{
    public static IParser<char, string> CreateParser()
        => Sequential(static s =>
        {
            s.Expect('/');
            s.Expect('*');

            while (!s.Input.IsAtEnd)
            {
                var c = s.Input.GetNext();
                if (c != '*')
                    continue;

                var lookahead = s.Input.Peek();
                if (lookahead == '/')
                {
                    s.Input.GetNext();

                    return new string(s.GetCapturedInputs());
                }
            }

            s.Fail("Could not find */");
            return "";
        });
}
