namespace ParserObjects.Utility
{
    public static class ParserDefaultStringifier
    {
        public static string ToString(IParser parser)
        {
            if (!string.IsNullOrEmpty(parser.Name))
                return $"<{parser.Name}>";
            return parser.GetType().Name;
        }
    }
}
