using System;

namespace ParserObjects.Internal.Utility;

/// <summary>
/// Base class for several internal exception types which are used to handle non-local
/// control flow within parsers.
/// </summary>
[Serializable]
public class ControlFlowException : Exception
{
    public ControlFlowException(string message) : base(message)
    {
    }
}
