using System;

namespace ParserObjects.Earley;

[Serializable]
public class GrammarException : Exception
{
    public GrammarException()
    {
    }

    public GrammarException(string message) : base(message)
    {
    }

    public GrammarException(string message, Exception inner) : base(message, inner)
    {
    }

    protected GrammarException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
