﻿using System;

namespace ParserObjects;

/// <summary>
/// A result which has a value on success, no value otherwise. Used to avoid passing null
/// result values around.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IOption<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    bool Success { get; }

    /// <summary>
    /// Gets the value if the operation succeeded. May throw an exception or return a
    /// meaningless default value otherwise. Check the Success flag before accessing this
    /// value.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Safely gets a value from this instance. If Success, the Value is returned. Otherwise
    /// the given default value is returned.
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    T GetValueOrDefault(T defaultValue);

    /// <summary>
    /// Returns true if Success is true and if the Value matches the provided value. False
    /// otherwise.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Is(T value);

    IOption<TResult> Select<TResult>(Func<T, TResult> selector);

    IOption<TResult> SelectMany<TResult>(Func<T, IOption<TResult>> selector);
}

/// <summary>
/// Represents the successful result of an operation. The Value should exist and be meaningful.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class SuccessOption<T> : IOption<T>
{
    public SuccessOption(T value)
    {
        Value = value;
    }

    public bool Success => true;

    public T Value { get; }

    public T GetValueOrDefault(T defaultValue) => Value;

    public bool Is(T value)
    {
        if (value == null)
            return Value == null;
        return value.Equals(Value);
    }

    public IOption<TResult> Select<TResult>(Func<T, TResult> selector)
        => new SuccessOption<TResult>(selector(Value));

    public IOption<TResult> SelectMany<TResult>(Func<T, IOption<TResult>> selector)
        => selector(Value);

    public override bool Equals(object obj)
    {
        return obj is SuccessOption<T> other && object.Equals(Value, other.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

/// <summary>
/// Represents an unsuccessful result of an operation. The Value does not exist and any
/// attempt to access it will result in an exception.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class FailureOption<T> : IOption<T>
{
    public static IOption<T> Instance { get; } = new FailureOption<T>();

    public bool Success => false;
    public T Value => throw new InvalidOperationException("This option does not contain a successful value");

    public T GetValueOrDefault(T defaultValue) => defaultValue;

    public bool Is(T value) => false;

    public IOption<TResult> Select<TResult>(Func<T, TResult> _)
        => FailureOption<TResult>.Instance;

    public IOption<TResult> SelectMany<TResult>(Func<T, IOption<TResult>> _)
        => FailureOption<TResult>.Instance;

    public override bool Equals(object obj)
    {
        return obj is FailureOption<T>;
    }

    public override int GetHashCode()
    {
        return 0;
    }
}
