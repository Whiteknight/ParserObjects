﻿using System;
using static ParserObjects.Parsers<char>;

namespace ParserObjects;

public static partial class Parsers
{
    /// <summary>
    /// Parses a single character of whitespace (' ', '\t', '\r', '\n','\v', etc).
    /// </summary>
    /// <returns></returns>
    public static IParser<char, char> WhitespaceCharacter() => _whitespaceCharacter.Value;

    private static readonly Lazy<IParser<char, char>> _whitespaceCharacter
        = new Lazy<IParser<char, char>>(
            static () => Match(char.IsWhiteSpace).Named("ws")
        );

    /// <summary>
    /// Parses a series of required whitespace characters and returns them as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> Whitespace() => _whitespace.Value;

    private static readonly Lazy<IParser<char, string>> _whitespace
        = new Lazy<IParser<char, string>>(
            static () => WhitespaceCharacter()
                .ListCharToString(true)
                .Named("whitespace")
        );

    /// <summary>
    /// Parses an optional series of whitespace characters and returns them as a string.
    /// </summary>
    /// <returns></returns>
    public static IParser<char, string> OptionalWhitespace() => _optionalWhitespace.Value;

    private static readonly Lazy<IParser<char, string>> _optionalWhitespace
        = new Lazy<IParser<char, string>>(
            static () => WhitespaceCharacter()
                .ListCharToString(false)
                .Named("whitespace?")
        );
}
