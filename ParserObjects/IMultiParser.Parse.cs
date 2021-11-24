﻿using ParserObjects.Sequences;
using ParserObjects.Utility;

namespace ParserObjects;

public static partial class MultiParserExtensions
{
    /// <summary>
    /// Parse a string using the given character multiparser.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="p"></param>
    /// <param name="s"></param>
    /// <returns></returns>
    public static IMultiResult<TOutput> Parse<TOutput>(this IMultiParser<char, TOutput> p, string s)
        => p.Parse(new ParseState<char>(new StringCharacterSequence(s, default), Defaults.LogMethod));
}
