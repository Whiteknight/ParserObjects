﻿using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IParser<TInput> Cache(IParser<TInput> p)
            => new Cache.NoOutputParser<TInput>(p);

        public static IParser<TInput, TOutput> Cache<TOutput>(IParser<TInput, TOutput> p)
            => new Cache.OutputParser<TInput, TOutput>(p);
    }
}