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
    }
}
