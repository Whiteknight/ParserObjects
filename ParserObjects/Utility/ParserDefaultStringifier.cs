namespace ParserObjects.Utility
{
    public static class ParserDefaultStringifier
    {
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
