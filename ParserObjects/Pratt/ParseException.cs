using ParserObjects.Internal.Utility;

namespace ParserObjects.Pratt;

/// <summary>
/// Internal control-flow exception type used to direct errors in the Pratt parser. Not for
/// external use.
/// </summary>
public class ParseException : ControlFlowException
{
    public ParseException(string message) : base(message)
    {
    }

    public ParseException(ParseExceptionSeverity severity, string message, IParser parser, Location location)
        : this(message)
    {
        Severity = severity;
        Parser = parser;
        Location = location;
    }

    public ParseExceptionSeverity Severity { get; }
    public IParser? Parser { get; }
    public Location? Location { get; }
}
