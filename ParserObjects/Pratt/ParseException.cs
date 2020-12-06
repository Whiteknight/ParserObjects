using System;
using ParserObjects.Parsers;

namespace ParserObjects.Pratt
{
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