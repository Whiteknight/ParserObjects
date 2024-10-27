using System;
using System.Collections.Concurrent;

namespace ParserObjects.Internal;

public static class ParserCache
{
    private static readonly ConcurrentDictionary<string, IParser> _parsers = new ConcurrentDictionary<string, IParser>();

    public static TParser GetOrCreate<TParser>(string name, Func<TParser> create)
        where TParser : class, IParser
    {
        var result = _parsers.GetOrAdd(name, (n, c) =>
        {
            var parser = c();
            if (parser.Name != n)
                parser = parser.Named(n);
            return parser;
        }, create);

        return result is TParser typed
            ? typed
            : throw new InvalidOperationException($"Parser {name} does not have the correct type");
    }
}
