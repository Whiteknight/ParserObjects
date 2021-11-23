namespace ParserObjects.Pratt;

// control-flow exception type, so that errors during user-callbacks can return to the
// engine immediately and be considered a failure of the current rule.

public enum ParseExceptionSeverity
{
    // Fail the current rule, but allow the engine to continue attempting to fill in the
    // current level of precidence
    Rule,

    // Fail the current precidence level
    Level,

    // Fail the entire Pratt.Parse() attempt
    Parser
}
