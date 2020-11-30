namespace ParserObjects.Regexes
{
    /// <summary>
    /// Type of the regex state. Used internally by the engine to dispatch matching logic
    /// </summary>
    public enum RegexStateType
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
        Alternation
    }
}
