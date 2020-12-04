using System;
using ParserObjects.Earley;
using ParserObjects.Parsers;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IMultiParser<TInput, TOutput> Earley<TOutput>(Func<Earley<TInput, TOutput>.SymbolFactory, INonterminal<TInput, TOutput>> setup)
        {
            var factory = new Earley<TInput, TOutput>.SymbolFactory();
            var startNonterminal = setup(factory);
            return new Earley<TInput, TOutput>.Parser(startNonterminal);
        }
    }
}
