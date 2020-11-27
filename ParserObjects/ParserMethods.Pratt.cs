using ParserObjects.Parsers;
using System;

namespace ParserObjects
{
    public static partial class ParserMethods<TInput>
    {
        public static IParser<TInput, TOutput> Pratt<TOperator, TOutput>(IParser<TInput, TOutput> values, Pratt<TInput, TOperator, TOutput>.IConfiguration config)
            => new Pratt<TInput, TOperator, TOutput>.Parser(values, config);

        public static IParser<TInput, TOutput> Pratt<TOperator, TOutput>(IParser<TInput, TOutput> values, Action<Pratt<TInput, TOperator, TOutput>.IConfiguration> setup)
        {
            var config = Pratt<TInput, TOperator, TOutput>.CreateConfiguration();
            setup?.Invoke(config);
            return new Pratt<TInput, TOperator, TOutput>.Parser(values, config);
        }
    }
}
