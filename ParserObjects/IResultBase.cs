﻿namespace ParserObjects;

/// <summary>
/// Base interface for results returned from a parse operation. The result should include
/// reference to the Parser which generated the reuslt and an indicator of success or failure.
/// Subclasses of this type will include more detailed information about the result.
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
    /// Try to get attached data with the given type. If none exists, returns failure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Option<T> TryGetData<T>();
}
