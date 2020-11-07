using System;

namespace ParserObjects.Parsers.Specialty.Regex
{

    [Serializable]
    public class RegexException : Exception
    {
        public RegexException() { }
        public RegexException(string message) : base(message) { }
        public RegexException(string message, Exception inner) : base(message, inner) { }
        protected RegexException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
