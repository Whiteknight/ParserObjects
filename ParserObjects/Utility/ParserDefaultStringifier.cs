namespace ParserObjects.Utility
{
    /// <summary>
    /// Default logic for implementing IParser.ToString()
    /// </summary>
    public static class ParserDefaultStringifier
    {
        /// <summary>
        /// Attempts to create a string representation of a string which is both sufficiently
        /// helpful but not overly verbose. Use the .Name if provided.
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static string ToString(IParser parser)
        {
            if (!string.IsNullOrEmpty(parser.Name))
                return $"<{parser.Name}>";
            var parserType = parser.GetType();
            if (parserType.DeclaringType != null)
                return $"{parserType.DeclaringType.Name}.{parserType.Name}";
            return parserType.Name;
        }
    }
}
