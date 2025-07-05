using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable RCS1256 // Invalid argument null check.

namespace ParserObjects.Internal;

/// <summary>
/// Helper methods for enforcing invariants.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Assert
{
    [return: NotNull]
    public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string parameterName = "")
        => value is null ? throw new ArgumentNullException(parameterName) : value!;

    [return: NotNull]
    public static IReadOnlyList<T> NotNullAndContainsNoNulls<T>([NotNull] IReadOnlyList<T> values, [CallerArgumentExpression(nameof(values))] string parameterName = "")
        where T : class
    {
        NotNull(values, parameterName);
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] == null)
                throw new ArgumentNullException(parameterName + $"[{i}]");
        }

        return values;
    }

    [return: NotNull]
    public static string NotNullOrEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string parameterName = "")
        => string.IsNullOrEmpty(value) ? throw new ArgumentException("string value may not be null or empty", parameterName) : value!;

    public static int NotLessThanOrEqualToZero(int value, [CallerArgumentExpression(nameof(value))] string parameterName = "")
        => value > 0
        ? value
        : throw new ArgumentOutOfRangeException(parameterName, "Value must be positive integer");

    public static int InRange(int value, int min, int max, [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        Debug.Assert(min <= max);
        return value < min || value > max
            ? throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {min} and {max}")
            : value;
    }

    public static int GreaterThanOrEqualTo(int value, int min, [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be greater than or equal to {min}");
        return value;
    }
}
