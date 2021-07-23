using System;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IMultiParser<TInput, TOutput> Earley<TOutput>(Func<Earley<TInput, TOutput>.SymbolFactory, INonterminal<TInput, TOutput>> setup)
            => Earley<TInput, TOutput>.Setup(setup);
    }
}
