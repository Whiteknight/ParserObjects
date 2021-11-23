using System;
using System.Runtime.Serialization;

namespace ParserObjects.Earley;

[Serializable]
public class ProductionFailedException : ControlFlowException
{
    public ProductionFailedException()
    {
    }

    public ProductionFailedException(string message)
        : base(message)
    {
    }

    public ProductionFailedException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected ProductionFailedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
