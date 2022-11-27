namespace ParserObjects;

/// <summary>
/// Base interface for results returned from a parse operation. The result should include
/// reference to the Parser which generated the reuslt, an indicator of success or failure, and
/// the location where the result occured. Subclasses of this type will include more detailed
/// information about the result.
/// </summary>
public interface IResultBase
{
    /// <summary>
    /// Gets the parser which created this result. Notice that this might not be the parser on
    /// which the Parse method was called, but may instead be some internal parser to which the
    /// task was delegated.
    /// </summary>
    IParser Parser { get; }

    /// <summary>
    /// Gets a value indicating whether the parse succeeded.
    /// </summary>
    bool Success { get; }

    /// <summary>
    /// Gets the approximate location of the successful parse in the input sequence. On failure, this
    /// value is undefined and may show the location of the start of the attempt, the location at
    /// which failure occured, null, or some other value.
    /// </summary>
    Location Location { get; }

    Option<T> TryGetData<T>();
}
