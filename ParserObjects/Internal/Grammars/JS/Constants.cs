using System.Collections.Generic;

namespace ParserObjects.Internal.Grammars.JS;

public static class Constants
{
    public static IReadOnlyDictionary<char, string> EscapableStringCharacters { get; }
        = new Dictionary<char, string>
        {
            { 'b', "\b" },
            { 'f', "\f" },
            { 'n', "\n" },
            { 'r', "\r" },
            { 't', "\t" },
            { 'v', "\v" },
            { '0', "\0" },
            { '\\', "\\" }
        };
}
