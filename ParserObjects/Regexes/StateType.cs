namespace ParserObjects.Regexes;

/// <summary>
/// Type of the regex state. Used internally by the engine to dispatch matching logic.
/// </summary>
public enum StateType
{
    /// <summary>
    /// The end state
    /// </summary>
    EndOfInput,

    /// <summary>
    /// The state is a nested, parenthesized grouping of one or more substates
    /// </summary>
    Group,

    /// <summary>
    /// The state requires matching a value
    /// </summary>
    MatchValue,

    /// <summary>
    /// The state is an alternation of substates
    /// </summary>
    Alternation,

    /// <summary>
    /// End sentinel, which causes the regex to break
    /// </summary>
    EndSentinel,

    /// <summary>
    /// Prevents a modifier from attaching to the previous state
    /// </summary>
    Fence
}
