using System.Collections.Generic;

namespace ParserObjects.Internal.Visitors;

public static class ListParsersVisitor
{
    public static IReadOnlyDictionary<int, IParser> Visit(IParser parser)
    {
        var parsers = new Dictionary<int, IParser>();
        if (parser != null)
            VisitInternal(parser, parsers);
        return parsers;
    }

    private static void VisitInternal(IParser parser, Dictionary<int, IParser> parsers)
    {
        if (parsers.ContainsKey(parser.Id))
            return;
        parsers.Add(parser.Id, parser);

        foreach (var child in parser.GetChildren())
            VisitInternal(child, parsers);
    }
}
