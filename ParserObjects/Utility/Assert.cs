using System;

namespace ParserObjects.Utility
{
    /// <summary>
    /// Helper methods for enforcing invariants
    /// </summary>
    public static class Assert
    {
        public static void ArgumentNotNull(object value, string parameterName)
        {
            if (value == null) 
                throw new ArgumentNullException(parameterName);
        }

        public static void ArrayNotNullAndContainsNoNulls<T>(T[] values, string parameterName)
            where T : class
        {
            ArgumentNotNull(values, parameterName);
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                    throw new ArgumentNullException(parameterName + $"[{i}]");
            }
        }

        public static void ArgumentNotNullOrEmpty(string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("string value may not be null or empty", parameterName);
        }

        public static void ArgumentGreaterThan(int value, int limit, string parameterName)
        {
            if (value <= limit)
                throw new ArgumentException($"Value {value} must be greater than {limit}", parameterName);
        }
    }
}
