using System;

namespace ParserObjects.Pratt
{
    /// <summary>
    /// Internal control-flow exception type used to direct errors in the Pratt parser. Not for
    /// external use.
    /// </summary>
    [Serializable]
    public class ParseException : ControlFlowException
    {
        public ParseException()
        {
        }

        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ParseException(ParseExceptionSeverity severity, string message, IParser parser, Location location)
            : this(message)
        {
            Severity = severity;
            Parser = parser;
            Location = location;
        }

        protected ParseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public ParseExceptionSeverity Severity { get; }
        public IParser Parser { get; }
        public Location Location { get; }
    }
}
