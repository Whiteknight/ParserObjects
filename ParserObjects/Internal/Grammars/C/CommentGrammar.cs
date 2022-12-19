using static ParserObjects.Parsers<char>;

namespace ParserObjects.Internal.Grammars.C;

public static class CommentGrammar
{
    public static IParser<char, string> CreateParser()
    {
        return Sequential(s =>
        {
            if (s.Input.GetNext() != '/')
                s.Fail();
            if (s.Input.GetNext() != '*')
                s.Fail();

            while (!s.Input.IsAtEnd)
            {
                var c = s.Input.GetNext();
                if (c == '*')
                {
                    var lookahead = s.Input.Peek();
                    if (lookahead == '/')
                    {
                        s.Input.GetNext();

                        return new string(s.GetCapturedInputs());
                    }
                }
            }

            s.Fail("Could not find */");
            return "";
        }).Named("C-Style Comment");
    }
}
