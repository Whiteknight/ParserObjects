using System;

namespace ParserObjects.Internal.Tries;

public class TrieInsertException : Exception
{
    public TrieInsertException(string message) : base(message)
    {
    }
}
