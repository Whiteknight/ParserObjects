﻿using System;
using ParserObjects.Internal.Parsers;

namespace ParserObjects;

public static class ParserResultsExtensions
{
    /// <summary>
    /// If the result is an error, invoke the callback to modify the result and result
    /// metadata. If the original result is success, return it without modification.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="parser"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static IParser<TInput, TOutput> TransformError<TInput, TOutput>(this IParser<TInput, TOutput> parser, Func<Transform<TInput, TOutput, TOutput>.SingleArguments, IResult<TOutput>> transform)
        => Parsers<TInput>.TransformResult<TOutput, TOutput>(parser, args =>
         {
             if (args.Result.Success)
                 return args.Result;
             return transform(args);
         });
}
