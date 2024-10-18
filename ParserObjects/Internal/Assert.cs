using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#pragma warning disable RCS1256 // Invalid argument null check.

namespace ParserObjects.Internal;

/// <summary>
/// Helper methods for enforcing invariants.
/// </summary>
public static class Assert
{
    public static void ArgumentNotNull(object? value, [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        if (value == null)
            throw new ArgumentNullException(parameterName);
    }

    public static void ArrayNotNullAndContainsNoNulls<T>(IReadOnlyList<T> values, [CallerArgumentExpression(nameof(values))] string parameterName = "")
        where T : class
    {
        ArgumentNotNull(values, parameterName);
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] == null)
                throw new ArgumentNullException(parameterName + $"[{i}]");
        }
    }

    public static void ArgumentNotNullOrEmpty(string value, [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("string value may not be null or empty", parameterName);
    }

    public static void ArgumentNotLessThanOrEqualToZero(int value, [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, "Value must be positive integer");
    }

    public static void ArgumentInRange(int value, int min, int max, [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        Debug.Assert(min <= max, "The bounds should not be inverted");
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {min} and {max}");
    }

    public static void ArgumentGreaterThanOrEqualTo(int value, int min, [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be greater than or equal to {min}");
    }
}
