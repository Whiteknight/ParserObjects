using System;
using ParserObjects.Parsers;
using ParserObjects.Pratt;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IParser<TInput, TOutput> Pratt<TOutput>(Action<IConfiguration<TInput, TOutput>> setup)
            => new PrattParser<TInput, TOutput>(setup);
    }
}