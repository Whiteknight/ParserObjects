using System.Collections.Generic;

namespace ParserObjects.Internal.Visitors;

public sealed class ListParsersVisitor
{
    public IReadOnlyDictionary<int, IParser> Visit(IParser parser)
    {
        if (parser == null)
            return new Dictionary<int, IParser>();
        var parsers = new Dictionary<int, IParser>();
        VisitInternal(parser, parsers);
        return parsers;
    }

    private void VisitInternal(IParser parser, Dictionary<int, IParser> parsers)
    {
        if (parsers.ContainsKey(parser.Id))
            return;
        parsers.Add(parser.Id, parser);

        foreach (var child in parser.GetChildren())
            VisitInternal(child, parsers);
    }
}
