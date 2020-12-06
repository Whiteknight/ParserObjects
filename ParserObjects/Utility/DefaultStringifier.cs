namespace ParserObjects.Utility
{
    /// <summary>
    /// Default logic for implementing INamed.ToString().
    /// </summary>
    public static class DefaultStringifier
    {
        /// <summary>
        /// Attempts to create a string representation of a string which is both sufficiently
        /// helpful but not overly verbose. Use the .Name if provided.
        /// </summary>
        /// <param name="named"></param>
        /// <returns></returns>
        public static string ToString(INamed named)
        {
            if (!string.IsNullOrEmpty(named.Name))
                return $"<{named.Name}>";
            var objType = named.GetType();
            if (objType.DeclaringType != null)
                return $"{objType.DeclaringType.Name}.{objType.Name}";
            return objType.Name;
        }
    }
}
