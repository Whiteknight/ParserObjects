using System;

namespace ParserObjects.Earley;

[Serializable]
public class GrammarException : Exception
{
    public GrammarException(string message) : base(message)
    {
    }
}
