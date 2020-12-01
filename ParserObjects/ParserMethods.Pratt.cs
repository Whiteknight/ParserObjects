﻿using ParserObjects.Parsers;
using System;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IParser<TInput, TOutput> Pratt<TOutput>(Action<Pratt<TInput, TOutput>.IConfiguration> setup) 
            => new Pratt<TInput, TOutput>.Parser(setup);
    }
}
