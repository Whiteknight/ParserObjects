using System;

namespace ParserObjects;

/// <summary>
/// A result which has a value on success, no value otherwise. Used to avoid passing null
/// result values around.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly record struct Option<T>(bool Success, T Value)
{
    /// <summary>
    /// Safely gets a value from this instance. If Success, the Value is returned. Otherwise
    /// the given default value is returned.
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public T GetValueOrDefault(T defaultValue) => Success ? Value : defaultValue;

    /// <summary>
    /// Returns true if Success is true and if the Value matches the provided value. False
    /// otherwise.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Is(T value)
    {
        if (!Success)
            return false;
        if (value == null)
            return Value == null;
        return Value!.Equals(value);
    }

    /// <summary>
    /// Return another Option object whose value (if it exists) is tranformed.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Option<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        if (!Success)
            return default;
        return new Option<TResult>(true, selector(Value!));
    }

    /// <summary>
    /// Return a new Option object whose value (if it exists) is tranformed.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Option<TResult> SelectMany<TResult>(Func<T, Option<TResult>> selector)
    {
        if (!Success)
            return default;
        return selector(Value!);
    }

    public override int GetHashCode()
    {
        if (!Success)
            return 0;
        return Value!.GetHashCode();
    }
}
