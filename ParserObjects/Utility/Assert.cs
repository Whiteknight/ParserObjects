using System;

namespace ParserObjects.Utility
{
    public static class Assert
    {
        public static void ArgumentNotNull(object value, string parameterName)
        {
            if (value == null) 
                throw new ArgumentNullException(parameterName);
        }

        public static void ArgumentNotNullOrEmpty(string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("string value may not be null or empty", parameterName);
        }
    }
}
