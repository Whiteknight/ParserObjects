﻿using System;
using ParserObjects.Internal.Parsers;
using ParserObjects.Pratt;

namespace ParserObjects;

public static partial class Parsers<TInput>
{
    /// <summary>
    /// Creates a Pratt parser, which is especially useful for mathematical expression parsing.
    /// </summary>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> Pratt<TOutput>(Action<IConfiguration<TInput, TOutput>> setup)
        => PrattParser<TInput, TOutput>.Configure(setup);
}