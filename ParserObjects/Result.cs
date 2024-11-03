using System;

namespace ParserObjects;

public readonly record struct Result<TValue>(
    IParser Parser,
    bool Success,
    string? InternalError,
    TValue? InternalValue,
    int Consumed,
    ResultData Data
)
{
    public static Result<TValue> Fail(IParser parser, string errorMessage, ResultData data = default)
        => new Result<TValue>(parser, false, errorMessage ?? string.Empty, default, 0, data);

    public static Result<TValue> Ok(IParser parser, TValue value, int consumed, ResultData data = default)
        => new Result<TValue>(parser, true, string.Empty, value, consumed, data);

    public string ErrorMessage
        => Success
        ? string.Empty
        : InternalError ?? string.Empty;

    public TValue Value
        => Success
        ? InternalValue!
        : throw new InvalidOperationException("This result has failed. There is no value to access: " + ErrorMessage);

    public bool IsValid => (Success && InternalValue != null) || (!Success && !string.IsNullOrEmpty(InternalError));

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

    public Option<TValue> ToOption()
        => Match(static v => new Option<TValue>(true, v), static () => default);

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TResult> onFailure)
        => Success
        ? onSuccess(Value)
        : onFailure();

    public Result<T> Select<T>(Func<TValue, T> selector)
        => Success
        ? new Result<T>(Parser, true, null, selector(Value), Consumed, Data)
        : new Result<T>(Parser, false, ErrorMessage, default, 0, Data);

    public Result<T> CastError<T>()
        => Success
        ? throw new InvalidOperationException("Can only cast error results without an explicit mapping")
        : new Result<T>(Parser, false, InternalError, default, 0, Data);

    public Result<object> AsObject() => Select<object>(static t => t!);

    public override string ToString()
        => Success
            ? $"{Parser} Ok"
            : $"{Parser} FAIL: {ErrorMessage}";
}
