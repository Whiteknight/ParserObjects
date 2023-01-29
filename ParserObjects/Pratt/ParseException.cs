using ParserObjects.Internal;

namespace ParserObjects.Pratt;

/// <summary>
/// Control-flow exception used to control the parse in the Pratt parser. In certain failure
/// scenarios a ParseException is used to immediately abandon the current user callback function
/// and return to the engine. This type SHOULD NOT be caught by user code.
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
