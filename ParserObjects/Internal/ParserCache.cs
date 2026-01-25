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
            return parser.Name == n
                ? parser
                : parser.Named(n);
        }, create);

        return result is TParser typed
            ? typed
            : throw new InvalidOperationException($"Parser {name} does not have the correct type");
    }

    public static class ErrorMessages
    {
        private static readonly string[] _innerParserFailMessages = [
            "Inner parser 1 failed.",
            "Inner parser 2 failed.",
            "Inner parser 3 failed.",
            "Inner parser 4 failed.",
            "Inner parser 5 failed.",
            "Inner parser 6 failed.",
            "Inner parser 7 failed.",
            "Inner parser 8 failed.",
            "Inner parser 9 failed.",
            "Inner parser 10 failed."
        ];

        private static readonly string[] _itemFailMessages = [
            "Item at position 1 does not match.",
            "Item at position 2 does not match.",
            "Item at position 3 does not match.",
            "Item at position 4 does not match.",
            "Item at position 5 does not match.",
            "Item at position 6 does not match.",
            "Item at position 7 does not match.",
            "Item at position 8 does not match.",
            "Item at position 9 does not match.",
            "Item at position 10 does not match."
        ];

        public static string GetInnerParserFailMessage(int index)
            => index < _innerParserFailMessages.Length
                ? _innerParserFailMessages[index]
                : $"Inner parser {index + 1} failed.";

        public static string GetItemFailMessage(int index)
            => index < _itemFailMessages.Length
                ? _itemFailMessages[index]
                : $"Item at position {index + 1} does not match.";
    }
}
