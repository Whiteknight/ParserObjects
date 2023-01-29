using System;

namespace ParserObjects.Internal;

/// <summary>
/// Base class for several internal exception types which are used to handle non-local
/// control flow within parsers.
/// </summary>
public class ControlFlowException : Exception
{
    public ControlFlowException(string message) : base(message)
    {
    }
}
