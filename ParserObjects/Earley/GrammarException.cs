using System;

namespace ParserObjects.Earley;

public class GrammarException : Exception
{
    public GrammarException(string message) : base(message)
    {
    }
}
