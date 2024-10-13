﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace ParserObjects;

/// <summary>
/// A wrapper struct for a list of objects. Can be used to get an object of a given type from
/// the list, if it exists.
/// </summary>
/// <param name="Data"></param>
public readonly record struct ResultData(object Data)
{
    public Option<T> TryGetData<T>()
    {
        if (Data == null)
            return default;

        if (Data is T dataTyped)
            return new Option<T>(true, dataTyped);

        if (Data is not IReadOnlyList<object> list)
            return default;

        foreach (var item in list)
        {
            if (item is T itemTyped)
                return new Option<T>(true, itemTyped);
        }

        return default;
    }
}

public readonly record struct Result<TValue>(
    IParser Parser,
    bool Success,
    string? InternalError,
    TValue? InternalValue,
    int Consumed,
    ResultData Data
)
{
    public static Result<TValue> Fail(IParser parser, string errorMessage)
        => new Result<TValue>(parser, false, errorMessage ?? string.Empty, default, 0, default);

    public static Result<TValue> Ok(IParser parser, TValue value, int consumed)
        => new Result<TValue>(parser, true, string.Empty, value, consumed, default);

    public string ErrorMessage => Success ? string.Empty : InternalError ?? string.Empty;

    public TValue Value
        => Success
        ? InternalValue!
        : throw new InvalidOperationException("This result has failed. There is no value to access: " + ErrorMessage);

    /// <summary>
    /// Safely get the value of the result, or the default value.
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public TValue GetValueOrDefault(TValue defaultValue)
        => Success ? Value : defaultValue;

    /// <summary>
    /// Safely get the value of the result, or the default value.
    /// </summary>
    /// <param name="getDefaultValue"></param>
    /// <returns></returns>
    public TValue GetValueOrDefault(Func<TValue> getDefaultValue)
        => Success ? Value : getDefaultValue();

    public Result<TValue> WithData(object data) => this with { Data = new ResultData(data) };

    public Result<TValue> With(ResultData data) => this with { Data = data };

    public Option<T> TryGetData<T>() => Data.TryGetData<T>();

    public Result<TValue> AdjustConsumed(int consumed)
        => this with { Consumed = consumed };

    public Result<T> Select<T>(Func<TValue, T> selector)
        => Success
        ? new Result<T>(Parser, true, null, selector(Value), Consumed, Data)
        : new Result<T>(Parser, false, ErrorMessage, default, 0, Data);

    public Result<T> CastError<T>()
        => Success
        ? throw new InvalidOperationException("Can only cast error results without an explicit mapping")
        : new Result<T>(Parser, false, InternalError, default, 0, Data);

    public Result<object> AsObject() => Select<object>(static t => t);

    public override string ToString()
        => Success
            ? $"{Parser} Ok"
            : $"{Parser} FAIL: {ErrorMessage}";
}
