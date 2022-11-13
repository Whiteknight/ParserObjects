using System;

namespace ParserObjects.Internal.Utility;

/// <summary>
/// Base class for several internal exception types which are used to handle non-local
/// control flow within parsers.
/// </summary>
[Serializable]
public class ControlFlowException : Exception
{
    public ControlFlowException() { }
    public ControlFlowException(string message) : base(message) { }
    public ControlFlowException(string message, Exception inner) : base(message, inner) { }
    protected ControlFlowException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
